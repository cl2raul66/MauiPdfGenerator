namespace MauiPdfGenerator.Fluent.Enums;

/// <summary>
/// Defines common predefined page sizes.
/// (Valores internos dependerán de la librería PDF subyacente)
/// </summary>
public enum PageSizeType
{
    /// <summary>
    /// ISO A4 size (210 x 297 mm).
    /// </summary>
    A4,
    /// <summary>
    /// US Letter size (8.5 x 11 inches).
    /// </summary>
    Letter,
    /// <summary>
    /// US Legal size (8.5 x 14 inches).
    /// </summary>
    Legal
    // Añadir otros tamaños comunes según sea necesario (A3, A5, B5, Tabloid, etc.)
}
