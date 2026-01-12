using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Diagnostics.Listeners;

public class ConsoleDiagnosticListener : IDiagnosticListener
{
    public void OnMessageSubmitted(DiagnosticMessage message)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine(message.ToString());
#endif
    }
}
