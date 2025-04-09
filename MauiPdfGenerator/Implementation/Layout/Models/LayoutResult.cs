using MauiPdfGenerator.Common.Geometry;

namespace MauiPdfGenerator.Implementation.Layout.Models;

/// <summary>
/// Encapsula el resultado de una operación de layout.
/// </summary>
internal class LayoutResult
{
    /// <summary>
    /// El elemento que fue medido y posicionado
    /// </summary>
    public object Element { get; }

    /// <summary>
    /// El tamaño final del elemento después de la medición
    /// </summary>
    public PdfSize MeasuredSize { get; }

    /// <summary>
    /// El rectángulo final donde se posicionó el elemento
    /// </summary>
    public PdfRectangle FinalRect { get; private set; }

    /// <summary>
    /// Indica si el layout se realizó correctamente
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Mensaje de error si el layout falló
    /// </summary>
    public string? Error { get; }

    private LayoutResult(object element, PdfSize measuredSize, bool success, string? error = null)
    {
        Element = element;
        MeasuredSize = measuredSize;
        Success = success;
        Error = error;
        FinalRect = PdfRectangle.Empty;
    }

    public static LayoutResult CreateSuccess(object element, PdfSize measuredSize)
        => new(element, measuredSize, true);

    public static LayoutResult CreateFailure(object element, string error)
        => new(element, PdfSize.Zero, false, error);

    internal void SetFinalRect(PdfRectangle rect) => FinalRect = rect;
}