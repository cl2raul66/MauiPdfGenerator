using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Diagnostics;

public class DiagnosticMessageTests
{
    [Fact]
    public void ToString_WithoutContextData_FormatsCorrectly()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Warning, "CODE-001", "This is a test message.");

        var result = message.ToString();

        Assert.Equal("[MauiPdfGenerator][CODE-001][Warning] This is a test message.", result);
    }

    [Fact]
    public void ToString_WithContextData_FormatsCorrectly()
    {
        var contextData = new Dictionary<string, object>
            {
                { "ElementId", "Image1" },
                { "SourceUrl", "http://example.com/image.png" }
            };
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "CODE-002", "Another test message.", null, contextData);

        var result = message.ToString();

        Assert.Equal("[MauiPdfGenerator][CODE-002][Error] Another test message. | Context: ElementId='Image1' SourceUrl='http://example.com/image.png'", result);
    }
}
