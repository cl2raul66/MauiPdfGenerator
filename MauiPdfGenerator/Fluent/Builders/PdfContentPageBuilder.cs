using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfContentPageBuilder : IPdfContentPage, IPdfContentPageBuilder, IPageReadyToBuild
{
    private readonly PdfDocumentBuilder _documentBuilder;
    private readonly PdfConfigurationBuilder _documentConfiguration;

    private PageSizeType? _pageSizeOverride;
    private Thickness? _marginsOverride;
    private Color? _backgroundColorOverride;
    private PageOrientationType? _pageOrientationOverride;

    private List<PdfElement> _pageElements = [];
    private float _pageSpacing = 5f;
    private PdfFontIdentifier? _pageDefaultFontFamily;
    private float _pageDefaultFontSize = PdfParagraph.DefaultFontSize;
    private Color _pageDefaultTextColor = PdfParagraph.DefaultTextColor;
    private FontAttributes _pageDefaultFontAttributes = PdfParagraph.DefaultFontAttributes;
    private TextDecorations _pageDefaultTextDecorations = PdfParagraph.DefaultTextDecorations;
    private TextTransform _pageDefaultTextTransform = PdfParagraph.DefaultTextTransform;


    public PdfContentPageBuilder(PdfDocumentBuilder documentBuilder, PdfConfigurationBuilder documentConfiguration)
    {
        _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));
        _documentConfiguration = documentConfiguration ?? throw new ArgumentNullException(nameof(documentConfiguration));

        _pageDefaultFontFamily = _documentConfiguration.FontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                 ?? _documentConfiguration.FontRegistry.GetFirstMauiRegisteredFontIdentifier();
    }

    public IPdfContentPage DefaultFont(Action<IFontDefaultsBuilder> fontDefaults)
    {
        ArgumentNullException.ThrowIfNull(fontDefaults);
        var defaultsBuilder = new FontDefaultsBuilder();
        fontDefaults(defaultsBuilder);

        if (defaultsBuilder.FamilyIdentifier.HasValue)
        {
            _pageDefaultFontFamily = defaultsBuilder.FamilyIdentifier.Value;
        }
        else
        {
            _pageDefaultFontFamily = _documentConfiguration.FontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                     ?? _documentConfiguration.FontRegistry.GetFirstMauiRegisteredFontIdentifier();
        }


        if (defaultsBuilder.FontSize.HasValue)
        {
            _pageDefaultFontSize = defaultsBuilder.FontSize.Value > 0
                ? defaultsBuilder.FontSize.Value
                : PdfParagraph.DefaultFontSize;
        }
        _pageDefaultFontAttributes = defaultsBuilder.FontAttribute;
        return this;
    }

    public IPdfContentPage DefaultTextDecorations(TextDecorations decorations)
    {
        _pageDefaultTextDecorations = decorations;
        return this;
    }

    public IPdfContentPage DefaultTextTransform(TextTransform transform)
    {
        _pageDefaultTextTransform = transform;
        return this;
    }

    public PdfFontIdentifier? GetPageDefaultFontFamily() => _pageDefaultFontFamily;
    public TextDecorations GetPageDefaultTextDecorations() => _pageDefaultTextDecorations;
    public TextTransform GetPageDefaultTextTransform() => _pageDefaultTextTransform;

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
    public IPdfContentPage Spacing(float value)
    {
        _pageSpacing = value >= 0 ? value : 0;
        return this;
    }
    public IPdfContentPage DefaultTextColor(Color color)
    {
        _pageDefaultTextColor = color ?? PdfParagraph.DefaultTextColor;
        return this;
    }
    public IPageReadyToBuild Content(Action<IPageContentBuilder> contentSetup)
    {
        ArgumentNullException.ThrowIfNull(contentSetup);
        var builder = new PageContentBuilder(_documentConfiguration.FontRegistry);
        contentSetup(builder);
        _pageElements = [.. builder.GetChildren()];
        return this;
    }
    public IPdfDocument Build() { return _documentBuilder; }
    public PageSizeType GetEffectivePageSize() => _pageSizeOverride ?? _documentConfiguration.GetPageSize;
    public Thickness GetEffectiveMargin() => _marginsOverride ?? _documentConfiguration.GetMargin;
    public PageOrientationType GetEffectivePageOrientation() => _pageOrientationOverride ?? _documentConfiguration.GetPageOrientation;
    public Color? GetEffectiveBackgroundColor() => _backgroundColorOverride;
    public IReadOnlyList<PdfElement> GetElements() => _pageElements.AsReadOnly();
    public float GetPageSpacing() => _pageSpacing;
    public float GetPageDefaultFontSize() => _pageDefaultFontSize;
    public Color GetPageDefaultTextColor() => _pageDefaultTextColor;
    public FontAttributes GetPageDefaultFontAttributes() => _pageDefaultFontAttributes;
}
