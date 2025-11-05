using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;
using System.Diagnostics;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfParagraphBuilder : IBuildablePdfElement, IPdfPageChildParagraph, IPdfLayoutChildParagraph, IPdfGridChildParagraph
{
    private readonly PdfParagraphData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfParagraphBuilder(string text, PdfFontRegistryBuilder fontRegistry)
    {
        _model = new PdfParagraphData(text);
        _fontRegistry = fontRegistry;
    }

    public PdfElementData GetModel() => _model;

    #region Public API (Implements the most specific interface: IPdfGridChildParagraph)

    public IPdfGridChildParagraph FontFamily(PdfFontIdentifier? family)
    {
        _model.CurrentFontFamily = family;
        if (family.HasValue)
        {
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);
            if (_model.ResolvedFontRegistration is null)
            {
                Debug.WriteLine($"[ParagraphBuilder.FontFamily] WARNING: The font with alias '{family.Value.Alias}' was not found in the document's font registry.");
            }
        }
        else
        {
            _model.ResolvedFontRegistration = null;
        }
        return this;
    }

    public IPdfGridChildParagraph FontSize(float size) { if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "Font size must be a positive value."); _model.CurrentFontSize = size; return this; }
    public IPdfGridChildParagraph TextColor(Color color) { _model.CurrentTextColor = color; return this; }
    public IPdfGridChildParagraph HorizontalTextAlignment(TextAlignment alignment) { _model.CurrentHorizontalTextAlignment = alignment; return this; }
    public IPdfGridChildParagraph VerticalTextAlignment(TextAlignment alignment) { _model.CurrentVerticalTextAlignment = alignment; return this; }
    public IPdfGridChildParagraph FontAttributes(FontAttributes attributes) { _model.CurrentFontAttributes = attributes; return this; }
    public IPdfGridChildParagraph LineBreakMode(LineBreakMode mode) { _model.CurrentLineBreakMode = mode; return this; }
    public IPdfGridChildParagraph TextDecorations(TextDecorations decorations) { _model.CurrentTextDecorations = decorations; return this; }
    public IPdfGridChildParagraph TextTransform(TextTransform transform) { _model.CurrentTextTransform = transform; return this; }
    public IPdfGridChildParagraph Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfGridChildParagraph Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildParagraph Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildParagraph Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfGridChildParagraph Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildParagraph Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildParagraph WidthRequest(double width) { _model.WidthRequest(width); return this; }
    public IPdfGridChildParagraph HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfGridChildParagraph BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfGridChildParagraph HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildParagraph VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfGridChildParagraph Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildParagraph Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildParagraph RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildParagraph ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }

    #endregion

    #region Explicit Interface Implementations to satisfy the compiler and maintain fluency

    // IPdfParagraph<IPdfPageChildParagraph>
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontFamily(PdfFontIdentifier? family) { FontFamily(family); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontSize(float size) { FontSize(size); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextColor(Color color) { TextColor(color); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.HorizontalTextAlignment(TextAlignment alignment) { HorizontalTextAlignment(alignment); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.VerticalTextAlignment(TextAlignment alignment) { VerticalTextAlignment(alignment); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontAttributes(FontAttributes attributes) { FontAttributes(attributes); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.LineBreakMode(LineBreakMode mode) { LineBreakMode(mode); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextDecorations(TextDecorations decorations) { TextDecorations(decorations); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextTransform(TextTransform transform) { TextTransform(transform); return this; }

    // IPdfParagraph<IPdfLayoutChildParagraph>
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontFamily(PdfFontIdentifier? family) { FontFamily(family); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontSize(float size) { FontSize(size); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextColor(Color color) { TextColor(color); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.HorizontalTextAlignment(TextAlignment alignment) { HorizontalTextAlignment(alignment); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.VerticalTextAlignment(TextAlignment alignment) { VerticalTextAlignment(alignment); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontAttributes(FontAttributes attributes) { FontAttributes(attributes); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.LineBreakMode(LineBreakMode mode) { LineBreakMode(mode); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextDecorations(TextDecorations decorations) { TextDecorations(decorations); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextTransform(TextTransform transform) { TextTransform(transform); return this; }

    // IPdfElement<IPdfPageChildParagraph>
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfElement<IPdfLayoutChildParagraph>
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfLayoutChild<IPdfLayoutChildParagraph>
    IPdfLayoutChildParagraph IPdfLayoutChild<IPdfLayoutChildParagraph>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildParagraph IPdfLayoutChild<IPdfLayoutChildParagraph>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    #endregion
}
