namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfPageChild<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
}
