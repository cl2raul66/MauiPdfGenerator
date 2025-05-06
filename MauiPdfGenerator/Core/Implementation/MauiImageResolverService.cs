using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Core;
using MauiApp = Microsoft.Maui.Controls.Application;
using MauiPdfGenerator.Common.Enums;


namespace MauiPdfGenerator.Core.Services.Implementation; // Ajusta el namespace

internal class MauiImageResolverService : IImageResolverService
{
    private static readonly HttpClient _httpClient = new();
    private readonly IServiceProvider _services;

    public MauiImageResolverService()
    {
        // Obtener el proveedor de servicios de la aplicación MAUI.
        // Esto asume que el servicio se crea en un punto donde Application.Current está disponible y tiene servicios.
        _services = MauiApp.Current?.Services ?? throw new InvalidOperationException("MAUI IServiceProvider not available.");
    }

    public async Task<Stream?> GetStreamAsync(object sourceData, PdfImageSourceKind kind)
    {
        try
        {
            switch (kind)
            {
                // ... (Casos Stream, File, Uri permanecen iguales) ...
                case PdfImageSourceKind.Stream when sourceData is Stream s:
                    if (!s.CanRead) { System.Diagnostics.Debug.WriteLine("Error: Provided image stream is not readable."); return null; }
                    if (s.CanSeek) { s.Position = 0; }
                    return s;

                case PdfImageSourceKind.File when sourceData is string filePath:
                    if (File.Exists(filePath)) { return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); }
                    else { System.Diagnostics.Debug.WriteLine($"Error: Image file not found at path: {filePath}"); return null; }

                case PdfImageSourceKind.Uri when sourceData is Uri uri:
                    if (uri.IsFile)
                    {
                        string localPath = uri.LocalPath;
                        if (File.Exists(localPath)) { return new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read); }
                        else { System.Diagnostics.Debug.WriteLine($"Error: Image file not found for local file URI: {uri}"); return null; }
                    }
                    else
                    {
                        var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode) { return await response.Content.ReadAsStreamAsync().ConfigureAwait(false); }
                        else { System.Diagnostics.Debug.WriteLine($"Error: Failed to download image from URI: {uri}. Status: {response.StatusCode}"); return null; }
                    }

                case PdfImageSourceKind.Resource when sourceData is string resourceName:
                    // --- INTENTO CON IImageSourceService ---
                    // Esto asume que resourceName es algo que FileImageSource puede entender
                    // para un recurso con acción de compilación MauiImage.
                    System.Diagnostics.Debug.WriteLine($"Attempting to load MauiImage resource via IImageSourceService: {resourceName}");

                    // Crear el FileImageSource. Para recursos de app, MAUI lo resuelve.
                    var imageSource = ImageSource.FromFile(resourceName);

                    if (imageSource == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: Could not create FileImageSource for resource: {resourceName}");
                        return null;
                    }

                    // Necesitamos el servicio para FileImageSource
                    var imageSourceService = _services.GetService<IImageSourceService<IFileImageSource>>();
                    if (imageSourceService == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: IImageSourceService<IFileImageSource> not found in services.");
                        // Podrías intentar con IImageSourceService<IImageSource> como fallback, pero es menos específico
                        var genericService = _services.GetService<IImageSourceService<ImageSource>>();
                        if (genericService != null && imageSource is IFileImageSource fis)
                        {
                            // Esto es un poco un hack, intentando usar el servicio genérico si el específico no está
                            // Puede que no funcione como se espera o que el método no sea compatible.
                            var result = await genericService.GetImageAsync(fis, 1.0f); // Escala 1.0f
                            if (result?.Value is Microsoft.Maui.Graphics.IImage mauiImage)
                            {
                                return await ConvertMauiImageToStreamAsync(mauiImage);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Error: Could not get IImage from generic IImageSourceService for {resourceName}. Error: {result?.Error}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Error: Generic IImageSourceService not found or type mismatch.");
                            return null;
                        }
                        return null;
                    }

                    if (imageSource is IFileImageSource fileImageSource)
                    {
                        // El segundo argumento es scale, 1.0f para tamaño original
                        var result = await imageSourceService.GetImageAsync(fileImageSource, 1.0f);

                        if (result?.Value is Microsoft.Maui.IImage mauiImage)
                        {
                            // Si tenemos IImage, lo convertimos a un Stream PNG (o JPEG)
                            return await ConvertMauiImageToStreamAsync(mauiImage);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Error: Could not get IImage from IImageSourceService for {resourceName}. Error: {result?.Error?.Message}");
                            // Log de qué tipo de error fue, si está disponible
                            if (result?.Error != null) System.Diagnostics.Debug.WriteLine($"Underlying error: {result.Error}");
                            return null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: ImageSource for {resourceName} is not an IFileImageSource.");
                        return null;
                    }

                default:
                    System.Diagnostics.Debug.WriteLine($"Error: Unhandled image source kind '{kind}' or mismatched data type '{sourceData?.GetType().Name}'.");
                    return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error resolving image stream for source '{sourceData}' (Kind: {kind}): {ex.ToString()}");
            return null;
        }
    }

    private async Task<Stream?> ConvertMauiImageToStreamAsync(IImage mauiImage)
    {
        try
        {
            var memoryStream = new MemoryStream();
            // Guardar como PNG por defecto. Podría ser configurable.
            // El ImageFormat y la calidad solo aplican a formatos con pérdida como JPEG.
            await mauiImage.SaveAsync(memoryStream, ImageFormat.Png); // O ImageFormat.Jpeg
            memoryStream.Position = 0; // Reiniciar para lectura
            return memoryStream;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error converting IImage to Stream: {ex.Message}");
            return null;
        }
        finally
        {
            // IImage tiene un método Dispose, es buena práctica llamarlo.
            // La documentación no es clara sobre quién es el dueño, pero por si acaso.
            (mauiImage as IDisposable)?.Dispose();
        }
    }
}
