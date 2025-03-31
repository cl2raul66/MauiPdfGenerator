//using MauiPdfGenerator.Graphics;
//using MauiPdfGenerator.Core.ObjectModel; // Needed if PdfObjectEntry moved here
//using MauiPdfGenerator.Core.DocumentStructure; // Needed for PdfResources etc. if used here
//using MauiPdfGenerator.Common;
//using MauiPdfGenerator.Fluent.Enums; // For PdfWriter reference// MauiPdfGenerator/Core/DocumentContext.cs 

//namespace MauiPdfGenerator.Core;

//// Internal class replacing the previous 'PdfContext' logic.
//// It holds the state and logic needed to build the PDF object structure,
//// managed internally by DocumentBuilder and PdfWriter.

///// <summary>
///// Internal class responsible for managing the overall document state and object creation
///// during the fluent building process. It interacts with PdfWriter.
///// This class is NOT directly exposed to the library user.
///// </summary>
//internal class DocumentContext : IDisposable
//{
//    private readonly PdfWriter _writer;
//    private readonly float _defaultWidth;
//    private readonly float _defaultHeight;

//    // References to core structure objects managed by PdfWriter
//    private readonly PdfIndirectObject _catalogIndirectObject;
//    private readonly PdfIndirectObject _pagesRootIndirectObject;
//    private readonly PdfArray _pageKidsArray; // The actual Kids array within the Pages dict

//    // Counter for pages added
//    private int _pageCount = 0;

//    // Store page build results until finalization
//    private readonly List<PageBuildResult> _pendingPages = new();

//    // Internal record to hold results from PageBuilder
//    private record PageBuildResult(
//        byte[] ContentStreamData,
//        PdfResources Resources,
//        float Width,
//        float Height);

//    internal DocumentContext(DocumentOptions options, PdfWriter writer)
//    {
//        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

//        // Determine default page size
//        var defaultPageSize = options?.DefaultPageSize ?? PageSize.Auto;
//        _defaultWidth = defaultPageSize.WidthPoints;
//        _defaultHeight = defaultPageSize.HeightPoints;


//        // --- Initialize Core Document Structure Objects VIA PdfWriter ---

//        // 1. Pages Root Node (/Pages dictionary)
//        _pageKidsArray = new PdfArray();
//        var pagesDict = new PdfDictionary
//        {
//            [Common.PdfConstants.Names.Type] = new PdfName(Common.PdfConstants.Names.Pages),
//            [Common.PdfConstants.Names.Kids] = _pageKidsArray,
//            [Common.PdfConstants.Names.Count] = new PdfInteger(0) // Will be updated later
//        };
//        _pagesRootIndirectObject = _writer.AddObject(pagesDict); // Add early to get reference

//        // 2. Catalog Node (/Catalog dictionary)
//        var catalogDict = new PdfCatalog(_pagesRootIndirectObject.CreateReference());
//        // TODO: Add Version, Metadata reference based on options
//        _catalogIndirectObject = _writer.AddObject(catalogDict);

//        // 3. Tell PdfWriter which object is the root Catalog
//        _writer.SetCatalog(catalogDict);

//        // TODO: Initialize Info Dictionary (optional)
//        // TODO: Initialize Security/Encryption
//    }

//    internal float DefaultWidth => _defaultWidth;
//    internal float DefaultHeight => _defaultHeight;

//    /// <summary>
//    /// Creates the necessary internal structures for a new page and returns a canvas.
//    /// Called by DocumentBuilder.Page().
//    /// </summary>
//    internal PdfCanvas InitializeNewPage(float width, float height, out PdfResources pageResources)
//    {
//        // --- Prepare Resources for this Page ---
//        // TODO: Implement resource sharing. For now, new resources per page.
//        pageResources = new PdfResources();

