using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace MauiPdfGenerator.Tests.SourceGenerators;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator> where TSourceGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, RoslynNormalizingVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
            var options = base.CreateCompilationOptions();
            return options.WithSpecificDiagnosticOptions(
                options.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: null);
            return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
        }
    }

    public class RoslynNormalizingVerifier : IVerifier
    {
        public void EqualOrDiff(string expected, string actual, string? message = null)
        {
            var expectedNormalized = Normalize(expected);
            var actualNormalized = Normalize(actual);
            Assert.Equal(expectedNormalized, actualNormalized);
        }

        private static string Normalize(string code)
        {
            return CSharpSyntaxTree.ParseText(code)
                .GetRoot()
                .NormalizeWhitespace()
                .ToFullString()
                .Trim();
        }

        public void True(bool value, string? message = null) => Assert.True(value, message);
        public void False(bool value, string? message = null) => Assert.False(value, message);

        [DoesNotReturn]
        public void Fail(string? message = null) => Assert.Fail(message);

        public IDisposable CreateContext(string? _) => new EmptyDisposable();

        public void Empty<T>(string collectionName, IEnumerable<T> collection) => Assert.Empty(collection);
        public void Equal<T>(T expected, T actual, string? _ = null) => Assert.Equal(expected, actual);
        public void LanguageIsSupported(string language) => Assert.Equal(LanguageNames.CSharp, language);
        public void NotEmpty<T>(string collectionName, IEnumerable<T> collection) => Assert.NotEmpty(collection);

        public void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T>? equalityComparer = null, string? _ = null)
        {
            if (equalityComparer is not null) Assert.Equal(expected, actual, equalityComparer);
            else Assert.Equal(expected, actual);
        }

        public IVerifier PushContext(string? _) => this;

        private sealed class EmptyDisposable : IDisposable { public void Dispose() { } }
    }
}
