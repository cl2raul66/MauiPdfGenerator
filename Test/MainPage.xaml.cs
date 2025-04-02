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
            using (var doc = PdfGenerator.CreateDocument())
            {
                doc.Configure(config =>
                {
                    config.PageSize(PageSizeType.A4)
                        .Spacing(16)
                        .Margins(50)
                        .Metadata()
                        .SetSecurity();
                });

                doc.AddPage(p =>
                {
                    p.AddGrid(g =>
                    {    
                        g.r
                        g.AddParagraph("Aquí esta el header");
                    });
                    p.ad(b =>
                    {
                        b.AddParagraph("Aquí esta el Body");
                    });
                    p.Footer(f =>
                    {
                        f.AddParagraph("Aquí esta el footer");
                    });
                });

                await doc.SaveAsync(targetFilePath);

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(targetFilePath)
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }
}
