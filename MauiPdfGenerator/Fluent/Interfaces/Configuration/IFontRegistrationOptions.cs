namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

/// <summary>
/// Provides options for configuring how a specific font is registered for the PDF document.
/// </summary>
public interface IFontRegistrationOptions
{
    /// <summary>
    /// Marks this font as the default font to be used for text elements
    /// in the PDF document unless another font is explicitly specified.
    /// If multiple fonts are marked as default, the last one registered will be used.
    /// </summary>
    /// <returns>The font registration options instance for further chaining.</returns>
    IFontRegistrationOptions Default();

    /// <summary>
    /// Specifies whether this font should be embedded within the generated PDF file.
    /// Embedding fonts ensures that the document displays correctly even on systems
    /// where the font is not installed, but increases the file size.
    /// It is highly recommended for custom fonts or fonts not commonly available.
    /// </summary>
    /// <param name="embed">True to embed the font (default), False otherwise.</param>
    /// <returns>The font registration options instance for further chaining.</returns>
    IFontRegistrationOptions IsEmbeddedFont(bool embed = true);

    // Future potential methods: .Subset(), .Encoding(...)
}
