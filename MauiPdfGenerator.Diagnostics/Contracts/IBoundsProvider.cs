using MauiPdfGenerator.Core.Models;

namespace MauiPdfGenerator.Diagnostics.Contracts;

// Interfaz para que los elementos puedan exponer sus límites al sistema de diagnóstico
internal interface IBoundsProvider
{
    PdfRect? Bounds { get; }
}
