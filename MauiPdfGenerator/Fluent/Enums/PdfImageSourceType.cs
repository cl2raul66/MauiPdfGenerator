namespace MauiPdfGenerator.Fluent.Enums;

/// <summary>
/// Especifica el tipo de fuente de una imagen.
/// </summary>
public enum PdfImageSourceType
{
    /// <summary>
    /// No se ha especificado ninguna fuente.
    /// </summary>
    None,

    /// <summary>
    /// La imagen se carga desde una ruta de archivo o URL.
    /// </summary>
    PathOrUrl,

    /// <summary>
    /// La imagen se proporciona como un array de bytes.
    /// </summary>
    Bytes,

    /// <summary>
    /// La imagen se proporciona como un Stream.
    /// </summary>
    Stream
}