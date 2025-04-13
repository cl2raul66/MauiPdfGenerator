using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Enums;

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
            using var doc = PdfGenerator.CreateDocument();
            doc.Configure(config =>
            {
                config.PageSize(PageSizeType.A4);
                config.Margins(50);
            });

            doc.PdfPage(pg =>
            {
                pg.Content(c =>
                {
                    c.Paragraph(p => p.Text("¡Hola Mundo 1!"));
                    c.Paragraph(p => p.Text("¡Hola Mundo 2!"));
                    c.Paragraph(p => p.Text("¡Hola Mundo 3!"));
                });
            });

            // Guardar y abrir el documento
            await doc.SaveAsync(targetFilePath);
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
