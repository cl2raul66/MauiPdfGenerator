using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Pages;
using MauiPdfGenerator.Core.Implementation.Sk.Utils;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Enums;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PdfReportPageRenderer : IPageRenderer
{
    public async Task<List<IReadOnlyList<PdfLayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        if (context.PageData is not PdfReportPageData reportData)
            throw new InvalidOperationException($"Invalid page data type. Expected {nameof(PdfReportPageData)}, but got {context.PageData.GetType().Name}.");

        context.Logger.LogDebug("Starting layout for ReportPage.");

        var pageBlocks = new List<IReadOnlyList<PdfLayoutInfo>>();
        var pageSize = SkiaUtils.GetSkPageSize(reportData.Size, reportData.Orientation);

        var pageContentRect = new PdfRect(
            (float)reportData.Padding.Left,
            (float)reportData.Padding.Top,
            pageSize.Width - (float)reportData.Padding.HorizontalThickness,
            pageSize.Height - (float)reportData.Padding.VerticalThickness
        );

        PdfLayoutElementData? contentToProcess = reportData.Content;
        int pageIndex = 0;

        while (contentToProcess is not null)
        {
            pageIndex++;
            bool isFirstPage = pageIndex == 1;

            float headerHeight = 0;
            PdfLayoutInfo? headerLayout = null;

            if (ShouldRender(reportData.HeaderOccurrence, isFirstPage) && reportData.Header is not null)
            {
                var renderer = context.RendererFactory.GetRenderer(reportData.Header);
                var headerContext = context with { Element = reportData.Header };

                var measure = await renderer.MeasureAsync(headerContext, new SKSize(pageContentRect.Width, float.PositiveInfinity));

                var headerRect = new PdfRect(pageContentRect.X, pageContentRect.Y, pageContentRect.Width, measure.Height);
                headerLayout = await renderer.ArrangeAsync(headerRect, headerContext);
                headerHeight = headerLayout.Value.Height;
            }

            float footerHeight = 0;
            PdfLayoutInfo? footerLayout = null;

            if (ShouldRender(reportData.FooterOccurrence, isFirstPage) && reportData.Footer is not null)
            {
                var renderer = context.RendererFactory.GetRenderer(reportData.Footer);
                var footerContext = context with { Element = reportData.Footer };

                var measure = await renderer.MeasureAsync(footerContext, new SKSize(pageContentRect.Width, float.PositiveInfinity));

                footerHeight = measure.Height;

                float footerY = pageContentRect.Bottom - footerHeight;
                var footerRect = new PdfRect(pageContentRect.X, footerY, pageContentRect.Width, footerHeight);
                footerLayout = await renderer.ArrangeAsync(footerRect, footerContext);
            }

            float availableHeight = pageContentRect.Height - headerHeight - footerHeight;

            if (availableHeight < 0)
            {
                context.Logger.LogWarning("Header and Footer consume all available space on page {PageIndex}.", pageIndex);
                availableHeight = 0;
            }

            var contentRect = new PdfRect(
                pageContentRect.X,
                pageContentRect.Y + headerHeight,
                pageContentRect.Width,
                availableHeight
            );

            var contentRenderer = context.RendererFactory.GetRenderer(contentToProcess);
            var contentContext = context with { Element = contentToProcess };

            await contentRenderer.MeasureAsync(contentContext, new SKSize(contentRect.Width, contentRect.Height));
            var contentArrange = await contentRenderer.ArrangeAsync(contentRect, contentContext);

            // 4. Ensamblar bloque de página con los elementos visibles
            var elements = new List<PdfLayoutInfo>();
            if (headerLayout.HasValue) elements.Add(headerLayout.Value);
            if (footerLayout.HasValue) elements.Add(footerLayout.Value);

            if (contentArrange.Height > 0 || contentArrange.RemainingElement is not null)
            {
                elements.Add(contentArrange);
            }

            pageBlocks.Add(elements);

            contentToProcess = contentArrange.RemainingElement as PdfLayoutElementData;
        }

        return pageBlocks;
    }

    public async Task RenderPageBlockAsync(SKCanvas canvas, IReadOnlyList<PdfLayoutInfo> pageBlock, PdfGenerationContext context)
    {
        foreach (var item in pageBlock)
        {
            var element = (PdfElementData)item.Element;
            var renderer = context.RendererFactory.GetRenderer(element);
            var itemContext = context with { Element = element };
            await renderer.RenderAsync(canvas, itemContext);
        }
    }

    private bool ShouldRender(PdfPageOccurrence occurrence, bool isFirstPage)
    {
        return occurrence switch
        {
            PdfPageOccurrence.AllPages => true,
            PdfPageOccurrence.FirstPageOnly => isFirstPage,
            PdfPageOccurrence.ExcludeFirstPage => !isFirstPage,
            _ => true
        };
    }
}
