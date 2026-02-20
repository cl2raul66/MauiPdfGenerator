using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Pages;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Utils;

namespace MauiPdfGenerator.Fluent.Builders.Pages;

internal class PdfReportPageBuilder : PdfPageBuilder<IPdfReportPage>, IPdfReportPage, IPageReadyToBuild
{
    private readonly PdfVerticalStackLayoutBuilder _headerBuilder;
    private readonly PdfVerticalStackLayoutBuilder _contentBuilder;
    private readonly PdfVerticalStackLayoutBuilder _footerBuilder;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    private PdfPageOccurrence _headerOccurrence = PdfPageOccurrence.AllPages;
    private PdfPageOccurrence _footerOccurrence = PdfPageOccurrence.AllPages;

    public PdfReportPageBuilder(
        PdfDocumentBuilder documentBuilder,
        PdfConfigurationBuilder documentConfiguration,
        PdfFontRegistryBuilder fontRegistry,
        IDiagnosticSink diagnosticSink)
        : base(documentBuilder, documentConfiguration, diagnosticSink)
    {
        _fontRegistry = fontRegistry;

        _headerBuilder = new PdfVerticalStackLayoutBuilder(fontRegistry, PageResources);
        _contentBuilder = new PdfVerticalStackLayoutBuilder(fontRegistry, PageResources);
        _footerBuilder = new PdfVerticalStackLayoutBuilder(fontRegistry, PageResources);
    }

    public IPageReadyToBuild Header(Action<IPdfStackLayoutBuilder> headerContent)
    {
        ArgumentNullException.ThrowIfNull(headerContent);
        var wrapper = new PdfStackLayoutContentBuilder(_headerBuilder, _fontRegistry, PageResources);
        headerContent(wrapper);
        return this;
    }

    public IPageReadyToBuild Content(Action<IPdfStackLayoutBuilder> contentSetup)
    {
        ArgumentNullException.ThrowIfNull(contentSetup);
        var wrapper = new PdfStackLayoutContentBuilder(_contentBuilder, _fontRegistry, PageResources);
        contentSetup(wrapper);
        return this;
    }

    public IPageReadyToBuild Footer(Action<IPdfStackLayoutBuilder> footerContent)
    {
        ArgumentNullException.ThrowIfNull(footerContent);
        var wrapper = new PdfStackLayoutContentBuilder(_footerBuilder, _fontRegistry, PageResources);
        footerContent(wrapper);
        return this;
    }

    public IPdfDocument Build() => _documentBuilder;

    public override PdfPageData BuildPageData()
    {
        var styleResolver = new StyleResolver(
            _documentConfiguration.ResourceDictionary,
            _diagnosticSink,
            _documentConfiguration.FontRegistry);

        var headerModel = (PdfLayoutElementData)_headerBuilder.GetModel();
        var contentModel = (PdfLayoutElementData)_contentBuilder.GetModel();
        var footerModel = (PdfLayoutElementData)_footerBuilder.GetModel();

        var allElements = new List<PdfElementData>();
        Traverse(headerModel, allElements);
        Traverse(contentModel, allElements);
        Traverse(footerModel, allElements);

        styleResolver.ApplyStyles(allElements, PageResources);

        return new PdfReportPageData(
            GetEffectivePageSize(),
            GetEffectivePageOrientation(),
            GetEffectivePadding(),
            GetEffectiveBackgroundColor(),
            headerModel,
            _headerOccurrence,
            footerModel,
            _footerOccurrence,
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
}
