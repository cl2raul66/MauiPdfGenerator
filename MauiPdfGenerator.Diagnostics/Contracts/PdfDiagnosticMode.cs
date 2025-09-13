namespace MauiPdfGenerator.Diagnostics.Contracts;

public enum PdfDiagnosticMode
{
    /// <summary>
    /// No visual diagnostics are rendered.
    /// </summary>
    Off,
    /// <summary>
    /// Renders a visual marker (e.g., a red box with a label) over the area of a diagnostic event.
    /// </summary>
    Markers
}
