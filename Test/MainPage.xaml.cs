using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Enums;

using static Microsoft.Maui.Graphics.Colors;

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
                        .Margins(50);
                });

                doc.PdfPage(p =>
                {
                    p.Content(c =>
                    {
                        c.Grid(g =>
                        {
                            g.Children(c =>
                            {
                                c.Paragraph(p =>
                                { 
                                    p.Text("Hola Mundo!")
                                    .HorizontalOptions(PdfHorizontalAlignment.Center)
                                    .FormattedText(ft =>
                                    {
                                        ft.AddSpan(s =>
                                        {
                                            s.TextColor(Blue).Text(" cruel");
                                        });
                                    });
                                }).Text("Hola mundo");
                            });
                        });
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
