using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
// Asegúrate de tener esta directiva si usas Debug.WriteLine (opcional, pero útil):
using System.Diagnostics;

// El namespace del PROYECTO GENERADOR (Biblioteca .NET Standard 2.0)
namespace MauiPdfGenerator.SourceGenerators
{
    [Generator]
    public class FontAliasGenerator : IIncrementalGenerator
    {
        // Lista de las 14 fuentes PDF estándar Base14.
        // Fuente: PDF 1.7 Specification, Section 5.5.1 Standard Type 1 Fonts
        private static readonly ImmutableArray<string> StandardPdfBase14Fonts = ImmutableArray.Create(
            // Times
            "Times-Roman",
            "Times-Bold",
            "Times-Italic",
            "Times-BoldItalic",
            // Helvetica (equivalente a Arial en muchos sistemas)
            "Helvetica",
            "Helvetica-Bold",
            "Helvetica-Oblique", // Equivalente a Italic
            "Helvetica-BoldOblique",
            // Courier (monoespaciada)
            "Courier",
            "Courier-Bold",
            "Courier-Oblique",
            "Courier-BoldOblique",
            // Symbolos
            "Symbol",
            "ZapfDingbats"
        );

        // El namespace donde se generará la clase de alias en el proyecto MAUI
        private const string OutputNamespace = "MauiPdfGenerator.Generated";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // PASO 1: Encontrar invocaciones .AddFont(file, alias)
            // Usamos la lógica que YA TE FUNCIONA gracias a Claude
            IncrementalValuesProvider<InvocationExpressionSyntax> potentialAddFontCalls = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsPotentialAddFontCall(node), // El predicado que funcionó
                    transform: static (ctx, ct) => GetValidAddFontCallIfCorrect(ctx, ct)) // La transformación que funcionó
                .Where(static m => m is not null)!;

            // PASO 2: Combinar con la compilación
            IncrementalValueProvider<(Compilation, ImmutableArray<InvocationExpressionSyntax>)> compilationAndInvocations
                = context.CompilationProvider.Combine(potentialAddFontCalls.Collect());

            // PASO 3: Generar el código fuente
            context.RegisterSourceOutput(compilationAndInvocations,
                static (spc, source) => GenerateAliasesClass(source.Item1, source.Item2, spc));

