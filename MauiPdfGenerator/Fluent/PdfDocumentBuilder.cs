using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Primitives;
using MauiPdfGenerator.Fluent.Primitives;
using MauiPdfGenerator.Core;

namespace MauiPdfGenerator.Fluent;

public class PdfDocumentBuilder
{
    private readonly DocumentModel _documentModel;
    private readonly PdfGenerationEngine _generationEngine;

    internal PdfDocumentBuilder(DocumentModel documentModel)
    {
        _documentModel = documentModel ?? throw new ArgumentNullException(nameof(documentModel));
        _generationEngine = new PdfGenerationEngine();
    }

    /// <summary>
    /// Configures document-level settings like default page size, margins, units, and metadata.
    /// </summary>
    /// <param name="configAction">An action to configure the document settings.</param>
    /// <returns>The current PdfDocumentBuilder instance for chaining.</returns>
    public PdfDocumentBuilder Configure(Action<DocumentConfigurator> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        var configurator = new DocumentConfigurator(_documentModel.Settings);
        configAction(configurator);
        return this;
    }

    /// <summary>
    /// Adds a new page to the document and provides a builder action to configure it.
    /// </summary>
    /// <param name="pageBuilderAction">An action that receives a PdfPageBuilder to configure the new page.</param>
    /// <returns>The current PdfDocumentBuilder instance for chaining.</returns>
    public PdfDocumentBuilder PdfPage(Action<PdfPageBuilder> pageBuilderAction) // Renamed AddPage
    {
        ArgumentNullException.ThrowIfNull(pageBuilderAction);

        var pageModel = new PageModel();
        _documentModel.Pages.Add(pageModel);
        var pageBuilder = new PdfPageBuilder(pageModel, _documentModel);
        pageBuilderAction(pageBuilder);

        return this;
    }

    /// <summary>
    /// Generates the PDF document and saves it.
    /// </summary>
    public async Task SaveAsync(Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        if (!outputStream.CanWrite) throw new ArgumentException("Output stream must be writable.", nameof(outputStream));
        if (_documentModel.Pages.Count == 0)
        {
            throw new InvalidOperationException("Cannot generate a PDF document with no pages. Use PdfPage() to add content.");
        }
        await _generationEngine.GeneratePdfAsync(_documentModel, outputStream).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates the PDF document based on the configuration and saves it to the specified file path.
    /// </summary>
    /// <param name="filePath">The full path of the file to create or overwrite.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        // Crear y gestionar el FileStream internamente
        // Usar using para asegurar que el stream se cierre correctamente
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            // Llamar a la sobrecarga que trabaja con Stream
            await SaveAsync(fileStream).ConfigureAwait(false);
            // El stream se cierra/dispose automáticamente al salir del bloque using
        }
    }

    // Internal helpers remain the same (MapUnit, MapFontAttributes, MapColor)
    internal static UnitOfMeasure MapUnit(Unit unit) => unit switch { Unit.Points => UnitOfMeasure.Points, Unit.Inches => UnitOfMeasure.Inches, Unit.Millimeters => UnitOfMeasure.Millimeters, _ => UnitOfMeasure.Millimeters };
    internal static PdfFontStyle MapFontAttributes(FontAttributes attributes) { PdfFontStyle style = PdfFontStyle.Normal; if ((attributes & FontAttributes.Bold) == FontAttributes.Bold) style |= PdfFontStyle.Bold; if ((attributes & FontAttributes.Italic) == FontAttributes.Italic) style |= PdfFontStyle.Italic; return style; }
    internal static PdfColor MapColor(Color color) { return new PdfColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255)); }

}
