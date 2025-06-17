using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PageContentBuilder : IPageContentBuilder
{
    private readonly List<PdfElement> _children = [];
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PageContentBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
    }

    internal IReadOnlyList<PdfElement> GetChildren() => _children.AsReadOnly();

    public PdfParagraph Paragraph(string text)
    {
        var paragraph = new PdfParagraph(text, _fontRegistry);
        _children.Add(paragraph);
        return paragraph;
    }

    public PdfHorizontalLine HorizontalLine()
    {
        var line = new PdfHorizontalLine();
        _children.Add(line);
        return line;
    }

    public PdfImage PdfImage(Stream stream)
    {
        var image = new PdfImage(stream);
        _children.Add(image);
        return image;
    }

    public PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfVerticalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        _children.Add(stack);
        return stack;
    }

    public PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfHorizontalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        _children.Add(stack);
        return stack;
    }

    public PdfGrid PdfGrid()
    {
        var grid = new PdfGrid(_fontRegistry);
        _children.Add(grid);
        return grid;
    }
}
