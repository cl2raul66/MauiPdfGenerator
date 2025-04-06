using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using System;
using System.Collections.Generic;
using Bumptech.Glide;

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
    private readonly List<object> _addedElements = new List<object>();

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
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        // Crear la implementación del builder de párrafo (CLASE AÚN NO EXISTE)
        var paragraphBuilder = new ParagraphBuilder(_pdfDocument, _resources);
        configAction(paragraphBuilder);
        _addedElements.Add(paragraphBuilder); // Track the configured builder
        return paragraphBuilder;
    }

    public IPdfImageBuilder Image(Action<IPdfImageBuilder> configAction)
    {
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        // Crear la implementación del builder de imagen (CLASE AÚN NO EXISTE)
        var imageBuilder = new ImageBuilder(_pdfDocument, _resources);
        configAction(imageBuilder);
        _addedElements.Add(imageBuilder);
        return imageBuilder;
    }

    public IPdfGridBuilder Grid(Action<IPdfGridBuilder> configAction)
    {
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        // Crear la implementación del builder de grid (CLASE AÚN NO EXISTE)
        var gridBuilder = new GridBuilder(_pdfDocument, _resources);
        configAction(gridBuilder);
        _addedElements.Add(gridBuilder);
        return gridBuilder;
    }

    public IPdfVerticalStackLayoutBuilder VerticalStackLayout(Action<IPdfVerticalStackLayoutBuilder> configAction)
    {
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        // Crear la implementación del VSL builder (CLASE AÚN NO EXISTE)
        var vslBuilder = new VerticalStackLayoutBuilder(_pdfDocument, _resources);
        configAction(vslBuilder);
        _addedElements.Add(vslBuilder);
        return vslBuilder;
    }

    public IPdfHorizontalStackLayoutBuilder HorizontalStackLayout(Action<IHorizontalStackLayoutBuilder> configAction)
    {
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        // Crear la implementación del HSL builder (CLASE AÚN NO EXISTE)
        var hslBuilder = new HorizontalStackLayoutBuilder(_pdfDocument, _resources);
        configAction(hslBuilder);
        _addedElements.Add(hslBuilder);
        return hslBuilder;
    }

    // --- Placeholders for other elements ---

    public IPdfTableBuilder Table(Action<IPdfTableBuilder> configAction)
    {
        // TODO: Implement TableBuilder
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        Console.WriteLine("Warning: ContainerContentBuilder.Table not implemented yet.");
        // var tableBuilder = new TableBuilder(...);
        // configAction(tableBuilder);
        // _addedElements.Add(tableBuilder);
        // return tableBuilder;
        throw new NotImplementedException();
    }

    public IPdfBulletListBuilder BulletList(Action<IBulletListBuilder> configAction)
    {
        // TODO: Implement BulletListBuilder
        if (configAction == null) throw new ArgumentNullException(nameof(configAction));
        Console.WriteLine("Warning: ContainerContentBuilder.BulletList not implemented yet.");
        // var listBuilder = new BulletListBuilder(...);
        // configAction(listBuilder);
        // _addedElements.Add(listBuilder);
        // return listBuilder;
        throw new NotImplementedException();
    }
}

// --- Placeholders for Builder Classes (Create these files next) ---
internal class ParagraphBuilder : IPdfParagraphBuilder
{
    public string ConfiguredText { get; private set; } = "";
    public double ConfiguredFontSize { get; private set; } = 12;
    // TODO: Add constructor and implement all IPdfParagraphBuilder methods
    public ParagraphBuilder(PdfDocument doc, PdfResources res) { /*...*/ }
    public IPdfParagraphBuilder Text(string text) { ConfiguredText = text; return this; }
    public IPdfParagraphBuilder FontSize(double size) { ConfiguredFontSize = size; return this; }
    // ... implement ALL other methods from IPdfParagraphBuilder ...
    public IPdfParagraphBuilder Width(double width) => throw new NotImplementedException();
    public IPdfParagraphBuilder Height(double height) => throw new NotImplementedException();
    public IPdfParagraphBuilder Margin(double uniformMargin) => throw new NotImplementedException();
    public IPdfParagraphBuilder Margin(double horizontal, double vertical) => throw new NotImplementedException();
    public IPdfParagraphBuilder Margin(double left, double top, double right, double bottom) => throw new NotImplementedException();
    public IPdfParagraphBuilder HorizontalOptions(PdfHorizontalAlignment alignment) => throw new NotImplementedException();
    public IPdfParagraphBuilder VerticalOptions(PdfVerticalAlignment alignment) => throw new NotImplementedException();
    public IPdfParagraphBuilder BackgroundColor(Microsoft.Maui.Graphics.Color color) => throw new NotImplementedException();
    public IPdfParagraphBuilder FormattedText(Action<IPdfFormattedTextBuilder> formattedTextAction) => throw new NotImplementedException();
    public IPdfParagraphBuilder FontFamily(string fontFamily) => throw new NotImplementedException();
    public IPdfParagraphBuilder TextColor(Microsoft.Maui.Graphics.Color color) => throw new NotImplementedException();
    public IPdfParagraphBuilder FontAttributes(PdfFontAttributes attributes) => throw new NotImplementedException();
    public IPdfParagraphBuilder TextDecorations(PdfTextDecorations decorations) => throw new NotImplementedException();
    public IPdfParagraphBuilder HorizontalTextAlignment(PdfTextAlignment alignment) => throw new NotImplementedException();
    public IPdfParagraphBuilder VerticalTextAlignment(PdfTextAlignment alignment) => throw new NotImplementedException();
    public IPdfParagraphBuilder LineHeight(double multiplier) => throw new NotImplementedException();
    public IPdfParagraphBuilder Padding(double uniformPadding) => throw new NotImplementedException();
    public IPdfParagraphBuilder Padding(double horizontal, double vertical) => throw new NotImplementedException();
    public IPdfParagraphBuilder Padding(double left, double top, double right, double bottom) => throw new NotImplementedException();
}
// internal class ImageBuilder : IPdfImageBuilder { /* ... */ }
// internal class GridBuilder : IPdfGridBuilder { /* ... */ }
// internal class VerticalStackLayoutBuilder : IPdfVerticalStackLayoutBuilder { /* ... */ }
// internal class HorizontalStackLayoutBuilder : IPdfHorizontalStackLayoutBuilder { /* ... */ }
// internal interface IPdfTableBuilder {} // Placeholder interface
// internal interface IPdfBulletListBuilder {} // Placeholder interface

// Fin namespace MauiPdfGenerator.Fluent.Builders
