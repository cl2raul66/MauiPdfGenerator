using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Primitives;

namespace Test;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void GeneratePdf_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");
        try
        {
            // Usa Unit en Fluent API
            var doc = PdfGenerator.CreateDocument();
            doc.Configure(config =>
            {
                config.PageSize(PageSizeType.Letter);                
                config.Margins(20); // Margins ahora usan el Unit configurado (mm)
                config.Font("Helvetica", 12); // Set default font
            });

            doc.PdfPage(pg =>
            {
                // Añadir texto en coordenadas específicas (usando el Unit especificado)
                pg.AddText("¡Hola Mundo 1!", 20, 30, Unit.Millimeters, textColor: Colors.Blue);
                pg.AddText("¡Hola Mundo 2!", 20, 45, Unit.Millimeters, fontSize: 16, attributes: FontAttributes.Bold);
                pg.AddText("¡Hola Mundo 3!", 20, 60, Unit.Millimeters); // Usa defaults

                // Añadir una imagen (ejemplo)
                // Supongamos que tienes un Stream llamado imageStream
                // pg.AddImage(imageStream, 20, 80, 50, 50, Unit.Millimeters);
            });

            // Guardar y abrir
            await doc.SaveAsync(targetFilePath); // No hace falta Stream aquí
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(targetFilePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }
}
