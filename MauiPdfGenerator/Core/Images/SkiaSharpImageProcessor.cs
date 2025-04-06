using MauiPdfGenerator.Core.Objects;
using SkiaSharp; // Requires SkiaSharp NuGet package

namespace MauiPdfGenerator.Core.Images;

/// <summary>
/// Implementation of IPdfImageProcessor using the SkiaSharp library.
/// Marked internal as it's an implementation detail.
/// </summary>
internal class SkiaSharpImageProcessor : IPdfImageProcessor
{
    private SKCodec? _codec;
    private SKBitmap? _bitmap; // Used only if full decoding is needed
    private byte[]? _imageData; // Stores either DCT data or pixel data
    private Stream? _originalStream; // Keep reference to original stream for JPEGs

    public int Width { get; private set; }
    public int Height { get; private set; }
    public PdfName PdfColorSpace { get; private set; } = PdfName.DeviceRGB; // Sensible default
    public int BitsPerComponent { get; private set; } = 8; // Common default
    public PdfName? PdfFilter { get; private set; }

    public void Load(Stream imageStream)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        if (!imageStream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(imageStream));

        // Keep the original stream reference in case it's needed (e.g., for JPEG)
        _originalStream = imageStream;

        // Ensure stream is seekable for SKCodec.Create
        if (!imageStream.CanSeek)
        {
            // Buffer non-seekable stream into memory
            var ms = new MemoryStream();
            imageStream.CopyTo(ms);
            ms.Position = 0;
            _originalStream = ms; // Use the memory stream now
            LoadInternal(_originalStream);
        }
        else
        {
            imageStream.Position = 0; // Ensure starting at the beginning
            LoadInternal(imageStream);
        }
    }

    private void LoadInternal(Stream seekableStream)
    {
        Dispose(); // Dispose previous resources

        _codec = SKCodec.Create(seekableStream);
        if (_codec is null)
        {
            throw new NotSupportedException("SkiaSharp could not decode the image stream. Unsupported format?");
        }

        Width = _codec.Info.Width;
        Height = _codec.Info.Height;
        BitsPerComponent = GetBestBitsPerComponent(_codec.Info.ColorType);

        SKEncodedImageFormat format = _codec.EncodedFormat;

        if (format == SKEncodedImageFormat.Jpeg)
        {
            PdfFilter = PdfName.DCTDecode;
            // --- CORRECCIÓN: Read original stream for JPEG data ---
            if (_originalStream is null) throw new InvalidOperationException("Original stream reference lost.");
            _originalStream.Position = 0; // Rewind
            using (var reader = new BinaryReader(_originalStream, System.Text.Encoding.Default, leaveOpen: true)) // Use BinaryReader to read bytes easily
            {
                // Read the entire stream into the byte array
                // Need to know stream length, check if possible
                if (_originalStream.CanSeek)
                {
                    _imageData = reader.ReadBytes((int)_originalStream.Length);
                }
                else
                {
                    // If original was not seekable (should have been buffered), this is an issue
                    // Fallback to reading into MemoryStream first if absolutely necessary
                    using (var tempMs = new MemoryStream())
                    {
                        _originalStream.CopyTo(tempMs);
                        _imageData = tempMs.ToArray();
                    }
                    System.Diagnostics.Debug.WriteLine("Warning: Re-reading non-seekable stream into memory for JPEG data.");
                }
            }
            // --- Fin Corrección ---

            PdfColorSpace = MapSkiaColorTypeToPdfColorSpace(_codec.Info.ColorType);
            BitsPerComponent = 8; // Typically 8 for JPEG
        }
        else // PNG, GIF, BMP, WEBP etc. Need to decode pixels
        {
            PdfFilter = PdfName.FlateDecode; // Assume Flate for pixel data

            SKColorType targetColorType = GetPdfCompatibleColorType(_codec.Info.ColorType, _codec.Info.AlphaType);
            PdfColorSpace = MapSkiaColorTypeToPdfColorSpace(targetColorType);
            BitsPerComponent = GetBestBitsPerComponent(targetColorType);

            var bitmapInfo = new SKImageInfo(Width, Height, targetColorType, SKAlphaType.Unpremul);
            _bitmap = SKBitmap.Decode(_codec, bitmapInfo);
            if (_bitmap == null)
            {
                // Attempt fallback decode if first try fails? Maybe just throw.
                throw new InvalidOperationException($"SkiaSharp failed to decode bitmap pixels for format {format}.");
            }

            _imageData = _bitmap.Bytes; // Get the raw pixel buffer

            // TODO: Handle alpha channel - requires creating a separate /SMask stream
        }
    }

    public byte[] GetImageData()
    {
        return _imageData ?? throw new InvalidOperationException("Image data has not been loaded or extracted.");
    }

    private SKColorType GetPdfCompatibleColorType(SKColorType original, SKAlphaType alpha)
    {
        bool hasAlpha = alpha != SKAlphaType.Opaque && alpha != SKAlphaType.Unknown; // Treat Unknown as Opaque? Or handle separately?

        switch (original)
        {
            case SKColorType.Gray8:
                return SKColorType.Gray8;

            // --- CORRECCIÓN: Removed Bgr888x ---
            case SKColorType.Rgb565:
            case SKColorType.Rgb888x:
            case SKColorType.Rgb101010x:
            case SKColorType.Bgra8888: // Often decoded to Rgba by Skia if requested
            case SKColorType.Argb4444:
            case SKColorType.Rgba8888:
            case SKColorType.Rgba1010102:
            case SKColorType.RgbaF16:
                // Decode to RGBA if alpha exists OR if original format implies it (like Rgba*)
                // Decode to RGBX (like Rgb888x if available) or maybe RGBA and ignore alpha later if no alpha needed
                if (hasAlpha || original.ToString().Contains("Rgba") || original.ToString().Contains("Argb") || original.ToString().Contains("Bgra"))
                    return SKColorType.Rgba8888; // Decode to standard RGBA
                else
                    // Need an RGB target without alpha. Rgb888x might work, or decode to Rgba8888 and strip alpha later.
                    // Let's decode to RGBA and handle alpha stripping when getting bytes if needed.
                    // Safest common target is often RGBA.
                    return SKColorType.Rgba8888; // Or maybe Rgb888x if supported as decode target? Testing needed.


            default:
                // Fallback or throw? Let's try RGBA as a general fallback.
                System.Diagnostics.Debug.WriteLine($"Warning: Unhandled original SKColorType '{original}'. Attempting decode to Rgba8888.");
                return SKColorType.Rgba8888;
        }
    }

    private PdfName MapSkiaColorTypeToPdfColorSpace(SKColorType colorType)
    {
        switch (colorType)
        {
            case SKColorType.Gray8:
                return PdfName.DeviceGray;

            // --- CORRECCIÓN: Removed Bgr888x cases, simplified RGB/RGBA mapping ---
            case SKColorType.Rgb565: // Less common, treat as RGB
            case SKColorType.Rgb888x:
            case SKColorType.Rgb101010x:
            case SKColorType.Argb4444: // Treat as RGB (alpha handled separately)
            case SKColorType.Rgba8888: // Treat as RGB (alpha handled separately)
            case SKColorType.Bgra8888: // Treat as RGB (alpha handled separately)
            case SKColorType.Rgba1010102: // Treat as RGB (alpha handled separately)
            case SKColorType.RgbaF16: // Treat as RGB (alpha handled separately, HDR needs care)
                return PdfName.DeviceRGB;

            default:
                System.Diagnostics.Debug.WriteLine($"Warning: Unhandled SKColorType '{colorType}' for PDF ColorSpace mapping. Defaulting to DeviceRGB.");
                return PdfName.DeviceRGB;
        }
    }

    private int GetBestBitsPerComponent(SKColorType colorType)
    {
        // --- CORRECCIÓN: Removed Bgr888x ---
        switch (colorType)
        {
            case SKColorType.Gray8:
            case SKColorType.Rgba8888:
            case SKColorType.Rgb888x:
            case SKColorType.Bgra8888:
                return 8;
            // For Rgb565, effective BPC is mixed (5, 6, 5) - PDF usually wants uniform BPC. Often upscaled to 8.
            case SKColorType.Rgb565:
                System.Diagnostics.Debug.WriteLine("Warning: Rgb565 color type encountered. Assuming 8 BitsPerComponent after potential conversion.");
                return 8;
            case SKColorType.RgbaF16:
                return 16; // HDR Float16
                           // Other high bit depth formats would need mapping
            default:
                return 8; // Default assumption
        }
    }


    public void Dispose()
    {
        _codec?.Dispose(); // Dispose codec
        _codec = null;
        _bitmap?.Dispose(); // Dispose bitmap if created
        _bitmap = null;
        _originalStream = null; // Release stream reference (don't dispose if passed externally and caller manages it?) - Needs policy. Assuming we buffer non-seekable, so internal MemoryStream is ok to dispose/lose ref.
        _imageData = null;
        GC.SuppressFinalize(this);
    }
}
