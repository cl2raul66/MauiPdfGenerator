namespace MauiPdfGenerator.Core.Images;

/// <summary>
/// Factory to create the appropriate IPdfImageProcessor.
/// Initially uses SkiaSharp, but can be modified to use platform-specific implementations.
/// </summary>
internal static class PdfImageProcessorFactory
{
    public static IPdfImageProcessor Create()
    {
        // --- FUTURE: Platform-specific implementations ---
        // #if ANDROID
        // return new AndroidImageProcessor();
        // #elif IOS
        // return new IosImageProcessor();
        // #elif WINDOWS
        // return new WindowsImageProcessor();
        // #else
        // throw new PlatformNotSupportedException("No native image processor available for this platform.");
        // #endif

        // --- CURRENT: Use SkiaSharp ---
        return new SkiaSharpImageProcessor();
    }
}
