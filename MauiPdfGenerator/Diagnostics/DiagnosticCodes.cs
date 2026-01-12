namespace MauiPdfGenerator.Diagnostics;

public static class DiagnosticCodes
{
    public const string LayoutOverflow = "LAYOUT-001";
    public const string PageContentOversized = "LAYOUT-002";
    public const string AtomicElementPaged = "LAYOUT-003";
    public const string IneffectiveVerticalOptions = "LAYOUT-004"; 

    public const string ImageDecodeError = "RESOURCE-001";
    public const string FontNotFound = "RESOURCE-002";
    public const string StyleKeyNotFound = "RESOURCE-003";    

    public const string InvalidCulture = "I18N-001";
}
