using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Styles;

/// <summary>
/// Represents a style for a VerticalStackLayout element.
/// This interface closes the generic type chain for styling purposes.
/// </summary>
public interface IPdfVerticalStackLayoutStyle : IPdfVerticalStackLayout<IPdfVerticalStackLayoutStyle>
{
    IPdfVerticalStackLayoutStyle Spacing(double spacing);
}
