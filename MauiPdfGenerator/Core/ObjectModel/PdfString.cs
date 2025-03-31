using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Core.ObjectModel;

internal class PdfString : PdfObject
{
    private readonly string _value;
    private readonly bool _isHex; // Determines if written as <...> or (...)

    // Constructor for literal strings
    public PdfString(string value, bool preferHex = false)
    {
        _value = value ?? string.Empty;
        // Simple heuristic: use hex if non-ASCII or many special chars might exist
        _isHex = preferHex || (_value.Any(c => c > 127 || c == '(' || c == ')' || c == '\\'));
    }

    // Constructor for hex strings from bytes (e.g., binary data, dates)
    public PdfString(byte[] data)
    {
        _value = Convert.ToHexString(data); // Store as hex representation
        _isHex = true;
    }

    public override async Task WriteAsync(Stream stream, PdfWriter? writer = null)
    {
        if (_isHex)
        {
            byte[] hexBytes;
            // If _value is already hex string from byte[] constructor
            if (_value.All(c => "0123456789abcdefABCDEF".Contains(c)))
            {
                hexBytes = PdfEncodings.StructureEncoding.GetBytes($"<{_value}>");
            }
            else // If it was a literal string we decided to encode as hex
            {
                // TODO: Encode _value bytes as hex properly based on chosen encoding (e.g., UTF16BE or PDFDocEncoding)
                // Simple placeholder using ASCII bytes for now
                byte[] dataBytes = PdfEncodings.StructureEncoding.GetBytes(_value);
                hexBytes = PdfEncodings.StructureEncoding.GetBytes($"<{Convert.ToHexString(dataBytes)}>");
            }
            await WriteBytesAsync(stream, hexBytes);
        }
        else // Literal string
        {
            string escapedValue = PdfEncodings.EscapePdfLiteralString(_value);
            await WriteBytesAsync(stream, PdfEncodings.StructureEncoding.GetBytes($"({escapedValue})"));
        }
    }
}
