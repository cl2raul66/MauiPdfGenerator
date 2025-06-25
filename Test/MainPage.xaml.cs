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
                    cfg.Margins(DefaultMarginType.Narrow);
                    cfg.MetaData(data =>
                    {
                        data.Title("MauiPdfGenerator sample");
                    });
                })
                .ContentPage()
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph($"[P1] Texto: 'Elemento 1 dentro del VerticalStackLayout.'\nHorizontalOptions: Center (personalizado)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)").HorizontalOptions(LayoutAlignment.Center);

                    c.Paragraph($"[P2] Texto: 'Elemento 2 dentro del VerticalStackLayout, con un texto un poco más largo para ver cómo se ajusta.'\nHorizontalOptions: predeterminado (Fill)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)");

                    c.Paragraph("--- GRID ---");
                    c.PdfGrid().VerticalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightGrey)
                        .RowDefinitions(rd =>
                        {
                            rd.GridLength(GridUnitType.Auto);
                            rd.GridLength(GridUnitType.Auto);
                        })
                        .ColumnDefinitions(cd =>
                        {
                            cd.GridLength(GridUnitType.Star);
                            cd.GridLength(GridUnitType.Star);
                        })
                        .Children(children =>
                        {
                            children.Paragraph($"[G1] Grid (0,0)\nTexto: 'Celda 0,0 con texto largo que debería ajustar el alto de la fila.'\nRow: 0\nColumn: 0\nHorizontalOptions: predeterminado (Start)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)\nTextColor: Red").TextColor(Colors.Red).BackgroundColor(Colors.LightCyan).Row(0).Column(0);

                            children.Paragraph($"[G2] Grid (0,1)\nTexto: 'Columna fija 0,1'\nRow: 0\nColumn: 1\nHorizontalOptions: predeterminado (Start)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)\nBackgroundColor: LightCyan").Row(0).Column(1).BackgroundColor(Colors.LightPink);

                            children.Paragraph($"[G3] Grid (1,0)\nTexto: 'Celda auto 1,0'\nRow: 1\nColumn: 0\nHorizontalOptions: predeterminado (Start)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)").Row(1).Column(0).BackgroundColor(Colors.LightPink);

                            
                            children.PdfImage(new MemoryStream(imageData)).Row(1).Column(1).BackgroundColor(Colors.LightCyan);
                            children.Paragraph($"[G4] Grid (1,1)\nImagen\nRow: 1\nColumn: 1\nHorizontalOptions: predeterminado (Start)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)").Column(1).Row(1);

                        });

                    c.Paragraph($"[P3] Texto: 'Elemento 3 después del HSL, de nuevo en el VerticalStackLayout.'\nHorizontalOptions: predeterminado (Fill)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)");
                    c.Paragraph("--- LÍNEA HORIZONTAL ---");
                    c.HorizontalLine();
                    c.Paragraph($"[IMG] Imagen fuera de grid\nHorizontalOptions: Center (personalizado)\nVerticalOptions: predeterminado (Start)\nMargin: predeterminado (0)\nPadding: predeterminado (0)\nHeightRequest: 64");
                    c.PdfImage(new MemoryStream(imageData)).Aspect(Aspect.AspectFit).HeightRequest(64).HorizontalOptions(LayoutAlignment.Center);
                    c.Paragraph($"[P4] Texto: Otro texto para el final");
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
