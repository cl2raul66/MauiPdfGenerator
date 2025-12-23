using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;
using Xunit;

namespace MauiPdfGenerator.Diagnostics.Test;

public class DiagnosticMessageTests
{
    [Fact]
    public void ToString_WithoutContextData_FormatsCorrectly()
    {
        // Arrange
        var message = new DiagnosticMessage(DiagnosticSeverity.Warning, "CODE-001", "This is a test message.");

        // Act
        var result = message.ToString();

        // Assert
        Assert.Equal("[MauiPdfGenerator][CODE-001][Warning] This is a test message.", result);
    }

    [Fact]
    public void ToString_WithContextData_FormatsCorrectly()
    {
        // Arrange
        var contextData = new Dictionary<string, object>
            {
                { "ElementId", "Image1" },
                { "SourceUrl", "http://example.com/image.png" }
            };
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "CODE-002", "Another test message.", null, contextData);

        // Act
        var result = message.ToString();

        // Assert
        Assert.Equal("[MauiPdfGenerator][CODE-002][Error] Another test message. | Context: ElementId='Image1' SourceUrl='http://example.com/image.png'", result);
    }
}
