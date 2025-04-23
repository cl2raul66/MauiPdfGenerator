namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Provides methods to register fonts that can be used within the PDF document.
/// Relies on fonts already configured in the .NET MAUI application.
/// </summary>
public interface IPdfFontRegistry
{
    /// <summary>
    /// Registers a font for use in the PDF, referencing it by the alias
    /// used during its registration in MauiProgram.ConfigureFonts (e.g., "OpenSansRegular").
    /// The library will attempt to locate and utilize this pre-registered MAUI font.
    /// </summary>
    /// <param name="fontFamilyAlias">The alias assigned to the font in the MAUI application's font configuration.</param>
    /// <returns>An <see cref="IFontRegistrationOptions"/> instance to configure this font registration further (e.g., mark as default, specify embedding).</returns>
    IFontRegistrationOptions Font(string fontFamilyAlias);

    /* Sobrecargas eliminadas basadas en la discusión anterior, pero mantenidas aquí comentadas por si reconsideramos:
    /// <summary>
    /// Registers a custom font from a Stream using a specific alias.
    /// </summary>
    /// <param name="alias">The name to use when referring to this font within the PDF generation context.</param>
    /// <param name="fontStream">The stream containing the font data (e.g., .ttf, .otf).</param>
    /// <returns>An <see cref="IFontRegistrationOptions"/> instance to configure this font registration further.</returns>
    // IFontRegistrationOptions Font(string alias, Stream fontStream);

    /// <summary>
    /// Registers a custom font from a file path using a specific alias.
    /// </summary>
    /// <param name="alias">The name to use when referring to this font within the PDF generation context.</param>
    /// <param name="filePath">The path to the font file (e.g., .ttf, .otf).</param>
    /// <returns>An <see cref="IFontRegistrationOptions"/> instance to configure this font registration further.</returns>
    // IFontRegistrationOptions Font(string alias, string filePath);
    */
}
