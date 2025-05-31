using Microsoft.Maui.Controls;
using System.Diagnostics;
using MauiPdfGenerator;
using MauiPdfGenerator.Fonts;
using static MauiPdfGenerator.Fonts.PdfFonts;
using static Microsoft.Maui.Graphics.Colors;
using static Microsoft.Maui.Controls.FontAttributes;
using static Microsoft.Maui.LineBreakMode;
using static Microsoft.Maui.TextAlignment;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;
using static MauiPdfGenerator.Fluent.Enums.DefaultMarginType;
using static MauiPdfGenerator.Fluent.Enums.PageOrientationType;
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
                    }).ConfigureFontRegistry(r =>
                    {
                        r.Font(PdfFonts.Comic).Default();
                    });
                })                                      
                .ContentPage()    
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph("P1: Default Comic (Regular expected)");
                    c.Paragraph("P2: Default Comic with BOLD attribute (Comic Bold expected)")
                        .FontAttributes(FontAttributes.Bold);
                    c.Paragraph("P3: Default Comic with ITALIC attribute (Comic Italic expected)")
                        .FontAttributes(FontAttributes.Italic);
                    c.Paragraph("P4: Explicitly ComicBoldFile (Comic Bold expected)")
                        .FontFamily(PdfFonts.ComicBold); 
                    c.Paragraph("P5: Explicitly OpenSansSemibold (OpenSans Semibold expected)")
                        .FontFamily(PdfFonts.OpenSansSemibold);
                    c.Paragraph("P6: Explicitly OpenSansSemibold (OpenSans Regular expected)")
                        .FontFamily(PdfFonts.OpenSansRegular);
                    c.HorizontalLine();
                    c.PdfImage(imageUriStream)
                         .WidthRequest(64).HeightRequest(64)
                         .Aspect(Aspect.AspectFit);
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