//        // TODO: Robust Font Management - Placeholder for Helvetica /F1
//        var fontDict = new PdfDictionary
//        {
//            [Common.PdfConstants.Names.Type] = new PdfName(Common.PdfConstants.Names.Font),
//            [Common.PdfConstants.Names.Subtype] = new PdfName(Common.PdfConstants.Names.Type1),
//            [Common.PdfConstants.Names.Name] = new PdfName("/F1"),
//            [Common.PdfConstants.Names.BaseFont] = new PdfName("/Helvetica")
//        };
//        var fontIndirectObject = _writer.AddObject(fontDict); // Add font dict to writer
//        pageResources.AddFont("/F1", fontIndirectObject.CreateReference()); // Add ref to page resources

//        // --- Create Canvas ---
//        // TODO: Pass resource manager to Canvas if needed
//        var canvas = new PdfCanvas(width, height);
//        return canvas;
//    }

//    /// <summary>
//    /// Stores the result of building a page, to be processed during finalization.
//    /// Called by DocumentBuilder.Page() after the user configuration action is done.
//    /// </summary>
//    internal void StorePageResult(byte[] contentStreamData, PdfResources resources, float width, float height)
//    {
//        var result = new PageBuildResult(contentStreamData, resources, width, height);
//        _pendingPages.Add(result);
//        _pageCount++;
//    }


//    /// <summary>
//    /// Finalizes the PDF structure by creating Page objects etc. in the PdfWriter.
//    /// Called internally before writing the document.
//    /// </summary>
//    internal async Task FinalizeStructureAsync()
//    {
//        // Update page count in the existing /Pages dictionary object
//        if (_pagesRootIndirectObject.ContainedObject is PdfDictionary pagesDict)
//        {
//            pagesDict[Common.PdfConstants.Names.Count] = new PdfInteger(_pageCount);
//        }
//        else { /* This case should not happen */ }

//        // Clear and repopulate the Kids array (references to page objects)
//        _pageKidsArray.Clear(); // Assuming PdfArray has Clear method

//        foreach (var pageResult in _pendingPages)
//        {
//            // 1. Add Content Stream Object to writer
//            var contentStream = new PdfStream(pageResult.ContentStreamData);
//            // TODO: Apply compression
//            var contentStreamIndirectObject = _writer.AddObject(contentStream);

//            // 2. Add Resources Object to writer
//            var resourcesIndirectObject = _writer.AddObject(pageResult.Resources);

//            // 3. Create Page Object Dictionary
//            var mediaBox = new PdfArray(new PdfObject[] {
//                new PdfInteger(0), new PdfInteger(0),
//                new PdfReal(pageResult.Width), new PdfReal(pageResult.Height)
//            });
//            var pageDict = new PdfPage(
//                _pagesRootIndirectObject.CreateReference(),
//                mediaBox,
//                resourcesIndirectObject.CreateReference(),
//                contentStreamIndirectObject.CreateReference()
//            );
//            // 4. Add Page Dictionary Object to writer
//            var pageIndirectObject = _writer.AddObject(pageDict);

//            // 5. Add reference to the Page object to the Kids array
//            _pageKidsArray.Add(pageIndirectObject.CreateReference());
//        }

//        // TODO: Finalize Info dict, Encryption dict etc.

//        await Task.CompletedTask; // Placeholder
//    }


//    public void Dispose()
//    {
//        // Nothing specific to dispose here usually, PdfWriter handles stream
//    }
//}

//// TODO: Add Clear() method to Core/ObjectModel/PdfArray.cs
//internal static class PdfArrayInternalExtensions
//{
//    // Temporary placeholder - Requires modification of PdfArray class
//    internal static void Clear(this PdfArray array)
//    {
//        // Access the internal list and clear it
//        // Example (assuming _items is the internal list):
//        // var itemsField = typeof(PdfArray).GetField("_items",
//        //    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        // if (itemsField?.GetValue(array) is List<PdfObject> list)
//        // {
//        //     list.Clear();
//        // }
//        // BETTER: Add a public Clear() method to PdfArray itself.
//        throw new NotImplementedException("PdfArray needs a Clear method");
//    }
//}
