using System.Diagnostics;
using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfParagraphBuilder : IBuildablePdfElement, IPdfPageChildParagraph, IPdfLayoutChildParagraph, IPdfGridChildParagraph, IPdfParagraph
{
    private readonly PdfParagraphData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfParagraphBuilder(string text, PdfFontRegistryBuilder fontRegistry)
    {
        _model = new PdfParagraphData(text);
        _fontRegistry = fontRegistry;
    }

    public PdfElementData GetModel() => _model;

    // NOTA: Los métodos públicos retornan 'this' (el builder completo).
    // Al retornar 'this', podemos hacer casting seguro en las interfaces explícitas.

    #region Public API (Retorna el tipo más específico para facilitar el uso general)
    public IPdfGridChildParagraph FontFamily(PdfFontIdentifier? family)
    {
        _model.FontFamilyProp.Set(family, PdfPropertyPriority.Local);
        if (family.HasValue)
        {
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);
            if (_model.ResolvedFontRegistration is null)
            {
                Debug.WriteLine($"[ParagraphBuilder.FontFamily] WARNING: Font '{family.Value.Alias}' not found.");
            }
        }
        else
        {
            _model.ResolvedFontRegistration = null;
        }
        return this;
    }

    public IPdfGridChildParagraph FontSize(float size) { if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size)); _model.FontSizeProp.Set(size, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph TextColor(Color color) { _model.TextColorProp.Set(color, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph HorizontalTextAlignment(TextAlignment alignment) { _model.HorizontalTextAlignmentProp.Set(alignment, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph VerticalTextAlignment(TextAlignment alignment) { _model.VerticalTextAlignmentProp.Set(alignment, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph FontAttributes(FontAttributes attributes) { _model.FontAttributesProp.Set(attributes, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph LineBreakMode(LineBreakMode mode) { _model.LineBreakModeProp.Set(mode, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph TextDecorations(TextDecorations decorations) { _model.TextDecorationsProp.Set(decorations, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph TextTransform(TextTransform transform) { _model.TextTransformProp.Set(transform, PdfPropertyPriority.Local); return this; }

    public IPdfGridChildParagraph Margin(double u) { _model.SetMargin(u); return this; }
    public IPdfGridChildParagraph Margin(double h, double v) { _model.SetMargin(h, v); return this; }
    public IPdfGridChildParagraph Margin(double l, double t, double r, double b) { _model.SetMargin(l, t, r, b); return this; }
    public IPdfGridChildParagraph Padding(double u) { _model.SetPadding(u); return this; }
    public IPdfGridChildParagraph Padding(double h, double v) { _model.SetPadding(h, v); return this; }
    public IPdfGridChildParagraph Padding(double l, double t, double r, double b) { _model.SetPadding(l, t, r, b); return this; }
    public IPdfGridChildParagraph WidthRequest(double w) { _model.SetWidthRequest(w); return this; }
    public IPdfGridChildParagraph HeightRequest(double h) { _model.SetHeightRequest(h); return this; }
    public IPdfGridChildParagraph BackgroundColor(Color? c) { _model.SetBackgroundColor(c); return this; }
    public IPdfGridChildParagraph HorizontalOptions(LayoutAlignment a) { _model.SetHorizontalOptions(a); return this; }
    public IPdfGridChildParagraph VerticalOptions(LayoutAlignment a) { _model.SetVerticalOptions(a); return this; }
    public IPdfGridChildParagraph Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildParagraph Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildParagraph RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildParagraph ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    public IPdfGridChildParagraph Style(string key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations (Con Casting Seguro)

    // IPdfParagraph (Para Estilos)
    IPdfParagraph IPdfParagraph<IPdfParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }

    IPdfParagraph IPdfElement<IPdfParagraph>.Style(string k) { Style(k); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfPageChildParagraph
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Style(string k) { Style(k); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfLayoutChildParagraph
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Style(string k) { Style(k); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildParagraph IPdfElement<IPdfLayoutChildParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildParagraph IPdfLayoutChild<IPdfLayoutChildParagraph>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildParagraph IPdfLayoutChild<IPdfLayoutChildParagraph>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfGridChildParagraph
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Style(string k) { Style(k); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfGridChildParagraph IPdfElement<IPdfGridChildParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfGridChildParagraph IPdfLayoutChild<IPdfGridChildParagraph>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfGridChildParagraph IPdfLayoutChild<IPdfGridChildParagraph>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }
    #endregion
}
