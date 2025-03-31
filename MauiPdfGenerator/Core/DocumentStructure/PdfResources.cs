using MauiPdfGenerator.Core.ObjectModel;

namespace MauiPdfGenerator.Core.DocumentStructure;

/// <summary>
/// Represents a PDF Resources dictionary, used by Pages or other objects.
/// Internal implementation detail.
/// </summary>
internal class PdfResources : PdfDictionary
{
    public PdfResources()
    {
        // Initialize common entries if needed, e.g., ProcSet for compatibility
        var procSetArray = new PdfArray();
        procSetArray.Add(new PdfName("/PDF"));
        procSetArray.Add(new PdfName("/Text"));
        procSetArray.Add(new PdfName("/ImageB")); // Add others as needed
        procSetArray.Add(new PdfName("/ImageC"));
        procSetArray.Add(new PdfName("/ImageI"));
        this[Common.PdfConstants.Names.ProcSet] = procSetArray;

        // Initialize Font and XObject dictionaries (can be added to later)
        this[Common.PdfConstants.Names.Font] = new PdfDictionary();
        this[Common.PdfConstants.Names.XObject] = new PdfDictionary();
    }

    /// <summary>
    /// Gets the dictionary used for Font resources.
    /// </summary>
    public PdfDictionary FontResources => (PdfDictionary)this[Common.PdfConstants.Names.Font];

    /// <summary>
    /// Gets the dictionary used for XObject resources (like images).
    /// </summary>
    public PdfDictionary XObjectResources => (PdfDictionary)this[Common.PdfConstants.Names.XObject];

    /// <summary>
    /// Adds a font resource with a given name (e.g., /F1).
    /// </summary>
    /// <param name="fontName">The name to assign (e.g., "/F1").</param>
    /// <param name="fontReference">Reference to the indirect Font dictionary object.</param>
    public void AddFont(string fontName, PdfReference fontReference)
    {
        FontResources[fontName] = fontReference;
    }

    /// <summary>
    /// Adds an XObject resource (e.g., an image) with a given name (e.g., /Im1).
    /// </summary>
    /// <param name="xObjectName">The name to assign (e.g., "/Im1").</param>
    /// <param name="xObjectReference">Reference to the indirect XObject stream object.</param>
    public void AddXObject(string xObjectName, PdfReference xObjectReference)
    {
        XObjectResources[xObjectName] = xObjectReference;
    }
}
