using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Core.Models;

internal record PdfGenerationContext(
    PdfPageData PageData,
    PdfFontRegistryBuilder FontRegistry,
    Dictionary<object, object> LayoutState,
    ILogger Logger,
    ElementRendererFactory RendererFactory,
    object? Element = null
);
