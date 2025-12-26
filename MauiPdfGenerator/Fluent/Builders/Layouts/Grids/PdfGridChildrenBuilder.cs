using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfGridChildrenBuilder(PdfFontRegistryBuilder fontRegistry) : IPdfGridChildrenBuilder
{
    private readonly PdfFontRegistryBuilder _fontRegistry = fontRegistry;
    internal List<IBuildablePdfElement> Children { get; } = [];

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

    public IPdfGridChildParagraph Paragraph(Action<IPdfSpanConfigurator> configure)
    {
        var builder = new PdfParagraphBuilder(configure, _fontRegistry);
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

    public void VerticalStackLayout(Action<IPdfGridChildVerticalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfVerticalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        AddChild(stackBuilder);
    }

    public void HorizontalStackLayout(Action<IPdfGridChildHorizontalStackLayout> layoutSetup)
    {
        var stackBuilder = new PdfHorizontalStackLayoutBuilder(_fontRegistry);
        layoutSetup(stackBuilder);
        AddChild(stackBuilder);
    }

    public void Grid(Action<IPdfGrid> layoutSetup)
    {
        var gridBuilder = new PdfGridBuilder(_fontRegistry);
        layoutSetup(gridBuilder);
        AddChild(gridBuilder);
    }
}
