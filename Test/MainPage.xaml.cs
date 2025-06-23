using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.Diagnostics;
using MauiPdfGenerator;
using MauiPdfGenerator.Fonts;
using Microsoft.Maui.Graphics;
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
                    c.Paragraph("Elemento 1 dentro del VerticalStackLayout.").HorizontalOptions(LayoutAlignment.Center);
                    c.Paragraph("Elemento 2 dentro del VerticalStackLayout, con un texto un poco más largo para ver cómo se ajusta.");
                    c.PdfGrid().HorizontalOptions(LayoutAlignment.Fill).BackgroundColor(Colors.LightGrey)
                        .RowDefinitions(rd =>
                        {
                            rd.GridLength(GridUnitType.Auto);
                            rd.GridLength(GridUnitType.Auto);
                        })
                        .ColumnDefinitions(cd =>
                        {
                            cd.GridLength(GridUnitType.Auto);
                            cd.GridLength(GridUnitType.Star);
                        })
                        .Children(children =>
                        {
                            children.Paragraph("Celda 0,0 con texto largo que debería ajustar el alto de la fila.").TextColor(Colors.Red).Row(0).Column(0);
                            children.Paragraph("Columna fija 0,1").Row(0).Column(1).BackgroundColor(Colors.LightCyan);

                            children.Paragraph("Celda auto 1,0").Row(1).Column(0);
                            children.PdfImage(new MemoryStream(imageData)).Row(1).Column(1);
                        });
                    c.Paragraph("Elemento 3 después del HSL, de nuevo en el VerticalStackLayout.");
                    c.HorizontalLine();
                    c.PdfImage(new MemoryStream(imageData)).Aspect(Aspect.AspectFit).HeightRequest(64).HorizontalOptions(LayoutAlignment.Center);                    
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
