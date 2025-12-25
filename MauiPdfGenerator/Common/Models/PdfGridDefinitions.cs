using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Common.Models;

/// <summary>
/// Representa la definición de una fila en un grid PDF.
/// </summary>
internal readonly record struct PdfRowDefinition(PdfGridLength Height);

/// <summary>
/// Representa la definición de una columna en un grid PDF.
/// </summary>
internal readonly record struct PdfColumnDefinition(PdfGridLength Width);