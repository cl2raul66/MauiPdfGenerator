using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public partial class PdfImage : PdfElement
{
    public Aspect CurrentAspect { get; private set; } = Microsoft.Maui.Aspect.AspectFit;

    internal double? RequestedWidth { get; private set; }

    internal double? RequestedHeight { get; private set; }

    internal object SourceData { get; }

    internal PdfImageSourceKind DeterminedSourceKind { get; }

    // --- Constructors ---
    public PdfImage(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        SourceData = stream;
        DeterminedSourceKind = PdfImageSourceKind.Stream;
    }

    public PdfImage(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        SourceData = uri;
        DeterminedSourceKind = PdfImageSourceKind.Uri;
    }

    public PdfImage(string source, PdfImageSourceType sourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);
        SourceData = source;

        DeterminedSourceKind = sourceType switch
        {
            PdfImageSourceType.IsMauiSource => PdfImageSourceKind.Resource,
            PdfImageSourceType.IsFileSource => PdfImageSourceKind.File,
            PdfImageSourceType.IsUriSource => PdfImageSourceKind.Uri,
            _ => DetermineSourceKindFromString(source)
        };
    }

    private static PdfImageSourceKind DetermineSourceKindFromString(string source)
    {
        // 1. Check if it's a valid absolute URI
        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFile))
        {
            return PdfImageSourceKind.Uri;
        }

        // 2. Check if it looks like a file path and exists (basic check)
        //    This is imperfect but a common case. Might need refinement for cross-platform paths.
        try
        {
            // Avoid DirectoryNotFoundException for paths like "C:"
            if (Path.IsPathRooted(source) && File.Exists(source))
            {
                return PdfImageSourceKind.File;
            }
            // Basic check for relative paths that might exist (less reliable)
            if (!Path.IsPathRooted(source) && File.Exists(Path.GetFullPath(source)))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: PdfImage source '{source}' interpreted as a relative file path. Consider using an absolute path or IsFileSource type for clarity.");
                return PdfImageSourceKind.File;
            }
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is IOException || ex is System.Security.SecurityException)
        {
            System.Diagnostics.Debug.WriteLine($"Info: Exception while checking if source '{source}' is a file: {ex.Message}. Assuming it's not a file path.");
            // Ignore exceptions related to invalid path chars or permissions, proceed to assume it's a resource
        }


        // 3. Default to MAUI Resource
        return PdfImageSourceKind.Resource;
    }

    // --- Fluent API Methods ---
    public PdfImage WidthRequest(double width)
    {
        RequestedWidth = width > 0 ? width : null;
        return this;
    }

    public PdfImage HeightRequest(double height)
    {
        RequestedHeight = height > 0 ? height : null;
        return this;
    }

    public PdfImage Aspect(Aspect aspect)
    {
        CurrentAspect = aspect;
        return this;
    }
}
