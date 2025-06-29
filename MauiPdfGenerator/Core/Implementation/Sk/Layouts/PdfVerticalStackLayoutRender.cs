using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfVerticalStackLayoutRender
{
    public async Task<MeasureOutput> MeasureAsync(PdfVerticalStackLayout vsl, PdfPageData pageDef, ElementsRender elementsRender, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        float totalHeight = 0;
        float maxWidth = 0;
        var childMeasures = new List<MeasureOutput>();

        var childAvailableWidth = availableRect.Width - (float)vsl.GetPadding.HorizontalThickness - (float)vsl.GetMargin.HorizontalThickness;

        foreach (var child in vsl.GetChildren)
        {
            var measure = await elementsRender.Measure(child, pageDef, SKRect.Create(0, 0, childAvailableWidth, float.PositiveInfinity), 0, layoutState, fontRegistry);
            childMeasures.Add(measure);
            totalHeight += measure.HeightRequired;
            maxWidth = Math.Max(maxWidth, measure.WidthRequired);
        }

        if (vsl.GetChildren.Count > 1)
        {
            totalHeight += vsl.GetSpacing * (vsl.GetChildren.Count - 1);
        }

        totalHeight += (float)vsl.GetPadding.VerticalThickness;
        maxWidth += (float)vsl.GetPadding.HorizontalThickness;

        layoutState[vsl] = childMeasures;

        return new MeasureOutput(totalHeight, totalHeight, maxWidth, [], null, false, 0, 0, 0, 0, null);
    }

    public async Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfVerticalStackLayout vsl, PdfPageData pageDef, ElementsRender elementsRender, SKRect availableRect, float currentY, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        if (!layoutState.TryGetValue(vsl, out var state) || state is not List<MeasureOutput> childMeasures)
        {
            throw new InvalidOperationException("VerticalStackLayout state was not calculated before rendering.");
        }

        float contentWidth = availableRect.Width - (float)vsl.GetMargin.HorizontalThickness - (float)vsl.GetPadding.HorizontalThickness;
        float contentHeight = 0;

        float y = currentY + (float)vsl.GetMargin.Top + (float)vsl.GetPadding.Top;

        for (int i = 0; i < vsl.GetChildren.Count; i++)
        {
            var child = vsl.GetChildren[i];
            var measure = childMeasures[i];

            float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? contentWidth : measure.WidthRequired;
            float childHeight = measure.HeightRequired;

            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentWidth - childWidth) / 2f,
                LayoutAlignment.End => contentWidth - childWidth,
                _ => 0f
            };

            float x = availableRect.Left + (float)vsl.GetMargin.Left + (float)vsl.GetPadding.Left + offsetX;

            await elementsRender.Render(canvas, child, pageDef, SKRect.Create(x, y, childWidth, childHeight), y, layoutState, fontRegistry);

            y += childHeight;
            if (i < vsl.GetChildren.Count - 1)
            {
                y += vsl.GetSpacing;
            }
            contentHeight += childHeight;
        }

        if (vsl.GetChildren.Count > 1)
        {
            contentHeight += vsl.GetSpacing * (vsl.GetChildren.Count - 1);
        }

        float totalHeight = contentHeight + (float)vsl.GetPadding.VerticalThickness + (float)vsl.GetMargin.VerticalThickness;
        float totalWidth = contentWidth + (float)vsl.GetPadding.HorizontalThickness + (float)vsl.GetMargin.HorizontalThickness;

        if (vsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            var bgRect = SKRect.Create(availableRect.Left + (float)vsl.GetMargin.Left, currentY + (float)vsl.GetMargin.Top, totalWidth - (float)vsl.GetMargin.HorizontalThickness, totalHeight - (float)vsl.GetMargin.VerticalThickness);
            canvas.DrawRect(bgRect, bgPaint);
        }

        return new RenderOutput(totalHeight, totalWidth, null, false, totalHeight);
    }
}
