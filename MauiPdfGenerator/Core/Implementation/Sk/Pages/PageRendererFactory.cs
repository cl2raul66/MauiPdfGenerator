using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Pages;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PageRendererFactory
{
    public IPageRenderer GetRenderer(PdfPageData pageData)
    {
        return pageData switch
        {
            PdfReportPageData => new PdfReportPageRenderer(),
            PdfContentPageData => new PdfContentPageRenderer(),

            _ => throw new NotSupportedException($"Page type '{pageData.GetType().Name}' is not supported by the PageRendererFactory.")
        };
    }
}
