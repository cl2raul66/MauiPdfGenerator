using MauiPdfGenerator.Common.Enums;

namespace MauiPdfGenerator.Common.Models;

/// <summary>
/// Representa una medida de longitud para filas y columnas de grid en PDF.
/// Equivalente a GridLength de .NET MAUI pero optimizado para float y sin dependencias de UI.
/// </summary>
internal readonly record struct PdfGridLength(float Value, PdfGridUnitType GridUnitType)
{
    /// <summary>
    /// Unidad absoluta (puntos).
    /// </summary>
    public static readonly PdfGridLength Absolute = new(1.0f, PdfGridUnitType.Absolute);

    /// <summary>
    /// Unidad automática.
    /// </summary>
    public static readonly PdfGridLength Auto = new(1.0f, PdfGridUnitType.Auto);

    /// <summary>
    /// Unidad proporcional (Star).
    /// </summary>
    public static readonly PdfGridLength Star = new(1.0f, PdfGridUnitType.Star);

    /// <summary>
    /// Crea una medida absoluta con el valor especificado.
    /// </summary>
    public static PdfGridLength FromAbsolute(float value) => new(value, PdfGridUnitType.Absolute);

    /// <summary>
    /// Crea una medida automática.
    /// </summary>
    public static PdfGridLength FromAuto() => Auto;

    /// <summary>
    /// Crea una medida proporcional con el valor especificado.
    /// </summary>
    public static PdfGridLength FromStar(float value = 1.0f) => new(value, PdfGridUnitType.Star);

    /// <summary>
    /// Indica si esta medida es absoluta.
    /// </summary>
    public bool IsAbsolute => GridUnitType == PdfGridUnitType.Absolute;

    /// <summary>
    /// Indica si esta medida es automática.
    /// </summary>
    public bool IsAuto => GridUnitType == PdfGridUnitType.Auto;

    /// <summary>
    /// Indica si esta medida es proporcional.
    /// </summary>
    public bool IsStar => GridUnitType == PdfGridUnitType.Star;
}