using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Interface used within the .Children() lambda to add content
/// (elements or nested layouts) to a container.
/// Methods return the specific builder for chaining position/styling.
/// </summary>
public interface IPdfContainerContentBuilder
{
    /// <summary>
    /// Adds and configures a Paragraph element.
    /// </summary>
    /// <param name="configAction">Action to configure the paragraph.</param>
    /// <returns>The configured IPdfParagraphBuilder for further chaining (e.g., .Row/.Column).</returns>
    IPdfParagraphBuilder Paragraph(Action<IPdfParagraphBuilder> configAction);

    /// <summary>
    /// Adds and configures an Image element.
    /// </summary>
    /// <param name="configAction">Action to configure the image.</param>
    /// <returns>The configured IPdfImageBuilder for further chaining.</returns>
    IPdfImageBuilder Image(Action<IPdfImageBuilder> configAction);

    /// <summary>
    /// Adds and configures a Grid layout container.
    /// </summary>
    /// <param name="configAction">Action to configure the grid layout.</param>
    /// <returns>The configured IPdfGridBuilder for further chaining.</returns>
    IPdfGridBuilder Grid(Action<IPdfGridBuilder> configAction);

    /// <summary>
    /// Adds and configures a Vertical Stack Layout container.
    /// </summary>
    /// <param name="configAction">Action to configure the vertical stack layout.</param>
    /// <returns>The configured IPdfVerticalStackLayoutBuilder for further chaining.</returns>
    IPdfVerticalStackLayoutBuilder VerticalStackLayout(Action<IPdfVerticalStackLayoutBuilder> configAction);

    /// <summary>
    /// Adds and configures a Horizontal Stack Layout container.
    /// </summary>
    /// <param name="configAction">Action to configure the horizontal stack layout.</param>
    /// <returns>The configured IPdfHorizontalStackLayoutBuilder for further chaining.</returns>
    IPdfHorizontalStackLayoutBuilder HorizontalStackLayout(Action<IPdfHorizontalStackLayoutBuilder> configAction);    
}
