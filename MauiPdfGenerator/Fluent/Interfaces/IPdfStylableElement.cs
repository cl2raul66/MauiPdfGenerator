using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfStylableElement<TSelf> where TSelf : IPdfStylableElement<TSelf>
{
    TSelf Style(PdfStyleIdentifier key);

    TSelf Style(string key) => Style(new PdfStyleIdentifier(key));
}