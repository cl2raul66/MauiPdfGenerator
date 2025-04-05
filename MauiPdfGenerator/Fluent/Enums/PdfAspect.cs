namespace MauiPdfGenerator.Fluent.Enums;

/// <summary>
/// Defines how an image is displayed within its bounds, handling scaling and aspect ratio.
/// Mirrors Microsoft.Maui.Aspect.
/// </summary>
public enum PdfAspect
{
    /// <summary>
    /// Scale the image to fit the view, preserving aspect ratio. Letterboxing/pillarboxing may occur.
    /// </summary>
    AspectFit,
    /// <summary>
    /// Scale the image to fill the view, preserving aspect ratio. Cropping may occur.
    /// </summary>
    AspectFill,
    /// <summary>
    /// Scale the image to fill the view completely, potentially distorting the aspect ratio.
    /// </summary>
    Fill,
    /// <summary>
    /// Center the image within the view without scaling it.
    /// </summary>
    Center
}
