namespace MauiPdfGenerator.Common.Enums;

/// <summary>
/// Define la jerarquía de autoridad para el valor de una propiedad.
/// Inspirado en el sistema de precedencia de valores de .NET MAUI.
/// </summary>
internal enum PdfPropertyPriority
{
    /// <summary>
    /// Valor por defecto del sistema (Constructor). Prioridad más baja.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Valor aplicado por un estilo implícito (basado en Tipo).
    /// </summary>
    ImplicitStyle = 1,

    /// <summary>
    /// Valor aplicado por un estilo explícito (basado en Clave/Key).
    /// </summary>
    ExplicitStyle = 2,

/// <summary>
/// Valor aplicado directamente por el usuario en el Builder. Prioridad máxima.
/// </summary>
    Local = 3
}

/// <summary>
/// Define los tipos de unidades para medidas de grid en PDF.
/// Similar a GridUnitType de .NET MAUI pero optimizado para el motor de PDF.
/// </summary>
internal enum PdfGridUnitType
{
    /// <summary>
    /// Unidad absoluta en puntos (pt).
    /// </summary>
    Absolute,

    /// <summary>
    /// Unidad automática: el tamaño se determina por el contenido.
    /// </summary>
    Auto,

    /// <summary>
    /// Unidad proporcional: ocupa el espacio restante proporcionalmente.
    /// </summary>
    Star
}
