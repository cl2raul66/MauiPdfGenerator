namespace MauiPdfGenerator.Fluent.Styles;

/// <summary>
/// Defines a reusable set of style or configuration properties for image elements.
/// </summary>
public class ImageStyle // Renamed from ImageOptions
{
    /// <summary>
    /// Specifies the encoding quality (e.g., for JPEG, 0-100).
    /// </summary>
    public int? Quality { get; set; }

    // Add other style/config properties as needed:
    // public ImageAspect Aspect { get; set; } // Example: Fill, AspectFit, AspectFill
    // public Rotation Rotation { get; set; } // Example: Rotate0, Rotate90, ...
    // ...

    public ImageStyle() { }
}
