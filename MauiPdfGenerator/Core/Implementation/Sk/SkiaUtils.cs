using MauiPdfGenerator.Fluent.Enums;
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal static class SkiaUtils
{
    public static async Task<SKTypeface?> CreateSkTypefaceAsync(
        string familyIdentifierAlias,
        FontAttributes fontAttributes,
        Func<string, Task<Stream?>> streamProvider,
        string? filePathToLoad)
    {
        ArgumentNullException.ThrowIfNull(streamProvider);

        SKTypeface? typefaceFromExplicitFile = null;
        string? actualFamilyNameFromExplicitFile = null;

        SKFontStyleWeight weightEnum = (fontAttributes & FontAttributes.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slantEnum = (fontAttributes & FontAttributes.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        var requestedStyle = new SKFontStyle(weightEnum, SKFontStyleWidth.Normal, slantEnum);
        int requestedWeightInt = (int)weightEnum;

        if (!string.IsNullOrEmpty(filePathToLoad))
        {
            try
            {
                // Para depurar el intento de carga desde un archivo específico.
                Debug.WriteLine($"[SkiaUtils] FontLoadAttempt: Trying file '{filePathToLoad}' for alias '{familyIdentifierAlias}', requested style: Weight={requestedWeightInt}, Slant={slantEnum}.");
                using Stream? fontStream = await streamProvider(filePathToLoad);
                if (fontStream is not null && fontStream.Length > 0)
                {
                    using var fontData = SKData.Create(fontStream);
                    if (fontData is not null)
                    {
                        typefaceFromExplicitFile = SKTypeface.FromData(fontData);
                        if (typefaceFromExplicitFile is not null)
                        {
                            actualFamilyNameFromExplicitFile = typefaceFromExplicitFile.FamilyName;
                            // Para confirmar la carga exitosa del archivo y sus propiedades.
                            Debug.WriteLine($"[SkiaUtils] FontLoadSuccess: File '{filePathToLoad}' loaded. Actual Family: '{actualFamilyNameFromExplicitFile}', File Weight: {typefaceFromExplicitFile.FontWeight}, File Slant: {typefaceFromExplicitFile.FontSlant}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Para registrar fallos al cargar desde el archivo.
                Debug.WriteLine($"[SkiaUtils] FontLoadError: File '{filePathToLoad}' failed to load: {ex.Message}");
                typefaceFromExplicitFile?.Dispose();
                typefaceFromExplicitFile = null;
            }
        }

        SKTypeface? finalTypeface = null;
        string baseFamilyNameToUseForSystemLookup = actualFamilyNameFromExplicitFile ?? familyIdentifierAlias;

        if (string.IsNullOrEmpty(baseFamilyNameToUseForSystemLookup))
        {
            // Para indicar que no hay nombre de fuente para buscar y se usará el default.
            Debug.WriteLine($"[SkiaUtils] FontLookup: No family name for lookup (alias or from file). Using SKTypeface.Default.");
            typefaceFromExplicitFile?.Dispose();
            return SKTypeface.Default;
        }

        if (typefaceFromExplicitFile != null)
        {
            // CASO ESPECIAL: Si el usuario NO especificó FontAttributes (o eran None),
            // Y tenemos un archivo explícito cargado, DEBEMOS USAR ESE ARCHIVO TAL CUAL.
            if (fontAttributes == FontAttributes.None)
            {
                // Para indicar que se usa el archivo explícito porque no se pidieron atributos.
                Debug.WriteLine($"[SkiaUtils] FontSelection: Using typeface from explicit file '{filePathToLoad}' because no explicit FontAttributes were requested (attributes are None).");
                finalTypeface = typefaceFromExplicitFile;
                typefaceFromExplicitFile = null;
            }
            else // El usuario SÍ pidió FontAttributes específicos
            {
                bool styleMatches = typefaceFromExplicitFile.FontWeight == requestedWeightInt && typefaceFromExplicitFile.FontSlant == slantEnum;
                if (styleMatches)
                {
                    // Para indicar que el archivo cargado ya coincide con el estilo solicitado.
                    Debug.WriteLine($"[SkiaUtils] FontSelection: Using typeface from file '{filePathToLoad}' as its style matches requested style.");
                    finalTypeface = typefaceFromExplicitFile;
                    typefaceFromExplicitFile = null;
                }
                else
                {
                    // Para indicar que el archivo cargado no coincide y se intentará una búsqueda del sistema.
                    Debug.WriteLine($"[SkiaUtils] FontSelection: File '{filePathToLoad}' (Family: '{actualFamilyNameFromExplicitFile}') loaded, but its style (W:{typefaceFromExplicitFile.FontWeight},S:{typefaceFromExplicitFile.FontSlant}) " +
                                    $"does not match requested style (W:{requestedWeightInt},S:{slantEnum}). Will attempt system lookup for family '{baseFamilyNameToUseForSystemLookup}'.");
                    // No se asigna finalTypeface aquí, se deja para la lógica de búsqueda del sistema.
                }
            }
        }

        if (finalTypeface is null)
        {
            // Para rastrear el intento de búsqueda en el sistema con el nombre y estilo.
            Debug.WriteLine($"[SkiaUtils] FontLookup: Attempting system font '{baseFamilyNameToUseForSystemLookup}' with style (W:{requestedWeightInt}, S:{slantEnum}).");
            finalTypeface = SKTypeface.FromFamilyName(baseFamilyNameToUseForSystemLookup, requestedStyle);

            if (finalTypeface is not null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && baseFamilyNameToUseForSystemLookup != SKTypeface.Default.FamilyName)
            {
                // Para indicar que la búsqueda con estilo resultó en la fuente default del sistema.
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font '{baseFamilyNameToUseForSystemLookup}' with style resolved to SKTypeface.Default. Discarding this result.");
                finalTypeface.Dispose();
                finalTypeface = null;
            }
            else if (finalTypeface is not null)
            {
                // Para confirmar una búsqueda exitosa en el sistema con estilo.
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font '{baseFamilyNameToUseForSystemLookup}' with style FOUND: '{finalTypeface.FamilyName}', W:{finalTypeface.FontWeight}, S:{finalTypeface.FontSlant}.");
            }
        }

        if (finalTypeface is null && typefaceFromExplicitFile is not null)
        {
            // Para indicar que, tras fallar la búsqueda con estilo, se recurre al archivo explícito aunque no coincida perfectamente.
            // Esto es importante si el filePathToLoad era para una fuente específica (ej. ComicBold.ttf)
            // y la búsqueda con estilo (que sería redundante en este caso si fontAttributes era None, pero se hizo porque no entró en el primer if)
            // falló en encontrar algo mejor.
            Debug.WriteLine($"[SkiaUtils] FontSelection: System lookup for styled font failed OR explicit font was preferred. Using typeface from explicit file '{filePathToLoad}' as a fallback or primary choice.");
            finalTypeface = typefaceFromExplicitFile;
            typefaceFromExplicitFile = null;
        }

        if (finalTypeface is null && familyIdentifierAlias != baseFamilyNameToUseForSystemLookup && !string.IsNullOrEmpty(familyIdentifierAlias))
        {
            // Para rastrear un intento de búsqueda adicional usando el alias original si es diferente y el lookup anterior falló.
            Debug.WriteLine($"[SkiaUtils] FontLookup: Attempting system font for original ALIAS '{familyIdentifierAlias}' with style (W:{requestedWeightInt}, S:{slantEnum}).");
            finalTypeface = SKTypeface.FromFamilyName(familyIdentifierAlias, requestedStyle);
            if (finalTypeface != null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && familyIdentifierAlias != SKTypeface.Default.FamilyName)
            {
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font ALIAS '{familyIdentifierAlias}' with style resolved to SKTypeface.Default. Discarding.");
                finalTypeface.Dispose();
                finalTypeface = null;
            }
            else if (finalTypeface != null)
            {
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font ALIAS '{familyIdentifierAlias}' with style FOUND: '{finalTypeface.FamilyName}', W:{finalTypeface.FontWeight}, S:{finalTypeface.FontSlant}.");
            }
        }

        if (finalTypeface is null)
        {
            // Para indicar que todos los intentos con estilo fallaron y se intentará sin estilo.
            Debug.WriteLine($"[SkiaUtils] FontLookup: Styled lookup failed. Attempting system font '{baseFamilyNameToUseForSystemLookup}' WITHOUT style.");
            finalTypeface = SKTypeface.FromFamilyName(baseFamilyNameToUseForSystemLookup);
            if (finalTypeface != null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && baseFamilyNameToUseForSystemLookup != SKTypeface.Default.FamilyName)
            {
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font '{baseFamilyNameToUseForSystemLookup}' (no style) resolved to SKTypeface.Default. Discarding.");
                finalTypeface.Dispose();
                finalTypeface = null;
            }
            else if (finalTypeface is not null)
            {
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font '{baseFamilyNameToUseForSystemLookup}' (no style) FOUND: '{finalTypeface.FamilyName}', W:{finalTypeface.FontWeight}, S:{finalTypeface.FontSlant}.");
            }
        }

        if (finalTypeface is null && familyIdentifierAlias != baseFamilyNameToUseForSystemLookup && !string.IsNullOrEmpty(familyIdentifierAlias))
        {
            Debug.WriteLine($"[SkiaUtils] FontLookup: Still no typeface. Attempting system font for ALIAS '{familyIdentifierAlias}' WITHOUT style.");
            finalTypeface = SKTypeface.FromFamilyName(familyIdentifierAlias);
            if (finalTypeface is not null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && familyIdentifierAlias != SKTypeface.Default.FamilyName)
            {
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font ALIAS '{familyIdentifierAlias}' (no style) resolved to SKTypeface.Default. Discarding.");
                finalTypeface.Dispose();
                finalTypeface = null;
            }
            else if (finalTypeface is not null)
            {
                Debug.WriteLine($"[SkiaUtils] FontLookup: System font ALIAS '{familyIdentifierAlias}' (no style) FOUND: '{finalTypeface.FamilyName}', W:{finalTypeface.FontWeight}, S:{finalTypeface.FontSlant}.");
            }
        }

        if (finalTypeface is null)
        {
            // Para registrar que todos los intentos fallaron y se usa el default.
            Debug.WriteLine($"[SkiaUtils] FontLookup: All lookups FAILED for alias '{familyIdentifierAlias}' with attributes '{fontAttributes}'. Using SKTypeface.Default.");
            finalTypeface = SKTypeface.Default;
        }

        typefaceFromExplicitFile?.Dispose(); // Asegura que el archivo cargado inicialmente se libere si no fue el finalTypeface.

        // Para mostrar la decisión final de SKTypeface.
        Debug.WriteLine($"[SkiaUtils] CreateSkTypefaceAsync FINAL RESULT for Alias='{familyIdentifierAlias}', Attrs='{fontAttributes}', Path='{filePathToLoad ?? "N/A"}': " +
                        $"SKTypeface Family='{finalTypeface.FamilyName}', Weight={finalTypeface.FontWeight}, Slant={finalTypeface.FontSlant}, IsBold={finalTypeface.IsBold}, IsItalic={finalTypeface.IsItalic}.");
        return finalTypeface;
    }

    internal static string? GetMauiFontFilePath(string fontAlias)
    {
        // Para depurar la obtención de rutas de fuentes de MAUI (actualmente no implementado).
        Debug.WriteLine($"[SkiaUtils.GetMauiFontFilePath] Functionality for alias '{fontAlias}' is currently not available.");
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
            case PageSizeType.A5: width = 419.53f; height = 595.28f; break;
            case PageSizeType.Letter: width = 612f; height = 792f; break;
            case PageSizeType.Legal: width = 612f; height = 1008f; break;
            case PageSizeType.Executive: width = 521.86f; height = 756.00f; break;
            case PageSizeType.B5: width = 498.89f; height = 708.66f; break;
            case PageSizeType.Tabloid: width = 792f; height = 1224f; break;
            case PageSizeType.Envelope_10: width = 297f; height = 684f; break;
            case PageSizeType.Envelope_DL: width = 311.81f; height = 623.62f; break;
            default:
                width = 595.28f; height = 841.89f; break;
        }
        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }
}
