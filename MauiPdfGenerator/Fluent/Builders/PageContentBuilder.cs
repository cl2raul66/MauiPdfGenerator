using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PageContentBuilder : IPageContentBuilder
{
    private readonly List<PdfElement> _children = [];

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

    internal IReadOnlyList<PdfElement> GetChildren() => _children.AsReadOnly();
}
