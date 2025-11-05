using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Diagnostics.Listeners;

public class LoggingDiagnosticListener : IDiagnosticListener
{
    private readonly ILogger<IDiagnosticSink> _logger;

    public LoggingDiagnosticListener(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<IDiagnosticSink>();
    }

    public void OnMessageSubmitted(DiagnosticMessage message)
    {
#if !DEBUG
        var logLevel = message.Severity switch
        {
            DiagnosticSeverity.Trace => LogLevel.Trace,
            DiagnosticSeverity.Debug => LogLevel.Debug,
            DiagnosticSeverity.Info => LogLevel.Information,
            DiagnosticSeverity.Warning => LogLevel.Warning,
            DiagnosticSeverity.Error => LogLevel.Error,
            DiagnosticSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };
        _logger.Log(logLevel, "{Message}", message.ToString());
#endif
    }
}
