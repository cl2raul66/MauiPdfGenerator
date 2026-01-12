using MauiPdfGenerator.Diagnostics.Enums;
using System.Text;

namespace MauiPdfGenerator.Diagnostics.Models;

public record DiagnosticMessage(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    DiagnosticRect? Bounds = null,
    IReadOnlyDictionary<string, object>? ContextData = null)
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"[MauiPdfGenerator][{Code}][{Severity}] {Message}");

        if (ContextData is not null && ContextData.Any())
        {
            sb.Append(" | Context: ");
            foreach (var kvp in ContextData)
            {
                sb.Append($"{kvp.Key}='{kvp.Value}' ");
            }
        }

        return sb.ToString().Trim();
    }
}
