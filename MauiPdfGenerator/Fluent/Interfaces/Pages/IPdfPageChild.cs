namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfPageChild<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    // Esta es la interfaz base para elementos a nivel de página.
    // Intencionalmente no tiene HorizontalOptions ni VerticalOptions.
}
