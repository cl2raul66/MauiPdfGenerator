using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Models;
using System.Diagnostics;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfParagraphBuilder : IPdfParagraph, IBuildablePdfElement
{
    private readonly PdfParagraphData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfParagraphBuilder(string text, PdfFontRegistryBuilder fontRegistry)
    {
        _model = new PdfParagraphData(text);
        _fontRegistry = fontRegistry;
    }

    public PdfElementData GetModel() => _model;

    public IPdfParagraph BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfParagraph FontAttributes(FontAttributes attributes) { _model.CurrentFontAttributes = attributes; return this; }
    public IPdfParagraph FontSize(float size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Font size must be a positive value.");
        _model.CurrentFontSize = size;
        return this;
    }
    public IPdfParagraph HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfParagraph HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfParagraph HorizontalTextAlignment(TextAlignment alignment) { _model.CurrentHorizontalTextAlignment = alignment; return this; }
    public IPdfParagraph LineBreakMode(LineBreakMode mode) { _model.CurrentLineBreakMode = mode; return this; }
    public IPdfParagraph Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfParagraph Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfParagraph Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfParagraph Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfParagraph Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfParagraph Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfParagraph TextColor(Color color) { _model.CurrentTextColor = color; return this; }
    public IPdfParagraph TextDecorations(TextDecorations decorations) { _model.CurrentTextDecorations = decorations; return this; }
    public IPdfParagraph TextTransform(TextTransform transform) { _model.CurrentTextTransform = transform; return this; }
    public IPdfParagraph VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfParagraph VerticalTextAlignment(TextAlignment alignment) { _model.CurrentVerticalTextAlignment = alignment; return this; }
    public IPdfParagraph WidthRequest(double width) { _model.WidthRequest(width); return this; }

    public IPdfParagraph FontFamily(PdfFontIdentifier? family)
    {
        _model.CurrentFontFamily = family;

        if (family.HasValue)
        {
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);

            if (_model.ResolvedFontRegistration is null)
            {
                Debug.WriteLine($"[ParagraphBuilder.FontFamily] WARNING: The font with alias '{family.Value.Alias}' was not found in the document's font registry. " +
                                  "A system or default font will be attempted during rendering if it is the font finally selected for the paragraph.");
            }
        }
        else
        {
            _model.ResolvedFontRegistration = null;
        }
        return this;
    }
}
