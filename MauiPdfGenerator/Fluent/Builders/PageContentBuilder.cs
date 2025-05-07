using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PageContentBuilder : IPageContentBuilder
{
    private readonly List<PdfElement> _children = [];

    internal IReadOnlyList<PdfElement> GetChildren() => _children.AsReadOnly();

    public PdfParagraph Paragraph(string text)
    {
        var paragraph = new PdfParagraph(text);
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
}
