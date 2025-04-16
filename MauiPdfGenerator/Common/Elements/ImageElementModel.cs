using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common.Elements;

/// <summary>
/// Represents an image element to be drawn on the page within a specific rectangle.
/// </summary>
internal class ImageElementModel : ElementModel // Position from base class might be redundant if TargetRect is primary
{
    /// <summary>
    /// The raw byte data of the image (e.g., encoded JPEG, PNG).
    /// </summary>
    public byte[] ImageData { get; } // Made readonly after construction

    /// <summary>
    /// The rectangle on the page where the image should be drawn.
    /// The image will be scaled to fit this rectangle, preserving aspect ratio by default (handled by Core).
    /// Units are defined by DocumentSettings.Units.
    /// </summary>
    public PdfRect TargetRect { get; } // Made readonly after construction

    /// <summary>
    /// Creates a new Image Element Model.
    /// </summary>
    /// <param name="imageData">The raw byte data of the image.</param>
    /// <param name="targetRect">The target rectangle on the page.</param>
    public ImageElementModel(byte[] imageData, PdfRect targetRect)
    {
        ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
        if (imageData.Length == 0)
            throw new ArgumentException("Image data cannot be empty.", nameof(imageData));

        TargetRect = targetRect;
        Position = targetRect.Location; // Set base Position for consistency
    }
}
