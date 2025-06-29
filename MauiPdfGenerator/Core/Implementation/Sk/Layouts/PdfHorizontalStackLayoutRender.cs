using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfHorizontalStackLayoutRender
{
    public async Task<MeasureOutput> MeasureAsync(PdfHorizontalStackLayout hsl, PdfPageData pageDef, ElementsRender elementsRender, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        float totalWidth = 0;
        float maxHeight = 0;
        var childMeasures = new List<MeasureOutput>();

        foreach (var child in hsl.GetChildren)
        {
            var measure = await elementsRender.Measure(child, pageDef, SKRect.Create(0, 0, float.PositiveInfinity, float.PositiveInfinity), 0, layoutState, fontRegistry);
            childMeasures.Add(measure);
            totalWidth += measure.WidthRequired;
            maxHeight = Math.Max(maxHeight, measure.HeightRequired);
        }

        if (hsl.GetChildren.Count > 1)
        {
            totalWidth += hsl.GetSpacing * (hsl.GetChildren.Count - 1);
        }

        totalWidth += (float)hsl.GetPadding.HorizontalThickness;
        maxHeight += (float)hsl.GetPadding.VerticalThickness;

        layoutState[hsl] = childMeasures;

        return new MeasureOutput(maxHeight, maxHeight, totalWidth, [], null, false, 0, 0, 0, 0, null);
    }

    public async Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfHorizontalStackLayout hsl, PdfPageData pageDef, ElementsRender elementsRender, SKRect availableRect, float currentY, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        if (!layoutState.TryGetValue(hsl, out var state) || state is not List<MeasureOutput> childMeasures)
        {
            throw new InvalidOperationException("HorizontalStackLayout state was not calculated before rendering.");
        }

        float totalMeasuredWidth = childMeasures.Sum(m => m.WidthRequired) + (hsl.GetChildren.Count > 1 ? hsl.GetSpacing * (hsl.GetChildren.Count - 1) : 0);
        float maxHeight = childMeasures.Any() ? childMeasures.Max(m => m.HeightRequired) : 0;

        float x = availableRect.Left + (float)hsl.GetMargin.Left + (float)hsl.GetPadding.Left;

        for (int i = 0; i < hsl.GetChildren.Count; i++)
        {
            var child = hsl.GetChildren[i];
            var measure = childMeasures[i];

            float childWidth = measure.WidthRequired;
            float childHeight = measure.HeightRequired;

            float offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (maxHeight - childHeight) / 2f,
                LayoutAlignment.End => maxHeight - childHeight,
                _ => 0f
            };

            float y = currentY + (float)hsl.GetMargin.Top + (float)hsl.GetPadding.Top + offsetY;

            await elementsRender.Render(canvas, child, pageDef, SKRect.Create(x, y, childWidth, childHeight), y, layoutState, fontRegistry);

            x += childWidth;
            if (i < hsl.GetChildren.Count - 1)
            {
                x += hsl.GetSpacing;
            }
        }

        float totalHeight = maxHeight + (float)hsl.GetPadding.VerticalThickness + (float)hsl.GetMargin.VerticalThickness;

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            var bgRect = SKRect.Create(availableRect.Left + (float)hsl.GetMargin.Left, currentY + (float)hsl.GetMargin.Top, totalMeasuredWidth + (float)hsl.GetPadding.HorizontalThickness, totalHeight - (float)hsl.GetMargin.VerticalThickness);
            canvas.DrawRect(bgRect, bgPaint);
        }

        return new RenderOutput(totalHeight, totalMeasuredWidth, null, false, totalHeight);
    }
}
