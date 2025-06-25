using Microsoft.Maui.Graphics;
using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public partial class PdfImage : PdfElement
{
    public Aspect CurrentAspect { get; private set; } = Microsoft.Maui.Aspect.AspectFit;

    internal Stream ImageStream { get; }

    public PdfImage(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        if (!stream.CanRead)
        {
            throw new ArgumentException("El Stream proporcionado para la imagen debe ser legible (CanRead debe ser true).", nameof(stream));
        }
        ImageStream = stream;
        // Si no hay WidthRequest ni HeightRequest, establecer Fill según orientación
        if (!GetWidthRequest.HasValue && !GetHeightRequest.HasValue)
        {
            // Por defecto, Fill horizontal (para retrato o cuadrado)
            HorizontalOptions(LayoutAlignment.Fill);
            // Si en el futuro se detecta orientación paisaje, se podría usar VerticalOptions(LayoutAlignment.Fill)
        }
    }

    public new PdfImage Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfImage Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfImage Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfImage Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfImage Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfImage Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfImage WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfImage HeightRequest(double height) { base.HeightRequest(height); return this; }

    // New: Fluent API for layout options and background
    public new PdfImage HorizontalOptions(LayoutAlignment layoutAlignment) { base.HorizontalOptions(layoutAlignment); return this; }
    public new PdfImage VerticalOptions(LayoutAlignment layoutAlignment) { base.VerticalOptions(layoutAlignment); return this; }
    public new PdfImage BackgroundColor(Color? color) { base.BackgroundColor(color); return this; }

    public PdfImage Aspect(Aspect aspect)
    {
        CurrentAspect = aspect;
        return this;
    }
}
