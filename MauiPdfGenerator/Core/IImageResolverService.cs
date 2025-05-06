using MauiPdfGenerator.Common.Enums;

namespace MauiPdfGenerator.Core;

internal interface IImageResolverService
{
    Task<Stream?> GetStreamAsync(object sourceData, PdfImageSourceKind kind);
}
