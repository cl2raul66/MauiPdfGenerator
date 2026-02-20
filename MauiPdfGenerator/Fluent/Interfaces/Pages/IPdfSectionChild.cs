namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfSectionChild<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
}
