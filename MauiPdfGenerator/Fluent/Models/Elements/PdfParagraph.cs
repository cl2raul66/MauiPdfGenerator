namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfParagraph : PdfElement
{
    public const string DefaultFontFamily = "Helvetica"; 
    public const float DefaultFontSize = 12f;
    public static readonly Color DefaultTextColor = Colors.Black;
    public const TextAlignment DefaultAlignment = TextAlignment.Start;
    public const FontAttributes DefaultFontAttributes = Microsoft.Maui.Controls.FontAttributes.None;
    public const LineBreakMode DefaultLineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap;

    internal string Text { get; }

    internal string? CurrentFontFamily { get; private set; }

    internal float CurrentFontSize { get; private set; }

    internal Color? CurrentTextColor { get; private set; }

    internal TextAlignment CurrentAlignment { get; private set; }

    internal FontAttributes? CurrentFontAttributes { get; private set; }

    internal LineBreakMode? CurrentLineBreakMode { get; private set; }

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

    public PdfParagraph FontAttributes(FontAttributes attributes)
    {
        CurrentFontAttributes = attributes;
        return this;
    }

    public PdfParagraph LineBreakMode(LineBreakMode mode)
    {
        CurrentLineBreakMode = mode;
        return this;
    }
}
