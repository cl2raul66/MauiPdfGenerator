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

        float finalConsumedHeight = totalHeightDrawn;

        if (totalHeightDrawn > 0)
        {
            float finalWidth = vsl.PdfHorizontalOptions == LayoutAlignment.Fill ? vslContentRectInParent.Width : totalWidthDrawn;
            float offsetX = 0;

            switch (vsl.PdfHorizontalOptions)
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

            float finalX = vslContentRectInParent.Left + offsetX;

            if (vsl.CurrentBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.CurrentBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalWidth, finalConsumedHeight);
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

        return new RenderOutput(finalConsumedHeight, totalWidthDrawn, continuation, requiresNewPage, finalConsumedHeight);
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

        // --- START OF FINAL SIMPLIFIED LOGIC ---
        // Single pass rendering. No more two-pass.
        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(SKRect.Create(hslContentRectInParent.Width, hslContentRectInParent.Height));

        float currentXinHsl = 0;
        float maxChildHeight = 0;
        float naturalContentWidth = 0;

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
            // The available rect for the child is from its current position to the end of the line.
            var childAvailableRect = SKRect.Create(currentXinHsl, 0, hslContentRectInParent.Width - currentXinHsl, hslContentRectInParent.Height);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, 0);

            if (result.HeightDrawnThisCall > 0)
            {
                maxChildHeight = Math.Max(maxChildHeight, result.VisualHeightDrawn);
                currentXinHsl += result.WidthDrawnThisCall;
            }

            if (result.RequiresNewPage || result.RemainingElement is not null)
            {
                // This logic is simplified as complex wrapping in HSL is not yet supported.
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
            }
        }

        // The natural width of the content is the final X position.
        naturalContentWidth = currentXinHsl;
        // --- END OF FINAL SIMPLIFIED LOGIC ---

        using SKPicture picture = recorder.EndRecording();

        float finalVisualHeight = hsl.PdfVerticalOptions == LayoutAlignment.Fill ? hslContentRectInParent.Height : maxChildHeight;

        if (finalVisualHeight > hslContentRectInParent.Height && finalVisualHeight > 0)
        {
            return new RenderOutput(0, 0, hsl, true);
        }

        if (maxChildHeight > 0)
        {
            // Determine the final width of the container/background
            float finalContainerWidth = hsl.PdfHorizontalOptions == LayoutAlignment.Fill ? hslContentRectInParent.Width : naturalContentWidth;

            // Determine the horizontal offset for the entire content block
            float offsetX = 0;
            switch (hsl.PdfHorizontalOptions)
            {
                case LayoutAlignment.Center:
                    offsetX = (hslContentRectInParent.Width - naturalContentWidth) / 2;
                    break;
                case LayoutAlignment.End:
                    offsetX = hslContentRectInParent.Width - naturalContentWidth;
                    break;
                case LayoutAlignment.Start:
                case LayoutAlignment.Fill:
                default:
                    offsetX = 0; // For 'Fill', the content block still starts at the left.
                    break;
            }
            float finalX = hslContentRectInParent.Left + offsetX;

            float contentOffsetY = 0;
            switch (hsl.PdfVerticalOptions)
            {
                case LayoutAlignment.Center:
                    contentOffsetY = (finalVisualHeight - maxChildHeight) / 2;
                    break;
                case LayoutAlignment.End:
                    contentOffsetY = finalVisualHeight - maxChildHeight;
                    break;
                case LayoutAlignment.Start:
                case LayoutAlignment.Fill:
                default:
                    contentOffsetY = 0;
                    break;
            }

            if (hsl.CurrentBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.CurrentBackgroundColor), Style = SKPaintStyle.Fill };
                // The background is drawn at the content's aligned X position, but with the container's final width.
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalVisualHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            // The content itself is always drawn relative to the aligned X position.
            canvas.Translate(finalX, startY + contentOffsetY);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfHorizontalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Any())
        {
            continuation = new PdfHorizontalStackLayout(remainingChildrenForNextPage, hsl);
        }

        return new RenderOutput(finalVisualHeight, naturalContentWidth, continuation, requiresNewPage, finalVisualHeight);
    }
}
