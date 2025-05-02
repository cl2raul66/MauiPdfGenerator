using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Core.Models;

internal record PdfPageData(
    PageSizeType Size, 
    PageOrientationType Orientation, 
    Thickness Margins,
    Color? BackgroundColor,
    string? DefaultFontAlias
);
