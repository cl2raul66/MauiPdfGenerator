using MauiPdfGenerator.Diagnostics.Contracts;

namespace MauiPdfGenerator.Diagnostics.Sinks;

internal interface IPdfDiagnosticsSink
{
    void Handle(PdfDiagnosticEvent diagnosticEvent);
}
