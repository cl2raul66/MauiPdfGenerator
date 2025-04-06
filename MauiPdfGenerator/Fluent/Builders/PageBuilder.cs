using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Common.Geometry; // For PdfRectangle
using System;
using System.Collections.Generic;
using MauiPdfGenerator.Core.Objects;

namespace MauiPdfGenerator.Fluent.Builders;

/// <summary>
/// Internal implementation of the page builder interface.
/// Manages the definition of content for a single PDF page.
/// </summary>
internal class PageBuilder : IPdfPageBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfPage _pdfPage; // The Core PdfPage object this builder targets
    private readonly DocumentBuilder _documentBuilder; // Access to document defaults/settings
    private readonly PdfResources _pageResources; // Resources specific to this page

    // Store the root content defined by the user (e.g., the root layout builder)
    // Option 1: Store the action itself
    private Action<IPdfContainerContentBuilder>? _contentAction;
    // Option 2: Store the result of the action (e.g., list of root builders/elements)
    private readonly List<object> _rootElements = new List<object>(); // Store builders or intermediate models

    // --- Page specific settings (Defaults from DocumentBuilder, can be overridden) ---
    private PdfRectangle _mediaBox;
    private float _marginLeft;
    private float _marginTop;
    private float _marginRight;
    private float _marginBottom;
    // TODO: Add properties for Page-specific Rotation, CropBox etc. if needed

    public PageBuilder(PdfDocument pdfDocument, PdfPage pdfPage, DocumentBuilder documentBuilder)
    {
        _pdfDocument = pdfDocument ?? throw new ArgumentNullException(nameof(pdfDocument));
        _pdfPage = pdfPage ?? throw new ArgumentNullException(nameof(pdfPage));
        _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));

        // Initialize with defaults from DocumentBuilder
        _mediaBox = _pdfPage.MediaBox; // Get the MediaBox assigned by DocumentBuilder
        _marginLeft = _documentBuilder.DefaultPageMarginLeft;
        _marginTop = _documentBuilder.DefaultPageMarginTop;
        _marginRight = _documentBuilder.DefaultPageMarginRight;
        _marginBottom = _documentBuilder.DefaultPageMarginBottom;

        // Assign the pre-created Resources dictionary from PdfPage
        _pageResources = (_pdfPage.Resources as PdfDictionary) as PdfResources ??
                         throw new InvalidOperationException("PdfPage is missing its Resources dictionary.");
        if (!(_pdfPage.Resources is PdfResources)) // Ensure it's our specific type if needed later
        {
            // This might indicate an issue if PdfPage constructor created a plain PdfDictionary
            Console.WriteLine("Warning: Page Resources dictionary is not of type PdfResources.");
        }

    }

    // --- IPdfPageBuilder Implementation ---

    /// <summary>
    /// Defines the primary content structure for this page.
    /// </summary>
    /// <param name="contentAction">Action to add content using the provided builder.</param>
    /// <returns>The page builder instance.</returns>
    public IPdfPageBuilder Content(Action<IPdfContainerContentBuilder> contentAction)
    {
        if (contentAction == null) throw new ArgumentNullException(nameof(contentAction));

        // Option 1: Store the action to execute later during FinalizePage
        // _contentAction = contentAction;

        // Option 2: Execute the action now and store the resulting builders/elements
        var contentBuilder = new ContainerContentBuilder(_pdfDocument, _pageResources, this /* or null context? */);
        contentAction(contentBuilder);
        _rootElements.AddRange(contentBuilder.GetAddedElements()); // Need GetAddedElements method

        return this;
    }

    // TODO: Implement IPdfPageBuilder.Configure(Action<IPdfPageConfigurator> configAction) if needed


    // --- Internal Methods ---

    /// <summary>
    /// Called by DocumentBuilder after the user's page build action has been executed.
    /// This method generates the actual PDF content stream for the page.
    /// </summary>
    internal void FinalizePage()
    {
        // 1. Create the Content Stream for this page
        var contentStream = new PdfContentStream(_pdfDocument, _pageResources);

        // 2. Define the available drawing area (considering margins)
        // Convert float margins to double for PdfRectangle
        PdfRectangle contentArea = new PdfRectangle(
            _mediaBox.X + (double)_marginLeft,
            _mediaBox.Y + (double)_marginBottom, // PDF Y increases upwards, bottom margin affects Y
            _mediaBox.Width - (double)_marginLeft - (double)_marginRight,
            _mediaBox.Height - (double)_marginTop - (double)_marginBottom
        );
        if (contentArea.Width < 0) contentArea = new PdfRectangle(contentArea.X, contentArea.Y, 0, contentArea.Height);
        if (contentArea.Height < 0) contentArea = new PdfRectangle(contentArea.X, contentArea.Y, contentArea.Width, 0);


        // 3. Process the stored root elements/builders (_rootElements)
        //    This is where the complex layout engine logic will reside.
        //    It needs to iterate through elements, calculate positions/sizes based on
        //    layout containers (Grid, StackLayout), and use the contentStream
        //    to draw them (e.g., contentStream.SetFont, .ShowText, .DrawXObject, etc.)
        //
        //    ***************************************************************
        //    *** PLACEHOLDER - Complex Layout & Drawing Logic Goes Here! ***
        //    ***************************************************************
        Console.WriteLine($"Finalizing Page: Processing {_rootElements.Count} root elements for page {_pdfPage.GetHashCode()}."); // Simple placeholder log
                                                                                                                                  // Example (very simplified, no real layout):
        double currentY = contentArea.Top; // Start from top (adjusting for PDF coordinate system)
        foreach (var elementBuilder in _rootElements)
        {
            if (elementBuilder is ParagraphBuilder paraBuilder) // Assuming storing builders directly
            {
                // Very basic text drawing placeholder
                contentStream.BeginText();
                // TODO: Need Font handling - Get a default font
                // PdfFontBase font = GetDefaultFont(); // Need font management
                // contentStream.SetFont(font, paraBuilder.ConfiguredFontSize);
                // Position text - needs proper calculation based on contentArea & text metrics
                contentStream.MoveTextPosition(contentArea.Left, currentY - paraBuilder.ConfiguredFontSize); // Move origin
                contentStream.ShowText(paraBuilder.ConfiguredText); // Needs proper encoding too
                contentStream.EndText();
                currentY -= (paraBuilder.ConfiguredFontSize + 5); // Move down (simplistic)
            }
            // else if (elementBuilder is ImageBuilder imgBuilder) { ... DrawXObject ... }
            // else if (elementBuilder is GridBuilder gridBuilder) { ... Layout Grid ... }
        }


        // 4. Finalize and get the content stream bytes
        contentStream.Dispose(); // Flushes writer, gets bytes, updates unfiltered data
        var contentStreamBytes = contentStream.UnfilteredData; // Get bytes *before* applying filters in base

        // 5. Add the content stream to the document as an indirect object
        var contentStreamIndirect = _pdfDocument.AddIndirectObject(contentStream);

        // 6. Set the /Contents entry in the PdfPage dictionary
        _pdfPage.Contents = contentStreamIndirect.Reference;

        // 7. Add Resources Dictionary indirectly (if not already shared/added)
        // Check if PdfResources dictionary itself needs to be an indirect object
        // Currently PdfPage.Resources holds the direct dictionary instance.
        // We need to add it to the document and set the page's /Resources to its reference.
        var resourcesIndirect = _pdfDocument.AddIndirectObject(_pageResources);
        _pdfPage.Add(PdfName.Resources, resourcesIndirect.Reference); // Update page entry to be the reference


        Console.WriteLine($"Page finalized. Content stream {contentStreamIndirect.Reference}, Resources {resourcesIndirect.Reference}.");
    }

    // Internal properties to provide context to child builders if needed
    internal double GetAvailableWidth() => _mediaBox.Width - (double)_marginLeft - (double)_marginRight;
    internal double GetAvailableHeight() => _mediaBox.Height - (double)_marginTop - (double)_marginBottom;
    internal PdfRectangle GetContentArea() => new PdfRectangle(
             _mediaBox.X + (double)_marginLeft,
             _mediaBox.Y + (double)_marginBottom,
             GetAvailableWidth(),
             GetAvailableHeight()
         );

}
