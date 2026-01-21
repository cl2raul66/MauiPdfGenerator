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
                    }
                    return invocationExpr; 
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
                    !string.IsNullOrWhiteSpace(aliasLiteralSyntax.Token.ValueText))
                {
                    userAlias = aliasLiteralSyntax.Token.ValueText;
                }
                else
                {
                    userAlias = Path.GetFileNameWithoutExtension(fontFileName);
                }

                if (string.IsNullOrWhiteSpace(userAlias))
                {
                    Debug.WriteLine($"[FontAliasGenerator] Warning: The font from file '{fontFileName}' was skipped because the resulting alias is null or empty.");
                    continue;
                }

                string csharpIdentifier = CreateValidCSharpIdentifier(userAlias);

                if (generatedCSharpIdentifiers.Add(csharpIdentifier))
                {
                    fontInfos.Add(new FontInfo(userAlias, csharpIdentifier));
                }
                else
                {
                    Debug.WriteLine($"[FontAliasGenerator] Warning: The original alias '{userAlias}' results in a duplicate C# identifier ('{csharpIdentifier}') and will be ignored for static property generation.");
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
                    propertiesBuilder.AppendLine($"        /// <summary>Identifier for the font with original alias '{EscapeStringForComment(info.OriginalAlias)}'.</summary>");
                    propertiesBuilder.AppendLine($"        public static {PdfFontIdentifierFullName} {info.CSharpIdentifier} {{ get; }} = new {PdfFontIdentifierFullName}(\"{EscapeStringForCode(info.OriginalAlias)}\");");
                    propertiesBuilder.AppendLine();
                }
            }
            else
            {
                propertiesBuilder.AppendLine("        // No MAUI font aliases found to generate PdfFontIdentifier identifiers.");
            }

            string propertiesContent = propertiesBuilder.ToString().TrimEnd('\r', '\n');

            var fileSource = new StringBuilder($@"// <auto-generated/>
// Generated by {nameof(FontAliasGenerator)} from MauiPdfGenerator.
// DO NOT MODIFY MANUALLY.

namespace {GeneratedClassNamespace}
{{
    /// <summary>
    /// Provides static <see cref=""{PdfFontIdentifierFullName}""/> identifiers
    /// for fonts registered in the MAUI application through calls to AddFont().
    /// These identifiers must be used when specifying fonts in the MauiPdfGenerator API.
    /// </summary>
    public static class {GeneratedClassName}
    {{
{propertiesContent}
    }}
}}");
            context.AddSource(GeneratedFileName, SourceText.From(fileSource.ToString(), Encoding.UTF8));
            Debug.WriteLine($"[FontAliasGenerator] {GeneratedFileName} generated.");
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

            StringBuilder sb = new();
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

    // Test comment for workflow
}
            }

            string candidate = sb.ToString();
            while (candidate.Length > 1 && candidate.EndsWith("_"))
            {
                candidate = candidate.Substring(0, candidate.Length - 1);
            }
            if (candidate == "_") candidate = "_Font"; 


            if (string.IsNullOrWhiteSpace(candidate)) 
                candidate = "_Font";

            if (candidate.Length > 0 && char.IsDigit(candidate[0]))
            {
                candidate = "_" + candidate;
            }

            if (SyntaxFacts.GetKeywordKind(candidate) != SyntaxKind.None ||
                SyntaxFacts.GetContextualKeywordKind(candidate) != SyntaxKind.None)
            {
                return "@" + candidate;
            }

            if (candidate == "_") 
            {
                string originalSanitized = Regex.Replace(input.Trim(), @"[^\p{L}\p{N}]", "");
                if (!string.IsNullOrWhiteSpace(originalSanitized))
                    return SanitizeAndShorten(originalSanitized); 
                return "_DefaultFontAlias";
            }

            return candidate;
        }

        private static string SanitizeAndShorten(string text, int maxLength = 50) 
        {
            if (string.IsNullOrEmpty(text)) return "_Unknown";
            var rg = new Regex("[^a-zA-Z0-9_]");
            string sanitized = rg.Replace(text, "");

            if (string.IsNullOrWhiteSpace(sanitized)) return "_InvalidChars";

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
