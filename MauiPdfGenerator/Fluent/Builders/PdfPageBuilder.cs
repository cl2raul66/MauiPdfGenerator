using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal abstract class PdfPageBuilder<TPageInterface> : IPdfConfigurablePage<TPageInterface>, IPdfPageBuilder
    where TPageInterface : class
{
    protected readonly PdfDocumentBuilder _documentBuilder;
    protected readonly PdfConfigurationBuilder _documentConfiguration;
    protected readonly IDiagnosticSink _diagnosticSink;

    protected PageSizeType? _pageSizeOverride;
    protected Thickness? _paddingOverride;
    protected Color? _backgroundColorOverride;
    protected PageOrientationType? _pageOrientationOverride;
    protected string? _cultureOverride;

    protected PdfFontIdentifier? _pageDefaultFontFamily;
    protected float _pageDefaultFontSize = Common.Models.Views.PdfParagraphData.DefaultFontSize;
    protected Color _pageDefaultTextColor = Common.Models.Views.PdfParagraphData.DefaultTextColor;
    protected FontAttributes _pageDefaultFontAttributes = Common.Models.Views.PdfParagraphData.DefaultFontAttributes;
    protected TextDecorations _pageDefaultTextDecorations = Common.Models.Views.PdfParagraphData.DefaultTextDecorations;
    protected TextTransform _pageDefaultTextTransform = Common.Models.Views.PdfParagraphData.DefaultTextTransform;

    public PdfResourceDictionary PageResources { get; } = new();

    protected PdfPageBuilder(
        PdfDocumentBuilder documentBuilder,
        PdfConfigurationBuilder documentConfiguration,
        IDiagnosticSink diagnosticSink)
    {
        _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));
        _documentConfiguration = documentConfiguration ?? throw new ArgumentNullException(nameof(documentConfiguration));
        _diagnosticSink = diagnosticSink ?? throw new ArgumentNullException(nameof(diagnosticSink));

        PageResources.Parent = _documentConfiguration.ResourceDictionary;

        _pageDefaultFontFamily = _documentConfiguration.FontRegistry.GetUserConfiguredDefaultFontIdentifier()
                                 ?? _documentConfiguration.FontRegistry.GetFirstMauiRegisteredFontIdentifier();
    }

    public abstract PdfPageData BuildPageData();

    public IPdfConfigurablePage<TPageInterface> PageSize(PageSizeType pageSizeType)
    {
        _pageSizeOverride = pageSizeType;
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> PageOrientation(PageOrientationType pageOrientationType)
    {
        _pageOrientationOverride = pageOrientationType;
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> Padding(float uniformPadding)
    {
        _paddingOverride = new Thickness(uniformPadding);
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> Padding(float verticalPadding, float horizontalPadding)
    {
        _paddingOverride = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding)
    {
        _paddingOverride = new Thickness(leftPadding, topPadding, rightPadding, bottomPadding);
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> Padding(DefaultPagePaddingType defaultPaddingType)
    {
        _paddingOverride = PdfPagePaddingTypeCalculator.GetThickness(defaultPaddingType);
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> BackgroundColor(Color backgroundColor)
    {
        _backgroundColorOverride = backgroundColor;
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> DefaultTextColor(Color color)
    {
        _pageDefaultTextColor = color ?? Common.Models.Views.PdfParagraphData.DefaultTextColor;
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> DefaultTextDecorations(TextDecorations decorations)
    {
        _pageDefaultTextDecorations = decorations;
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> DefaultTextTransform(TextTransform transform)
    {
        _pageDefaultTextTransform = transform;
        return this;
    }

    public IPdfConfigurablePage<TPageInterface> Culture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            _cultureOverride = "en-US";
            return this;
        }

        try
        {
            var cultureInfo = System.Globalization.CultureInfo.GetCultureInfo(cultureName);
            _cultureOverride = cultureName;
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            _cultureOverride = null;
            _diagnosticSink.Submit(new MauiPdfGenerator.Diagnostics.Models.DiagnosticMessage(
                MauiPdfGenerator.Diagnostics.Enums.DiagnosticSeverity.Warning,
                MauiPdfGenerator.Diagnostics.DiagnosticCodes.InvalidCulture,
                $"Page culture '{cultureName}' not found. Using document culture or 'en-US'."
            ));
        }

        return this;
    }

    public IPdfConfigurablePage<TPageInterface> DefaultFont(Action<IPdfFontDefaultsBuilder> fontDefaults)
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

    public IPdfConfigurablePage<TPageInterface> Resources(Action<IPdfResourceBuilder> resourceBuilderAction)
    {
        ArgumentNullException.ThrowIfNull(resourceBuilderAction);
        var resourceBuilder = new PdfResourceBuilder(PageResources);
        resourceBuilderAction(resourceBuilder);
        return this;
    }

    protected PageSizeType GetEffectivePageSize() => _pageSizeOverride ?? _documentConfiguration.GetPageSize;
    protected Thickness GetEffectivePadding() => _paddingOverride ?? _documentConfiguration.GetPadding;
    protected PageOrientationType GetEffectivePageOrientation() => _pageOrientationOverride ?? _documentConfiguration.GetPageOrientation;
    protected Color? GetEffectiveBackgroundColor() => _backgroundColorOverride;
    protected string GetEffectiveCulture() => _cultureOverride ?? _documentConfiguration.Culture;
}
