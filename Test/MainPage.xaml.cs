using Microsoft.Maui.Controls;
using System.Diagnostics;
using MauiPdfGenerator;
using MauiPdfGenerator.Fonts;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models;

namespace Test;

public partial class MainPage : ContentPage
{
    private readonly IPdfDocumentFactory pdfDocFactory;

    public MainPage(IPdfDocumentFactory pdfDocumentFactory)
    {
        InitializeComponent();
        pdfDocFactory = pdfDocumentFactory;
    }

    private async void GeneratePdf_Clicked(object sender, EventArgs e)
    {
        byte[] imageData;
        using (var httpClient = new HttpClient())
        {
            var uri = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
            imageData = await httpClient.GetByteArrayAsync(uri);
        }

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            Image i = new();

            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        data.Title("MauiPdfGenerator sample");
                    });
                })
                .ContentPage()
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph("Elemento 1 dentro del VerticalStackLayout.");
                    c.Paragraph("Elemento 2 dentro del VerticalStackLayout, con un texto un poco más largo para ver cómo se ajusta.");
                    c.PdfGrid()
                        .BackgroundColor(Colors.LightSlateGray)
                        .HorizontalOptions(LayoutAlignment.Center)
                        .RowDefinitions(rd =>
                        {
                            rd.GridLength(GridUnitType.Auto);
                            rd.GridLength(GridUnitType.Auto);
                            rd.GridLength(GridUnitType.Star);
                            rd.GridLength(100);
                        })
                        .ColumnDefinitions(cd =>
                        {
                            cd.GridLength(GridUnitType.Auto);
                            cd.GridLength(GridUnitType.Star);
                            cd.GridLength(100);
                        })
                        .Children(children =>
                        {
                            children.Paragraph("Grid").ColumnSpan(3);
                            children.Paragraph("Autosized cell").Row(1);
                            children.Paragraph("Other cell").Row(1).Column(1);
                            children.Paragraph("Span two rows (or more if you want)").Row(1).Column(2).RowSpan(2);
                            children.Paragraph("Autosized cell in with").Row(2);
                            children.Paragraph("Leftover space").Row(2).Column(1);
                            children.Paragraph("Span 2 columns").Row(3).ColumnSpan(2);
                            children.Paragraph("Fixed 100x100").Row(3).Column(2);
                        });
                    c.Paragraph("Elemento 3 después del HSL, de nuevo en el VerticalStackLayout.");

                    c.HorizontalLine();

                    c.PdfImage(new MemoryStream(imageData)).Aspect(Aspect.Fill)
                         .WidthRequest(64).HeightRequest(64);

                }).Build()
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
