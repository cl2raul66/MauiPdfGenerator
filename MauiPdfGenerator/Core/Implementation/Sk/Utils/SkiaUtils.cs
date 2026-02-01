using MauiPdfGenerator.Fluent.Enums;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Utils;

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
                        }
                    }
                }
            }
            catch (Exception)
            {
                typefaceFromExplicitFile?.Dispose();
                typefaceFromExplicitFile = null;
            }
        }
        SKTypeface? finalTypeface = null;
        string baseFamilyNameToUseForSystemLookup = actualFamilyNameFromExplicitFile ?? familyIdentifierAlias;
        if (string.IsNullOrEmpty(baseFamilyNameToUseForSystemLookup))
        {
            typefaceFromExplicitFile?.Dispose();
            return SKTypeface.Default;
        }
        if (typefaceFromExplicitFile is not null)
        {
            if (fontAttributes == FontAttributes.None)
            {
                finalTypeface = typefaceFromExplicitFile;
                typefaceFromExplicitFile = null;
            }
            else
            {
                bool styleMatches = typefaceFromExplicitFile.FontWeight == requestedWeightInt && typefaceFromExplicitFile.FontSlant == slantEnum;
                if (styleMatches)
                {
                    finalTypeface = typefaceFromExplicitFile;
                    typefaceFromExplicitFile = null;
                }
            }
        }
        if (finalTypeface is null)
        {
            finalTypeface = SKTypeface.FromFamilyName(baseFamilyNameToUseForSystemLookup, requestedStyle);
            if (finalTypeface is not null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && baseFamilyNameToUseForSystemLookup != SKTypeface.Default.FamilyName)
            {
                finalTypeface.Dispose();
                finalTypeface = null;
            }
        }
        if (finalTypeface is null && typefaceFromExplicitFile is not null)
        {
            finalTypeface = typefaceFromExplicitFile;
            typefaceFromExplicitFile = null;
        }
        if (finalTypeface is null && familyIdentifierAlias != baseFamilyNameToUseForSystemLookup && !string.IsNullOrEmpty(familyIdentifierAlias))
        {
            finalTypeface = SKTypeface.FromFamilyName(familyIdentifierAlias, requestedStyle);
            if (finalTypeface is not null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && familyIdentifierAlias != SKTypeface.Default.FamilyName)
            {
                finalTypeface.Dispose();
                finalTypeface = null;
            }
        }
        if (finalTypeface is null)
        {
            finalTypeface = SKTypeface.FromFamilyName(baseFamilyNameToUseForSystemLookup);
            if (finalTypeface is not null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && baseFamilyNameToUseForSystemLookup != SKTypeface.Default.FamilyName)
            {
                finalTypeface.Dispose();
                finalTypeface = null;
            }
        }
        if (finalTypeface is null && familyIdentifierAlias != baseFamilyNameToUseForSystemLookup && !string.IsNullOrEmpty(familyIdentifierAlias))
        {
            finalTypeface = SKTypeface.FromFamilyName(familyIdentifierAlias);
            if (finalTypeface is not null && finalTypeface.FamilyName == SKTypeface.Default.FamilyName && familyIdentifierAlias != SKTypeface.Default.FamilyName)
            {
                finalTypeface.Dispose();
                finalTypeface = null;
            }
        }
        if (finalTypeface is null)
        {
            finalTypeface = SKTypeface.Default;
        }
        typefaceFromExplicitFile?.Dispose();
        return finalTypeface;
    }
    internal static string? GetMauiFontFilePath(string fontAlias)
    {
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
