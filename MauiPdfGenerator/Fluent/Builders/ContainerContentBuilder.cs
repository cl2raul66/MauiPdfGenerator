using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders;

/// <summary>
/// Internal implementation of the content builder used within .Content()/.Children() lambdas.
/// Creates and configures element/layout builders and tracks them.
/// </summary>
internal class ContainerContentBuilder : IPdfContainerContentBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfResources _resources;
    private readonly object? _parentContext; // Could be PageBuilder, GridBuilder, etc. for context

    // Stores the builders created by this instance
    private readonly List<object> _addedElements = [];

    public ContainerContentBuilder(PdfDocument pdfDocument, PdfResources resources, object? parentContext)
    {
        _pdfDocument = pdfDocument ?? throw new ArgumentNullException(nameof(pdfDocument));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _parentContext = parentContext; // Store context if needed later
    }

    /// <summary>
    /// Gets the list of element/layout builders that were added via this content builder.
    /// Used by the parent (e.g., PageBuilder) to retrieve the content.
    /// </summary>
    /// <returns>A list of builder instances.</returns>
    internal List<object> GetAddedElements()
    {
        return _addedElements;
    }


    // --- IPdfContainerContentBuilder Implementation ---

    public IPdfParagraphBuilder Paragraph(Action<IPdfParagraphBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        // Crear la implementación del builder de párrafo (CLASE AÚN NO EXISTE)
        var paragraphBuilder = new ParagraphBuilder(_pdfDocument, _resources);
        configAction(paragraphBuilder);
        _addedElements.Add(paragraphBuilder); // Track the configured builder
        return paragraphBuilder;
    }

    public IPdfImageBuilder Image(Action<IPdfImageBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        // Crear la implementación del builder de imagen (CLASE AÚN NO EXISTE)
        var imageBuilder = new ImageBuilder(_pdfDocument, _resources);
        configAction(imageBuilder);
        _addedElements.Add(imageBuilder);
        return imageBuilder;
    }

    public IPdfGridBuilder Grid(Action<IPdfGridBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        // Crear la implementación del builder de grid (CLASE AÚN NO EXISTE)
        var gridBuilder = new GridBuilder(_pdfDocument, _resources);
        configAction(gridBuilder);
        _addedElements.Add(gridBuilder);
        return gridBuilder;
    }

    public IPdfVerticalStackLayoutBuilder VerticalStackLayout(Action<IPdfVerticalStackLayoutBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        // Crear la implementación del VSL builder (CLASE AÚN NO EXISTE)
        var vslBuilder = new VerticalStackLayoutBuilder(_pdfDocument, _resources);
        configAction(vslBuilder);
        _addedElements.Add(vslBuilder);
        return vslBuilder;
    }

    public IPdfHorizontalStackLayoutBuilder HorizontalStackLayout(Action<IPdfHorizontalStackLayoutBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        // Crear la implementación del HSL builder (CLASE AÚN NO EXISTE)
        var hslBuilder = new HorizontalStackLayoutBuilder(_pdfDocument, _resources);
        configAction(hslBuilder);
        _addedElements.Add(hslBuilder);
        return hslBuilder;
    }
}
