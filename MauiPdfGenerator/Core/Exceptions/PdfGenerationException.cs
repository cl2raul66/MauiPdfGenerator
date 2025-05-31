namespace MauiPdfGenerator.Core.Exceptions;

internal class PdfGenerationException : Exception
{
    public PdfGenerationException() : base("An error occurred during PDF generation.") { }
    public PdfGenerationException(string message) : base(message) { }

    public PdfGenerationException(string message, Exception innerException) : base(message, innerException) { }
}
