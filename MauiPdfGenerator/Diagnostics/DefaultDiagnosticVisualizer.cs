using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics;

public class DefaultDiagnosticVisualizer : IDiagnosticVisualizer
{
    public bool CanVisualize(DiagnosticMessage message)
    {
        return message.Bounds.HasValue;
    }

    public void Visualize(IDiagnosticCanvas canvas, DiagnosticMessage message)
    {
        var color = message.Severity switch
        {
            Enums.DiagnosticSeverity.Error or Enums.DiagnosticSeverity.Critical => Colors.Red,
            Enums.DiagnosticSeverity.Warning => Colors.Orange,
            _ => Colors.Blue
        };

        var bounds = message.Bounds!.Value;
        canvas.DrawRectangle(bounds, color, 1f, isDashed: false);
        var label = $"[{message.Code}]";
        canvas.DrawLabel(label, new PointF(bounds.Left + 2, bounds.Top + 2), color, 8f);
    }
}
