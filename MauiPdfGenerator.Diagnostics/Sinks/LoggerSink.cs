using MauiPdfGenerator.Diagnostics.Contracts;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Diagnostics.Sinks;

internal class LoggerSink(ILogger logger) : IPdfDiagnosticsSink
{
    public void Handle(PdfDiagnosticEvent diagnosticEvent)
    {
        var logLevel = diagnosticEvent.Severity switch
        {
            PdfDiagnosticSeverity.Info => LogLevel.Information,
            PdfDiagnosticSeverity.Warning => LogLevel.Warning,
            PdfDiagnosticSeverity.Error => LogLevel.Error,
            PdfDiagnosticSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Debug
        };

        var eventId = new EventId((int)diagnosticEvent.Code, diagnosticEvent.Code.ToString());

        logger.Log(logLevel, eventId, "[{Code}] {Message} (Component: {ComponentType}, Page: {PageIndex})",
            diagnosticEvent.Code,
            diagnosticEvent.Message,
            diagnosticEvent.ComponentType,
            diagnosticEvent.PageIndex);
    }
}
