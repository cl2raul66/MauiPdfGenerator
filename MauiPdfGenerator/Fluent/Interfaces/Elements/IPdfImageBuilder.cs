using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

/// <summary>
/// Builds an image element to be included in the PDF document.
/// Inspired by Microsoft.Maui.Controls.Image.
/// </summary>
public interface IPdfImageBuilder : IPdfViewBuilder<IPdfImageBuilder> // Inherits Width, Height, Margin, Alignments, BgColor etc.
{
    // --- Image Source Definition (Choose ONE) ---

    /// <summary>
    /// Sets the image source from a file path or a network URL.
    /// The implementation will handle resolving the path or downloading the URL.
    /// Calling this clears previously set sources (Stream, Byte Array).
    /// </summary>
    /// <param name="pathOrUrl">The local file system path or the absolute URL of the image.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfImageBuilder Source(string pathOrUrl);

    /// <summary>
    /// Sets the image source from a byte array containing the image data
    /// (e.g., loaded from a database, generated in memory).
    /// Calling this clears previously set sources (Path/URL, Stream).
    /// </summary>
    /// <param name="imageData">The byte array representing the image (e.g., PNG, JPEG data).</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfImageBuilder Source(byte[] imageData);

    /// <summary>
    /// Sets the image source from a Stream.
    /// Calling this clears previously set sources (Path/URL, Byte Array).
    /// IMPORTANT: The caller may be responsible for the lifetime and disposal of the stream
    /// depending on the underlying PDF library's implementation. Check documentation.
    /// The stream should be readable and positioned at the beginning.
    /// </summary>
    /// <param name="imageStream">The stream containing the image data.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfImageBuilder Source(Stream imageStream);

    // --- Image Scaling and Presentation ---

    /// <summary>
    /// Sets how the image should be resized or positioned to fill its allocated space,
    /// controlling aspect ratio preservation and scaling.
    /// </summary>
    /// <param name="aspect">The aspect scaling option (e.g., AspectFit, AspectFill, Fill, Center).</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfImageBuilder Aspect(PdfAspect aspect);

    // --- PDF Specific Enhancements (Future Considerations) ---
    // IPdfImageBuilder Rotate(double degrees);
    // IPdfImageBuilder Opacity(double value); // 0.0 to 1.0
    // IPdfImageBuilder CompressionQuality(float quality); // 0.0f to 1.0f
}
