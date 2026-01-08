using Xunit;
using Moq;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Diagnostics;

public class DiagnosticSinkTests
{
    [Fact]
    public void Submit_WithSingleListener_ListenerReceivesMessage()
    {
        var listenerMock = new Mock<IDiagnosticListener>();
        var listeners = new List<IDiagnosticListener> { listenerMock.Object };
        var diagnosticSink = new DiagnosticSink(listeners);
        var message = new DiagnosticMessage(DiagnosticSeverity.Warning, "IMG-001", "Image decode error");

        diagnosticSink.Submit(message);

        listenerMock.Verify(l => l.OnMessageSubmitted(message), Times.Once);
    }

    [Fact]
    public void Submit_WithMultipleListeners_AllListenersReceiveMessage()
    {
        var listener1Mock = new Mock<IDiagnosticListener>();
        var listener2Mock = new Mock<IDiagnosticListener>();
        var listeners = new List<IDiagnosticListener> { listener1Mock.Object, listener2Mock.Object };
        var diagnosticSink = new DiagnosticSink(listeners);
        var message = new DiagnosticMessage(DiagnosticSeverity.Info, "LAYOUT-001", "Layout overflow");

        diagnosticSink.Submit(message);

        listener1Mock.Verify(l => l.OnMessageSubmitted(message), Times.Once);
        listener2Mock.Verify(l => l.OnMessageSubmitted(message), Times.Once);
    }

    [Fact]
    public void Submit_WithNoListeners_DoesNotThrowException()
    {
        var listeners = new List<IDiagnosticListener>();
        var diagnosticSink = new DiagnosticSink(listeners);
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "FONT-001", "Font not found");

        var exception = Record.Exception(() => diagnosticSink.Submit(message));

        Assert.Null(exception);
    }
}
