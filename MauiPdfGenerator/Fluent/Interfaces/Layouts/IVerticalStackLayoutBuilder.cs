namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Interface for building a Vertical Stack Layout.
/// Arranges child elements vertically.
/// </summary>
public interface IVerticalStackLayoutBuilder : ILayoutContainerBuilder, ILayoutConfigurator<IVerticalStackLayoutBuilder>
{
    /// <summary>
    /// Sets the vertical space between child elements in the stack.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IVerticalStackLayoutBuilder Spacing(float value);

    // Podrían añadirse HorizontalOptions, VerticalOptions si queremos controlar
    // la alineación de los hijos DENTRO del VSL (aunque MAUI lo hace en los hijos)
}
