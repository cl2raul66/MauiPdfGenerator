using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics.Interfaces;

public interface IDiagnosticCanvas
{
    void DrawRectangle(DiagnosticRect bounds, Color color, float thickness, bool isDashed);
    void DrawLabel(string text, PointF position, Color color, float fontSize);
}
