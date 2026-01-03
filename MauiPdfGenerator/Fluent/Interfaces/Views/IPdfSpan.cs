using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Views;

public interface IPdfSpan : IPdfElement<IPdfSpan>, IPdfSpanStyles
{
    IPdfSpan Text(string text);
}
