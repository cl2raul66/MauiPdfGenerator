using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics.Interfaces;

public interface IDiagnosticSink
{
    void Submit(DiagnosticMessage message);
}
