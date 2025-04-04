namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Builds a Vertical Stack Layout, arranging children vertically.
/// </summary>
public interface IPdfVerticalStackLayoutBuilder : IPdfViewBuilder<IPdfVerticalStackLayoutBuilder>
{
    /// <summary>
    /// Sets the vertical space between child elements.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfVerticalStackLayoutBuilder Spacing(double value);

    /// <summary>
    /// Defines the children (elements and layouts) within this Vertical Stack Layout.
    /// </summary>
    /// <param name="childrenAction">Action to add content.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfVerticalStackLayoutBuilder Children(Action<IPdfContainerContentBuilder> childrenAction);
}
