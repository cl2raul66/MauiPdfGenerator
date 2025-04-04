namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Builds a Horizontal Stack Layout, arranging children horizontally.
/// </summary>
public interface IPdfHorizontalStackLayoutBuilder : IPdfViewBuilder<IPdfHorizontalStackLayoutBuilder>
{
    /// <summary>
    /// Sets the horizontal space between child elements.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfHorizontalStackLayoutBuilder Spacing(double value);

    /// <summary>
    /// Defines the children (elements and layouts) within this Horizontal Stack Layout.
    /// </summary>
    /// <param name="childrenAction">Action to add content.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfHorizontalStackLayoutBuilder Children(Action<IPdfContainerContentBuilder> childrenAction);
}
