// MauiPdfGenerator/Core/Utils/FontManager.cs
using SkiaSharp;
using System;
using System.Collections.Generic;
using MauiPdfGenerator.Common.Primitives; // For PdfFont, PdfFontStyle

namespace MauiPdfGenerator.Core.Utils;

/// <summary>
/// Manages loading and caching of SKTypeface objects based on PdfFont definitions.
/// Ensures that font resources are handled efficiently.
/// </summary>
internal class PdfFontManager : IDisposable
{
    private readonly Dictionary<(string Name, PdfFontStyle Style), SKTypeface> _typefaceCache = new();
    private bool _disposedValue;

    /// <summary>
    /// Retrieves or creates an SKTypeface corresponding to the requested PdfFont.
    /// Attempts to find the best match in the system or registered custom fonts.
    /// </summary>
    /// <param name="font">The font definition from the Common model.</param>
    /// <returns>An SKTypeface instance, potentially from cache, or a default fallback.</returns>
    public SKTypeface GetTypeface(PdfFont font)
    {
        var cacheKey = (font.Name, font.Style);

        if (_typefaceCache.TryGetValue(cacheKey, out var cachedTypeface))
        {
            return cachedTypeface;
        }

        // Translate PdfFontStyle to SKFontStyleWeight and SKFontStyleSlant
        SKFontStyleWeight weight = (font.Style & PdfFontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = (font.Style & PdfFontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        // SKFontStyleWidth could be added if needed (Condensed, Expanded) - PdfFontStyle doesn't cover it.
        var skStyle = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

        // Try to get the specific style first
        SKTypeface typeface = SKTypeface.FromFamilyName(font.Name, skStyle);

        // Fallback if the specific style wasn't found directly (Skia might synthesize it)
        if (typeface == null || typeface.FamilyName != font.Name) // Check FamilyName as FromFamilyName might return a default/fallback
        {
            // Try creating default style and let Skia attempt synthesis if needed (or maybe it already did)
            typeface = SKTypeface.FromFamilyName(font.Name, SKFontStyle.Normal); // Try base font first

            // If still not found, or base font found doesn't match, try default system font
            if (typeface == null || (typeface.FamilyName != font.Name && font.Name != SKTypeface.Default.FamilyName))
            {
                Console.WriteLine($"Warning: Font '{font.Name}' not found. Falling back to default font.");
                typeface = SKTypeface.Default; // Use system default
            }

            // If we got *a* typeface (even default), create the specific style from it if possible
            // SkiaSharp often handles synthesizing bold/italic if the base font is found.
            typeface = typeface = SKTypeface.Default ?? SKTypeface.FromFamilyName(null);
        }

        // Add to cache - even if it's a fallback, cache the result for this request
        _typefaceCache[cacheKey] = typeface;


        // TODO: Implement mechanism for registering custom fonts from files/streams
        // Example:
        // SKTypeface custom = SKTypeface.FromFile("path/to/myfont.ttf");
        // _typefaceCache.Add(("MyCustomFont", PdfFontStyle.Normal), custom);

        return typeface;
    }

    // --- IDisposable Implementation ---

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects).
                // SKTypeface instances retrieved from SKTypeface.FromFamilyName/Default
                // are typically managed by SkiaSharp's internal cache and might not need explicit disposal here.
                // However, if YOU loaded from File/Stream, you WOULD need to dispose them.
                foreach (var typeface in _typefaceCache.Values)
                {
                    // Only dispose if *you* created it from a stream/file that needs closing.
                    // Typefaces from FromFamilyName or Default are generally cached/shared by Skia.
                    // typeface.Dispose(); // Be careful with this - usually not needed for system fonts.
                }
                _typefaceCache.Clear();
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // Set large fields to null
            _disposedValue = true;
        }
    }

    // // Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FontManager()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
