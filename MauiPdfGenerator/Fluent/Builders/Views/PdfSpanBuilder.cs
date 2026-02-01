using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Views;

internal class PdfSpanBuilder :
    IPdfBuildableSpan,  
    IPdfSpan, 
    IPdfTextStyles 
{
    private readonly PdfSpanData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly PdfResourceDictionary? _resourceDictionary;

    public PdfSpanBuilder(int textLength, PdfFontRegistryBuilder fontRegistry, PdfResourceDictionary? resourceDictionary = null)
    {
        _model = new PdfSpanData { TextLength = textLength };
        _fontRegistry = fontRegistry;
        _resourceDictionary = resourceDictionary;
    }

    internal PdfSpanData GetModel() => _model;

    public IPdfBuildableSpan Style(PdfStyleIdentifier key)
    {
        ((IPdfTextStyles)this).ApplyStyle(key);
        return this;
    }

    public IPdfBuildableSpan Text(string text) => this;



    public IPdfBuildableSpan FontFamily(PdfFontIdentifier? family) { ((IPdfTextStyles)this).ApplyFontFamily(family); return this; }
    public IPdfBuildableSpan FontSize(float size) { ((IPdfTextStyles)this).ApplyFontSize(size); return this; }
    public IPdfBuildableSpan TextColor(Color color) { ((IPdfTextStyles)this).ApplyTextColor(color); return this; }
    public IPdfBuildableSpan FontAttributes(FontAttributes attributes) { ((IPdfTextStyles)this).ApplyFontAttributes(attributes); return this; }
    public IPdfBuildableSpan TextDecorations(TextDecorations decorations) { ((IPdfTextStyles)this).ApplyTextDecorations(decorations); return this; }
    public IPdfBuildableSpan TextTransform(TextTransform transform) { ((IPdfTextStyles)this).ApplyTextTransform(transform); return this; }



    public IPdfBuildableSpan Margin(double u) => this;
    public IPdfBuildableSpan Margin(double h, double v) => this;
    public IPdfBuildableSpan Margin(double l, double t, double r, double b) => this;
    public IPdfBuildableSpan Padding(double u) => this;
    public IPdfBuildableSpan Padding(double h, double v) => this;
    public IPdfBuildableSpan Padding(double l, double t, double r, double b) => this;
    public IPdfBuildableSpan WidthRequest(double w) => this;
    public IPdfBuildableSpan HeightRequest(double h) => this;
    public IPdfBuildableSpan BackgroundColor(Color? c) => this;

    public IPdfBuildableSpan Style(string key) => Style(new PdfStyleIdentifier(key));



    IPdfBuildableSpan IPdfStylableElement<IPdfBuildableSpan>.Style(PdfStyleIdentifier k) => Style(k);

    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.Margin(double u) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.Margin(double h, double v) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.Margin(double l, double t, double r, double b) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.Padding(double u) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.Padding(double h, double v) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.Padding(double l, double t, double r, double b) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.WidthRequest(double w) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.HeightRequest(double h) => this;
    IPdfBuildableSpan IPdfElement<IPdfBuildableSpan>.BackgroundColor(Color? c) => this;


    
    IPdfSpan IPdfSpan.Text(string text) => this;

    IPdfSpan IPdfSpanStyles.FontFamily(PdfFontIdentifier? f) { FontFamily(f); return this; }
    IPdfSpan IPdfSpanStyles.FontSize(float s) { FontSize(s); return this; }
    IPdfSpan IPdfSpanStyles.TextColor(Color c) { TextColor(c); return this; }
    IPdfSpan IPdfSpanStyles.FontAttributes(FontAttributes a) { FontAttributes(a); return this; }
    IPdfSpan IPdfSpanStyles.TextDecorations(TextDecorations d) { TextDecorations(d); return this; }
    IPdfSpan IPdfSpanStyles.TextTransform(TextTransform t) { TextTransform(t); return this; }

    IPdfSpan IPdfElement<IPdfSpan>.Margin(double u) => this;
    IPdfSpan IPdfElement<IPdfSpan>.Margin(double h, double v) => this;
    IPdfSpan IPdfElement<IPdfSpan>.Margin(double l, double t, double r, double b) => this;
    IPdfSpan IPdfElement<IPdfSpan>.Padding(double u) => this;
    IPdfSpan IPdfElement<IPdfSpan>.Padding(double h, double v) => this;
    IPdfSpan IPdfElement<IPdfSpan>.Padding(double l, double t, double r, double b) => this;
    IPdfSpan IPdfElement<IPdfSpan>.WidthRequest(double w) => this;
    IPdfSpan IPdfElement<IPdfSpan>.HeightRequest(double h) => this;
    IPdfSpan IPdfElement<IPdfSpan>.BackgroundColor(Color? c) => this;

        

    void IPdfTextStyles.ApplyFontFamily(PdfFontIdentifier? family)
    {
        if (family.HasValue)
        {
            _model.FontFamilyProp.Set(family, PdfPropertyPriority.Local);
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);
        }
    }

    void IPdfTextStyles.ApplyFontSize(float size) => _model.FontSizeProp.Set(size, PdfPropertyPriority.Local);
    void IPdfTextStyles.ApplyTextColor(Color color) => _model.TextColorProp.Set(color, PdfPropertyPriority.Local);
    void IPdfTextStyles.ApplyFontAttributes(FontAttributes attributes) => _model.FontAttributesProp.Set(attributes, PdfPropertyPriority.Local);
    void IPdfTextStyles.ApplyTextDecorations(TextDecorations decorations) => _model.TextDecorationsProp.Set(decorations, PdfPropertyPriority.Local);
    void IPdfTextStyles.ApplyTextTransform(TextTransform transform) => _model.TextTransformProp.Set(transform, PdfPropertyPriority.Local);

    void IPdfTextStyles.ApplyStyle(PdfStyleIdentifier key)
    {
        if (_resourceDictionary is null) return;

        var setter = _resourceDictionary.GetCombinedSetter(key);
        if (setter is null) return;

        var tempBuilder = new PdfSpanBuilder(0, _fontRegistry, _resourceDictionary);

        setter(tempBuilder);

        var tempModel = tempBuilder.GetModel();

        MergeProperties(_model, tempModel);
    }

    private void MergeProperties(PdfSpanData target, PdfSpanData source)
    {
        if (source.FontFamilyProp.Priority > PdfPropertyPriority.Default)
        {
            target.FontFamilyProp.Set(source.FontFamilyProp.Value, PdfPropertyPriority.Local);
            target.ResolvedFontRegistration = source.ResolvedFontRegistration;
        }
        if (source.FontSizeProp.Priority > PdfPropertyPriority.Default)
        {
            target.FontSizeProp.Set(source.FontSizeProp.Value, PdfPropertyPriority.Local);
        }
        if (source.TextColorProp.Priority > PdfPropertyPriority.Default)
        {
            target.TextColorProp.Set(source.TextColorProp.Value, PdfPropertyPriority.Local);
        }
        if (source.FontAttributesProp.Priority > PdfPropertyPriority.Default)
        {
            target.FontAttributesProp.Set(source.FontAttributesProp.Value, PdfPropertyPriority.Local);
        }
        if (source.TextDecorationsProp.Priority > PdfPropertyPriority.Default)
        {
            target.TextDecorationsProp.Set(source.TextDecorationsProp.Value, PdfPropertyPriority.Local);
        }
        if (source.TextTransformProp.Priority > PdfPropertyPriority.Default)
        {
            target.TextTransformProp.Set(source.TextTransformProp.Value, PdfPropertyPriority.Local);
        }
    }
}
