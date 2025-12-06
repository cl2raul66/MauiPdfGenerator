using MauiPdfGenerator.Common.Models.Styling;

namespace MauiPdfGenerator.Common.Models.Elements;

internal class PdfImageData : PdfElementData
{
    internal PdfStyledProperty<Aspect> AspectProp { get; } = new(Microsoft.Maui.Aspect.AspectFit);
    internal Aspect CurrentAspect => AspectProp.Value;

    internal Stream ImageStream { get; }

    internal PdfImageData() : base() { ImageStream = Stream.Null; }

    internal PdfImageData(Stream stream) : base()
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        if (!stream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(stream));
        ImageStream = stream;
    }
}
