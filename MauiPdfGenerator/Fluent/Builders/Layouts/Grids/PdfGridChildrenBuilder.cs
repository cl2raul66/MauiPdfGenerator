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
        return builder;
    }

    public IPdfGridChildImage Image(Stream stream)
    {
        var builder = new PdfImageBuilder(stream);
        AddChild(builder);
        return builder;
    }

    public IPdfGridChildHorizontalLine HorizontalLine()
    {
        var builder = new PdfHorizontalLineBuilder();
        AddChild(builder);
        return builder;
    }

    public IPdfGridChildVerticalStackLayout VerticalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        // CORRECCIÓN: Se pasa el fontRegistry al constructor.
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        AddChild(stackBuilder);
        return stackBuilder;
    }

    public IPdfGridChildHorizontalStackLayout HorizontalStackLayout(Action<IPdfStackLayoutBuilder> content)
    {
        // CORRECCIÓN: Se pasa el fontRegistry al constructor.
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        var contentBuilder = new PdfStackLayoutContentBuilder(stackBuilder, _fontRegistry);
        content(contentBuilder);
        AddChild(stackBuilder);
        return stackBuilder;
    }

    public IPdfGrid Grid()
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        AddChild(gridBuilder);
        return gridBuilder;
    }
}
