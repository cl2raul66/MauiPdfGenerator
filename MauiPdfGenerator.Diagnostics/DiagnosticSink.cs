using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics;

public class DiagnosticSink : IDiagnosticSink
{
    private readonly IEnumerable<IDiagnosticListener> _listeners;

    public DiagnosticSink(IEnumerable<IDiagnosticListener> listeners)
    {
        _listeners = listeners;
    }

    public void Submit(DiagnosticMessage message)
    {
        foreach (var listener in _listeners)
        {
            listener.OnMessageSubmitted(message);
        }
    }
}
