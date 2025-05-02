using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfContentPageBuilder : IPdfContentPage, IPdfPageBuilder
{
    private readonly PdfDocumentBuilder _documentBuilder;
    private readonly PdfConfigurationBuilder _documentConfiguration;
    // Fields to store page-specific overrides (e.g., _pageSizeOverride, _marginsOverride)
    private PageSizeType? _pageSizeOverride;
    private Thickness? _marginsOverride;
    private Color? _backgroundColorOverride;
    private PageOrientationType? _pageOrientationOverride;

    public PdfContentPageBuilder(PdfDocumentBuilder documentBuilder, PdfConfigurationBuilder documentConfiguration)
    {
        _documentBuilder = documentBuilder;
        _documentConfiguration = documentConfiguration;
    }

    public IPdfContentPage Content(Action<IPdfContentPage> pageContent)
    {
        ArgumentNullException.ThrowIfNull(pageContent);

        pageContent(this);
        return this;
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

    public IPdfContentPage Margins(float uniformMargin) {  return this; }
    public IPdfContentPage Margins(float verticalMargin, float horizontalMargin) {  return this; }
    public IPdfContentPage Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin) { return this; }
    public IPdfContentPage Margins(DefaultMarginType defaultMarginType) {  return this; }
    public IPdfContentPage BackgroundColor(Color backgroundColor) { return this; }

    public IPdfContentPage DefaultFont(string fontAlias)
    {
        return this;
    }

    public IPdfDocument Build()
    {
        // Simply return the parent document builder instance to continue the chain
        return _documentBuilder;
    }
}
