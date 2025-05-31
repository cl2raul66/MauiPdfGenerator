using MauiPdfGenerator.Fluent.Enums;
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal static class SkiaUtils
{
    public static async Task<SKTypeface?> CreateSkTypefaceAsync(
        string familyIdentifierAlias, // Este es el alias que viene del usuario/configuración
        FontAttributes fontAttributes,
        Func<string, Task<Stream?>> streamProvider,
        string? filePathIfEmbedding) // Este es el FilePath del FontRegistration
    {
        ArgumentNullException.ThrowIfNull(streamProvider);

        SKTypeface? typefaceFromFile = null;
        string? actualFamilyNameFromFile = null;

        // Paso 1: Intentar cargar el typeface desde el archivo (si filePathIfEmbedding existe)
        // Esto sirve para dos propósitos:
        //   a) Si se va a incrustar, este es el typeface que se usará.
        //   b) Si no se va a incrustar (o falla la incrustación pero el archivo existe),
        //      podemos obtener el FamilyName real del archivo para usarlo con SKTypeface.FromFamilyName.
        if (!string.IsNullOrEmpty(filePathIfEmbedding))
        {
            try
            {
                Debug.WriteLine($"[SkiaUtils] Attempting to load font from file: {filePathIfEmbedding}");
                using Stream? fontStream = await streamProvider(filePathIfEmbedding);
                if (fontStream is not null && fontStream.Length > 0)
                {
                    // Necesitamos una copia del stream si SKData.Create no lo hace,
                    // o si el stream original se cierra prematuramente.
                    // SKData.Create(Stream) hace una copia interna si el stream no es un MemoryStream.
                    using var fontData = SKData.Create(fontStream);
                    if (fontData is not null)
                    {
                        typefaceFromFile = SKTypeface.FromData(fontData);
                        if (typefaceFromFile is not null)
                        {
                            actualFamilyNameFromFile = typefaceFromFile.FamilyName;
                            Debug.WriteLine($"[SkiaUtils] Successfully loaded typeface from file '{filePathIfEmbedding}'. Actual FamilyName: '{actualFamilyNameFromFile}'. Will be used for embedding if requested.");
                            // Si se solicita incrustar, ya tenemos el typeface correcto.
                            // No es necesario hacer más nada, simplemente retornamos este typeface.
                            // La decisión de si se usa para incrustar o solo para obtener el nombre
                            // se toma basándose en si filePathIfEmbedding fue pasado para incrustar (implícito por su nombre)
                            // o solo como una ruta conocida. El parámetro `filePathIfEmbedding` indica que se *debería* usar
                            // este archivo para la incrustación si es posible.
                            return typefaceFromFile;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SkiaUtils] Failed to load font from file '{filePathIfEmbedding}': {ex.Message}. Will attempt system font reference.");
                typefaceFromFile?.Dispose(); // Asegurarse de liberar si se creó parcialmente
                typefaceFromFile = null;
            }
        }

        // Paso 2: Determinar el nombre a usar para SKTypeface.FromFamilyName
        // Prioridad:
        //   1. Nombre real obtenido del archivo (actualFamilyNameFromFile), si se pudo cargar.
        //   2. El alias original (familyIdentifierAlias) si no se pudo obtener del archivo.
        //   3. Si familyIdentifierAlias es nulo o vacío, usar Default.

        string nameToUseForSkiaLookup;
        if (!string.IsNullOrEmpty(actualFamilyNameFromFile))
        {
            nameToUseForSkiaLookup = actualFamilyNameFromFile;
            Debug.WriteLine($"[SkiaUtils] Using actual family name from file ('{actualFamilyNameFromFile}') for system font lookup.");
        }
        else if (!string.IsNullOrEmpty(familyIdentifierAlias))
        {
            nameToUseForSkiaLookup = familyIdentifierAlias;
            Debug.WriteLine($"[SkiaUtils] Using provided alias ('{familyIdentifierAlias}') for system font lookup (file not available or actual name not read).");
        }
        else
        {
            Debug.WriteLine("[SkiaUtils] familyIdentifierAlias is null or empty, and no file-based name available. Using system default typeface.");
            return SKTypeface.Default;
        }

        // Paso 3: Intentar obtener el typeface del sistema usando el nombre determinado.
        SKFontStyleWeight weight = (fontAttributes & FontAttributes.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = (fontAttributes & FontAttributes.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        var style = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);

        Debug.WriteLine($"[SkiaUtils] Attempting SKTypeface.FromFamilyName for '{nameToUseForSkiaLookup}' with style {style}.");
        SKTypeface? systemTypeface = SKTypeface.FromFamilyName(nameToUseForSkiaLookup, style);

        if (systemTypeface is null || (systemTypeface.FamilyName == SKTypeface.Default.FamilyName && nameToUseForSkiaLookup != SKTypeface.Default.FamilyName))
        {
            Debug.WriteLine($"[SkiaUtils] Specific style ({style}) not found or resolved to default for font '{nameToUseForSkiaLookup}'. Trying family default without style.");
            systemTypeface?.Dispose(); // Liberar el anterior si se obtuvo uno por defecto
            systemTypeface = SKTypeface.FromFamilyName(nameToUseForSkiaLookup);
        }

        if (systemTypeface is null || (systemTypeface.FamilyName == SKTypeface.Default.FamilyName && nameToUseForSkiaLookup != SKTypeface.Default.FamilyName))
        {
            Debug.WriteLine($"[SkiaUtils] Font family '{nameToUseForSkiaLookup}' not found on system by Skia. Using system default typeface. PDF will reference '{familyIdentifierAlias}'.");
            systemTypeface?.Dispose(); // Liberar el anterior si se obtuvo uno por defecto
            systemTypeface = SKTypeface.Default;
        }

        // Si llegamos aquí, significa que no se incrustó la fuente desde el archivo (o no había archivo para incrustar).
        // Devolvemos el typeface del sistema que se encontró (o el Default).
        // El PDF referenciará 'nameToUseForSkiaLookup' o, si se usó Default, el visor decidirá.
        // Es importante que si systemTypeface es el Default, el PDF aún podría intentar referenciar 'nameToUseForSkiaLookup',
        // y el visor haría la sustitución. SkiaSharp podría no tener una forma de "forzar" la referencia a un nombre
        // si solo puede resolver SKTypeface.Default. La metadata del PDF es la que lleva el nombre.
        Debug.WriteLine($"[SkiaUtils] Final SKTypeface for PDF font (referenced): Effective Skia FamilyName='{systemTypeface.FamilyName}', Weight={systemTypeface.FontWeight}, Slant={systemTypeface.FontSlant}. Original alias was '{familyIdentifierAlias}'.");
        return systemTypeface;
    }

    internal static string? GetMauiFontFilePath(string fontAlias)
    {
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
            // Tamaños comunes en puntos PostScript (72 DPI)
            case PageSizeType.A3: width = 841.89f; height = 1190.55f; break; // 297 x 420 mm
            case PageSizeType.A4: width = 595.28f; height = 841.89f; break; // 210 x 297 mm
            case PageSizeType.A5: width = 419.53f; height = 595.28f; break; // 148 x 210 mm
            case PageSizeType.Letter: width = 612f; height = 792f; break;    // 8.5 x 11 inches
            case PageSizeType.Legal: width = 612f; height = 1008f; break;   // 8.5 x 14 inches
            case PageSizeType.Executive: width = 521.86f; height = 756.00f; break; // 7.25 x 10.5 inches
            case PageSizeType.B5: width = 498.89f; height = 708.66f; break; // 176 x 250 mm (ISO B5)
            case PageSizeType.Tabloid: width = 792f; height = 1224f; break; // 11 x 17 inches
            // Sobres - estos son aproximados y pueden variar.
            case PageSizeType.Envelope_10: width = 297f; height = 684f; break; // #10 Envelope (4.125 x 9.5 inches)
            case PageSizeType.Envelope_DL: width = 311.81f; height = 623.62f; break; // DL Envelope (110 x 220 mm)
            default: // A4 por defecto
                width = 595.28f; height = 841.89f; break;
        }
        return orientation == PageOrientationType.Landscape ? new SKSize(height, width) : new SKSize(width, height);
    }
}
