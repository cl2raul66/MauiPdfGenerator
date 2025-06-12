using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfVerticalStackLayout : PdfLayoutElement<PdfVerticalStackLayout>, IStackLayoutBuilder
{
    private readonly List<PdfElement> _children = [];
    private readonly PdfFontRegistryBuilder _fontRegistry;

    internal IReadOnlyList<PdfElement> Children => _children.AsReadOnly();

    internal PdfVerticalStackLayout(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
        // Default options for a VSL
        GetHorizontalOptions = LayoutAlignment.Start;
        GetVerticalOptions = LayoutAlignment.Start;
    }

    internal PdfVerticalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfVerticalStackLayout originalStyleSource)
    {
        _children.AddRange(remainingChildren);
        _fontRegistry = originalStyleSource._fontRegistry;
        GetSpacing = originalStyleSource.GetSpacing;
        GetBackgroundColor = originalStyleSource.GetBackgroundColor;
        GetHorizontalOptions = originalStyleSource.GetHorizontalOptions;
        GetVerticalOptions = originalStyleSource.GetVerticalOptions;
        Margin(originalStyleSource.GetMargin);
        Padding(originalStyleSource.GetPadding);
    }

    public PdfParagraph Paragraph(string text) { var p = new PdfParagraph(text, _fontRegistry); _children.Add(p); return p; }
    public PdfHorizontalLine HorizontalLine() { var line = new PdfHorizontalLine(); _children.Add(line); return line; }
    public PdfImage PdfImage(Stream stream) { var img = new PdfImage(stream); _children.Add(img); return img; }
    public PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content) { var stack = new PdfVerticalStackLayout(_fontRegistry); content(stack); _children.Add(stack); return stack; }
    public PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content) { var stack = new PdfHorizontalStackLayout(_fontRegistry); content(stack); _children.Add(stack); return stack; }
}
