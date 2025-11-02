namespace MauiPdfGenerator.Common.Models.Elements;

internal class PdfImageData : PdfElementData
{
    internal Aspect CurrentAspect { get; set; } = Aspect.AspectFit;

    internal Stream ImageStream { get; }

    internal PdfImageData(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        if (!stream.CanRead)
        {
            throw new ArgumentException("El Stream proporcionado para la imagen debe ser legible (CanRead debe ser true).", nameof(stream));
        }
        ImageStream = stream;
    }
}
