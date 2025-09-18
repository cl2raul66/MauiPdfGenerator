using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics.Interfaces;

public interface IDiagnosticListener
{
    void OnMessageSubmitted(DiagnosticMessage message);
}
