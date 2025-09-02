using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfContentPageBuilder : IPdfContentPage, IPdfContentPageBuilder, IPageReadyToBuild
{
    private readonly PdfDocumentBuilder _documentBuilder;
    private readonly PdfConfigurationBuilder _documentConfiguration;

    private PageSizeType? _pageSizeOverride;
    private Thickness? _paddingOverride;
    private Color? _backgroundColorOverride;
    private PageOrientationType? _pageOrientationOverride;

    private List<PdfElementData> _pageElements = [];
    private PdfFontIdentifier? _pageDefaultFontFamily;
    private float _pageDefaultFontSize = Common.Models.Elements.PdfParagraphData.DefaultFontSize;
    private Color _pageDefaultTextColor = Common.Models.Elements.PdfParagraphData.DefaultTextColor;
    private FontAttributes _pageDefaultFontAttributes = Common.Models.Elements.PdfParagraphData.DefaultFontAttributes;
    private TextDecorations _pageDefaultTextDecorations = Common.Models.Elements.PdfParagraphData.DefaultTextDecorations;
    private TextTransform _pageDefaultTextTransform = Common.Models.Elements.PdfParagraphData.DefaultTextTransform;


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
            if (defaultsBuilder.FontSize.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultsBuilder.FontSize), "Default font size must be positive.");
            _pageDefaultFontSize = defaultsBuilder.FontSize.Value;
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
    public IPdfContentPage Padding(float uniformPadding)
    {
        _paddingOverride = new Thickness(uniformPadding);
        return this;
    }
    public IPdfContentPage Padding(float verticalPadding, float horizontalPadding)
    {
        _paddingOverride = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }
    public IPdfContentPage Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding)
    {
        _paddingOverride = new Thickness(leftPadding, topPadding, rightPadding, bottomPadding);
        return this;
    }
    public IPdfContentPage Padding(DefaultPagePaddingType defaultPaddingType)
    {
        _paddingOverride = PagePaddingTypeCalculator.GetThickness(defaultPaddingType);
        return this;
    }
    public IPdfContentPage BackgroundColor(Color backgroundColor)
    {
        _backgroundColorOverride = backgroundColor;
        return this;
    }
    public IPdfContentPage DefaultTextColor(Color color)
    {
        _pageDefaultTextColor = color ?? Common.Models.Elements.PdfParagraphData.DefaultTextColor;
        return this;
    }
    public IPageReadyToBuild Content(Action<IPageContentBuilder> contentSetup)
    {
        ArgumentNullException.ThrowIfNull(contentSetup);
        var builder = new PageContentBuilder(_documentConfiguration.FontRegistry);
        contentSetup(builder);
        var buildableChildren = builder.GetBuildableChildren();

        if (buildableChildren.Count == 1 && buildableChildren[0].GetModel() is PdfLayoutElementData)
        {
            _pageElements.Add(buildableChildren[0].GetModel());
        }
        else
        {
            var implicitRoot = new PdfVerticalStackLayoutBuilder();
            foreach (var child in buildableChildren)
            {
                implicitRoot.Add(child);
            }
            _pageElements.Add(implicitRoot.GetModel());
        }

        return this;
    }
    public IPdfDocument Build() { return _documentBuilder; }
    public PageSizeType GetEffectivePageSize() => _pageSizeOverride ?? _documentConfiguration.GetPageSize;
    public Thickness GetEffectivePadding() => _paddingOverride ?? _documentConfiguration.GetPadding;
    public PageOrientationType GetEffectivePageOrientation() => _pageOrientationOverride ?? _documentConfiguration.GetPageOrientation;
    public Color? GetEffectiveBackgroundColor() => _backgroundColorOverride;
    public IReadOnlyList<PdfElementData> GetElements() => _pageElements.AsReadOnly();
    public float GetPageDefaultFontSize() => _pageDefaultFontSize;
    public Color GetPageDefaultTextColor() => _pageDefaultTextColor;
    public FontAttributes GetPageDefaultFontAttributes() => _pageDefaultFontAttributes;

    IPdfContentPage IPdfPage<IPdfContentPage>.PageSize(PageSizeType pageSizeType) => PageSize(pageSizeType);
    IPdfContentPage IPdfPage<IPdfContentPage>.PageOrientation(PageOrientationType pageOrientationType) => PageOrientation(pageOrientationType);
    IPdfContentPage IPdfPage<IPdfContentPage>.Padding(float uniformPadding) => Padding(uniformPadding);
    IPdfContentPage IPdfPage<IPdfContentPage>.Padding(float verticalPadding, float horizontalPadding) => Padding(verticalPadding, horizontalPadding);
    IPdfContentPage IPdfPage<IPdfContentPage>.Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding) => Padding(leftPadding, topPadding, rightPadding, bottomPadding);
    IPdfContentPage IPdfPage<IPdfContentPage>.Padding(DefaultPagePaddingType defaultPaddingType) => Padding(defaultPaddingType);
    IPdfContentPage IPdfPage<IPdfContentPage>.BackgroundColor(Color backgroundColor) => BackgroundColor(backgroundColor);
    IPdfDocument IPdfPage<IPdfContentPage>.Build() => Build();
}
