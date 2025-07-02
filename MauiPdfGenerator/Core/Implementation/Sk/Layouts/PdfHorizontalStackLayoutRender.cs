using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfHorizontalStackLayoutRender : IElementRenderer
{
    public async Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var hsl = (PdfHorizontalStackLayout)element;
        float totalWidth = 0;
        float maxHeight = 0;
        var childMeasures = new List<LayoutInfo>();

        foreach (var child in hsl.GetChildren)
        {
            var renderer = rendererFactory.GetRenderer(child);
            var measure = await renderer.MeasureAsync(child, rendererFactory, pageDef, SKRect.Create(0, 0, float.PositiveInfinity, float.PositiveInfinity), layoutState, fontRegistry);
            childMeasures.Add(measure);
            totalWidth += measure.Width;
            maxHeight = Math.Max(maxHeight, measure.Height);
        }

        if (hsl.GetChildren.Count > 1)
        {
            totalWidth += hsl.GetSpacing * (hsl.GetChildren.Count - 1);
        }

        totalWidth += (float)hsl.GetPadding.HorizontalThickness + (float)hsl.GetMargin.HorizontalThickness;
        maxHeight += (float)hsl.GetPadding.VerticalThickness + (float)hsl.GetMargin.VerticalThickness;

        layoutState[hsl] = childMeasures;

        return new LayoutInfo(hsl, totalWidth, maxHeight);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var hsl = (PdfHorizontalStackLayout)element;
        if (!layoutState.TryGetValue(hsl, out var state) || state is not List<LayoutInfo> childMeasures)
        {
            throw new InvalidOperationException("HorizontalStackLayout state was not calculated before rendering.");
        }

        float contentWidth = childMeasures.Sum(m => m.Width) + (hsl.GetChildren.Count > 1 ? hsl.GetSpacing * (hsl.GetChildren.Count - 1) : 0);
        float contentHeight = childMeasures.Any() ? childMeasures.Max(m => m.Height) : 0;

        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            var bgRect = SKRect.Create(
                renderRect.Left + (float)hsl.GetMargin.Left,
                renderRect.Top + (float)hsl.GetMargin.Top,
                contentWidth + (float)hsl.GetPadding.HorizontalThickness,
                contentHeight + (float)hsl.GetPadding.VerticalThickness);
            canvas.DrawRect(bgRect, bgPaint);
        }

        float x = renderRect.Left + (float)hsl.GetMargin.Left + (float)hsl.GetPadding.Left;

        for (int i = 0; i < hsl.GetChildren.Count; i++)
        {
            var child = hsl.GetChildren[i];
            var measure = childMeasures[i];
            var renderer = rendererFactory.GetRenderer(child);

            float offsetY = child.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (contentHeight - measure.Height) / 2f,
                LayoutAlignment.End => contentHeight - measure.Height,
                _ => 0f
            };

            float y = renderRect.Top + (float)hsl.GetMargin.Top + (float)hsl.GetPadding.Top + offsetY;
            var childRect = SKRect.Create(x, y, measure.Width, measure.Height);

            await renderer.RenderAsync(canvas, child, rendererFactory, pageDef, childRect, layoutState, fontRegistry);

            x += measure.Width;
            if (i < hsl.GetChildren.Count - 1)
            {
                x += hsl.GetSpacing;
            }
        }
    }
}
