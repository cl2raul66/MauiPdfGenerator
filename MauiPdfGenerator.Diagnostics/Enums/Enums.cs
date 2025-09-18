namespace MauiPdfGenerator.Diagnostics.Enums;

public enum DiagnosticSeverity
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public enum VisualDiagnosticMode
{
    /// <summary>
    /// No visual diagnostics will be rendered on the PDF. This is the default.
    /// </summary>
    Off,
    /// <summary>
    /// Visual diagnostics (e.g., layout bounds, overflow errors) will be rendered on the PDF.
    /// </summary>
    On
}
