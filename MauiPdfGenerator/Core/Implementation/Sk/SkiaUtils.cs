using MauiPdfGenerator.Fluent.Enums;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal static class SkiaUtils
{
    /// <summary>
    /// Creates an SKTypeface based on family name and MAUI FontAttributes.
    /// Attempts to find the specific style, falls back to family default, then system default.
    /// </summary>
    public static SKTypeface? CreateSkTypeface(string fontFamily, FontAttributes fontAttributes)
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            System.Diagnostics.Debug.WriteLine("Warning: CreateSkTypeface called with null or empty font family. Using default typeface.");
            return SKTypeface.Default;
        }

        SKFontStyleWeight weight = (fontAttributes & FontAttributes.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = (fontAttributes & FontAttributes.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        var style = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

        // Attempt 1: Get specific style
        SKTypeface? typeface = SKTypeface.FromFamilyName(fontFamily, style);

        // Attempt 2: Fallback to family name default style if specific style failed
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Debug: Specific style ({style}) not found for font '{fontFamily}'. Trying family default.");
            typeface = SKTypeface.FromFamilyName(fontFamily);
        }

        // Attempt 3: Fallback to system default if family name itself wasn't found
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Font family '{fontFamily}' not found. Using system default typeface.");
            typeface = SKTypeface.Default; // SKTypeface.Default should generally always return something
        }

        // Final check if even SKTypeface.Default failed (highly unlikely)
        if (typeface is null)
        {
            System.Diagnostics.Debug.WriteLine($"Critical Warning: Failed to obtain any valid typeface, including system default.");
        }

        return typeface;
    }

    /// <summary>
    /// Converts a MAUI Color to an SKColor. Defaults to Black if null.
    /// </summary>
    public static SKColor ConvertToSkColor(Color? mauiColor)
    {
        if (mauiColor is null) return SKColors.Black;

        // Clamp values just in case they are outside the 0-1 range
        float red = Math.Clamp(mauiColor.Red, 0f, 1f);
        float green = Math.Clamp(mauiColor.Green, 0f, 1f);
        float blue = Math.Clamp(mauiColor.Blue, 0f, 1f);
        float alpha = Math.Clamp(mauiColor.Alpha, 0f, 1f);

        return new SKColor(
            (byte)(red * 255),
            (byte)(green * 255),
            (byte)(blue * 255),
            (byte)(alpha * 255)
        );
    }

    /// <summary>
    /// Gets the SKSize corresponding to a PageSizeType and orientation.
    /// Dimensions are in points (1/72 inch).
    /// </summary>
    public static SKSize GetSkPageSize(PageSizeType size, PageOrientationType orientation)
    {
        float width, height;
        switch (size)
        {
            // ISO Sizes (Points)
            case PageSizeType.A3: width = 841.89f; height = 1190.55f; break; // 297 x 420 mm
            case PageSizeType.A4: width = 595.28f; height = 841.89f; break;  // 210 x 297 mm
            case PageSizeType.A5: width = 419.53f; height = 595.28f; break;  // 148 x 210 mm
            case PageSizeType.B5: width = 498.90f; height = 708.66f; break;  // 176 x 250 mm (ISO B5)

            // North American Sizes (Points)
            case PageSizeType.Letter: width = 612f; height = 792f; break;    // 8.5 x 11 in
            case PageSizeType.Legal: width = 612f; height = 1008f; break;   // 8.5 x 14 in
            case PageSizeType.Tabloid: width = 792f; height = 1224f; break;  // 11 x 17 in
            case PageSizeType.Executive: width = 522f; height = 756f; break; // 7.25 x 10.5 in

            // Envelopes (Width/Height based on Portrait orientation)
            case PageSizeType.Envelope_10: width = 297f; height = 684f; break;  // 4.125 x 9.5 in
            case PageSizeType.Envelope_DL: width = 311.81f; height = 623.62f; break; // 110 x 220 mm

            default:
                System.Diagnostics.Debug.WriteLine($"Warning: Unsupported PageSizeType '{size}'. Defaulting to A4.");
                width = 595.28f; height = 841.89f; // Default to A4
                break;
        }

        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }
}
