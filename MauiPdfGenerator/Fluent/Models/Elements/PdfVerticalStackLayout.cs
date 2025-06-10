using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfVerticalStackLayout : PdfElement, IStackLayoutBuilder
{
    private readonly List<PdfElement> _children = [];
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public IReadOnlyList<PdfElement> Children => _children.AsReadOnly();

    internal float CurrentSpacing { get; private set; }
    internal Color? CurrentBackgroundColor { get; private set; }
    internal LayoutAlignment PdfHorizontalOptions { get; private set; } = LayoutAlignment.Start;
    internal LayoutAlignment PdfVerticalOptions { get; private set; } = LayoutAlignment.Start;

    internal PdfVerticalStackLayout(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    internal PdfVerticalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfVerticalStackLayout originalStyleSource)
    {
        _children.AddRange(remainingChildren);
        _fontRegistry = originalStyleSource._fontRegistry;
        CurrentSpacing = originalStyleSource.CurrentSpacing;
        CurrentBackgroundColor = originalStyleSource.CurrentBackgroundColor;
        PdfHorizontalOptions = originalStyleSource.PdfHorizontalOptions;
        PdfVerticalOptions = originalStyleSource.PdfVerticalOptions;
        Margin(originalStyleSource.GetMargin);
    }

    public PdfVerticalStackLayout Spacing(float value)
    {
        CurrentSpacing = value;
        return this;
    }

    public PdfVerticalStackLayout BackgroundColor(Color color)
    {
        CurrentBackgroundColor = color;
        return this;
    }

    public PdfVerticalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        PdfHorizontalOptions = layoutAlignment;
        return this;
    }

    public PdfVerticalStackLayout VerticalOptions(LayoutAlignment layoutAlignment)
    {
        PdfVerticalOptions = layoutAlignment;
        return this;
    }

    public PdfParagraph Paragraph(string text)
    {
        var p = new PdfParagraph(text, _fontRegistry);
        _children.Add(p);
        return p;
    }
    public PdfHorizontalLine HorizontalLine()
    {
        var line = new PdfHorizontalLine();
        _children.Add(line);
        return line;
    }
    public PdfImage PdfImage(Stream stream)
    {
        var img = new PdfImage(stream);
        _children.Add(img);
        return img;
    }
    public PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfVerticalStackLayout(_fontRegistry);
        content(stack);
        _children.Add(stack);
        return stack;
    }
    public PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfHorizontalStackLayout(_fontRegistry);
        content(stack);
        _children.Add(stack);
        return stack;
    }
}
