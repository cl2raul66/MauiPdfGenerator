using MauiPdfGenerator.Fluent.Builders.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfGridChildrenBuilder : IPdfGridChildrenBuilder
{
    private readonly PdfFontRegistryBuilder _fontRegistry;
    internal List<IBuildablePdfElement> Children { get; } = [];

    public PdfGridChildrenBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    private void AddChild(IBuildablePdfElement element)
    {
        Children.Add(element);
    }

    public IPdfGridChildParagraph Paragraph(string text)
    {
        var builder = new PdfParagraphBuilder(text, _fontRegistry);
        AddChild(builder);
        return new PdfGridChildParagraphBuilder(builder);
    }

    public IPdfGridChildImage Image(Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        AddChild(builder);
        return new PdfGridChildImageBuilder(builder);
    }

    public IPdfGridChildHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        AddChild(builder);
        return new PdfGridChildHorizontalLineBuilder(builder);
    }

    public IPdfGridChildVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder();
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        AddChild(stackBuilder);
        return new PdfGridChildVerticalStackLayoutBuilder(stackBuilder);
    }

    public IPdfGridChildHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder();
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        AddChild(stackBuilder);
        return new PdfGridChildHorizontalStackLayoutBuilder(stackBuilder);
    }

    public IPdfGrid Grid()
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        AddChild(gridBuilder);
        return gridBuilder;
    }
}