            // Puedes mantener o quitar la salida de diagnóstico de Claude si ya no la necesitas
            // context.RegisterPostInitializationOutput(ctx => { /* ... código diagnóstico ... */ });
        }

        // --- MÉTODOS DE DETECCIÓN (USA LOS QUE TE FUNCIONARON) ---
        // Estos métodos deben ser exactamente los que Claude te dio y que
        // confirmaste que funcionaban para detectar las fuentes de tu MAUI App.

        static bool IsPotentialAddFontCall(SyntaxNode node)
        {
            // Lógica de Claude que funcionó (probablemente similar a esto):
            return node is InvocationExpressionSyntax ies &&
                  ies.Expression is MemberAccessExpressionSyntax maes &&
                  maes.Name.Identifier.Text.Equals("AddFont", StringComparison.OrdinalIgnoreCase); // O Ordinal si funcionó así
        }

        static InvocationExpressionSyntax? GetValidAddFontCallIfCorrect(GeneratorSyntaxContext context, CancellationToken ct)
        {
            // Lógica de Claude que funcionó (probablemente similar a esto):
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            if (invocationExpr.ArgumentList is not null &&
                invocationExpr.ArgumentList.Arguments.Count == 2)
            {
                var aliasArgument = invocationExpr.ArgumentList.Arguments[1];
                if (aliasArgument.Expression is LiteralExpressionSyntax literalExpr &&
                    literalExpr.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    if (!string.IsNullOrWhiteSpace(literalExpr.Token.ValueText))
                    {
                        return invocationExpr;
                    }
                }
            }
            return null;
        }

        // --- GENERACIÓN DEL CÓDIGO (MODIFICADO PARA INCLUIR ESTÁNDAR) ---

        static void GenerateAliasesClass(Compilation compilation, ImmutableArray<InvocationExpressionSyntax> discoveredInvocations, SourceProductionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            Debug.WriteLine($"[FontAliasGenerator] Starting GenerateAliasesClass. Discovered invocations count: {discoveredInvocations.Length}");

            // Usar HashSet para manejar duplicados automáticamente (estándar vs descubiertas, y descubiertas entre sí)
            // Usar StringComparer.Ordinal para ser sensible a mayúsculas/minúsculas (como MAUI)
            var finalAliases = new HashSet<string>(StringComparer.Ordinal);

            // 1. Añadir siempre las fuentes estándar PDF Base14
            foreach (var stdFont in StandardPdfBase14Fonts)
            {
                // No debería haber nulos/blancos, pero por seguridad
                if (!string.IsNullOrWhiteSpace(stdFont))
                {
                    finalAliases.Add(stdFont);
                    // Debug.WriteLine($"[FontAliasGenerator] Added standard font: {stdFont}");
                }
            }
            Debug.WriteLine($"[FontAliasGenerator] Alias count after standard fonts: {finalAliases.Count}");

            // 2. Añadir las fuentes descubiertas del proyecto MAUI (si las hay)
            if (!discoveredInvocations.IsDefaultOrEmpty)
            {
                foreach (var invocationExpr in discoveredInvocations)
                {
                    // Extraer alias (asumiendo que GetValidAddFontCallIfCorrect ya validó)
                    var aliasArgument = invocationExpr.ArgumentList!.Arguments[1];
                    var literalExpr = (LiteralExpressionSyntax)aliasArgument.Expression!;
                    string discoveredAlias = literalExpr.Token.ValueText; // Ya sin comillas

                    // Add devolverá true si se añadió, false si ya existía
                    bool added = finalAliases.Add(discoveredAlias);
                    // if (added) Debug.WriteLine($"[FontAliasGenerator] Added discovered font: {discoveredAlias}");
                    // else Debug.WriteLine($"[FontAliasGenerator] Discovered font '{discoveredAlias}' already exists.");
                }
            }
            else
            {
                Debug.WriteLine("[FontAliasGenerator] No discovered AddFont invocations from MAUI project.");
            }

            Debug.WriteLine($"[FontAliasGenerator] Total final unique aliases: {finalAliases.Count}");

            // Si no hay NINGÚN alias (ni estándar ni descubierto), no generar archivo.
            // Esto es improbable si StandardPdfBase14Fonts tiene elementos.
            if (!finalAliases.Any())
            {
                Debug.WriteLine("[FontAliasGenerator] No aliases to generate. Skipping file output.");
                return;
            }

            // Generar el código fuente usando el namespace de salida correcto
            string sourceCode = GenerateSourceCodeString(finalAliases);
            context.AddSource("MauiFontAliases.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
            Debug.WriteLine("[FontAliasGenerator] MauiFontAliases.g.cs source added to compilation.");
        }

        private static string GenerateSourceCodeString(HashSet<string> aliases)
        {
            // Usamos el namespace OutputNamespace definido arriba ("MauiPdfGenerator.Generated")
            var sourceBuilder = new StringBuilder($@"// <auto-generated/>
// Generated by MauiPdfGenerator.SourceGenerators.FontAliasGenerator
#pragma warning disable
namespace {OutputNamespace} // Namespace para la CLASE GENERADA
{{
    /// <summary>
    /// Provides compile-time safe constants for font aliases.
    /// Includes standard PDF Base14 fonts and fonts registered via .AddFont(file, alias)
    /// in the MAUI application. Generated by MauiPdfGenerator.SourceGenerators.
    /// </summary>
    public static class MauiFontAliases
    {{
");
            // Usar Ordinal para evitar colisiones de identificadores C# si los alias solo difieren en mayúsculas/minúsculas
            // pero generan el mismo identificador (ej. "My_Font" y "my_font" -> My_Font)
            var generatedIdentifiers = new HashSet<string>(StringComparer.Ordinal);

            // Ordenar alfabéticamente (ignorando mayúsculas) para una salida consistente
            foreach (string alias in aliases.OrderBy(a => a, StringComparer.OrdinalIgnoreCase))
            {
                string identifier = CreateValidIdentifier(alias);
                if (generatedIdentifiers.Add(identifier)) // Solo añadir si el IDENTIFICADOR es nuevo
                {
                    string escapedAlias = System.Security.SecurityElement.Escape(alias) ?? alias;
                    sourceBuilder.AppendLine($"        /// <summary>Font alias constant for '{escapedAlias}'. Value: \"{alias}\"</summary>");
                    // El valor de la constante es el alias ORIGINAL (sensible a mayúsculas)
                    sourceBuilder.AppendLine($"        public const string {identifier} = \"{alias}\";");
                    sourceBuilder.AppendLine();
                }
                else
                {
                    // Opcional: Podrías loggear o añadir un diagnóstico si se omite un identificador duplicado
                    Debug.WriteLine($"[FontAliasGenerator] Skipping duplicate identifier '{identifier}' for alias '{alias}'.");
                }
            }
            sourceBuilder.AppendLine("    }"); // Fin clase
            sourceBuilder.AppendLine("}"); // Fin namespace
            sourceBuilder.AppendLine("#pragma warning restore");
            return sourceBuilder.ToString();
        }

        // --- MÉTODO UTILITARIO (USA EL QUE TE FUNCIONÓ) ---
        private static string CreateValidIdentifier(string input)
        {
            // Lógica de Claude que funcionó (probablemente similar a esto):
            if (string.IsNullOrWhiteSpace(input)) return "_";
            string identifier = input.Trim();
            identifier = Regex.Replace(identifier, @"[^\p{L}\p{N}_]", "_"); // Permitir letras Unicode, números, _
            if (identifier.Length > 0 && char.IsDigit(identifier[0])) identifier = "_" + identifier;
            else if (string.IsNullOrEmpty(identifier.Replace("_", ""))) return "_"; // Si solo quedan '_' o vacío
            // Comprobar palabras clave C#
            if (SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None)
                identifier = "@" + identifier;
            return identifier;
        }
    }
}
