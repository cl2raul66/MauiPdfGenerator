using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfContentPageBuilder<TContent> : IPdfConfigurablePage<TContent>, IPageReadyToBuild, IPdfContentPageBuilder where TContent : class
{
    private readonly PdfDocumentBuilder _documentBuilder;
    private readonly PdfConfigurationBuilder _documentConfiguration;
    private readonly IBuildablePdfElement _contentBuilder;
    private readonly TContent _contentApi;

    private PageSizeType? _pageSizeOverride;
    private Thickness? _paddingOverride;
    private Color? _backgroundColorOverride;
    private PageOrientationType? _pageOrientationOverride;
    private PdfFontIdentifier? _pageDefaultFontFamily;
    private float _pageDefaultFontSize = Common.Models.Views.PdfParagraphData.DefaultFontSize;
    private Color _pageDefaultTextColor = Common.Models.Views.PdfParagraphData.DefaultTextColor;
    private FontAttributes _pageDefaultFontAttributes = Common.Models.Views.PdfParagraphData.DefaultFontAttributes;
    private TextDecorations _pageDefaultTextDecorations = Common.Models.Views.PdfParagraphData.DefaultTextDecorations;
    private TextTransform _pageDefaultTextTransform = Common.Models.Views.PdfParagraphData.DefaultTextTransform;

    public PdfResourceDictionary PageResources { get; } = new(); 

    public PdfContentPageBuilder(PdfDocumentBuilder documentBuilder, PdfConfigurationBuilder documentConfiguration, PdfFontRegistryBuilder fontRegistry)
    {
        _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));
        _documentConfiguration = documentConfiguration ?? throw new ArgumentNullException(nameof(documentConfiguration));

        PageResources.Parent = _documentConfiguration.ResourceDictionary;

        if (typeof(TContent) == typeof(IPdfVerticalStackLayout))
            _contentBuilder = new PdfVerticalStackLayoutBuilder(fontRegistry, PageResources);
        else if (typeof(TContent) == typeof(IPdfHorizontalStackLayout))
            _contentBuilder = new PdfHorizontalStackLayoutBuilder(fontRegistry, PageResources);
        else if (typeof(TContent) == typeof(IPdfGrid))
            _contentBuilder = new PdfGridBuilder(fontRegistry, PageResources);
        else
            throw new NotSupportedException($"The layout type '{typeof(TContent).Name}' is not supported as a root content element for a page.");

        _contentApi = (TContent)_contentBuilder;

        ((PdfLayoutElementData)_contentBuilder.GetModel()).SetVerticalOptions(LayoutAlignment.Fill);

        _pageDefaultFontFamily = _documentConfiguration.FontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                 ?? _documentConfiguration.FontRegistry.GetFirstMauiRegisteredFontIdentifier();
    }

    public IPdfConfigurablePage<TContent> PageSize(PageSizeType pageSizeType) { _pageSizeOverride = pageSizeType; return this; }
    public IPdfConfigurablePage<TContent> PageOrientation(PageOrientationType pageOrientationType) { _pageOrientationOverride = pageOrientationType; return this; }
    public IPdfConfigurablePage<TContent> Padding(float uniformPadding) { _paddingOverride = new Thickness(uniformPadding); return this; }
    public IPdfConfigurablePage<TContent> Padding(float verticalPadding, float horizontalPadding) { _paddingOverride = new Thickness(horizontalPadding, verticalPadding); return this; }
    public IPdfConfigurablePage<TContent> Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding) { _paddingOverride = new Thickness(leftPadding, topPadding, rightPadding, bottomPadding); return this; }
    public IPdfConfigurablePage<TContent> Padding(DefaultPagePaddingType defaultPaddingType) { _paddingOverride = PdfPagePaddingTypeCalculator.GetThickness(defaultPaddingType); return this; }
    public IPdfConfigurablePage<TContent> BackgroundColor(Color backgroundColor) { _backgroundColorOverride = backgroundColor; return this; }
    public IPdfConfigurablePage<TContent> DefaultTextColor(Color color) { _pageDefaultTextColor = color ?? Common.Models.Views.PdfParagraphData.DefaultTextColor; return this; }
    public IPdfConfigurablePage<TContent> DefaultTextDecorations(TextDecorations decorations) { _pageDefaultTextDecorations = decorations; return this; }
    public IPdfConfigurablePage<TContent> DefaultTextTransform(TextTransform transform) { _pageDefaultTextTransform = transform; return this; }
    public IPdfConfigurablePage<TContent> DefaultFont(Action<IPdfFontDefaultsBuilder> fontDefaults)
    {
        ArgumentNullException.ThrowIfNull(fontDefaults);
        var defaultsBuilder = new PdfFontDefaultsBuilder();
        fontDefaults(defaultsBuilder);
        if (defaultsBuilder.FamilyIdentifier.HasValue) _pageDefaultFontFamily = defaultsBuilder.FamilyIdentifier.Value;
        if (defaultsBuilder.FontSize.HasValue)
        {
            if (defaultsBuilder.FontSize.Value <= 0) throw new ArgumentOutOfRangeException(nameof(defaultsBuilder.FontSize), "Default font size must be positive.");
            _pageDefaultFontSize = defaultsBuilder.FontSize.Value;
        }
        _pageDefaultFontAttributes = defaultsBuilder.FontAttribute;
        return this;
    }

    public IPageReadyToBuild Content(Action<TContent> contentSetup)
    {
        ArgumentNullException.ThrowIfNull(contentSetup);
        contentSetup(_contentApi);
        return this;
    }

    public IPdfDocument Build() => _documentBuilder;

    #region IPdfContentPageBuilder (Internal Getters for Core)
    public PageSizeType GetEffectivePageSize() => _pageSizeOverride ?? _documentConfiguration.GetPageSize;
    public Thickness GetEffectivePadding() => _paddingOverride ?? _documentConfiguration.GetPadding;
    public PageOrientationType GetEffectivePageOrientation() => _pageOrientationOverride ?? _documentConfiguration.GetPageOrientation;
    public Color? GetEffectiveBackgroundColor() => _backgroundColorOverride;
    public PdfLayoutElementData GetContent() => (PdfLayoutElementData)_contentBuilder.GetModel();
    public PdfFontIdentifier? GetPageDefaultFontFamily() => _pageDefaultFontFamily;
    public float GetPageDefaultFontSize() => _pageDefaultFontSize;
    public Color GetPageDefaultTextColor() => _pageDefaultTextColor;
    public FontAttributes GetPageDefaultFontAttributes() => _pageDefaultFontAttributes;
    public TextDecorations GetPageDefaultTextDecorations() => _pageDefaultTextDecorations;
    public TextTransform GetPageDefaultTextTransform() => _pageDefaultTextTransform;

    public IPdfConfigurablePage<TContent> Resources(Action<IPdfResourceBuilder> resourceBuilderAction)
    {
        ArgumentNullException.ThrowIfNull(resourceBuilderAction);
        var resourceBuilder = new PdfResourceBuilder(PageResources);
        resourceBuilderAction(resourceBuilder);
        return this;
    }
    #endregion
}
