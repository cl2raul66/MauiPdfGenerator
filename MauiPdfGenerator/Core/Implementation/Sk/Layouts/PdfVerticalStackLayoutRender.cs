using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class PdfVerticalStackLayoutRender : IElementRenderer
{
    public async Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var vsl = (PdfVerticalStackLayout)element;
        float totalHeight = 0;
        float maxWidth = 0;
        var childMeasures = new List<LayoutInfo>();

        var childAvailableWidth = availableRect.Width - (float)vsl.GetPadding.HorizontalThickness - (float)vsl.GetMargin.HorizontalThickness;

        foreach (var child in vsl.GetChildren)
        {
            var renderer = rendererFactory.GetRenderer(child);
            var measure = await renderer.MeasureAsync(child, rendererFactory, pageDef, SKRect.Create(0, 0, childAvailableWidth, float.PositiveInfinity), layoutState, fontRegistry);
            childMeasures.Add(measure);
            totalHeight += measure.Height;
            maxWidth = Math.Max(maxWidth, measure.Width);
        }

        if (vsl.GetChildren.Count > 1)
        {
            totalHeight += vsl.GetSpacing * (vsl.GetChildren.Count - 1);
        }

        totalHeight += (float)vsl.GetPadding.VerticalThickness + (float)vsl.GetMargin.VerticalThickness;
        maxWidth += (float)vsl.GetPadding.HorizontalThickness + (float)vsl.GetMargin.HorizontalThickness;

        layoutState[vsl] = childMeasures;

        return new LayoutInfo(vsl, maxWidth, totalHeight);
    }

    public async Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var vsl = (PdfVerticalStackLayout)element;
        if (!layoutState.TryGetValue(vsl, out var state) || state is not List<LayoutInfo> childMeasures)
        {
            throw new InvalidOperationException("VerticalStackLayout state was not calculated before rendering.");
        }

        float contentWidth = renderRect.Width - (float)vsl.GetMargin.HorizontalThickness - (float)vsl.GetPadding.HorizontalThickness;

        if (vsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(renderRect, bgPaint);
        }

        float y = renderRect.Top + (float)vsl.GetMargin.Top + (float)vsl.GetPadding.Top;

        for (int i = 0; i < vsl.GetChildren.Count; i++)
        {
            var child = vsl.GetChildren[i];
            var measure = childMeasures[i];
            var renderer = rendererFactory.GetRenderer(child);

            float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? contentWidth : measure.Width;

            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentWidth - childWidth) / 2f,
                LayoutAlignment.End => contentWidth - childWidth,
                _ => 0f
            };

            float x = renderRect.Left + (float)vsl.GetMargin.Left + (float)vsl.GetPadding.Left + offsetX;
            var childRect = SKRect.Create(x, y, childWidth, measure.Height);

            await renderer.RenderAsync(canvas, child, rendererFactory, pageDef, childRect, layoutState, fontRegistry);

            y += measure.Height;
            if (i < vsl.GetChildren.Count - 1)
            {
                y += vsl.GetSpacing;
            }
        }
    }
}
