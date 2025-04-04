using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

/// <summary>
/// Builds an image element.
/// </summary>
public interface IPdfImageBuilder : IPdfViewBuilder<IPdfImageBuilder>
{
    /// <summary>
    /// Sets the image source from a file path or URL.
    /// </summary>
    /// <param name="pathOrUrl">The file path or URL of the image.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfImageBuilder Source(string pathOrUrl);

    // --- Overloads for other sources ---
    // IPdfImageBuilder Source(byte[] imageData);
    // IPdfImageBuilder Source(Stream imageStream);

    /// <summary>
    /// Sets how the image should be resized to fill its allocated space.
    /// (Need PdfAspect enum mirroring Microsoft.Maui.Aspect)
    /// </summary>
    /// <param name="aspect">The aspect scaling.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfImageBuilder Aspect(PdfAspect aspect); // Requires PdfAspect Enum
}
