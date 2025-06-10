// Ignore Spelling: vsl hsl

using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class LayoutRenderer
{
    public async Task<RenderOutput> RenderVerticalStackLayoutAsync(SKCanvas canvas, PdfVerticalStackLayout vsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        float leftMargin = (float)vsl.GetMargin.Left;
        float rightMargin = (float)vsl.GetMargin.Right;

        SKRect vslContentRectInParent = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (vslContentRectInParent.Width <= 0 || vslContentRectInParent.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(SKRect.Create(vslContentRectInParent.Width, vslContentRectInParent.Height));

        float currentYinVsl = 0;
        float totalHeightDrawn = 0;
        float totalWidthDrawn = 0;

        var childrenToRender = new Queue<PdfElement>(vsl.Children);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Any())
        {
            var child = childrenToRender.Dequeue();
            var childAvailableRect = SKRect.Create(0, currentYinVsl, vslContentRectInParent.Width, vslContentRectInParent.Height - currentYinVsl);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, currentYinVsl);

            if (result.HeightDrawnThisCall > 0)
            {
                currentYinVsl += result.HeightDrawnThisCall;
                totalHeightDrawn += result.HeightDrawnThisCall;
                totalWidthDrawn = Math.Max(totalWidthDrawn, result.WidthDrawnThisCall + (float)child.GetMargin.HorizontalThickness);
            }

            if (result.RequiresNewPage || result.RemainingElement is not null)
            {
                requiresNewPage = true;
                if (result.RemainingElement is not null)
                {
                    remainingChildrenForNextPage.Add(result.RemainingElement);
                }
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            if (childrenToRender.Any())
            {
                if (currentYinVsl + vsl.CurrentSpacing > vslContentRectInParent.Height + 0.01f)
                {
                    requiresNewPage = true;
                    remainingChildrenForNextPage.AddRange(childrenToRender);
                    break;
                }
                currentYinVsl += vsl.CurrentSpacing;
                totalHeightDrawn += vsl.CurrentSpacing;
            }
        }

        using SKPicture picture = recorder.EndRecording();

        if (totalHeightDrawn > 0)
        {
            // --- START OF CORRECTION ---
            // Un VerticalStackLayout se alinea horizontalmente. Usamos PdfHorizontalOptions.
            float finalWidth = vsl.PdfHorizontalOptions == LayoutAlignment.Fill ? vslContentRectInParent.Width : totalWidthDrawn;
            float offsetX = 0;

            switch (vsl.PdfHorizontalOptions) // Usar la opción correcta
            {
                case LayoutAlignment.Center:
                    offsetX = (vslContentRectInParent.Width - finalWidth) / 2;
                    break;
                case LayoutAlignment.End:
                    offsetX = vslContentRectInParent.Width - finalWidth;
                    break;
                case LayoutAlignment.Start:
                case LayoutAlignment.Fill:
                default:
                    offsetX = 0;
                    break;
            }
            // --- END OF CORRECTION ---

            float finalX = vslContentRectInParent.Left + offsetX;

            if (vsl.CurrentBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.CurrentBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalWidth, totalHeightDrawn);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            canvas.Translate(finalX, startY);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfVerticalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Any())
        {
            continuation = new PdfVerticalStackLayout(remainingChildrenForNextPage, vsl);
        }

        return new RenderOutput(totalHeightDrawn, vslContentRectInParent.Width, continuation, requiresNewPage, totalHeightDrawn);
    }

    public async Task<RenderOutput> RenderHorizontalStackLayoutAsync(SKCanvas canvas, PdfHorizontalStackLayout hsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        float leftMargin = (float)hsl.GetMargin.Left;
        float rightMargin = (float)hsl.GetMargin.Right;

        SKRect hslContentRectInParent = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (hslContentRectInParent.Width <= 0 || hslContentRectInParent.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(SKRect.Create(hslContentRectInParent.Width, hslContentRectInParent.Height));

        float currentXinHsl = 0;
        float maxChildHeight = 0;
        float totalWidthDrawn = 0;

        var childrenToRender = new Queue<PdfElement>(hsl.Children);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Any())
        {
            var child = childrenToRender.Peek();

            if (currentXinHsl > 0 && (currentXinHsl + 1) > hslContentRectInParent.Width)
            {
                requiresNewPage = true;
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            childrenToRender.Dequeue();
            var childAvailableRect = SKRect.Create(currentXinHsl, 0, hslContentRectInParent.Width - currentXinHsl, hslContentRectInParent.Height);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, 0);

            if (result.HeightDrawnThisCall > 0)
            {
                maxChildHeight = Math.Max(maxChildHeight, result.VisualHeightDrawn);
                currentXinHsl += result.WidthDrawnThisCall;
                totalWidthDrawn += result.WidthDrawnThisCall;
            }

            if (result.RequiresNewPage || result.RemainingElement is not null)
            {
                requiresNewPage = true;
                if (result.RemainingElement is not null)
                {
                    remainingChildrenForNextPage.Add(result.RemainingElement);
                }
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            if (childrenToRender.Any())
            {
                currentXinHsl += hsl.CurrentSpacing;
                totalWidthDrawn += hsl.CurrentSpacing;
            }
        }

        using SKPicture picture = recorder.EndRecording();

        // --- START OF CORRECTION ---
        // Un HorizontalStackLayout se alinea verticalmente. Usamos PdfVerticalOptions.
        float finalHeight = hsl.PdfVerticalOptions == LayoutAlignment.Fill ? hslContentRectInParent.Height : maxChildHeight;
        // --- END OF CORRECTION ---

        if (finalHeight > hslContentRectInParent.Height && finalHeight > 0)
        {
            return new RenderOutput(0, 0, hsl, true);
        }

        if (maxChildHeight > 0)
        {
            float offsetY = 0;
            // --- START OF CORRECTION ---
            switch (hsl.PdfVerticalOptions) // Usar la opción correcta
            {
                case LayoutAlignment.Center:
                    offsetY = (hslContentRectInParent.Height - finalHeight) / 2;
                    break;
                case LayoutAlignment.End:
                    offsetY = hslContentRectInParent.Height - finalHeight;
                    break;
                case LayoutAlignment.Start:
                case LayoutAlignment.Fill:
                default:
                    offsetY = 0;
                    break;
            }
            // --- END OF CORRECTION ---

            float finalY = startY + offsetY;

            if (hsl.CurrentBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.CurrentBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(hslContentRectInParent.Left, finalY, totalWidthDrawn, finalHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            canvas.Translate(hslContentRectInParent.Left, finalY);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfHorizontalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Any())
        {
            continuation = new PdfHorizontalStackLayout(remainingChildrenForNextPage, hsl);
        }

        return new RenderOutput(finalHeight, totalWidthDrawn, continuation, requiresNewPage, finalHeight);
    }
}
