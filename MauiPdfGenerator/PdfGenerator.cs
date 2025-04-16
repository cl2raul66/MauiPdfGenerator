using MauiPdfGenerator.Common;
using MauiPdfGenerator.Fluent;       // Need PdfDocumentBuilder

namespace MauiPdfGenerator;

/// <summary>
/// Static entry point for creating and generating PDF documents using a fluent API.
/// </summary>
public static class PdfGenerator
{
    /// <summary>
    /// Creates a new PDF document builder instance to start defining a PDF document.
    /// </summary>
    /// <returns>A new instance of PdfDocumentBuilder to fluently configure the document.</returns>
    public static PdfDocumentBuilder CreateDocument()
    {
        // Create the root of the internal document model
        var documentModel = new DocumentModel();

        // Instantiate and return the fluent builder, passing the model
        return new PdfDocumentBuilder(documentModel);
    }

    // Potentially add other static helper methods or constants here in the future if needed.
}