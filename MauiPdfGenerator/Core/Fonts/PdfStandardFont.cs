using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Common.Geometry;

namespace MauiPdfGenerator.Core.Fonts;

/// <summary>
/// Represents one of the 14 standard PDF Type 1 fonts.
/// These fonts are not embedded in the document.
/// </summary>
internal class PdfStandardFont : PdfFontBase
{
    public StandardFontType StandardType { get; }
    private PdfDictionary? _fontDictionary; // Cache the dictionary

    // Map enum to PDF BaseFont names
    private static readonly Dictionary<StandardFontType, string> BaseFontNames = new()
    {
        { StandardFontType.TimesRoman, "Times-Roman" },
        { StandardFontType.TimesBold, "Times-Bold" },
        { StandardFontType.TimesItalic, "Times-Italic" },
        { StandardFontType.TimesBoldItalic, "Times-BoldItalic" },
        { StandardFontType.Helvetica, "Helvetica" },
        { StandardFontType.HelveticaBold, "Helvetica-Bold" },
        { StandardFontType.HelveticaOblique, "Helvetica-Oblique" },
        { StandardFontType.HelveticaBoldOblique, "Helvetica-BoldOblique" },
        { StandardFontType.Courier, "Courier" },
        { StandardFontType.CourierBold, "Courier-Bold" },
        { StandardFontType.CourierOblique, "Courier-Oblique" },
        { StandardFontType.CourierBoldOblique, "Courier-BoldOblique" },
        { StandardFontType.Symbol, "Symbol" },
        { StandardFontType.ZapfDingbats, "ZapfDingbats" }
    };

    public PdfStandardFont(StandardFontType standardType)
    {
        StandardType = standardType;
    }

    internal override PdfObject GetPdfObject(PdfDocument document)
    {
        if (_fontDictionary is null)
        {
            _fontDictionary = new PdfDictionary
            {
                { PdfName.Type, PdfName.Font },
                { PdfName.Subtype, PdfName.Get("Type1") }, // Standard fonts are Type1
                { PdfName.Get("BaseFont"), PdfName.Get(BaseFontNames[StandardType]) }
            };
            // Standard fonts (except Symbol/ZapfDingbats) typically use WinAnsiEncoding or MacRomanEncoding
            // Specifying it can help ensure viewer compatibility.
            if (StandardType is not StandardFontType.Symbol && StandardType is not StandardFontType.ZapfDingbats)
            {
                _fontDictionary.Add(PdfName.Get("Encoding"), PdfName.Get("WinAnsiEncoding")); // Common default
            }
        }
        return _fontDictionary;
    }

    internal override byte[] EncodeText(string text)
    {
        if (StandardType is StandardFontType.Symbol || StandardType is StandardFontType.ZapfDingbats)
        {
            return System.Text.Encoding.ASCII.GetBytes(text);
        }

        // Usar Windows-1252 (WinAnsiEncoding) para caracteres especiales españoles
        return System.Text.Encoding.GetEncoding(1252).GetBytes(text);
    }

    internal override double GetTextWidth(string text, double fontSize)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        // --- APROXIMACIÓN MUY BÁSICA ---
        // Se NECESITAN métricas AFM reales para precisión.
        // Estos factores son estimaciones MUY generales.
        double averageCharWidthFactor = 0.55; // Factor promedio para fuentes proporcionales (Helvetica/Times)

        // Courier es monoespaciada, ~60% del tamaño de punto por carácter.
        if (StandardType is StandardFontType.Courier or StandardFontType.CourierBold or StandardFontType.CourierOblique or StandardFontType.CourierBoldOblique)
        {
            averageCharWidthFactor = 0.6;
        }
        // Símbolos pueden variar mucho, usamos un factor un poco mayor como estimación.
        else if (StandardType is StandardFontType.Symbol or StandardFontType.ZapfDingbats)
        {
            averageCharWidthFactor = 0.65;
        }

        // Calculamos el ancho multiplicando la longitud del texto por el tamaño y el factor.
        return text.Length * fontSize * averageCharWidthFactor;
    }
}
