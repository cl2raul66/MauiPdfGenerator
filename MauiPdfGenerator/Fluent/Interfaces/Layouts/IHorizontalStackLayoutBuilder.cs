namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Interface for building a Horizontal Stack Layout.
/// Arranges child elements horizontally.
/// </summary>
public interface IHorizontalStackLayoutBuilder : ILayoutContainerBuilder, ILayoutConfigurator<IHorizontalStackLayoutBuilder>
{
    /// <summary>
    /// Sets the horizontal space between child elements in the stack.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IHorizontalStackLayoutBuilder Spacing(float value);

    // Podrían añadirse HorizontalOptions, VerticalOptions si queremos controlar
    // la alineación de los hijos DENTRO del HSL.
}
