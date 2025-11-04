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

    public IPdfGridChildVerticalStackLayout VerticalStackLayout(Action<IPdfGridChildVerticalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        AddChild(stackBuilder);
        return stackBuilder;
    }

    public IPdfGridChildHorizontalStackLayout HorizontalStackLayout(Action<IPdfGridChildHorizontalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        AddChild(stackBuilder);
        return stackBuilder;
    }

    public IPdfGrid Grid(Action<IPdfGrid> layoutSetup)
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        layoutSetup(gridBuilder);
        AddChild(gridBuilder);
        return gridBuilder;
    }
}
