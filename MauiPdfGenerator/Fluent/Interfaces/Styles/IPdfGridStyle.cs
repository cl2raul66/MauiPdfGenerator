using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Styles;

/// <summary>
/// Represents a style for a Grid element.
/// This interface closes the generic type chain for styling purposes.
/// </summary>
public interface IPdfGridStyle : IPdfGrid<IPdfGridStyle>
{
    IPdfGridStyle RowSpacing(double spacing);
    IPdfGridStyle ColumnSpacing(double spacing);
}
