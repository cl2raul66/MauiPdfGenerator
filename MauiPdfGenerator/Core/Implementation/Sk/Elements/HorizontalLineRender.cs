using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models.Elements;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class HorizontalLineRender : IElementRenderer
{
    private record LineLayoutCache(SKPoint RelativeStart, SKPoint RelativeEnd);

    public Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
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

        float startX = (float)line.GetPadding.Left;
        float endX = boxWidth - (float)line.GetPadding.Right;
        float lineY = boxHeight / 2f;

        var startPoint = new SKPoint(startX, lineY);
        var endPoint = new SKPoint(endX, lineY);

        context.LayoutState[line] = new LineLayoutCache(startPoint, endPoint);

        var totalWidth = boxWidth + (float)line.GetMargin.HorizontalThickness;
        var totalHeight = boxHeight + (float)line.GetMargin.VerticalThickness;

        // LOG: Añadimos un log específico para los casos con Margin.
        if (line.GetMargin != new Thickness(0))
        {
            context.Logger.LogInformation("[HL.Measure] Caso con Margin={Margin}. Cajón (WxH)={BoxW}x{BoxH}. Total (WxH)={TotalW}x{TotalH}",
                line.GetMargin, boxWidth, boxHeight, totalWidth, totalHeight);
        }

        return Task.FromResult(new LayoutInfo(line, totalWidth, totalHeight));
    }

    public Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalLineData line)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalLineData)} or is null.");

        if (!context.LayoutState.TryGetValue(line, out var state) || state is not LineLayoutCache cache)
        {
            context.Logger.LogError("HorizontalLine layout cache not found for element. MeasureAsync was likely not called or failed.");
            return Task.CompletedTask;
        }

        // --- LÓGICA DE MARGEN CORREGIDA Y LOGS ---
        // 1. El renderRect recibido es el "pie de imprenta" total, incluyendo márgenes.
        // 2. Calculamos el "cajón" real del elemento restando el margen.
        var elementBox = new SKRect(
            renderRect.Left + (float)line.GetMargin.Left,
            renderRect.Top + (float)line.GetMargin.Top,
            renderRect.Right - (float)line.GetMargin.Right,
            renderRect.Bottom - (float)line.GetMargin.Bottom
        );

        if (line.GetMargin != new Thickness(0))
        {
            context.Logger.LogInformation("[HL.Render] Caso con Margin={Margin}. renderRect recibido={RenderRect}. Cajón de elemento calculado={ElementBox}",
                line.GetMargin, renderRect, elementBox);
        }

        if (line.GetBackgroundColor is not null)
        {
            // El fondo se dibuja en el cajón del elemento, no en el área del margen.
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

        // 3. Aplicamos el offset del "cajón" del elemento a las coordenadas relativas.
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
}
