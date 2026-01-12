using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics.Listeners;

public interface IVisualDiagnosticStore
{
    IReadOnlyList<DiagnosticMessage> GetPendingMessages();
    void ClearPendingMessages();
}

public class VisualDiagnosticListener : IDiagnosticListener, IVisualDiagnosticStore
{
    private readonly List<DiagnosticMessage> _pendingMessages = new();

    public void OnMessageSubmitted(DiagnosticMessage message)
    {
        if (message.Bounds.HasValue)
        {
            _pendingMessages.Add(message);
        }
    }

    public IReadOnlyList<DiagnosticMessage> GetPendingMessages() => _pendingMessages;

    public void ClearPendingMessages() => _pendingMessages.Clear();
}
