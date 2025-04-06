using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure;

namespace MauiPdfGenerator.Core.Images;

internal class PdfImageXObject : PdfStream
{
    public int Width { get; }
    public int Height { get; }
    public PdfName ColorSpace { get; }
    public int BitsPerComponent { get; }
    public PdfName? ExplicitFilter { get; } // Filter determined by processor

    // Constructor now takes processed data
    private PdfImageXObject(PdfDictionary dictionary, byte[] imageData, int width, int height, PdfName colorSpace, int bpc, PdfName? explicitFilter)
        : base(dictionary, imageData)
    {
        Width = width;
        Height = height;
        ColorSpace = colorSpace;
        BitsPerComponent = bpc;
        ExplicitFilter = explicitFilter; // Store the filter determined by the processor

        // Remove /Filter from base dictionary if data is already compressed (e.g., DCT)
        // The base PdfStream will NOT apply Flate if /Filter is null or missing
        if (explicitFilter != PdfName.FlateDecode) // Only keep Flate for base stream to apply
        {
            Dictionary.Remove(PdfName.Filter);
        }
        else
        {
            // Ensure FlateDecode is set if needed
            Dictionary.Add(PdfName.Filter, PdfName.FlateDecode);
            // TODO: Add DecodeParms if needed (e.g., PNG predictor)
        }
    }

    public static PdfImageXObject Create(PdfDocument document, Stream imageStream)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (imageStream == null) throw new ArgumentNullException(nameof(imageStream));

        // Use the factory to get a processor instance
        using (IPdfImageProcessor processor = PdfImageProcessorFactory.Create())
        {
            processor.Load(imageStream); // Process the stream

            // Extract results from the processor
            int width = processor.Width;
            int height = processor.Height;
            PdfName colorSpace = processor.PdfColorSpace;
            int bpc = processor.BitsPerComponent;
            byte[] imageData = processor.GetImageData();
            PdfName? filter = processor.PdfFilter; // DCTDecode, FlateDecode, or null

            // TODO: Handle Alpha Mask (/SMask) using processor.GetAlphaMaskData()
            // TODO: Handle Indexed Color Space using processor.GetIndexedColorPalette()

            // Create the image dictionary
            var dict = new PdfDictionary();
            dict.Add(PdfName.Type, PdfName.XObject);
            dict.Add(PdfName.Subtype, PdfName.Image);
            dict.Add(PdfName.Width, new PdfNumber(width));
            dict.Add(PdfName.Height, new PdfNumber(height));
            dict.Add(PdfName.ColorSpace, colorSpace); // Use determined color space
            dict.Add(PdfName.BitsPerComponent, new PdfNumber(bpc)); // Use determined BPC

            // Add /Filter only if it's needed for DECODING by the PDF reader
            // (Flate for pixels, maybe LZW, RunLength etc. later).
            // DCT data should NOT have /Filter entry here as it's already encoded.
            if (filter == PdfName.FlateDecode /* || other filters */)
            {
                dict.Add(PdfName.Filter, filter);
                // Add DecodeParms if processor provided them
                // dict.Add(PdfName.DecodeParms, processor.GetDecodeParms());
            }

            // Create the PdfImageXObject instance
            var imageXObject = new PdfImageXObject(dict, imageData, width, height, colorSpace, bpc, filter);

            return imageXObject;
        }
    }

    // --- Add other Create methods (e.g., from byte[]) ---
    public static PdfImageXObject Create(PdfDocument document, byte[] imageData)
    {
        using (var ms = new MemoryStream(imageData))
        {
            return Create(document, ms);
        }
    }
}
