namespace MauiPdfGenerator.Diagnostics;

public static class DiagnosticCodes
{
    // Errores de Layout
    public const string LayoutOverflow = "LAYOUT-001";
    public const string PageContentOversized = "LAYOUT-002";
    public const string AtomicElementPaged = "LAYOUT-003"; // Informativo
    public const string IneffectiveVerticalOptions = "LAYOUT-004"; // Informativo

    // Errores de Recursos
    public const string ImageDecodeError = "RESOURCE-001";
    public const string FontNotFound = "RESOURCE-002";
}
