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
        using var httpClient = new HttpClient(); 
        var uri2 = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
        using Stream imageUriStream = await httpClient.GetStreamAsync(uri2);
        
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
                        Title = "MauiPdfGenerator sample";
                    });
                })                                      
                .ContentPage()    
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph("P1: Default Comic (Regular expected)").TextDecorations(TextDecorations.Underline).HorizontalAlignment(TextAlignment.Center);
                    c.Paragraph("P2: Default Comic with BOLD attribute (Comic Bold expected)")
                        .FontAttributes(FontAttributes.Bold).HorizontalAlignment(TextAlignment.End);
                    c.Paragraph("P3: Default Comic with ITALIC attribute (Comic Italic expected)")
                        .FontAttributes(FontAttributes.Italic).VerticalAlignment(TextAlignment.End);
                    c.Paragraph("P4: Explicitly ComicBoldFile (Comic Bold expected)")
                        .FontFamily(PdfFonts.ComicBold).TextDecorations(TextDecorations.Strikethrough); 
                    c.Paragraph("P5: Explicitly OpenSansSemibold (OpenSans Semibold expected)")
                        .FontFamily(PdfFonts.OpenSansSemibold);
                    c.Paragraph("P6: Explicitly OpenSansSemibold (OpenSans Regular expected)")
                        .FontFamily(PdfFonts.OpenSansRegular);
                    c.HorizontalLine();
                    c.PdfImage(imageUriStream)
                         .WidthRequest(64).HeightRequest(64)
                         .Aspect(Aspect.AspectFit);
                }).Build()
                .ContentPage().DefaultTextDecorations(TextDecorations.Strikethrough).DefaultTextTransform(TextTransform.Uppercase).Content(c =>
                {
                    c.Paragraph("Hola mundo").TextTransform(TextTransform.None);
                    c.Paragraph("Hola mundo").TextDecorations(TextDecorations.None);
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
