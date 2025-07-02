using MauiPdfGenerator.Core.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PageRendererFactory
{
    public IPageRenderer GetRenderer(PdfPageData pageData)
    { 
        // Por ahora, solo tenemos un tipo de página.
        // En el futuro, aquí podríamos tener un switch basado en un
        // nuevo 'pageData.PageType' enum.
        // switch (pageData.PageType) {
        //     case PageType.Calendar: return new CalendarPageRenderer();
        //     default: return new PdfContentPageRenderer();
        // }
        return new PdfContentPageRenderer();
    }
}
