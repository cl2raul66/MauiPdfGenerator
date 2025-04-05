using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure;

namespace MauiPdfGenerator.Core.Fonts
{
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

        // TODO: Implement encoding/width logic for standard fonts (needs AFM metrics usually)
    }
}
