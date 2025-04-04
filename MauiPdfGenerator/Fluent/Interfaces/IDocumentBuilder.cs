namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Main interface for building a PDF document.
/// </summary>
public interface IDocumentBuilder : IDisposable
{
    /// <summary>
    /// Configures global document settings like default page size, margins, metadata, and security.
    /// </summary>
    /// <param name="configAction">Action to configure the document settings.</param>
    /// <returns>The document builder instance for chaining.</returns>
    IDocumentBuilder Configure(Action<IDocumentConfigurator> configAction);

    /// <summary>
    /// Adds a new page to the document and allows configuration of its content.
    /// </summary>
    /// <param name="pageAction">Action to build the content of the page.</param>
    /// <returns>The document builder instance for chaining (to add more pages).</returns>
    IDocumentBuilder AddPage(Action<IPdfPageBuilder> pageAction);

    /// <summary>
    /// Generates the PDF document and saves it to the specified file path asynchronously.
    /// This is typically the final action performed on the builder.
    /// </summary>
    /// <param name="filePath">The full path where the PDF file will be saved.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync(string filePath);

    // Futuro: Podría haber un GenerateAsync() que devuelva byte[] o Stream
    // Task<byte[]> GenerateAsync();
    // Task<Stream> GenerateAsyncStream();
}
