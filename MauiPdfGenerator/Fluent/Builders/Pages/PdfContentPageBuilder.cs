using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Pages;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Utils;

namespace MauiPdfGenerator.Fluent.Builders.Pages;

internal class PdfContentPageBuilder<TContent> : PdfPageBuilder<TContent>, IPdfContentPage<TContent>, IPageReadyToBuild, IPdfContentPageBuilder where TContent : class
{
    private readonly IBuildablePdfElement _contentBuilder;
    private readonly TContent _contentApi;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfContentPageBuilder(
        PdfDocumentBuilder documentBuilder,
        PdfConfigurationBuilder documentConfiguration,
        PdfFontRegistryBuilder fontRegistry,
        IDiagnosticSink diagnosticSink)
        : base(documentBuilder, documentConfiguration, diagnosticSink)
    {
        _fontRegistry = fontRegistry;

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
    }

    public IPageReadyToBuild Content(Action<TContent> contentSetup)
    {
        ArgumentNullException.ThrowIfNull(contentSetup);
        contentSetup(_contentApi);
        return this;
    }

    public IPdfDocument Build() => _documentBuilder;

    public override PdfPageData BuildPageData()
    {
        var styleResolver = new StyleResolver(
            _documentConfiguration.ResourceDictionary,
            _diagnosticSink,
            _documentConfiguration.FontRegistry);

        var contentModel = GetContent();
        var allElements = new List<PdfElementData>();
        Traverse(contentModel, allElements);

        styleResolver.ApplyStyles(allElements, PageResources);

        return new PdfContentPageData(
            GetEffectivePageSize(),
            GetEffectivePageOrientation(),
            GetEffectivePadding(),
            GetEffectiveBackgroundColor(),
            contentModel,
            _pageDefaultFontFamily,
            _pageDefaultFontSize,
            _pageDefaultTextColor,
            _pageDefaultFontAttributes,
            _pageDefaultTextDecorations,
            _pageDefaultTextTransform,
            GetEffectiveCulture()
        );
    }

    // Métodos auxiliares para compatibilidad interna si es necesario
    public PdfLayoutElementData GetContent() => (PdfLayoutElementData)_contentBuilder.GetModel();
    public PdfFontIdentifier? GetPageDefaultFontFamily() => _pageDefaultFontFamily;
    public float GetPageDefaultFontSize() => _pageDefaultFontSize;
    public Color GetPageDefaultTextColor() => _pageDefaultTextColor;
    public FontAttributes GetPageDefaultFontAttributes() => _pageDefaultFontAttributes;
    public TextDecorations GetPageDefaultTextDecorations() => _pageDefaultTextDecorations;
    public TextTransform GetPageDefaultTextTransform() => _pageDefaultTextTransform;

    private void Traverse(PdfElementData element, List<PdfElementData> list)
    {
        list.Add(element);
        if (element is PdfLayoutElementData layout)
        {
            foreach (var child in layout.GetChildren)
            {
                Traverse(child, list);
            }
        }
    }

    public new PageSizeType GetEffectivePageSize() => base.GetEffectivePageSize();
    public new Thickness GetEffectivePadding() => base.GetEffectivePadding();
    public new PageOrientationType GetEffectivePageOrientation() => base.GetEffectivePageOrientation();
    public new Color? GetEffectiveBackgroundColor() => base.GetEffectiveBackgroundColor();
    public new string GetEffectiveCulture() => base.GetEffectiveCulture();
}
