using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Views;

internal class PdfSpanBuilder : IPdfSpan, IPdfTextStyles
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

    public IPdfSpan Text(string text)
    {
        return this;
    }

    #region IPdfElement<IPdfSpan> Implementation

    public IPdfSpan Margin(double uniformMargin)
    {
        return this;
    }

    public IPdfSpan Margin(double horizontalMargin, double verticalMargin)
    {
        return this;
    }

    public IPdfSpan Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        return this;
    }

    public IPdfSpan Padding(double uniformPadding)
    {
        return this;
    }

    public IPdfSpan Padding(double horizontalPadding, double verticalPadding)
    {
        return this;
    }

    public IPdfSpan Padding(double leftPadding, double topPadding, double rightPadding, double bottomPadding)
    {
        return this;
    }

    public IPdfSpan WidthRequest(double width)
    {
        return this;
    }

    public IPdfSpan HeightRequest(double height)
    {
        return this;
    }

    public IPdfSpan BackgroundColor(Color? color)
    {
        return this;
    }

    public IPdfSpan Style(PdfStyleIdentifier key)
    {
        ((IPdfTextStyles)this).ApplyStyle(key);
        return this;
    }

    #endregion

    #region IPdfTextStyles Explicit Implementation

    void IPdfTextStyles.ApplyFontFamily(PdfFontIdentifier? family)
    {
        if (family.HasValue)
        {
            _model.FontFamilyProp.Set(family, PdfPropertyPriority.Local);
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);
        }
    }

    void IPdfTextStyles.ApplyFontSize(float size)
    {
        _model.FontSizeProp.Set(size, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyTextColor(Color color)
    {
        _model.TextColorProp.Set(color, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyFontAttributes(FontAttributes attributes)
    {
        _model.FontAttributesProp.Set(attributes, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyTextDecorations(TextDecorations decorations)
    {
        _model.TextDecorationsProp.Set(decorations, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyTextTransform(TextTransform transform)
    {
        _model.TextTransformProp.Set(transform, PdfPropertyPriority.Local);
    }

    void IPdfTextStyles.ApplyStyle(PdfStyleIdentifier key)
    {
        if (_resourceDictionary is null)
        {
            return;
        }

        var setter = _resourceDictionary.GetCombinedSetter(key);
        if (setter is null)
        {
            return;
        }

        var tempSpan = new PdfSpanData();
        setter(tempSpan);

        MergeProperties(_model, tempSpan);
    }

    #endregion

    #region IPdfSpanStyles Implementation (Fluent API)

    public IPdfSpan FontFamily(PdfFontIdentifier? family)
    {
        ((IPdfTextStyles)this).ApplyFontFamily(family);
        return this;
    }

    public IPdfSpan FontSize(float size)
    {
        ((IPdfTextStyles)this).ApplyFontSize(size);
        return this;
    }

    public IPdfSpan TextColor(Color color)
    {
        ((IPdfTextStyles)this).ApplyTextColor(color);
        return this;
    }

    public IPdfSpan FontAttributes(FontAttributes attributes)
    {
        ((IPdfTextStyles)this).ApplyFontAttributes(attributes);
        return this;
    }

    public IPdfSpan TextDecorations(TextDecorations decorations)
    {
        ((IPdfTextStyles)this).ApplyTextDecorations(decorations);
        return this;
    }

    public IPdfSpan TextTransform(TextTransform transform)
    {
        ((IPdfTextStyles)this).ApplyTextTransform(transform);
        return this;
    }

    public IPdfSpan Style(string key)
    {
        return Style(new PdfStyleIdentifier(key));
    }

    #endregion

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
