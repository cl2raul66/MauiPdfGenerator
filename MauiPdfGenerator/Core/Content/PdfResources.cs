using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure; // Might need PdfDocument reference

namespace MauiPdfGenerator.Core.Content;

/// <summary>
/// Represents a Resource dictionary associated with content streams (e.g., for a Page).
/// Manages resources like Fonts, XObjects (Images), etc., assigning them unique names within the dictionary.
/// Section 7.8.3.
/// </summary>
internal class PdfResources : PdfDictionary
{
    private readonly PdfDocument _document; // Needed to add resource objects indirectly
    private readonly Dictionary<object, PdfName> _resourceMap = []; // Maps resource instance (Font, Image) to its PDF name (/F1, /Im1)
    private int _nextFontId = 1;
    private int _nextImageId = 1;
    // Add counters for other resource types (ExtGState, Pattern, etc.) as needed

    // Lazily created sub-dictionaries
    private PdfDictionary? _fontDict;
    private PdfDictionary? _xobjectDict;

    public PdfResources(PdfDocument document) : base()
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Gets or creates the /Font sub-dictionary.
    /// </summary>
    private PdfDictionary FontDict => _fontDict ??= GetOrCreateSubDirectory(PdfName.Font);

    /// <summary>
    /// Gets or creates the /XObject sub-dictionary.
    /// </summary>
    private PdfDictionary XObjectDict => _xobjectDict ??= GetOrCreateSubDirectory(PdfName.XObject);

    /// <summary>
    /// Gets a resource name (/F1, /Im1, etc.) for a given resource object (like a PdfFont or PdfImageXObject instance),
    /// adding the resource to the dictionary and document if it's not already present.
    /// </summary>
    /// <param name="resourceObject">The high-level resource object instance.</param>
    /// <returns>The PDF Name assigned to this resource within the dictionary.</returns>
    public PdfName GetResourceName(object resourceObject)
    {
        // Check if this exact resource instance is already registered
        if (_resourceMap.TryGetValue(resourceObject, out var existingName))
        {
            return existingName;
        }

        // Resource is new, determine its type and add it
        PdfName newName;
        PdfObject resourcePdfObject;

        switch (resourceObject)
        {
            // --- Font Handling ---
            // Assuming we have a Core.Fonts.PdfFontBase or similar base class/interface
            case Core.Fonts.PdfFontBase font: // Adjust type as needed
                newName = PdfName.Get($"F{_nextFontId++}");
                resourcePdfObject = font.GetPdfObject(_document); // Method to get/create the font dictionary
                FontDict.Add(newName, _document.AddIndirectObject(resourcePdfObject).Reference); // Add font dict indirectly
                break;

            // --- Image Handling ---
            // Assuming PdfImageXObject IS the PdfObject (stream with dictionary)
            case Core.Images.PdfImageXObject imageXObject:
                newName = PdfName.Get($"Im{_nextImageId++}");
                resourcePdfObject = imageXObject; // The image XObject is the resource itself
                XObjectDict.Add(newName, _document.AddIndirectObject(resourcePdfObject).Reference); // Add image stream indirectly
                break;

            // --- Add cases for other resource types ---
            // case PdfExtGState extGState: ...
            // case PdfPattern pattern: ...

            default:
                throw new NotSupportedException($"Unsupported resource type: {resourceObject.GetType().FullName}");
        }

        _resourceMap.Add(resourceObject, newName); // Cache the mapping
        return newName;
    }


    /// <summary>
    /// Helper to get or create a sub-dictionary (like /Font or /XObject).
    /// </summary>
    private PdfDictionary GetOrCreateSubDirectory(PdfName subDirectoryKey)
    {
        if (this[subDirectoryKey] is PdfReference subDictRef)
        {
            // TODO: Need a way to resolve references within PdfDocument
            // For now, assume we store direct dictionaries temporarily or always create new
            var existingDict = _document.GetIndirectObjects().FirstOrDefault(io => io.Reference.Equals(subDictRef))?.Value as PdfDictionary;
            if (existingDict != null) return existingDict;
            // Fallback or error if reference cannot be resolved easily - simplify for now
        }

        if (this[subDirectoryKey] is PdfDictionary directDict)
        {
            return directDict;
        }


        // Not found or not a dictionary, create a new one
        var newDict = new PdfDictionary();
        // Add the new dictionary itself indirectly to the document for proper structure
        Add(subDirectoryKey, _document.AddIndirectObject(newDict).Reference);
        return newDict;
    }
}
