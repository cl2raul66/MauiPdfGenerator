namespace MauiPdfGenerator.Implementation.Layout.Models;

/// <summary>
/// Define las restricciones que se aplican durante el proceso de layout.
/// </summary>
internal class LayoutConstraints
{
    /// <summary>
    /// Restricciones sin limitaciones
    /// </summary>
    public static readonly LayoutConstraints None = new();

    /// <summary>
    /// Indica si el elemento debe ajustarse al ancho disponible
    /// </summary>
    public bool FillWidth { get; set; }

    /// <summary>
    /// Indica si el elemento debe ajustarse al alto disponible
    /// </summary>
    public bool FillHeight { get; set; }

    /// <summary>
    /// Ancho mínimo requerido
    /// </summary>
    public double? MinWidth { get; set; }

    /// <summary>
    /// Alto mínimo requerido
    /// </summary>
    public double? MinHeight { get; set; }

    /// <summary>
    /// Ancho máximo permitido
    /// </summary>
    public double? MaxWidth { get; set; }

    /// <summary>
    /// Alto máximo permitido
    /// </summary>
    public double? MaxHeight { get; set; }

    /// <summary>
    /// Márgenes a aplicar
    /// </summary>
    public Thickness Margin { get; set; } = Thickness.Zero;

    /// <summary>
    /// Padding a aplicar
    /// </summary>
    public Thickness Padding { get; set; } = Thickness.Zero;
}