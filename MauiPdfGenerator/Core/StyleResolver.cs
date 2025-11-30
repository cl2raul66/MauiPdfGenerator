using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Common.Models.Styling;

namespace MauiPdfGenerator.Core;

internal class StyleResolver
{
    private readonly PdfResourceDictionary _resourceDictionary;

    public StyleResolver(PdfResourceDictionary resourceDictionary)
    {
        _resourceDictionary = resourceDictionary;
    }

    public void ApplyStyles(List<PdfElementData> elements)
    {
        foreach (var element in elements)
        {
            if (element.StyleKey is null) continue;

            var setter = _resourceDictionary.GetCombinedSetter(element.StyleKey);
            if (setter is null)
            {
                // In a real library, this should probably log to the DiagnosticsSink
                Console.WriteLine($"WARNING: Style with key '{element.StyleKey}' not found.");
                continue;
            }

            // Create a temporary instance to hold the styled properties
            var styledInstance = (PdfElementData)Activator.CreateInstance(element.GetType());
            setter(styledInstance);

            // Merge properties, respecting precedence (Local > Style)
            MergeProperties(element, styledInstance);
        }
    }

    private void MergeProperties(PdfElementData target, PdfElementData styledSource)
    {
        // --- Merge properties from the base PdfElementData ---
        if (target.GetBackgroundColor is null && styledSource.GetBackgroundColor is not null)
        {
            target.BackgroundColor(styledSource.GetBackgroundColor);
        }
        // Margin, Padding, WidthRequest, HeightRequest, etc. could be added here if needed.
        // For now, we focus on the most common style properties.

        // --- Merge properties for specific element types ---
        if (target is PdfParagraphData targetParagraph && styledSource is PdfParagraphData styledParagraph)
        {
            MergeParagraphProperties(targetParagraph, styledParagraph);
        }
        else if (target is PdfImageData targetImage && styledSource is PdfImageData styledImage)
        {
            MergeImageProperties(targetImage, styledImage);
        }
        else if (target is PdfHorizontalLineData targetLine && styledSource is PdfHorizontalLineData styledLine)
        {
            MergeHorizontalLineProperties(targetLine, styledLine);
        }
        // Add other element types here (Grid, StackLayouts, etc.)
    }

    private void MergeParagraphProperties(PdfParagraphData target, PdfParagraphData styledSource)
    {
        if (target.CurrentFontFamily is null && styledSource.CurrentFontFamily is not null)
        {
            target.CurrentFontFamily = styledSource.CurrentFontFamily;
            target.ResolvedFontRegistration = styledSource.ResolvedFontRegistration;
        }
        if (target.CurrentFontSize == 0 && styledSource.CurrentFontSize != 0)
        {
            target.CurrentFontSize = styledSource.CurrentFontSize;
        }
        if (target.CurrentTextColor is null && styledSource.CurrentTextColor is not null)
        {
            target.CurrentTextColor = styledSource.CurrentTextColor;
        }
        if (target.CurrentFontAttributes is null && styledSource.CurrentFontAttributes is not null)
        {
            target.CurrentFontAttributes = styledSource.CurrentFontAttributes;
        }
        if (target.CurrentHorizontalTextAlignment == PdfParagraphData.DefaultHorizontalTextAlignment && styledSource.CurrentHorizontalTextAlignment != PdfParagraphData.DefaultHorizontalTextAlignment)
        {
            target.CurrentHorizontalTextAlignment = styledSource.CurrentHorizontalTextAlignment;
        }
        if (target.CurrentVerticalTextAlignment == PdfParagraphData.DefaultVerticalTextAlignment && styledSource.CurrentVerticalTextAlignment != PdfParagraphData.DefaultVerticalTextAlignment)
        {
            target.CurrentVerticalTextAlignment = styledSource.CurrentVerticalTextAlignment;
        }
        if (target.CurrentLineBreakMode is null && styledSource.CurrentLineBreakMode is not null)
        {
            target.CurrentLineBreakMode = styledSource.CurrentLineBreakMode;
        }
        if (target.CurrentTextDecorations is null && styledSource.CurrentTextDecorations is not null)
        {
            target.CurrentTextDecorations = styledSource.CurrentTextDecorations;
        }
        if (target.CurrentTextTransform is null && styledSource.CurrentTextTransform is not null)
        {
            target.CurrentTextTransform = styledSource.CurrentTextTransform;
        }
    }

    private void MergeImageProperties(PdfImageData target, PdfImageData styledSource)
    {
        if (target.CurrentAspect == Aspect.AspectFit && styledSource.CurrentAspect != Aspect.AspectFit)
        {
            target.CurrentAspect = styledSource.CurrentAspect;
        }
    }

    private void MergeHorizontalLineProperties(PdfHorizontalLineData target, PdfHorizontalLineData styledSource)
    {
        if (target.LineColor is null && styledSource.LineColor is not null)
        {
            target.LineColor = styledSource.LineColor;
        }
        if (target.Thickness == 1.0 && styledSource.Thickness != 1.0)
        {
            target.Thickness = styledSource.Thickness;
        }
    }
}
