using MauiPdfGenerator.Diagnostics.Contracts;

namespace MauiPdfGenerator.Diagnostics;

public class PdfDiagnosticOptions
{
    public bool Enabled { get; set; } = true;
    public PdfDiagnosticSeverity MinSeverity { get; set; } = PdfDiagnosticSeverity.Warning;
    public PdfDiagnosticMode Mode { get; set; } = PdfDiagnosticMode.Markers;
}