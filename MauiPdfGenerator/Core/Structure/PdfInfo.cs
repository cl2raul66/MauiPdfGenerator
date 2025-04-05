using MauiPdfGenerator.Core.Objects;

namespace MauiPdfGenerator.Core.Structure;

/// <summary>
/// Represents the optional Document Information dictionary (/Info). Section 14.3.3.
/// Provides basic metadata about the PDF document.
/// </summary>
internal class PdfInfo : PdfDictionary
{
    // Convenience properties mapping to standard /Info dictionary keys.

    public string? Title
    {
        get => (this[PdfName.Get("Title")] as PdfString)?.ToString(); // Simple ToString for debugging/retrieval might need better decoding
        set => AddOrRemove(PdfName.Get("Title"), value);
    }

    public string? Author
    {
        get => (this[PdfName.Get("Author")] as PdfString)?.ToString();
        set => AddOrRemove(PdfName.Get("Author"), value);
    }

    public string? Subject
    {
        get => (this[PdfName.Get("Subject")] as PdfString)?.ToString();
        set => AddOrRemove(PdfName.Get("Subject"), value);
    }

    public string? Keywords
    {
        get => (this[PdfName.Get("Keywords")] as PdfString)?.ToString();
        set => AddOrRemove(PdfName.Get("Keywords"), value);
    }

    public string? Creator
    {
        get => (this[PdfName.Get("Creator")] as PdfString)?.ToString();
        set => AddOrRemove(PdfName.Get("Creator"), value);
    }

    public string? Producer
    {
        get => (this[PdfName.Get("Producer")] as PdfString)?.ToString();
        set => AddOrRemove(PdfName.Get("Producer"), value);
    }

    public DateTimeOffset? CreationDate
    {
        get => GetDate(PdfName.Get("CreationDate"));
        set => AddOrRemoveDate(PdfName.Get("CreationDate"), value);
    }

    public DateTimeOffset? ModDate
    {
        get => GetDate(PdfName.Get("ModDate"));
        set => AddOrRemoveDate(PdfName.Get("ModDate"), value);
    }

    // Helper to add/remove string entries
    private void AddOrRemove(PdfName key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Remove(key);
        }
        else
        {
            // PDF spec recommends using UTF-16BE with BOM for broader compatibility in metadata
            Add(key, new PdfString(value, useUtf16: true));
        }
    }

    // Helper to add/remove date entries (PDF Date format: D:YYYYMMDDHHmmSSOHH'mm')
    private void AddOrRemoveDate(PdfName key, DateTimeOffset? value)
    {
        if (!value.HasValue)
        {
            Remove(key);
        }
        else
        {
            // Format date according to PDF specification Section 7.9.4
            string dateString = $"D:{value.Value:yyyyMMddHHmmss}{value.Value.Offset.Hours:+00;-00;+00}'{value.Value.Offset.Minutes:00}'";
            // Use PdfString constructor that defaults to PDFDocEncoding/Latin1 for this format
            Add(key, new PdfString(dateString, useUtf16: false));
        }
    }

    // Helper to parse date entries (basic implementation)
    private DateTimeOffset? GetDate(PdfName key)
    {
        if (this[key] is PdfString pdfDateStr)
        {
            string raw = pdfDateStr.ToString() ?? ""; // Use basic string representation
            if (raw.StartsWith("D:"))
            {
                // Basic parsing - a robust parser is needed for full compliance
                // Format: D:YYYYMMDDHHmmSSOHH'mm'
                try
                {
                    // Extract main part and offset
                    string mainPart = raw.Substring(2, 14); // YYYYMMDDHHmmSS
                    string offsetPart = raw.Length > 16 ? raw.Substring(16) : "Z"; // O HH'mm' or Z

                    int year = int.Parse(mainPart.Substring(0, 4));
                    int month = int.Parse(mainPart.Substring(4, 2));
                    int day = int.Parse(mainPart.Substring(6, 2));
                    int hour = int.Parse(mainPart.Substring(8, 2));
                    int minute = int.Parse(mainPart.Substring(10, 2));
                    int second = int.Parse(mainPart.Substring(12, 2));

                    TimeSpan offset = TimeSpan.Zero;
                    if (offsetPart != "Z" && offsetPart.Length >= 3)
                    {
                        char sign = offsetPart[0];
                        int offsetHours = int.Parse(offsetPart.Substring(1, 2));
                        int offsetMinutes = offsetPart.Length >= 5 ? int.Parse(offsetPart.Substring(4, 2)) : 0; // Skip '
                        offset = new TimeSpan(offsetHours, offsetMinutes, 0);
                        if (sign == '-') offset = -offset;
                    }

                    return new DateTimeOffset(year, month, day, hour, minute, second, offset);
                }
                catch { /* Parsing failed, return null */ }
            }
        }
        return null;
    }

    public PdfInfo() : base()
    {
        // Optionally set default Producer/Creator here
        Producer = $"MauiPdfGenerator Library"; // Example
                                                // Creator = "Your Application Name";
    }
}
