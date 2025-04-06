namespace MauiPdfGenerator.Fluent.Builders;

/// <summary>
/// Internal helper to store a child element/layout builder along with its grid positioning info.
/// </summary>
internal class GridChildInfo
{
    public object Builder { get; } // The actual builder (e.g., ParagraphBuilder, ImageBuilder)
    public int Row { get; internal set; } = 0;
    public int Column { get; internal set; } = 0;
    public int RowSpan { get; internal set; } = 1;
    public int ColumnSpan { get; internal set; } = 1;

    public GridChildInfo(object builder)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    // Static dictionary to temporarily hold positioning info set by extension methods
    // Key: Instance of the builder, Value: GridChildInfo with position set
    // NOTE: This approach using a static dictionary might have issues with concurrency
    // if builders were somehow shared across threads (unlikely for Fluent API usage, but possible).
    // A more robust approach might involve an attached property mechanism or passing context.
    // For single-threaded Fluent API usage, this is simpler.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, GridChildInfo> _positionMap =
        new System.Runtime.CompilerServices.ConditionalWeakTable<object, GridChildInfo>();

    /// <summary>
    /// Stores temporary position info for a builder, associated via ConditionalWeakTable.
    /// Called by the .Row(), .Column(), etc. extension methods.
    /// </summary>
    internal static void SetPositionInfo(object builder, int row, int column, int rowSpan, int columnSpan)
    {
        // Use GetOrCreateValue to ensure we have an entry for this builder instance
        // and update its position.
        var info = _positionMap.GetOrCreateValue(builder);
        info.Row = row;
        info.Column = column;
        info.RowSpan = rowSpan;
        info.ColumnSpan = columnSpan;
    }

    /// <summary>
    /// Retrieves and removes the stored position info for a builder.
    /// Called by GridBuilder when adding the child.
    /// </summary>
    internal static GridChildInfo GetAndRemovePositionInfo(object builder)
    {
        // TryGetValue gets the info if it exists.
        // Remove ensures we clean up the weak table entry.
        if (_positionMap.TryGetValue(builder, out var info))
        {
            _positionMap.Remove(builder);
            return info;
        }
        // Return default position if no extension methods were called for this child
        return new GridChildInfo(builder);
    }
}
