using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfContentPageBuilder : IPdfContentPage, IPdfContentPageBuilder
{
    private readonly PdfDocumentBuilder _documentBuilder;
    private readonly PdfConfigurationBuilder _documentConfiguration;

    private PageSizeType? _pageSizeOverride;
    private Thickness? _marginsOverride;
    private Color? _backgroundColorOverride;
    private PageOrientationType? _pageOrientationOverride;
    private string? _defaultFontAliasOverride;
    private Action<IPdfContentPage>? _content;

    public PdfContentPageBuilder(PdfDocumentBuilder documentBuilder, PdfConfigurationBuilder documentConfiguration)
    {
        _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));
        _documentConfiguration = documentConfiguration ?? throw new ArgumentNullException(nameof(documentConfiguration));
    }

    public IPdfContentPage PageSize(PageSizeType pageSizeType)
    {
        _pageSizeOverride = pageSizeType;
        return this;
    }
    public IPdfContentPage PageOrientation(PageOrientationType pageOrientationType)
    {
        _pageOrientationOverride = pageOrientationType;
        return this;
    }
    public IPdfContentPage Margins(float uniformMargin)
    {
        _marginsOverride = new Thickness(uniformMargin);
        return this;
    }
    public IPdfContentPage Margins(float verticalMargin, float horizontalMargin)
    {
        _marginsOverride = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }
    public IPdfContentPage Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin)
    {
        _marginsOverride = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public IPdfContentPage Margins(DefaultMarginType defaultMarginType)
    {
        _marginsOverride = MarginCalculator.GetThickness(defaultMarginType);
        return this;
    }

    public IPdfContentPage BackgroundColor(Color backgroundColor)
    {
        _backgroundColorOverride = backgroundColor;
        return this;
    }
    public IPdfContentPage DefaultFont(string fontAlias)
    {
        ArgumentException.ThrowIfNullOrEmpty(fontAlias); 
        _defaultFontAliasOverride = fontAlias;
        return this;
    }
    public IPdfContentPage Content(Action<IPdfContentPage> pageContent)
    {
        ArgumentNullException.ThrowIfNull(pageContent);
        _content = pageContent;
        return this;
    }
    public IPdfDocument Build() { return _documentBuilder; }


    public PageSizeType GetEffectivePageSize() => _pageSizeOverride ?? _documentConfiguration.GetPageSize;
    public Thickness GetEffectiveMargin() => _marginsOverride ?? _documentConfiguration.GetMargin;
    public PageOrientationType GetEffectivePageOrientation() => _pageOrientationOverride ?? _documentConfiguration.GetPageOrientation;
    public Color? GetEffectiveBackgroundColor() => _backgroundColorOverride;
    public string? GetEffectiveDefaultFontAlias() => _defaultFontAliasOverride ?? _documentConfiguration.FontRegistry.GetDefaultFontAlias();
    public Action<IPdfContentPage>? GetContentPage() => _content;
}
