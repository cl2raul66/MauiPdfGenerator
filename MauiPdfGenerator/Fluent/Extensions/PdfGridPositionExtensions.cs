using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator.Fluent.Extensions;

/// <summary>
/// Provides extension methods for positioning elements within a PdfGrid.
/// Mimics MAUI's Grid attached properties.
/// </summary>
public static class PdfGridPositionExtensions
{
    // --- Keys for storing position info (Example - implementation depends on builders) ---
    internal static readonly object RowKey = new object();
    internal static readonly object ColumnKey = new object();
    internal static readonly object RowSpanKey = new object();
    internal static readonly object ColumnSpanKey = new object();

    /// <summary>
    /// Sets the row index for an element within a PdfGrid.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the view builder.</typeparam>
    /// <param name="builder">The view builder instance.</param>
    /// <param name="row">The zero-based row index.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If row is negative.</exception>
    public static TBuilder Row<TBuilder>(this TBuilder builder, int row)
        where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (row < 0) throw new ArgumentOutOfRangeException(nameof(row), "Row index must be non-negative.");
        // --- Implementation Detail ---
        // How this is stored depends on the concrete builder implementation.
        // Option 1: Builders have internal methods/properties.
        // Option 2: Builders use a dictionary/property bag (e.g., via Get/SetProperty on base).
        // builder.SetProperty(RowKey, row); // Example using property bag
        return builder;
    }

    /// <summary>
    /// Sets the column index for an element within a PdfGrid.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the view builder.</typeparam>
    /// <param name="builder">The view builder instance.</param>
    /// <param name="column">The zero-based column index.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If column is negative.</exception>
    public static TBuilder Column<TBuilder>(this TBuilder builder, int column)
        where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (column < 0) throw new ArgumentOutOfRangeException(nameof(column), "Column index must be non-negative.");
        // builder.SetProperty(ColumnKey, column); // Example
        return builder;
    }

    /// <summary>
    /// Sets the number of rows an element spans within a PdfGrid.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the view builder.</typeparam>
    /// <param name="builder">The view builder instance.</param>
    /// <param name="span">The number of rows to span (must be 1 or greater).</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If span is less than 1.</exception>
    public static TBuilder RowSpan<TBuilder>(this TBuilder builder, int span)
        where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (span < 1) throw new ArgumentOutOfRangeException(nameof(span), "Row span must be 1 or greater.");
        // builder.SetProperty(RowSpanKey, span); // Example
        return builder;
    }

    /// <summary>
    /// Sets the number of columns an element spans within a PdfGrid.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the view builder.</typeparam>
    /// <param name="builder">The view builder instance.</param>
    /// <param name="span">The number of columns to span (must be 1 or greater).</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If span is less than 1.</exception>
    public static TBuilder ColumnSpan<TBuilder>(this TBuilder builder, int span)
         where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (span < 1) throw new ArgumentOutOfRangeException(nameof(span), "Column span must be 1 or greater.");
        // builder.SetProperty(ColumnSpanKey, span); // Example
        return builder;
    }

    // --- Helper methods (Internal or Public?) to retrieve the values ---
    // These would be used by the PdfGrid layout logic internally.
    /*
    internal static int GetRow<TBuilder>(this TBuilder builder) where TBuilder : IPdfViewBuilder<TBuilder>
        => (int?)builder.GetProperty(RowKey) ?? 0; // Default to 0 if not set

    internal static int GetColumn<TBuilder>(this TBuilder builder) where TBuilder : IPdfViewBuilder<TBuilder>
        => (int?)builder.GetProperty(ColumnKey) ?? 0; // Default to 0

    internal static int GetRowSpan<TBuilder>(this TBuilder builder) where TBuilder : IPdfViewBuilder<TBuilder>
        => (int?)builder.GetProperty(RowSpanKey) ?? 1; // Default to 1

    internal static int GetColumnSpan<TBuilder>(this TBuilder builder) where TBuilder : IPdfViewBuilder<TBuilder>
        => (int?)builder.GetProperty(ColumnSpanKey) ?? 1; // Default to 1
    */
}
