using MauiPdfGenerator.Diagnostics.Contracts;
using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Diagnostics.Visualizer;

internal interface IPdfDiagnosticVisualizer
{
    Task DrawOverlayAsync(SKCanvas canvas, PdfRect bounds, PdfDiagnosticEvent diagnosticEvent, PdfGenerationContext context);
}
