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
        SKRect vslMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (vsl.GetWidthRequest.HasValue)
        {
            vslMarginRect.Right = vslMarginRect.Left + (float)vsl.GetWidthRequest.Value;
        }

        if (vslMarginRect.Width <= 0 || vslMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        SKRect vslPaddedContentRect = new(
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

        while (childrenToRender.Count != 0)
        {
            var child = childrenToRender.Dequeue();
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

            if (childrenToRender.Count != 0)
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

        float visualContentHeight = totalContentHeightDrawn;

        float finalLayoutHeight = vsl.GetHeightRequest.HasValue ?
            (float)vsl.GetHeightRequest.Value :
            visualContentHeight + (float)vsl.GetPadding.VerticalThickness;

        float naturalLayoutWidth = totalContentWidthDrawn + (float)vsl.GetPadding.HorizontalThickness;

        if (totalContentHeightDrawn > 0)
        {
            float finalContainerWidth = vsl.GetHorizontalOptions is LayoutAlignment.Fill ? vslMarginRect.Width : naturalLayoutWidth;
            if (vsl.GetWidthRequest.HasValue) finalContainerWidth = (float)vsl.GetWidthRequest.Value;

            float offsetX = 0;

            switch (vsl.GetHorizontalOptions)
            {
                case LayoutAlignment.Center: offsetX = (vslMarginRect.Width - finalContainerWidth) / 2; break;
                case LayoutAlignment.End: offsetX = vslMarginRect.Width - finalContainerWidth; break;
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
            canvas.Translate(finalX + (float)vsl.GetPadding.Left, startY + (float)vsl.GetPadding.Top);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfVerticalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Count != 0)
        {
            continuation = new PdfVerticalStackLayout(remainingChildrenForNextPage, vsl);
        }

        float consumedWidth = vsl.GetWidthRequest.HasValue ? (float)vsl.GetWidthRequest.Value : naturalLayoutWidth;
        return new RenderOutput(finalLayoutHeight, consumedWidth, continuation, requiresNewPage, visualContentHeight);
    }

    public async Task<RenderOutput> RenderHorizontalStackLayoutAsync(SKCanvas canvas, PdfHorizontalStackLayout hsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        float leftMargin = (float)hsl.GetMargin.Left;
        float rightMargin = (float)hsl.GetMargin.Right;
        SKRect hslMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);

        if (hsl.GetWidthRequest.HasValue)
        {
            hslMarginRect.Right = hslMarginRect.Left + (float)hsl.GetWidthRequest.Value;
        }

        if (hslMarginRect.Width <= 0 || hslMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }

        SKRect hslPaddedContentRect = new(
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
                maxChildContentHeight = Math.Max(maxChildContentHeight, result.HeightDrawnThisCall);
                currentXinHsl += result.WidthDrawnThisCall;
            }

            if (childrenToRender.Count != 0)
            {
                currentXinHsl += hsl.GetSpacing;
            }
        }

        float naturalContentWidth = currentXinHsl;

        using SKPicture picture = recorder.EndRecording();

        float layoutContentHeight = maxChildContentHeight;

        float finalVisualHeight = hsl.GetHeightRequest.HasValue ?
            (float)hsl.GetHeightRequest.Value :
            layoutContentHeight + (float)hsl.GetPadding.VerticalThickness;

        if (finalVisualHeight > hslMarginRect.Height && finalVisualHeight > 0)
        {
            return new RenderOutput(0, 0, hsl, true);
        }

        if (layoutContentHeight > 0)
        {
            float naturalLayoutWidth = naturalContentWidth + (float)hsl.GetPadding.HorizontalThickness;
            float finalContainerWidth = hsl.GetHorizontalOptions == LayoutAlignment.Fill ? hslMarginRect.Width : naturalLayoutWidth;
            if (hsl.GetWidthRequest.HasValue) finalContainerWidth = (float)hsl.GetWidthRequest.Value;

            float offsetX = hsl.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (hslMarginRect.Width - finalContainerWidth) / 2,
                LayoutAlignment.End => offsetX = hslMarginRect.Width - finalContainerWidth,
                _ => offsetX = 0
            };
            float finalX = hslMarginRect.Left + offsetX;

            float availableContentHeight = finalVisualHeight - (float)hsl.GetPadding.VerticalThickness;
            float contentOffsetY = 0;
            contentOffsetY = hsl.GetVerticalOptions switch
            {
                LayoutAlignment.Center => (availableContentHeight - layoutContentHeight) / 2,
                LayoutAlignment.End => availableContentHeight - layoutContentHeight,
                _ => 0,
            };
            if (hsl.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalVisualHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            canvas.Translate(finalX + (float)hsl.GetPadding.Left, startY + (float)hsl.GetPadding.Top + contentOffsetY);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfHorizontalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Count != 0)
        {
            continuation = new PdfHorizontalStackLayout(remainingChildrenForNextPage, hsl);
        }

        float consumedWidth = hsl.GetWidthRequest.HasValue ? (float)hsl.GetWidthRequest.Value : naturalContentWidth + (float)hsl.GetPadding.HorizontalThickness;
        return new RenderOutput(finalVisualHeight, consumedWidth, continuation, requiresNewPage, layoutContentHeight);
    }
}
