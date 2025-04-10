using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Core.Images; // Necesario para PdfImageXObject
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Implementation.Layout.Managers;

namespace MauiPdfGenerator.Implementation.Builders;

/// <summary>
/// Internal implementation for building PDF Image elements.
/// Stores configuration set via the fluent API.
/// </summary>
internal class ImageBuilder : IPdfImageBuilder, IInternalViewBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfResources _resources;

    // --- Configuration Storage ---

    // Source (only one should be actively used)
    private string? _sourcePathOrUrl;
    private byte[]? _sourceBytes;
    private Stream? _sourceStream;
    private PdfImageSourceType _sourceType = PdfImageSourceType.None;

    // Presentation
    private PdfAspect _aspect = PdfAspect.AspectFit; // Default aspect

    // View Properties (from IPdfViewBuilder)
    private double? _explicitWidth;
    private double? _explicitHeight;
    private Thickness _margin = Thickness.Zero; // Usa el struct auxiliar definido en ParagraphBuilder.cs o muévelo a Common
    private PdfHorizontalAlignment _horizontalOptions = PdfHorizontalAlignment.Start;
    private PdfVerticalAlignment _verticalOptions = PdfVerticalAlignment.Start;
    private Color? _backgroundColor; // Less common for images, but possible

    // Propiedades públicas para PageBuilder/Layout
    public PdfImageSourceType ConfiguredSourceType => _sourceType;
    public string? ConfiguredSourcePathOrUrl => _sourcePathOrUrl;
    public byte[]? ConfiguredSourceBytes => _sourceBytes;
    public Stream? ConfiguredSourceStream => _sourceStream;
    public PdfAspect ConfiguredAspect => _aspect;
    public double? ConfiguredWidth => _explicitWidth;
    public double? ConfiguredHeight => _explicitHeight;
    public Thickness ConfiguredMargin => _margin;
    public PdfHorizontalAlignment ConfiguredHorizontalOptions => _horizontalOptions;
    public PdfVerticalAlignment ConfiguredVerticalOptions => _verticalOptions;
    public Color? ConfiguredBackgroundColor => _backgroundColor;

    // Almacena el objeto XObject una vez creado (durante Finalize/Layout)
    internal PdfImageXObject? PdfImageXObject { get; private set; }

    public ImageBuilder(PdfDocument pdfDocument, PdfResources resources)
    {
        _pdfDocument = pdfDocument;
        _resources = resources;
    }

    // --- IPdfImageBuilder Implementation ---

    public IPdfImageBuilder Source(string pathOrUrl)
    {
        ClearSources();
        _sourcePathOrUrl = pathOrUrl;
        _sourceType = PdfImageSourceType.PathOrUrl;
        return this;
    }

    public IPdfImageBuilder Source(byte[] imageData)
    {
        ClearSources();
        _sourceBytes = imageData;
        _sourceType = PdfImageSourceType.Bytes;
        return this;
    }

    public IPdfImageBuilder Source(Stream imageStream)
    {
        ClearSources();
        _sourceStream = imageStream;
        _sourceType = PdfImageSourceType.Stream;
        return this;
    }

    public IPdfImageBuilder Aspect(PdfAspect aspect)
    {
        _aspect = aspect;
        return this;
    }

    // --- IPdfViewBuilder Implementation ---

    public IPdfImageBuilder Width(double width)
    {
        _explicitWidth = width >= 0 ? width : null;
        return this;
    }

    public IPdfImageBuilder Height(double height)
    {
        _explicitHeight = height >= 0 ? height : null;
        return this;
    }

    public IPdfImageBuilder Margin(double uniformMargin)
    {
        _margin = new Thickness(uniformMargin);
        return this;
    }

    public IPdfImageBuilder Margin(double horizontal, double vertical)
    {
        _margin = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfImageBuilder Margin(double left, double top, double right, double bottom)
    {
        _margin = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfImageBuilder HorizontalOptions(PdfHorizontalAlignment alignment)
    {
        _horizontalOptions = alignment;
        return this;
    }

    public IPdfImageBuilder VerticalOptions(PdfVerticalAlignment alignment)
    {
        _verticalOptions = alignment;
        return this;
    }

    public IPdfImageBuilder BackgroundColor(Color? color)
    {
        _backgroundColor = color;
        return this;
    }

    // --- Internal Methods ---

    /// <summary>
    /// Called during the layout/drawing phase to ensure the PdfImageXObject is created.
    /// </summary>
    internal PdfImageXObject GetOrCreatePdfImageXObject()
    {
        if (PdfImageXObject == null)
        {
            CreatePdfImageXObject();
            if (PdfImageXObject == null)
            {
                // Fallback or error handling if creation failed
                throw new InvalidOperationException("Failed to create PDF image object. Ensure a valid source was provided.");
            }
        }
        return PdfImageXObject;
    }


    // --- Private Helpers ---

    private void ClearSources()
    {
        _sourcePathOrUrl = null;
        _sourceBytes = null;
        // We might not want to dispose the stream here if it was passed externally.
        // The caller who provided the stream is responsible for its lifetime.
        // If we created a MemoryStream internally, we should dispose it. Needs careful handling.
        _sourceStream = null; // Clear the reference
        _sourceType = PdfImageSourceType.None;
        PdfImageXObject = null; // Reset created object if source changes
    }

    private void CreatePdfImageXObject()
    {
        if (_sourceType == PdfImageSourceType.None)
        {
            Console.WriteLine("Warning: Attempting to create image object with no source set.");
            return; // Or throw?
        }

        try
        {
            // TODO: Handle path/URL resolution properly. Needs access to file system or HTTP client.
            // For MAUI resources (like "dotnet_bot.png"), we need a way to get a Stream.
            // This might involve platform services or MAUI helpers if we integrate more deeply.
            // For now, assume pathOrUrl is a direct file path for simplicity.

            Stream? imageStream = null;
            bool ownStream = false; // Flag to know if we need to dispose the stream

            switch (_sourceType)
            {
                case PdfImageSourceType.PathOrUrl:
                    if (!string.IsNullOrEmpty(_sourcePathOrUrl))
                    {
                        // Simplistic file handling - NO URL or MAUI Resource handling yet!
                        if (File.Exists(_sourcePathOrUrl))
                        {
                            imageStream = new FileStream(_sourcePathOrUrl, FileMode.Open, FileAccess.Read, FileShare.Read);
                            ownStream = true; // We opened it, we should close it
                        }
                        else
                        {
                            Console.WriteLine($"Error: Image file not found at path: {_sourcePathOrUrl}");
                            // How to handle MAUI embedded resources? Needs investigation.
                            // Example: var stream = await FileSystem.OpenAppPackageFileAsync(path); ??
                            throw new FileNotFoundException("Image file not found (URL/Resource handling not implemented).", _sourcePathOrUrl);
                        }
                    }
                    break;
                case PdfImageSourceType.Bytes:
                    if (_sourceBytes != null)
                    {
                        imageStream = new MemoryStream(_sourceBytes);
                        ownStream = true; // We created it from bytes
                    }
                    break;
                case PdfImageSourceType.Stream:
                    imageStream = _sourceStream;
                    ownStream = false; // Assume external stream, caller manages lifetime
                    break;
            }

            if (imageStream != null)
            {
                try
                {
                    // Use the Core factory method which uses the abstracted IPdfImageProcessor
                    PdfImageXObject = PdfImageXObject.Create(_pdfDocument, imageStream);
                }
                finally
                {
                    if (ownStream)
                    {
                        imageStream.Dispose(); // Dispose stream only if we created it
                    }
                    // Do not dispose externally provided streams
                }
            }
            else
            {
                throw new InvalidOperationException("Could not obtain a valid image stream from the specified source.");
            }

        }
        catch (Exception ex)
        {
            // Log error appropriately
            Console.WriteLine($"Error creating PDF image object: {ex.Message}");
            // Optionally re-throw or handle gracefully
            // throw; // Re-throw to indicate failure
            PdfImageXObject = null; // Ensure it's null on failure
        }
    }

} // Fin clase ImageBuilder
// Fin namespace
