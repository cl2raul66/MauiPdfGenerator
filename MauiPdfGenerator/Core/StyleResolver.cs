using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Core;

internal class StyleResolver
{
    private readonly PdfResourceDictionary _resourceDictionary;
    private readonly IDiagnosticSink _diagnosticSink;

    public StyleResolver(PdfResourceDictionary resourceDictionary, IDiagnosticSink diagnosticSink)
    {
        _resourceDictionary = resourceDictionary;
        _diagnosticSink = diagnosticSink;
    }

    public void ApplyStyles(List<PdfElementData> elements)
    {
        foreach (var element in elements)
        {
            if (element.StyleKey is null) continue;

            var setter = _resourceDictionary.GetCombinedSetter(element.StyleKey);
            if (setter is null)
            {
                var message = new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    DiagnosticCodes.StyleKeyNotFound,
                    $"Style with key '{element.StyleKey}' not found in ResourceDictionary.",
                    []
                );
                _diagnosticSink.Post(message);
                continue;
            }

            var styledInstance = (PdfElementData)Activator.CreateInstance(element.GetType());
            setter(styledInstance);

            MergeProperties(element, styledInstance);
        }
    }

    private void MergeProperties(PdfElementData target, PdfElementData styledSource)
    {
        if (target.GetBackgroundColor is null && styledSource.GetBackgroundColor is not null)
        {
            target.BackgroundColor(styledSource.GetBackgroundColor);
        }

        if (!target._horizontalOptionsSet && styledSource._horizontalOptionsSet)
        {
            target.HorizontalOptions(styledSource.GetHorizontalOptions);
        }

        if (!target._verticalOptionsSet && styledSource._verticalOptionsSet)
        {
            target.VerticalOptions(styledSource.GetVerticalOptions);
        }

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
        else if (target is PdfVerticalStackLayoutData targetVSL && styledSource is PdfVerticalStackLayoutData styledVSL)
        {
            MergeVerticalStackLayoutProperties(targetVSL, styledVSL);
        }
        else if (target is PdfHorizontalStackLayoutData targetHSL && styledSource is PdfHorizontalStackLayoutData styledHSL)
        {
            MergeHorizontalStackLayoutProperties(targetHSL, styledHSL);
        }
        else if (target is PdfGridData targetGrid && styledSource is PdfGridData styledGrid)
        {
            MergeGridProperties(targetGrid, styledGrid);
        }
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

    private void MergeVerticalStackLayoutProperties(PdfVerticalStackLayoutData target, PdfVerticalStackLayoutData styledSource)
    {
        if (target.Spacing == 0 && styledSource.Spacing != 0)
        {
            target.Spacing = styledSource.Spacing;
        }
    }

    private void MergeHorizontalStackLayoutProperties(PdfHorizontalStackLayoutData target, PdfHorizontalStackLayoutData styledSource)
    {
        if (target.Spacing == 0 && styledSource.Spacing != 0)
        {
            target.Spacing = styledSource.Spacing;
        }
    }

    private void MergeGridProperties(PdfGridData target, PdfGridData styledSource)
    {
        if (target.GetRowSpacing == 0 && styledSource.GetRowSpacing != 0)
        {
            target.RowSpacing(styledSource.GetRowSpacing);
        }
        if (target.GetColumnSpacing == 0 && styledSource.GetColumnSpacing != 0)
        {
            target.ColumnSpacing(styledSource.GetColumnSpacing);
        }
    }
}
