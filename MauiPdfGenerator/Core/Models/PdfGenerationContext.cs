using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core.Implementation.Sk.Utils;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Core.Models;

internal record PdfGenerationContext(
    PdfPageData PageData,
    PdfFontRegistryBuilder FontRegistry,
    Dictionary<object, object> LayoutState,
    ILogger Logger,
    IElementRendererFactory RendererFactory,
    IDiagnosticSink DiagnosticSink,
    object? Element = null
);
