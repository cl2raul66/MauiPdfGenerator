namespace MauiPdfGenerator.Common.Models.Layouts;

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
        if (parentType == typeof(PdfVerticalStackLayoutData))
            return VerticalStackLayoutDefaults;
        else if (parentType == typeof(PdfHorizontalStackLayoutData))
            return HorizontalStackLayoutDefaults;
        else if (elementType.IsAssignableTo(typeof(PdfLayoutElementData)))
            return GridDefaults;

        return ContentPageDefaults;
    }
}