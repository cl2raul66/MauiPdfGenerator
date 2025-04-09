using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Interface for configuring global document settings.
/// </summary>
public interface IDocumentConfigurator
{
    /// <summary>
    /// Sets the default page size using a predefined standard (e.g., A4, Letter).
    /// </summary>
    /// <param name="size">The predefined page size.</param>
    /// <returns>The document configurator instance for chaining.</returns>
    IDocumentConfigurator PageSize(PageSizeType size);

    // Futuro: Sobrecarga para tamaño personalizado
    // IDocumentConfigurator PageSize(float width, float height);

    /// <summary>
    /// Sets the default spacing between elements added directly to pages (if not overridden).
    /// </summary>
    /// <param name="value">The spacing value (units depend on the underlying PDF library).</param>
    /// <returns>The document configurator instance for chaining.</returns>
    IDocumentConfigurator Spacing(float value);

    /// <summary>
    /// Sets the default uniform page margins (same for top, right, bottom, left).
    /// </summary>
    /// <param name="uniformMargin">The margin value.</param>
    /// <returns>The document configurator instance for chaining.</returns>
    IDocumentConfigurator Margins(float uniformMargin);

    // Futuro: Sobrecarga para márgenes individuales
    // IDocumentConfigurator Margins(float top, float right, float bottom, float left);
    // IDocumentConfigurator Margins(Thicknesss margins); // Usando un modelo

    // --- These would now likely take Actions with specific configurator interfaces ---
    IDocumentConfigurator Metadata(Action<IMetadataConfigurator> metadataAction); // Placeholder interface
    IDocumentConfigurator SetSecurity(Action<ISecurityConfigurator> securityAction); // Placeholder interface
}

// Placeholder interfaces needed for Metadata/Security Actions
public interface IMetadataConfigurator { /* Title, Author, Subject... */ }
public interface ISecurityConfigurator { /* Password, Permissions... */ }
