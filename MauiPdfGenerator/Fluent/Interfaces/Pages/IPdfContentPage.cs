using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage : IPdfPage<IPdfContentPage>
{
    IPdfContentPage Content(Action<IPdfContentPage> pageContent);
}
