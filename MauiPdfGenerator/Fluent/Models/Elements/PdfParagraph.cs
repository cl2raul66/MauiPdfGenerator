using MauiPdfGenerator.Fluent.Builders;
using System.Diagnostics;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfParagraph : PdfElement
{
    public const float DefaultFontSize = 12f;
    public static readonly Color DefaultTextColor = Colors.Black;
    public const TextAlignment DefaultHorizontalTextAlignment = TextAlignment.Start;
    public const TextAlignment DefaultVerticalTextAlignment = TextAlignment.Start;
    public const FontAttributes DefaultFontAttributes = Microsoft.Maui.Controls.FontAttributes.None;
    public const LineBreakMode DefaultLineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap;
    public const TextDecorations DefaultTextDecorations = Microsoft.Maui.TextDecorations.None;
    public const TextTransform DefaultTextTransform = Microsoft.Maui.TextTransform.None;

    internal string Text { get; } 
    internal PdfFontIdentifier? CurrentFontFamily { get; private set; }
    internal float CurrentFontSize { get; private set; }
    internal Color? CurrentTextColor { get; private set; }
    internal TextAlignment CurrentHorizontalTextAlignment { get; private set; }
    internal TextAlignment CurrentVerticalTextAlignment { get; private set; }
    internal FontAttributes? CurrentFontAttributes { get; private set; }
    internal LineBreakMode? CurrentLineBreakMode { get; private set; }
    internal TextDecorations? CurrentTextDecorations { get; private set; }
    internal TextTransform? CurrentTextTransform { get; private set; }
    internal bool IsContinuation { get; private set; } = false;

    private readonly PdfFontRegistryBuilder? _fontRegistryRef;
    internal FontRegistration? ResolvedFontRegistration { get; private set; }

    internal PdfParagraph(string text, PdfFontRegistryBuilder? fontRegistry)
    {
        Text = text ?? string.Empty;
        _fontRegistryRef = fontRegistry;

        CurrentFontFamily = null;
        CurrentFontSize = 0;
        CurrentTextColor = null;
        CurrentHorizontalTextAlignment = DefaultHorizontalTextAlignment;
        CurrentVerticalTextAlignment = DefaultVerticalTextAlignment;
        CurrentFontAttributes = null;
        CurrentLineBreakMode = null;
        CurrentTextDecorations = null;
        CurrentTextTransform = null;
    }

    internal PdfParagraph(string text, PdfParagraph originalStyleSource)
    {
        Text = text ?? string.Empty;
        _fontRegistryRef = originalStyleSource._fontRegistryRef;

        this.CurrentFontFamily = originalStyleSource.CurrentFontFamily;
        this.ResolvedFontRegistration = originalStyleSource.ResolvedFontRegistration;
        this.CurrentFontSize = originalStyleSource.CurrentFontSize;
        this.CurrentTextColor = originalStyleSource.CurrentTextColor;
        this.CurrentHorizontalTextAlignment = originalStyleSource.CurrentHorizontalTextAlignment;
        this.CurrentVerticalTextAlignment = originalStyleSource.CurrentVerticalTextAlignment;
        this.CurrentFontAttributes = originalStyleSource.CurrentFontAttributes;
        this.CurrentLineBreakMode = originalStyleSource.CurrentLineBreakMode;
        this.CurrentTextDecorations = originalStyleSource.CurrentTextDecorations;
        this.CurrentTextTransform = originalStyleSource.CurrentTextTransform;
        this.Margin(originalStyleSource.GetMargin.Left, originalStyleSource.GetMargin.Top, originalStyleSource.GetMargin.Right, originalStyleSource.GetMargin.Bottom);
        this.IsContinuation = true;
    }

    public new PdfParagraph Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfParagraph Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfParagraph Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfParagraph Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfParagraph Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfParagraph Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfParagraph WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfParagraph HeightRequest(double height) { base.HeightRequest(height); return this; }

    public new PdfParagraph HorizontalOptions(LayoutAlignment layoutAlignment) { base.HorizontalOptions(layoutAlignment); return this; }
    public new PdfParagraph VerticalOptions(LayoutAlignment layoutAlignment) { base.VerticalOptions(layoutAlignment); return this; }
    public new PdfParagraph BackgroundColor(Color? color) { base.BackgroundColor(color); return this; }

    public PdfParagraph FontFamily(PdfFontIdentifier? family)
    {
        CurrentFontFamily = family;

        if (family.HasValue && _fontRegistryRef is not null)
        {
            ResolvedFontRegistration = _fontRegistryRef.GetFontRegistration(family.Value);

            if (ResolvedFontRegistration is null)
            {
                Debug.WriteLine($"[PdfParagraph.FontFamily] WARNING: The font with alias '{family.Value.Alias}' was not found in the document's font registry. " +
                                  "A system or default font will be attempted during rendering if it is the font finally selected for the paragraph.");
            }
        }
        else
        {
            ResolvedFontRegistration = null;
        }
        return this;
    }

    public PdfParagraph FontSize(float size)
    {
        CurrentFontSize = size > 0 ? size : 0;
        return this;
    }

    public PdfParagraph TextColor(Color color)
    {
        CurrentTextColor = color;
        return this;
    }

    public PdfParagraph HorizontalTextAlignment(TextAlignment alignment)
    {
        CurrentHorizontalTextAlignment = alignment;
        return this;
    }

    public PdfParagraph VerticalTextAlignment(TextAlignment alignment)
    {
        CurrentVerticalTextAlignment = alignment;
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

    public PdfParagraph TextDecorations(TextDecorations decorations)
    {
        CurrentTextDecorations = decorations;
        return this;
    }

    public PdfParagraph TextTransform(TextTransform transform)
    {
        CurrentTextTransform = transform;
        return this;
    }
}
