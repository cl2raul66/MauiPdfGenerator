using MauiPdfGenerator.Fluent.Enums;
using Microsoft.Maui.Graphics;
using MauiPdfGenerator.Fluent.Models.Layouts;

namespace MauiPdfGenerator.Fluent.Models;

public class PdfLayoutDefaultOptions
{
    public record struct DefaultOptions(
        LayoutAlignment HorizontalOptions,
        LayoutAlignment VerticalOptions,
        double Margin = 0,
        double Padding = 0,
        Color? BackgroundColor = null
    );

    private static readonly DefaultOptions ContentPageDefaults = new(
        HorizontalOptions: LayoutAlignment.Fill,
        VerticalOptions: LayoutAlignment.Start
    );

    private static readonly DefaultOptions GridCellDefaults = new(
        HorizontalOptions: LayoutAlignment.Start,
        VerticalOptions: LayoutAlignment.Start
    );

    private static readonly DefaultOptions VerticalStackLayoutDefaults = new(
        HorizontalOptions: LayoutAlignment.Fill,
        VerticalOptions: LayoutAlignment.Start
    );

    private static readonly DefaultOptions HorizontalStackLayoutDefaults = new(
        HorizontalOptions: LayoutAlignment.Start,
        VerticalOptions: LayoutAlignment.Fill
    );

    private static readonly DefaultOptions GridDefaults = new(
        HorizontalOptions: LayoutAlignment.Fill,
        VerticalOptions: LayoutAlignment.Start
    );

    public static DefaultOptions GetDefaultOptions(Type parentType, Type elementType)
    {
        if (parentType == typeof(PdfGrid))
            return GridCellDefaults;
        else if (parentType == typeof(PdfVerticalStackLayout))
            return VerticalStackLayoutDefaults;
        else if (parentType == typeof(PdfHorizontalStackLayout))
            return HorizontalStackLayoutDefaults;
        else if (elementType.IsAssignableTo(typeof(PdfLayoutElement)))
            return GridDefaults;

        return ContentPageDefaults;
    }
}