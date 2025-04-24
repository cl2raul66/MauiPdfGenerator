using MauiPdfGenerator;
using Microsoft.Maui.Controls;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;
using static MauiPdfGenerator.SourceGenerators.MauiFontAliases;

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
            var doc = PdfGenerator.CreateDocument();
            doc.Configuration(config =>
            {
                config.PageSize(Letter);
                config.Margins(20);
                config.PdfFontRegistry(f =>
                {
                    f.Font(OpenSansRegular).Default();
                    f.Font(OpenSansSemibold).IsEmbeddedFont();
                });
                config.MetaData(md =>
                {
                    md.CreationDate(DateTime.Now);
                    md.Title("Sample");
                    md.Author("Test");
                    md.Creator("Test");
                });
            });

            doc.Page().Content(c => {
                {
                    c.Paragraph("¡Hola Mundo!").FontFamily(OpenSansRegular).TextColors(Colors.Blue);
                    c.Paragraph(p =>
                    {
                        p.Text("¡Hola Mundo!").FontSize(16).FontAttributes(FontAttributes.Bold);
                        p.Text("¡Hola Mundo!").FontSize(14).TextTransform(TextTransform.Uppercase);
                    }).FontFamily(OpenSansSemibold);
                }
            });

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
