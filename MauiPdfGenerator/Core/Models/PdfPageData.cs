﻿using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Core.Models;

internal record PdfPageData(
    PageSizeType Size, 
    PageOrientationType Orientation, 
    Thickness Margins,
    Color? BackgroundColor,

    IReadOnlyList<PdfElement> Elements,
    float PageDefaultSpacing,
    string PageDefaultFontFamily,
    float PageDefaultFontSize,
    Color PageDefaultTextColor,
    FontAttributes PageDefaultFontAttributes
);
