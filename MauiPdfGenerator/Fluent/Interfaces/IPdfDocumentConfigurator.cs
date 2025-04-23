using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Provides methods for configuring global settings and metadata for the PDF document.
/// </summary>
public interface IPdfDocumentConfigurator
{
    /// <summary>
    /// Sets the default page size for the document (e.g., A4, Letter).
    /// </summary>
    /// <param name="sizeType">The desired page size type.</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator PageSize(PageSizeType sizeType);
    // Considerar añadir: PageSize(float width, float height) para Custom

    /// <summary>
    /// Sets the default page orientation (Portrait or Landscape).
    /// </summary>
    /// <param name="orientationType">The desired page orientation.</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator PageOrientation(PageOrientationType orientationType);

    /// <summary>
    /// Sets uniform margins for all page edges.
    /// </summary>
    /// <param name="uniformMargin">The margin value (in points) to apply to all sides.</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator Margins(float uniformMargin);

    /// <summary>
    /// Sets vertical and horizontal margins for the pages.
    /// </summary>
    /// <param name="verticalMargin">The margin value (in points) for top and bottom edges.</param>
    /// <param name="horizontalMargin">The margin value (in points) for left and right edges.</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator Margins(float verticalMargin, float horizontalMargin);

    /// <summary>
    /// Sets individual margins for each page edge.
    /// </summary>
    /// <param name="leftMargin">Left margin (in points).</param>
    /// <param name="topMargin">Top margin (in points).</param>
    /// <param name="rightMargin">Right margin (in points).</param>
    /// <param name="bottomMargin">Bottom margin (in points).</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin);

    /// <summary>
    /// Configures the standard PDF metadata (Title, Author, etc.).
    /// </summary>
    /// <param name="metadataAction">An action that receives an <see cref="IPdfMetaData"/> instance for setting metadata properties.</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator MetaData(Action<IPdfMetaData> metadataAction);

    /// <summary>
    /// Configures the fonts available for use within the PDF document.
    /// Allows registering fonts known to the MAUI application by their alias.
    /// </summary>
    /// <param name="fontRegistryAction">An action that receives an <see cref="IPdfFontRegistry"/> instance for registering fonts.</param>
    /// <returns>The document configurator instance for further chaining.</returns>
    IPdfDocumentConfigurator PdfFontRegistry(Action<IPdfFontRegistry> fontRegistryAction);
}
