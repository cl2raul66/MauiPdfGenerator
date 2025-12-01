namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

public interface IPdfResourceBuilder
{
    IPdfResourceBuilder Style<TElementStyle>(string key, Action<TElementStyle> setup)
        where TElementStyle : class, IPdfElement<TElementStyle>;

    IPdfResourceBuilder Style<TElementStyle>(string key, string basedOn, Action<TElementStyle> setup)
        where TElementStyle : class, IPdfElement<TElementStyle>;
}
