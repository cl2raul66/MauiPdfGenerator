﻿namespace MauiPdfGenerator.Fluent.Models.Elements;

public partial class PdfImage : PdfElement
{
    public Aspect CurrentAspect { get; private set; } = Microsoft.Maui.Aspect.AspectFit;

    internal double? RequestedWidth { get; private set; }

    internal double? RequestedHeight { get; private set; }

    internal Stream ImageStream { get; }

    public PdfImage(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        if (!stream.CanRead)
        {
            throw new ArgumentException("El Stream proporcionado para la imagen debe ser legible (CanRead debe ser true).", nameof(stream));
        }
        ImageStream = stream;
    }

    public PdfImage WidthRequest(double width)
    {
        RequestedWidth = width > 0 ? width : null;
        return this;
    }

    public PdfImage HeightRequest(double height)
    {
        RequestedHeight = height > 0 ? height : null;
        return this;
    }

    public PdfImage Aspect(Aspect aspect)
    {
        CurrentAspect = aspect;
        return this;
    }
}
