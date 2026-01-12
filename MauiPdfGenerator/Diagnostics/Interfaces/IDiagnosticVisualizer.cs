using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics.Interfaces;

public interface IDiagnosticVisualizer
{
    bool CanVisualize(DiagnosticMessage message);
    void Visualize(IDiagnosticCanvas canvas, DiagnosticMessage message);
}
