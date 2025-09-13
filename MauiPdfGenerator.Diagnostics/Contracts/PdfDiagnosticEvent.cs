namespace MauiPdfGenerator.Diagnostics.Contracts;

public record PdfDiagnosticEvent(
    PdfDiagnosticCode Code,
    PdfDiagnosticSeverity Severity, 
    PdfDiagnosticCategory Category, 
    string ComponentType, 
    string Message, 
    int PageIndex, 
    object? ElementData = null
);
