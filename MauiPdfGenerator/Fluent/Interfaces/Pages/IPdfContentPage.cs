namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage<TContent> : IPdfConfigurablePage<TContent> where TContent : class
{
    IPageReadyToBuild Content(Action<TContent> contentSetup);
}
