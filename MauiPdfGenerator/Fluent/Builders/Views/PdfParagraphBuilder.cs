using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Utils;

namespace MauiPdfGenerator.Fluent.Builders.Views;

internal class PdfParagraphBuilder :
    IBuildablePdfElement,
    IPdfPageChildParagraph,
    IPdfLayoutChildParagraph,
    IPdfGridChildParagraph,
    IPdfParagraph
{
    private readonly PdfParagraphData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly PdfResourceDictionary? _resourceDictionary;

    public PdfParagraphBuilder(string text, PdfFontRegistryBuilder fontRegistry, PdfResourceDictionary? resourceDictionary = null)
    {
        _model = new PdfParagraphData(text);
        _fontRegistry = fontRegistry;
        _resourceDictionary = resourceDictionary;
    }

    public PdfParagraphBuilder(Action<IPdfSpanText> configure, PdfFontRegistryBuilder fontRegistry, PdfResourceDictionary? resourceDictionary = null)
    {
        _model = new PdfParagraphData(string.Empty);
        _fontRegistry = fontRegistry;
        _resourceDictionary = resourceDictionary;

        var configurator = new PdfSpanConfigurator(fontRegistry, resourceDictionary);
        configure(configurator);

        var (text, spans) = configurator.Build();
        _model.SetText(text);
        _model.SetSpans(spans);
    }

    public PdfElementData GetModel() => _model;

    public IPdfGridChildParagraph Culture(string culture) { _model.Culture = culture; return this; }

    public IPdfGridChildParagraph Style(PdfStyleIdentifier key)
    {
        _model.Style(key);
        return this;
    }

    public IPdfGridChildParagraph FontFamily(PdfFontIdentifier? family)
    {
        _model.FontFamilyProp.Set(family, PdfPropertyPriority.Local);
        if (family.HasValue)
        {
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);
        }
        else
        {
            _model.ResolvedFontRegistration = null;
        }
        return this;
    }

    public IPdfGridChildParagraph FontSize(float size) { _model.FontSizeProp.Set(size, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph TextColor(Color color) { _model.TextColorProp.Set(color, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph HorizontalTextAlignment(TextAlignment alignment) { _model.HorizontalTextAlignmentProp.Set(alignment, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph VerticalTextAlignment(TextAlignment alignment) { _model.VerticalTextAlignmentProp.Set(alignment, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph FontAttributes(FontAttributes attributes) { _model.FontAttributesProp.Set(attributes, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph LineBreakMode(LineBreakMode mode) { _model.LineBreakModeProp.Set(mode, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph TextDecorations(TextDecorations decorations) { _model.TextDecorationsProp.Set(decorations, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph TextTransform(TextTransform transform) { _model.TextTransformProp.Set(transform, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph CharacterSpacing(float spacing) { _model.CharacterSpacingProp.Set(spacing, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph WordSpacing(float spacing) { _model.WordSpacingProp.Set(spacing, PdfPropertyPriority.Local); return this; }
    public IPdfGridChildParagraph LineSpacing(float spacing) { _model.LineSpacingProp.Set(spacing, PdfPropertyPriority.Local); return this; }

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



    IPdfParagraph IPdfParagraph<IPdfParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.CharacterSpacing(float s) { CharacterSpacing(s); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.WordSpacing(float s) { WordSpacing(s); return this; }
    IPdfParagraph IPdfParagraph<IPdfParagraph>.LineSpacing(float s) { LineSpacing(s); return this; }

    IPdfParagraph IPdfElement<IPdfParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfParagraph IPdfElement<IPdfParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    IPdfParagraph IPdfParagraph<IPdfParagraph>.Culture(string culture) { Culture(culture); return this; }



    IPdfPageChildParagraph IPdfStylableElement<IPdfPageChildParagraph>.Style(PdfStyleIdentifier k) { Style(k); return this; }

    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.CharacterSpacing(float s) { CharacterSpacing(s); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.WordSpacing(float s) { WordSpacing(s); return this; }
    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.LineSpacing(float s) { LineSpacing(s); return this; }

    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildParagraph IPdfElement<IPdfPageChildParagraph>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }



    IPdfLayoutChildParagraph IPdfStylableElement<IPdfLayoutChildParagraph>.Style(PdfStyleIdentifier k) { Style(k); return this; }

    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.CharacterSpacing(float s) { CharacterSpacing(s); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.WordSpacing(float s) { WordSpacing(s); return this; }
    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.LineSpacing(float s) { LineSpacing(s); return this; }

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

    IPdfLayoutChildParagraph IPdfParagraph<IPdfLayoutChildParagraph>.Culture(string culture) { Culture(culture); return this; }



    IPdfGridChildParagraph IPdfStylableElement<IPdfGridChildParagraph>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.FontSize(float s) { FontSize(s); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.TextColor(Color c) { TextColor(c); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.HorizontalTextAlignment(TextAlignment a) { HorizontalTextAlignment(a); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.VerticalTextAlignment(TextAlignment a) { VerticalTextAlignment(a); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.LineBreakMode(LineBreakMode m) { LineBreakMode(m); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.TextTransform(TextTransform t) { TextTransform(t); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.CharacterSpacing(float s) { CharacterSpacing(s); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.WordSpacing(float s) { WordSpacing(s); return this; }
    IPdfGridChildParagraph IPdfParagraph<IPdfGridChildParagraph>.LineSpacing(float s) { LineSpacing(s); return this; }

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
    IPdfGridChildParagraph IPdfGridChild<IPdfGridChildParagraph>.Row(int r) { Row(r); return this; }
    IPdfGridChildParagraph IPdfGridChild<IPdfGridChildParagraph>.Column(int c) { Column(c); return this; }
    IPdfGridChildParagraph IPdfGridChild<IPdfGridChildParagraph>.RowSpan(int s) { RowSpan(s); return this; }
    IPdfGridChildParagraph IPdfGridChild<IPdfGridChildParagraph>.ColumnSpan(int s) { ColumnSpan(s); return this; }

    IPdfPageChildParagraph IPdfParagraph<IPdfPageChildParagraph>.Culture(string culture) { Culture(culture); return this; }
}
