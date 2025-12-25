using MauiPdfGenerator.Common.Models.Views;

namespace MauiPdfGenerator.Core.Implementation.Sk.Views;

/// <summary>
/// Factory para crear renderers de elementos PDF.
/// </summary>
internal interface IElementRendererFactory
{
    IElementRenderer GetRenderer(object element);
}
