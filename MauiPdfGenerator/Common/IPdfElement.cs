namespace MauiPdfGenerator.Common;

internal interface IPdfElement
{
    IElement? Parent { get; }
}
