// Ignore Spelling: vsl hsl

using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class LayoutRenderer
{
    public async Task<RenderOutput> RenderVerticalStackLayoutAsync(SKCanvas canvas, PdfVerticalStackLayout vsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        // The available area for the layout, inside its parent, after applying its own margin.
        float leftMargin = (float)vsl.GetMargin.Left;
        float rightMargin = (float)vsl.GetMargin.Right;
        SKRect vslMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (vslMarginRect.Width <= 0 || vslMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        // The area for the CHILDREN, inside the layout's PADDING.
        // This rect is relative to the recording canvas, which starts at (0,0).
        SKRect vslPaddedContentRect = new SKRect(
            0, 0,
            vslMarginRect.Width - (float)vsl.GetPadding.HorizontalThickness,
            vslMarginRect.Height - (float)vsl.GetPadding.VerticalThickness
        );

        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(vslPaddedContentRect);

        float currentYinVsl = 0;
        float totalContentHeightDrawn = 0;
        float totalContentWidthDrawn = 0;

        var childrenToRender = new Queue<PdfElement>(vsl.Children);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Any())
        {
            var child = childrenToRender.Dequeue();
            // The available rect for a child is relative to the recording canvas.
            var childAvailableRect = SKRect.Create(0, currentYinVsl, vslPaddedContentRect.Width, vslPaddedContentRect.Height - currentYinVsl);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, currentYinVsl);

            if (result.HeightDrawnThisCall > 0)
            {
                currentYinVsl += result.HeightDrawnThisCall;
                totalContentHeightDrawn += result.HeightDrawnThisCall;
                totalContentWidthDrawn = Math.Max(totalContentWidthDrawn, result.WidthDrawnThisCall);
            }

            if (result.RequiresNewPage || result.RemainingElement is not null)
            {
                requiresNewPage = true;
                if (result.RemainingElement is not null) remainingChildrenForNextPage.Add(result.RemainingElement);
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            if (childrenToRender.Any())
            {
                if (currentYinVsl + vsl.GetSpacing > vslPaddedContentRect.Height + 0.01f)
                {
                    requiresNewPage = true;
                    remainingChildrenForNextPage.AddRange(childrenToRender);
                    break;
                }
                currentYinVsl += vsl.GetSpacing;
                totalContentHeightDrawn += vsl.GetSpacing;
            }
        }

        using SKPicture picture = recorder.EndRecording();

        // The final height/width of the layout includes its content and padding.
        float finalLayoutHeight = totalContentHeightDrawn + (float)vsl.GetPadding.VerticalThickness;
        float naturalLayoutWidth = totalContentWidthDrawn + (float)vsl.GetPadding.HorizontalThickness;

        if (totalContentHeightDrawn > 0)
        {
            float finalContainerWidth = vsl.GetHorizontalOptions == LayoutAlignment.Fill ? vslMarginRect.Width : naturalLayoutWidth;
            float offsetX = 0;

            switch (vsl.GetHorizontalOptions)
            {
                case LayoutAlignment.Center: offsetX = (vslMarginRect.Width - naturalLayoutWidth) / 2; break;
                case LayoutAlignment.End: offsetX = vslMarginRect.Width - naturalLayoutWidth; break;
                default: offsetX = 0; break;
            }

            float finalX = vslMarginRect.Left + offsetX;

            if (vsl.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalLayoutHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            // Translate to the final aligned position, then account for padding to draw the children.
            canvas.Translate(finalX + (float)vsl.GetPadding.Left, startY + (float)vsl.GetPadding.Top);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfVerticalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Any())
        {
            continuation = new PdfVerticalStackLayout(remainingChildrenForNextPage, vsl);
        }

        // The height consumed by this element in the parent layout is its total final height.
        return new RenderOutput(finalLayoutHeight, naturalLayoutWidth, continuation, requiresNewPage, totalContentHeightDrawn);
    }

    public async Task<RenderOutput> RenderHorizontalStackLayoutAsync(SKCanvas canvas, PdfHorizontalStackLayout hsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        float leftMargin = (float)hsl.GetMargin.Left;
        float rightMargin = (float)hsl.GetMargin.Right;
        SKRect hslMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (hslMarginRect.Width <= 0 || hslMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        SKRect hslPaddedContentRect = new SKRect(
            0, 0,
            hslMarginRect.Width - (float)hsl.GetPadding.HorizontalThickness,
            hslMarginRect.Height - (float)hsl.GetPadding.VerticalThickness
        );

        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(hslPaddedContentRect);

        float currentXinHsl = 0;
        float maxChildContentHeight = 0;

        var childrenToRender = new Queue<PdfElement>(hsl.Children);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Any())
        {
            var child = childrenToRender.Dequeue();

            if (currentXinHsl > 0 && (currentXinHsl + 1) > hslPaddedContentRect.Width)
            {
                requiresNewPage = true;
                remainingChildrenForNextPage.Add(child);
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            var childAvailableRect = SKRect.Create(currentXinHsl, 0, hslPaddedContentRect.Width - currentXinHsl, hslPaddedContentRect.Height);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, 0);

            if (result.HeightDrawnThisCall > 0)
            {
                // We use HeightDrawnThisCall because it includes the child's own padding.
                maxChildContentHeight = Math.Max(maxChildContentHeight, result.HeightDrawnThisCall);
                currentXinHsl += result.WidthDrawnThisCall;
            }

            if (childrenToRender.Any())
            {
                currentXinHsl += hsl.GetSpacing;
            }
        }

        float naturalContentWidth = currentXinHsl;

        using SKPicture picture = recorder.EndRecording();

        float layoutContentHeight = maxChildContentHeight;
        float finalVisualHeight = hsl.GetVerticalOptions == LayoutAlignment.Fill ? hslMarginRect.Height : layoutContentHeight + (float)hsl.GetPadding.VerticalThickness;

        if (finalVisualHeight > hslMarginRect.Height && finalVisualHeight > 0)
        {
            return new RenderOutput(0, 0, hsl, true);
        }

        if (layoutContentHeight > 0)
        {
            float naturalLayoutWidth = naturalContentWidth + (float)hsl.GetPadding.HorizontalThickness;
            float finalContainerWidth = hsl.GetHorizontalOptions == LayoutAlignment.Fill ? hslMarginRect.Width : naturalLayoutWidth;
            float offsetX = 0;

            switch (hsl.GetHorizontalOptions)
            {
                case LayoutAlignment.Center: offsetX = (hslMarginRect.Width - naturalLayoutWidth) / 2; break;
                case LayoutAlignment.End: offsetX = hslMarginRect.Width - naturalLayoutWidth; break;
                default: offsetX = 0; break;
            }
            float finalX = hslMarginRect.Left + offsetX;

            // --- START OF CORRECTION ---
            float contentOffsetY = 0;
            // The available vertical space for the content, inside the layout's padding.
            float availableContentHeight = finalVisualHeight - (float)hsl.GetPadding.VerticalThickness;

            switch (hsl.GetVerticalOptions)
            {
                case LayoutAlignment.Center:
                    contentOffsetY = (availableContentHeight - layoutContentHeight) / 2;
                    break;
                case LayoutAlignment.End:
                    contentOffsetY = availableContentHeight - layoutContentHeight;
                    break;
                // 'Start' and 'Fill' both align content to the top of the padded area.
                case LayoutAlignment.Start:
                case LayoutAlignment.Fill:
                default:
                    contentOffsetY = 0;
                    break;
            }

            if (hsl.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalVisualHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            // The final translation includes the layout's top padding PLUS the calculated content offset.
            canvas.Translate(finalX + (float)hsl.GetPadding.Left, startY + (float)hsl.GetPadding.Top + contentOffsetY);
            canvas.DrawPicture(picture);
            canvas.Restore();
            // --- END OF CORRECTION ---
        }

        PdfHorizontalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Any())
        {
            continuation = new PdfHorizontalStackLayout(remainingChildrenForNextPage, hsl);
        }

        return new RenderOutput(finalVisualHeight, naturalContentWidth + (float)hsl.GetPadding.HorizontalThickness, continuation, requiresNewPage, layoutContentHeight);
    }
}
