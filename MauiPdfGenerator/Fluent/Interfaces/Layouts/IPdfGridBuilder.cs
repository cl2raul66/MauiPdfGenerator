using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Builds a Grid Layout, arranging children in rows and columns.
/// </summary>
public interface IPdfGridBuilder : IPdfViewBuilder<IPdfGridBuilder>
{
    /// <summary>
    /// Defines all grid columns using PdfGridLength definitions. Replaces existing definitions.
    /// Use with 'using static MauiPdfGenerator.Fluent.Models.PdfGridLength;'.
    /// Example: .ColumnDefinitions(Fixed(50), Star(1), Auto)
    /// </summary>
    /// <param name="widths">An array of PdfGridLength structs defining column widths.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfGridBuilder ColumnDefinitions(params PdfGridLength[] widths);

    /// <summary>
    /// Defines all grid rows using PdfGridLength definitions. Replaces existing definitions.
    /// Use with 'using static MauiPdfGenerator.Fluent.Models.PdfGridLength;'.
    /// Example: .RowDefinitions(Auto, Star(2))
    /// </summary>
    /// <param name="heights">An array of PdfGridLength structs defining row heights.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfGridBuilder RowDefinitions(params PdfGridLength[] heights);

    // --- Could add AddColumnDefinition / AddRowDefinition later if needed ---

    /// <summary>
    /// Sets the spacing between columns.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfGridBuilder ColumnSpacing(double value);

    /// <summary>
    /// Sets the spacing between rows.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfGridBuilder RowSpacing(double value);

    /// <summary>
    /// Sets uniform padding inside the grid borders.
    /// </summary>
    IPdfGridBuilder Padding(double uniformPadding);

    /// <summary>
    /// Sets horizontal and vertical padding inside the grid borders.
    /// </summary>
    IPdfGridBuilder Padding(double horizontal, double vertical);

    /// <summary>
    /// Sets individual padding values inside the grid borders.
    /// </summary>
    IPdfGridBuilder Padding(double left, double top, double right, double bottom);


    /// <summary>
    /// Defines the children (elements and layouts) within this Grid.
    /// Use .Row(), .Column(), .RowSpan(), .ColumnSpan() extension methods
    /// on the returned builders to position children within the grid cells.
    /// </summary>
    /// <param name="childrenAction">Action to add content.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfGridBuilder Children(Action<IPdfContainerContentBuilder> childrenAction);
}
