using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Implementation.Layout.Managers;

internal interface IInternalViewBuilder
{
    PdfHorizontalAlignment ConfiguredHorizontalOptions { get; }
    PdfVerticalAlignment ConfiguredVerticalOptions { get; }
    // Añadir otras propiedades comunes si son necesarias para el layout
    // double? ConfiguredWidth { get; }
    // double? ConfiguredHeight { get; }
    // Microsoft.Maui.Thickness ConfiguredMargin { get; }
}
