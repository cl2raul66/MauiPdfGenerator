using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Styles;

/// <summary>
/// Represents a style for a HorizontalStackLayout element.
/// This interface closes the generic type chain for styling purposes.
/// </summary>
public interface IPdfHorizontalStackLayoutStyle : IPdfHorizontalStackLayout<IPdfHorizontalStackLayoutStyle>
{
    IPdfHorizontalStackLayoutStyle Spacing(double spacing);
}
