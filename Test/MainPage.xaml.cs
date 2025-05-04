using MauiPdfGenerator;
using static Microsoft.Maui.Graphics.Colors;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;
using static MauiPdfGenerator.Fluent.Enums.DefaultMarginType;
using static MauiPdfGenerator.SourceGenerators.MauiFontAliases;
using static MauiPdfGenerator.Fluent.Enums.PageOrientationType;
using static Microsoft.Maui.Controls.FontAttributes;

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

            await doc
                .Configuration(cfg =>
                {
                    cfg.Margins(Narrow);
                    cfg.MetaData(data =>
                    {
                        Title = "MauiPdfGenerator sample";
                    });
                })
                .ContentPage()
                .DefaultFont(f => f.Size(10).Attributes(Italic|Bold)) 
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph("Este párrafo usará Times New Roman 10pt por defecto.");
                    c.Paragraph("Este a 18pt.").FontFamily("Times New Roman").FontSize(18f).FontAttributes(None); 
                    c.Paragraph("Este a 10pt.").FontSize(10f); 
                })
                .Build()
            .SaveAsync(targetFilePath);

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
