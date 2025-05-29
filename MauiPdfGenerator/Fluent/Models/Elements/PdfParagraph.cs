using MauiPdfGenerator.Fluent.Models; // Asegurar using

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfParagraph : PdfElement
{
    // Ya no hay un DefaultFontFamily aquí, se resolverá a null si no se especifica
    public const float DefaultFontSize = 12f;
    public static readonly Color DefaultTextColor = Colors.Black;
    public const TextAlignment DefaultAlignment = TextAlignment.Start;
    public const FontAttributes DefaultFontAttributes = Microsoft.Maui.Controls.FontAttributes.None;
    public const LineBreakMode DefaultLineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap;

    internal string Text { get; }
    // CurrentFontFamily es ahora nullable
    internal PdfFontIdentifier? CurrentFontFamily { get; private set; }
    internal float CurrentFontSize { get; private set; }
    internal Color? CurrentTextColor { get; private set; }
    internal TextAlignment CurrentAlignment { get; private set; }
    internal FontAttributes? CurrentFontAttributes { get; private set; }
    internal LineBreakMode? CurrentLineBreakMode { get; private set; }
    internal bool IsContinuation { get; private set; } = false;

    public PdfParagraph(string text)
    {
        Text = text ?? string.Empty;
        CurrentFontFamily = null; // Null significa "no especificado", se resolverá más tarde
        CurrentFontSize = 0;      // 0 significa "no especificado", se resolverá más tarde
    }

    internal PdfParagraph(string text, PdfParagraph originalStyleSource) : this(text)
    {
        this.CurrentFontFamily = originalStyleSource.CurrentFontFamily;
        this.CurrentFontSize = originalStyleSource.CurrentFontSize;
        this.CurrentTextColor = originalStyleSource.CurrentTextColor;
        this.CurrentAlignment = originalStyleSource.CurrentAlignment;
        this.CurrentFontAttributes = originalStyleSource.CurrentFontAttributes;
        this.CurrentLineBreakMode = originalStyleSource.CurrentLineBreakMode;
        this.IsContinuation = true;
    }

    // Acepta PdfFontIdentifier?
    public PdfParagraph FontFamily(PdfFontIdentifier? family)
    {
        CurrentFontFamily = family;
        return this;
    }

    public PdfParagraph FontSize(float size)
    {
        CurrentFontSize = size > 0 ? size : 0;
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
