namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfParagraph : PdfElement
{
    public const string DefaultFontFamily = "Helvetica"; 
    public const float DefaultFontSize = 12f;
    public static readonly Color DefaultTextColor = Colors.Black;
    public const TextAlignment DefaultAlignment = TextAlignment.Start;
    public const FontAttributes DefaultFontAttributes = FontAttributes.None;

    public string Text { get; }

    public string CurrentFontFamily { get; private set; } = DefaultFontFamily;

    public float CurrentFontSize { get; private set; } = DefaultFontSize;

    public Color CurrentTextColor { get; private set; } = DefaultTextColor;

    public TextAlignment CurrentAlignment { get; private set; } = DefaultAlignment;

    public FontAttributes CurrentFontAttributes { get; private set; } = DefaultFontAttributes;

    public PdfParagraph(string text)
    {
        Text = text ?? string.Empty; 
    }

    public PdfParagraph FontFamily(string family)
    {
        CurrentFontFamily = string.IsNullOrWhiteSpace(family) ? DefaultFontFamily : family;
        return this;
    }

    public PdfParagraph FontSize(float size)
    {
        CurrentFontSize = size > 0 ? size : DefaultFontSize;
        return this;
    }

    public PdfParagraph TextColor(Color color)
    {
        CurrentTextColor = color ?? DefaultTextColor;
        return this;
    }

    public PdfParagraph Alignment(TextAlignment alignment)
    {
        CurrentAlignment = alignment;
        return this;
    }

    public PdfParagraph FontAttribute(FontAttributes attributes)
    {
        CurrentFontAttributes = attributes;
        return this;
    }
}
