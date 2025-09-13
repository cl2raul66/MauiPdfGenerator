using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models.Elements;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class HorizontalLineRender : IElementRenderer
{
    private record LineLayoutCache(SKPoint RelativeStart, SKPoint RelativeEnd, PdfRect FinalRect);

    public Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfHorizontalLineData line)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalLineData)} or is null.");

        float boxWidth;
        if (line.GetWidthRequest.HasValue)
        {
            boxWidth = (float)line.GetWidthRequest.Value;
        }
        else
        {
            boxWidth = availableRect.Width - (float)line.GetMargin.HorizontalThickness;
        }

        float boxHeight = line.GetHeightRequest.HasValue
            ? (float)line.GetHeightRequest.Value
            : line.CurrentThickness + (float)line.GetPadding.VerticalThickness;

        var totalWidth = boxWidth + (float)line.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)line.GetMargin.VerticalThickness;

        return Task.FromResult(new PdfLayoutInfo(line, totalWidth, totalHeight));
    }

    public Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalLineData line)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalLineData)} or is null.");

        var elementBoxWidth = finalRect.Width - (float)line.GetMargin.HorizontalThickness;
        var elementBoxHeight = finalRect.Height - (float)line.GetMargin.VerticalThickness;

        float startX = (float)line.GetPadding.Left;
        float endX = elementBoxWidth - (float)line.GetPadding.Right;
        float lineY = elementBoxHeight / 2f;

        var startPoint = new SKPoint(startX, lineY);
        var endPoint = new SKPoint(endX, lineY);

        context.LayoutState[line] = new LineLayoutCache(startPoint, endPoint, finalRect);

        return Task.FromResult(new PdfLayoutInfo(line, finalRect.Width, finalRect.Height, finalRect));
    }

    public Task RenderAsync(SKCanvas canvas, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalLineData line)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalLineData)} or is null.");

        if (!context.LayoutState.TryGetValue(line, out var state) || state is not LineLayoutCache cache)
        {
            context.Logger.LogError("HorizontalLine layout cache not found for element. ArrangeAsync was likely not called or failed.");
            return Task.CompletedTask;
        }

        var elementBox = new SKRect(
            cache.FinalRect.Left + (float)line.GetMargin.Left,
            cache.FinalRect.Top + (float)line.GetMargin.Top,
            cache.FinalRect.Right - (float)line.GetMargin.Right,
            cache.FinalRect.Bottom - (float)line.GetMargin.Bottom
        );

        if (line.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(line.GetBackgroundColor), Style = SKPaintStyle.Fill };
            canvas.DrawRect(elementBox, bgPaint);
        }

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SkiaUtils.ConvertToSkColor(line.CurrentColor),
            StrokeWidth = line.CurrentThickness,
            IsAntialias = true
        };

        var finalStart = cache.RelativeStart;
        finalStart.Offset(elementBox.Left, elementBox.Top);

        var finalEnd = cache.RelativeEnd;
        finalEnd.Offset(elementBox.Left, elementBox.Top);

        if (finalEnd.X > finalStart.X)
        {
            canvas.DrawLine(finalStart, finalEnd, paint);
        }

        return Task.CompletedTask;
    }

    public Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context)
    {
        // Una línea desbordada simplemente no se dibuja.
        return Task.CompletedTask;
    }
}
