using MauiPdfGenerator.Diagnostics.Contracts;
using MauiPdfGenerator.Diagnostics.Visualizer;
using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Diagnostics.Sinks;

internal class VisualOverlaySink : IPdfDiagnosticsSink
{
    private readonly Dictionary<int, List<(PdfDiagnosticEvent, PdfRect?)>> _diagnosticsByPage = new();
    private readonly IPdfDiagnosticVisualizer _visualizer = new OverlayVisualizer();

    public void Handle(PdfDiagnosticEvent diagnosticEvent)
    {
        if (!_diagnosticsByPage.TryGetValue(diagnosticEvent.PageIndex, out var events))
        {
            events = new List<(PdfDiagnosticEvent, PdfRect?)>();
            _diagnosticsByPage[diagnosticEvent.PageIndex] = events;
        }

        // Intentamos obtener los bounds del elemento si existen en el contexto del evento
        var bounds = (diagnosticEvent.ElementData as IBoundsProvider)?.Bounds;
        events.Add((diagnosticEvent, bounds));
    }

    public async Task RenderDiagnosticsAsync(SKCanvas canvas, int pageIndex, PdfGenerationContext context)
    {
        if (!_diagnosticsByPage.TryGetValue(pageIndex, out var events))
        {
            return;
        }

        foreach (var (diagnosticEvent, bounds) in events)
        {
            if (bounds.HasValue)
            {
                await _visualizer.DrawOverlayAsync(canvas, bounds.Value, diagnosticEvent, context);
            }
        }
    }
}
