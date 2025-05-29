using MauiPdfGenerator.Fluent.Enums;
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal static class SkiaUtils
{
    public static async Task<SKTypeface?> CreateSkTypefaceAsync(
        string familyIdentifier,
        FontAttributes fontAttributes,
        Func<string, Task<Stream?>> streamProvider,
        string? filePathIfEmbedding)
    {
        ArgumentNullException.ThrowIfNull(streamProvider);

        SKTypeface? typeface = null;

        if (!string.IsNullOrEmpty(filePathIfEmbedding))
        {
            try
            {
                Debug.WriteLine($"[SkiaUtils] Attempting to load font for embedding from: {filePathIfEmbedding}");
                using Stream? fontStream = await streamProvider(filePathIfEmbedding);
                if (fontStream is not null && fontStream.Length > 0)
                {
                    using var fontData = SKData.Create(fontStream);
                    if (fontData is not null)
                    {
                        typeface = SKTypeface.FromData(fontData);
                        if (typeface is not null)
                        {
                            Debug.WriteLine($"[SkiaUtils] Successfully EMBEDDED typeface '{typeface.FamilyName}' from '{filePathIfEmbedding}'.");
                            return typeface;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SkiaUtils] Failed to embed font from '{filePathIfEmbedding}': {ex.Message}. Will attempt reference.");
            }
        }

        if (string.IsNullOrEmpty(familyIdentifier))
        {
            Debug.WriteLine("[SkiaUtils] familyIdentifier is null or empty. Using system default typeface.");
            return SKTypeface.Default;
        }

        SKFontStyleWeight weight = (fontAttributes & FontAttributes.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = (fontAttributes & FontAttributes.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        var style = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        string nameToUseForSkia = familyIdentifier;

        Debug.WriteLine($"[SkiaUtils] Attempting SKTypeface.FromFamilyName for '{nameToUseForSkia}' with style {style}.");
        typeface = SKTypeface.FromFamilyName(nameToUseForSkia, style);

        if (typeface is null)
        {
            Debug.WriteLine($"[SkiaUtils] Specific style ({style}) not found for font '{nameToUseForSkia}'. Trying family default.");
            typeface = SKTypeface.FromFamilyName(nameToUseForSkia);
        }

        if (typeface is null)
        {
            Debug.WriteLine($"[SkiaUtils] Font family '{nameToUseForSkia}' not found on system by Skia. Using system default typeface. PDF will reference '{familyIdentifier}'.");
            typeface = SKTypeface.Default;
        }

        Debug.WriteLine($"[SkiaUtils] Final SKTypeface for PDF font '{familyIdentifier}': Effective Skia FamilyName='{typeface.FamilyName}', Weight={typeface.FontWeight}, Slant={typeface.FontSlant}");
        return typeface;
    }

    internal static string? GetMauiFontFilePath(string fontAlias)
    {
        // Placeholder - Original logic using MauiFontRegistryHelper removed
        Debug.WriteLine($"[SkiaUtils.GetMauiFontFilePath] Functionality requiring MauiFontRegistryHelper for alias '{fontAlias}' is currently not available.");
        return null;
    }

    public static SKColor ConvertToSkColor(Color? mauiColor)
    {
        if (mauiColor is null) return SKColors.Black;
        float r = Math.Clamp(mauiColor.Red, 0f, 1f);
        float g = Math.Clamp(mauiColor.Green, 0f, 1f);
        float b = Math.Clamp(mauiColor.Blue, 0f, 1f);
        float a = Math.Clamp(mauiColor.Alpha, 0f, 1f);
        return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
    }

    public static SKSize GetSkPageSize(PageSizeType size, PageOrientationType orientation)
    {
        float width, height;
        switch (size)
        {
            case PageSizeType.A3: width = 841.89f; height = 1190.55f; break;
            case PageSizeType.A4: width = 595.28f; height = 841.89f; break;
            default:
                width = 595.28f; height = 841.89f; break; // A4
        }
        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }
}
