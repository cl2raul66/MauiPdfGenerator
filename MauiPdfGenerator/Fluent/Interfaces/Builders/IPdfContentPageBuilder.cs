using Microsoft.Maui.Controls;
using Microsoft.Maui;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

internal interface IPdfContentPageBuilder : IPdfPageBuilder
{
    PageSizeType GetEffectivePageSize();
    Thickness GetEffectivePadding();
    PageOrientationType GetEffectivePageOrientation();
    Color? GetEffectiveBackgroundColor();
    IReadOnlyList<PdfElement> GetElements();
    float GetPageSpacing();
    PdfFontIdentifier? GetPageDefaultFontFamily();
    float GetPageDefaultFontSize();
    Color GetPageDefaultTextColor();
    FontAttributes GetPageDefaultFontAttributes();
    TextDecorations GetPageDefaultTextDecorations();
    TextTransform GetPageDefaultTextTransform();
}
