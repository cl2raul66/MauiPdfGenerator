using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable; // Necesario para ImmutableArray
using System.Text;
using System.Text.RegularExpressions;

namespace MauiPdfGenerator.SourceGenerators;

[Generator]
public class FontAliasGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // PASO 1: Filtrar nodos de sintaxis para encontrar posibles candidatos
        // Buscamos expresiones de invocación que podrían ser llamadas a AddFont
        IncrementalValuesProvider<InvocationExpressionSyntax> invocationExpressions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node), // Filtro rápido inicial
                transform: static (ctx, ct) => GetSemanticTargetForGeneration(ctx, ct)) // Obtener el nodo relevante si pasa el filtro semántico
            .Where(static m => m is not null)!; // Filtra los nulos donde la transformación no encontró un objetivo válido

        // PASO 2: Combinar los resultados con la compilación
        // Esto permite usar el modelo semántico si es necesario (aunque aquí nos basamos en sintaxis)
        IncrementalValueProvider<(Compilation, ImmutableArray<InvocationExpressionSyntax>)> compilationAndInvocations
            = context.CompilationProvider.Combine(invocationExpressions.Collect());

        // PASO 3: Generar el código fuente
        context.RegisterSourceOutput(compilationAndInvocations,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    /// <summary>
    /// Filtro rápido inicial: ¿Podría este nodo ser una llamada a AddFont?
    /// </summary>
    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // Solo nos interesan las llamadas a métodos (InvocationExpressionSyntax)
        return node is InvocationExpressionSyntax ies &&
               // Cuyo nombre *podría* ser AddFont (no distingue mayúsculas/minúsculas aquí para ser más permisivo inicialmente)
               ies.Expression is MemberAccessExpressionSyntax maes &&
               maes.Name.Identifier.Text.Equals("AddFont", StringComparison.OrdinalIgnoreCase);
        // Podríamos añadir más heurísticas si fuera necesario
    }

    /// <summary>
    /// Filtro semántico (opcional pero bueno): ¿Es realmente la llamada AddFont que buscamos
    /// y tiene los argumentos correctos?
    /// </summary>
    static InvocationExpressionSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        // Aquí podríamos usar el modelo semántico (context.SemanticModel) para verificar
        // el tipo al que pertenece AddFont, los tipos de los argumentos, etc.
        // Por ahora, nos quedaremos con la lógica sintáctica que teníamos en el ISyntaxReceiver
        // por simplicidad, pero esta es la ubicación correcta para el análisis semántico.

        if (invocationExpr.ArgumentList is not null &&
            invocationExpr.ArgumentList.Arguments.Count == 2)
        {
            var aliasArgument = invocationExpr.ArgumentList.Arguments[1]; // El segundo argumento

            // Asegurarse de que sea una cadena literal
            if (aliasArgument.Expression is LiteralExpressionSyntax literalExpr &&
                literalExpr.IsKind(SyntaxKind.StringLiteralExpression))
            {
                // Devuelve el nodo de invocación si cumple los criterios sintácticos básicos
                return invocationExpr;
            }
        }

        // No es el objetivo que buscamos
        return null;
    }

    /// <summary>
    /// Método principal donde se extraen los alias y se genera el código.
    /// </summary>
    static void Execute(Compilation compilation, ImmutableArray<InvocationExpressionSyntax> invocations, SourceProductionContext context)
    {
        // Si el token de cancelación lo indica, salir temprano
        context.CancellationToken.ThrowIfCancellationRequested();

        if (invocations.IsDefaultOrEmpty)
        {
            // No se encontraron invocaciones candidatas
            return;
        }

        // Usar un HashSet para evitar duplicados de alias
        var discoveredAliases = new HashSet<string>();

        foreach (var invocationExpr in invocations)
        {
            // Extraer el alias (ya sabemos que tiene 2 args y el 2do es string literal
            // gracias a GetSemanticTargetForGeneration)
            var aliasArgument = invocationExpr.ArgumentList!.Arguments[1];
            var literalExpr = (LiteralExpressionSyntax)aliasArgument.Expression!;
            string alias = literalExpr.Token.ValueText;

            if (!string.IsNullOrWhiteSpace(alias))
            {
                discoveredAliases.Add(alias);
            }
        }

        // Si no se descubrieron alias válidos, no generar archivo
        if (!discoveredAliases.Any())
            return;

        // Construir el código fuente (igual que antes)
        var sourceBuilder = new StringBuilder(@"// <auto-generated/>
#pragma warning disable
namespace MauiPdfGenerator.SourceGenerators
{
    /// <summary>
    /// Provides compile-time safe constants for font aliases potentially registered
    /// via .AddFont(file, alias) in the MAUI application.
    /// Generated by MauiPdfGenerator.SourceGenerators.
    /// </summary>
    public static class MauiFontAliases
    {
");

        var generatedIdentifiers = new HashSet<string>();

        // Ordenar alfabéticamente para una salida consistente
        foreach (string alias in discoveredAliases.OrderBy(a => a))
        {
            string identifier = CreateValidIdentifier(alias);

            if (generatedIdentifiers.Add(identifier))
            {
                sourceBuilder.AppendLine($"        /// <summary>Alias for font '{alias}'.</summary>");
                sourceBuilder.AppendLine($"        public const string {identifier} = \"{alias}\";");
                sourceBuilder.AppendLine();
            }
        }

        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}");
        sourceBuilder.AppendLine("#pragma warning restore");

        // Añade el archivo generado a la compilación
        context.AddSource("MauiFontAliases.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    // El método CreateValidIdentifier permanece igual que antes
    private static string CreateValidIdentifier(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "_";

        string identifier = input.Trim();
        identifier = Regex.Replace(identifier, @"[^a-zA-Z0-9_]", "_");

        if (char.IsDigit(identifier[0]))
        {
            identifier = "_" + identifier;
        }
        // Opcional: Comprobar palabras clave (menos probable para fuentes)
        // if (SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(identifier))) identifier = "@" + identifier;

        return identifier;
    }
}


