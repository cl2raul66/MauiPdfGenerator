// --- START OF FILE MauiPdfGenerator/Fluent/DocumentConfigurator.cs ---
using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Primitives;
using MauiPdfGenerator.Fluent.Primitives;

namespace MauiPdfGenerator.Fluent;

/// <summary>
/// Helper class provided to the Configure action for setting document defaults.
/// </summary>
public class DocumentConfigurator
{
    private readonly DocumentSettings _settings;

    internal DocumentConfigurator(DocumentSettings settings)
    {
        _settings = settings;
    }

    /// <summary> Sets the default unit for subsequent calls in the configuration block or document. </summary>
    public DocumentConfigurator Unit(Unit unit)
    {
        _settings.Units = PdfDocumentBuilder.MapUnit(unit);
        return this;
    }

    /// <summary> Sets the default page size using a standard type and optional orientation. Page size is stored internally in points. </summary>
    public DocumentConfigurator PageSize(PageSizeType pageSizeType, PageOrientationType orientation = PageOrientationType.Portrait)
    {
        _settings.DefaultPageSize = MapPageSizeType(pageSizeType, orientation); // Use internal static helper
        return this;
    }

    /// <summary> Sets the default page size using specific dimensions (assumed points) and optional orientation. Page size is stored internally in points. </summary>
    public DocumentConfigurator PageSize(Size pageSize, PageOrientationType orientation = PageOrientationType.Portrait)
    {
        float width = (float)pageSize.Width;
        float height = (float)pageSize.Height;
        if (orientation is PageOrientationType.Landscape && height > width)
        {
            (width, height) = (height, width);
        }
        else if (orientation is PageOrientationType.Portrait && width > height)
        {
            (width, height) = (height, width);
        }
        _settings.DefaultPageSize = new PdfSize(width, height); // Store as points
        return this;
    }

    /// <summary> Sets the default margins for all sides (uses current document unit). </summary>
    public DocumentConfigurator Margins(float allSides)
    {
        // Store margin value as provided; Core will interpret using _settings.Units
        _settings.DefaultMargins = new PdfMargins(allSides);
        return this;
    }

    /// <summary> Sets the default margins for each side (uses current document unit). </summary>
    public DocumentConfigurator Margins(float left, float top, float right, float bottom)
    {
        // Store margin values as provided; Core will interpret using _settings.Units
        _settings.DefaultMargins = new PdfMargins(left, top, right, bottom);
        return this;
    }

    /// <summary> Sets the default font. Font size is assumed to be in points. </summary>
    public DocumentConfigurator Font(string fontFamily, float fontSize, FontAttributes attributes = FontAttributes.None)
    {
        // Font size stored as provided, assumed points by convention
        _settings.DefaultFont = new PdfFont(fontFamily, fontSize, PdfDocumentBuilder.MapFontAttributes(attributes));
        return this;
    }

    /// <summary> Sets PDF metadata. </summary>
    public DocumentConfigurator Metadata(string? author = null, string? title = null, string? subject = null, string? keywords = null)
    {
        if (author is not null) _settings.Author = author;
        if (title is not null) _settings.Title = title;
        if (subject is not null) _settings.Subject = subject;
        if (keywords is not null) _settings.Keywords = keywords;
        return this;
    }

    /// <summary> Maps PageSizeType and PageOrientationType to a PdfSize in points. </summary>
    internal static PdfSize MapPageSizeType(PageSizeType type, PageOrientationType orientation)
    {
        PdfSize size = type switch // Get base portrait size in points
        {
            PageSizeType.A4 => DocumentSettings.PageSizes.A4,
            PageSizeType.Letter => DocumentSettings.PageSizes.Letter,
            PageSizeType.Legal => DocumentSettings.PageSizes.Legal,
            PageSizeType.A3 => DocumentSettings.PageSizes.A3,
            PageSizeType.A5 => DocumentSettings.PageSizes.A5,
            _ => DocumentSettings.PageSizes.A4 // Default
        };

        // Apply orientation swap if needed
        if (orientation is PageOrientationType.Landscape)
        {
            return new PdfSize(size.Height, size.Width); // Swap dimensions
        }
        // else Portrait
        return size; // Return original portrait dimensions
    }
}
// --- END OF FILE MauiPdfGenerator/Fluent/DocumentConfigurator.cs ---
