using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions; 

namespace MauiPdfGenerator.SourceGenerators
{
    [Generator]
    public class FontAliasGenerator : IIncrementalGenerator
    {
        private const string GeneratedClassNamespace = "MauiPdfGenerator.Fonts";
        private const string GeneratedClassName = "PdfFonts";
        private const string GeneratedFileName = "PdfFonts.g.cs";
        private const string PdfFontIdentifierFullName = "global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<InvocationExpressionSyntax> potentialAddFontCalls = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsPotentialAddFontCall(node),
                    transform: static (ctx, ct) => GetValidAddFontCallIfCorrect(ctx, ct))
                .Where(static m => m is not null)!;

            IncrementalValueProvider<ImmutableArray<InvocationExpressionSyntax>> mauiInvocations
                = potentialAddFontCalls.Collect();

            context.RegisterSourceOutput(mauiInvocations,
                static (spc, source) => GenerateSourceFile(source, spc));
        }

        static bool IsPotentialAddFontCall(SyntaxNode node)
        {
            return node is InvocationExpressionSyntax ies &&
                   ies.Expression is MemberAccessExpressionSyntax maes &&
                   maes.Name.Identifier.Text.Equals("AddFont", StringComparison.Ordinal);
        }

        static InvocationExpressionSyntax? GetValidAddFontCallIfCorrect(GeneratorSyntaxContext context, CancellationToken ct)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;

            if (invocationExpr.ArgumentList is not null &&
                invocationExpr.ArgumentList.Arguments.Count >= 1 &&
                invocationExpr.ArgumentList.Arguments.Count <= 2)
            {
                var fileArgument = invocationExpr.ArgumentList.Arguments[0];
                if (fileArgument.Expression is LiteralExpressionSyntax fileLiteral &&
                    fileLiteral.IsKind(SyntaxKind.StringLiteralExpression) &&
                    !string.IsNullOrWhiteSpace(fileLiteral.Token.ValueText))
                {
                    if (invocationExpr.ArgumentList.Arguments.Count == 2)
                    {
                        var aliasArgument = invocationExpr.ArgumentList.Arguments[1];
                        if (aliasArgument.Expression is LiteralExpressionSyntax aliasLiteral &&
                            aliasLiteral.IsKind(SyntaxKind.StringLiteralExpression) &&
                            !string.IsNullOrWhiteSpace(aliasLiteral.Token.ValueText))
                        {
                            return invocationExpr;
                        }
                        if (!(aliasArgument.Expression is LiteralExpressionSyntax aliasLitCheck && aliasLitCheck.IsKind(SyntaxKind.StringLiteralExpression)) &&
                            !(aliasArgument.Expression is IdentifierNameSyntax idName && idName.Identifier.ValueText == "null"))
                        {
                            return null;
                        }
                        // Si llega aquí, el segundo argumento es un literal de cadena válido o "null", por lo que es una llamada válida.
                        // Si aliasLiteral fue null o vacío arriba, se manejará al extraer el userAlias.
                    }
                    return invocationExpr; // Válido si solo hay filename o si el alias es un literal (o null)
                }
            }
            return null;
        }

        static void GenerateSourceFile(ImmutableArray<InvocationExpressionSyntax> discoveredInvocations, SourceProductionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (discoveredInvocations.IsDefaultOrEmpty)
            {
                GeneratePdfFontsFile(context, []);
                return;
            }

            var fontInfos = new List<FontInfo>();
            var generatedCSharpIdentifiers = new HashSet<string>(StringComparer.Ordinal);

            foreach (var invocationExpr in discoveredInvocations)
            {
                var fileLiteralSyntax = (LiteralExpressionSyntax)invocationExpr.ArgumentList!.Arguments[0].Expression!;
                string fontFileName = fileLiteralSyntax.Token.ValueText;
                string userAlias;

                if (invocationExpr.ArgumentList!.Arguments.Count == 2 &&
                    invocationExpr.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax aliasLiteralSyntax &&
                    aliasLiteralSyntax.IsKind(SyntaxKind.StringLiteralExpression) &&
                    !string.IsNullOrWhiteSpace(aliasLiteralSyntax.Token.ValueText)) // Asegurar que el alias no sea solo espacios en blanco
                {
                    userAlias = aliasLiteralSyntax.Token.ValueText;
                }
                else
                {
                    userAlias = Path.GetFileNameWithoutExtension(fontFileName);
                }

                if (string.IsNullOrWhiteSpace(userAlias))
                {
                    Debug.WriteLine($"[FontAliasGenerator] Advertencia: Se omitió la fuente del archivo '{fontFileName}' porque el alias resultante es nulo o está vacío.");
                    continue;
                }

                string csharpIdentifier = CreateValidCSharpIdentifier(userAlias);

                if (generatedCSharpIdentifiers.Add(csharpIdentifier))
                {
                    fontInfos.Add(new FontInfo(userAlias, csharpIdentifier));
                }
                else
                {
                    Debug.WriteLine($"[FontAliasGenerator] Advertencia: El alias original '{userAlias}' resulta en un identificador C# duplicado ('{csharpIdentifier}') y será ignorado para la generación de la propiedad estática.");
                }
            }

            GeneratePdfFontsFile(context, fontInfos);
        }

        private static void GeneratePdfFontsFile(SourceProductionContext context, IEnumerable<FontInfo> fontInfos)
        {
            var propertiesBuilder = new StringBuilder();
            if (fontInfos.Any())
            {
                foreach (var info in fontInfos)
                {
                    propertiesBuilder.AppendLine($"        /// <summary>Identificador para la fuente con alias original '{EscapeStringForComment(info.OriginalAlias)}'.</summary>");
                    // CORRECCIÓN: Eliminar 'readonly' de la propiedad
                    propertiesBuilder.AppendLine($"        public static {PdfFontIdentifierFullName} {info.CSharpIdentifier} {{ get; }} = new {PdfFontIdentifierFullName}(\"{EscapeStringForCode(info.OriginalAlias)}\");");
                    propertiesBuilder.AppendLine();
                }
            }
            else
            {
                propertiesBuilder.AppendLine("        // No se encontraron alias de fuentes MAUI para generar identificadores PdfFontIdentifier.");
            }

            string propertiesContent = propertiesBuilder.ToString().TrimEnd('\r', '\n');

            var fileSource = new StringBuilder($@"// <auto-generated/>
// Generado por {nameof(FontAliasGenerator)} de MauiPdfGenerator.
// NO MODIFICAR MANUALMENTE.

namespace {GeneratedClassNamespace}
{{
    /// <summary>
    /// Proporciona identificadores <see cref=""{PdfFontIdentifierFullName}""/> estáticos
    /// para las fuentes registradas en la aplicación MAUI a través de llamadas a AddFont().
    /// Estos identificadores deben usarse al especificar fuentes en la API de MauiPdfGenerator.
    /// </summary>
    public static class {GeneratedClassName}
    {{
{propertiesContent}
    }}
}}
");
            context.AddSource(GeneratedFileName, SourceText.From(fileSource.ToString(), Encoding.UTF8));
            Debug.WriteLine($"[FontAliasGenerator] {GeneratedFileName} generado.");
        }

        private static string EscapeStringForCode(string? value)
        {
            if (value is null) return string.Empty;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
        private static string EscapeStringForComment(string? value)
        {
            if (value is null) return string.Empty;
            return value.Replace("<", "<").Replace(">", ">").Replace("&", "&");
        }

        private static string CreateValidCSharpIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "_Font";

            StringBuilder sb = new StringBuilder();
            bool firstChar = true;
            bool lastCharWasUnderscore = false;

            foreach (char c in input)
            {
                if (firstChar)
                {
                    if (char.IsLetter(c) || c == '_')
                    {
                        sb.Append(c);
                        firstChar = false;
                        lastCharWasUnderscore = (c == '_');
                    }
                    else if (char.IsDigit(c))
                    {
                        sb.Append('_');
                        sb.Append(c);
                        firstChar = false;
                        lastCharWasUnderscore = false;
                    }
                }
                else
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                        lastCharWasUnderscore = false;
                    }
                    else if (c == '_')
                    {
                        if (!lastCharWasUnderscore)
                        {
                            sb.Append('_');
                            lastCharWasUnderscore = true;
                        }
                    }
                    else
                    {
                        if (!lastCharWasUnderscore)
                        {
                            sb.Append('_');
                            lastCharWasUnderscore = true;
                        }
                    }
                }
            }

            string candidate = sb.ToString();
            while (candidate.Length > 1 && candidate.EndsWith("_"))
            {
                candidate = candidate.Substring(0, candidate.Length - 1);
            }
            if (candidate == "_") candidate = "_Font"; // Si solo queda un _


            if (string.IsNullOrWhiteSpace(candidate)) // Fallback adicional
                candidate = "_Font";

            // Asegurar que el primer carácter no sea un dígito (ya manejado arriba, pero por si acaso)
            if (candidate.Length > 0 && char.IsDigit(candidate[0]))
            {
                candidate = "_" + candidate;
            }


            if (SyntaxFacts.GetKeywordKind(candidate) != SyntaxKind.None ||
                SyntaxFacts.GetContextualKeywordKind(candidate) != SyntaxKind.None)
            {
                return "@" + candidate;
            }

            if (candidate == "_") // Si después de todo sigue siendo solo "_"
            {
                string originalSanitized = Regex.Replace(input.Trim(), @"[^\p{L}\p{N}]", "");
                if (!string.IsNullOrWhiteSpace(originalSanitized))
                    return SanitizeAndShorten(originalSanitized); // Usar función auxiliar
                return "_DefaultFontAlias";
            }

            return candidate;
        }
        // Función auxiliar para limpiar y acortar si CreateValidCSharpIdentifier produce algo muy genérico
        private static string SanitizeAndShorten(string text, int maxLength = 50) // Aumentar un poco el maxLength
        {
            if (string.IsNullOrEmpty(text)) return "_Unknown";
            // Permitir números, remover el resto que no sea letra o _
            var rg = new Regex("[^a-zA-Z0-9_]");
            string sanitized = rg.Replace(text, "");

            if (string.IsNullOrWhiteSpace(sanitized)) return "_InvalidChars";

            // Quitar guiones bajos múltiples o al inicio/final si no son el único carácter
            sanitized = Regex.Replace(sanitized, "_{2,}", "_");
            if (sanitized.Length > 1) sanitized = sanitized.Trim('_');
            if (sanitized == "_") return "_UnderscoreOnly";


            if (char.IsDigit(sanitized[0])) sanitized = "_" + sanitized;
            if (sanitized.Length == 0) return "_EmptyAfterSanitize";


            if (sanitized.Length > maxLength) sanitized = sanitized.Substring(0, maxLength);

            return sanitized;
        }

        private readonly struct FontInfo
        {
            public string OriginalAlias { get; }
            public string CSharpIdentifier { get; }

            public FontInfo(string originalAlias, string csharpIdentifier)
            {
                OriginalAlias = originalAlias;
                CSharpIdentifier = csharpIdentifier;
            }
        }
    }
}
