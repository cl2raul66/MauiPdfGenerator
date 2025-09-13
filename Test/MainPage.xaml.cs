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

    private async void GeneratePdfOnlyParagraph_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        data.Title("MauiPdfGenerator sample - Experimental Paragraphs");
                    });
                })
                .ContentPage()
                .Content(c =>
                {
                    // --- Casos de Uso Básicos ---
                    c.Paragraph("[P1] Texto simple, con propiedades predeterminadas");
                    c.Paragraph("[P2] Texto simple, con HorizontalOptions Center")
                        .HorizontalOptions(LayoutAlignment.Center);
                    c.Paragraph("[P3] Texto simple, con HorizontalOptions Fill")
                        .HorizontalOptions(LayoutAlignment.Fill);
                    c.Paragraph("[P4] Texto simple, con HorizontalOptions End")
                        .HorizontalOptions(LayoutAlignment.End);
                    c.Paragraph("[P5] Texto simple, con HorizontalOptions Start")
                        .HorizontalOptions(LayoutAlignment.Start);
                    c.Paragraph("[P6] Texto simple, con TextColor Blue")
                        .TextColor(Colors.Blue);
                    c.Paragraph("[P7] Texto simple, con TextTransform None")
                        .TextTransform(TextTransform.None);
                    c.Paragraph("[P8] Texto simple, con TextTransform Default")
                        .TextTransform(TextTransform.Default);
                    c.Paragraph("[P9] Texto simple, con TextTransform Lowercase")
                        .TextTransform(TextTransform.Lowercase);
                    c.Paragraph("[P10] Texto simple, con TextTransform Uppercase")
                        .TextTransform(TextTransform.Uppercase);
                    c.Paragraph("[P11] Texto simple, con FontAttributes None")
                        .FontAttributes(FontAttributes.None);
                    c.Paragraph("[P12] Texto simple, con FontAttributes Italic")
                        .FontAttributes(FontAttributes.Italic);
                    c.Paragraph("[P13] Texto simple, con FontAttributes Bold")
                        .FontAttributes(FontAttributes.Bold);
                    c.Paragraph("[P14] Texto simple, con FontAttributes Italic and Bold")
                        .FontAttributes(FontAttributes.Italic | FontAttributes.Bold);
                    c.Paragraph("[P15] Texto simple, con FontSize 22")
                        .FontSize(22);
                    c.Paragraph("[P16] Texto simple, con FontFamily OpenSansRegular")
                        .FontFamily(PdfFonts.OpenSansRegular);
                    c.Paragraph("[P17] Texto simple, con FontFamily OpenSansSemibold")
                        .FontFamily(PdfFonts.OpenSansSemibold);
                    c.Paragraph("[P18] Texto simple, con FontFamily Comic")
                        .FontFamily(PdfFonts.Comic);
                    c.Paragraph("[P19] Texto simple, con HeightRequest 72, BackgroundColor LightBlue, VerticalOptions Center")
                        .HeightRequest(72)
                        .BackgroundColor(Colors.LightBlue)
                        .VerticalOptions(LayoutAlignment.Center);
                    c.Paragraph("Siguiente texto [P20] con LineBreakMode WordWrap");
                    c.Paragraph("Lorem ipsum dolor sit amet lorem et kasd erat nonumy eu ipsum sed. Sit invidunt et possim vero aliquyam sadipscing stet et erat amet lorem eirmod stet lorem possim nulla. Diam sed voluptua hendrerit no. Lorem tempor nulla takimata nonumy et takimata dolores magna vel sadipscing. Zzril exerci est iriure sit labore facilisis lorem takimata sit kasd dolore labore.")
                        .LineBreakMode(LineBreakMode.WordWrap);
                    c.Paragraph("Siguiente texto [P21] con LineBreakMode NoWrap");
                    c.Paragraph("Lorem ipsum dolor sit amet lorem et kasd erat nonumy eu ipsum sed. Sit invidunt et possim vero aliquyam sadipscing stet et erat amet lorem eirmod stet lorem possim nulla. Diam sed voluptua hendrerit no. Lorem tempor nulla takimata nonumy et takimata dolores magna vel sadipscing. Zzril exerci est iriure sit labore facilisis lorem takimata sit kasd dolore labore.")
                        .LineBreakMode(LineBreakMode.NoWrap);
                    c.Paragraph("Siguiente texto [P22] con LineBreakMode CharacterWrap");
                    c.Paragraph("Lorem ipsum dolor sit amet lorem et kasd erat nonumy eu ipsum sed. Sit invidunt et possim vero aliquyam sadipscing stet et erat amet lorem eirmod stet lorem possim nulla. Diam sed voluptua hendrerit no. Lorem tempor nulla takimata nonumy et takimata dolores magna vel sadipscing. Zzril exerci est iriure sit labore facilisis lorem takimata sit kasd dolore labore.")
                        .LineBreakMode(LineBreakMode.CharacterWrap);
                    c.Paragraph("Siguiente texto [P23] con LineBreakMode HeadTruncation");
                    c.Paragraph("Lorem ipsum dolor sit amet lorem et kasd erat nonumy eu ipsum sed. Sit invidunt et possim vero aliquyam sadipscing stet et erat amet lorem eirmod stet lorem possim nulla. Diam sed voluptua hendrerit no. Lorem tempor nulla takimata nonumy et takimata dolores magna vel sadipscing. Zzril exerci est iriure sit labore facilisis lorem takimata sit kasd dolore labore.")
                        .LineBreakMode(LineBreakMode.HeadTruncation);
                    c.Paragraph("Siguiente texto [P24] con LineBreakMode MiddleTruncation");
                    c.Paragraph("Lorem ipsum dolor sit amet lorem et kasd erat nonumy eu ipsum sed. Sit invidunt et possim vero aliquyam sadipscing stet et erat amet lorem eirmod stet lorem possim nulla. Diam sed voluptua hendrerit no. Lorem tempor nulla takimata nonumy et takimata dolores magna vel sadipscing. Zzril exerci est iriure sit labore facilisis lorem takimata sit kasd dolore labore.")
                        .LineBreakMode(LineBreakMode.MiddleTruncation);
                    c.Paragraph("Siguiente texto [P25] con LineBreakMode TailTruncation");
                    c.Paragraph("Lorem ipsum dolor sit amet lorem et kasd erat nonumy eu ipsum sed. Sit invidunt et possim vero aliquyam sadipscing stet et erat amet lorem eirmod stet lorem possim nulla. Diam sed voluptua hendrerit no. Lorem tempor nulla takimata nonumy et takimata dolores magna vel sadipscing. Zzril exerci est iriure sit labore facilisis lorem takimata sit kasd dolore labore.")
                        .LineBreakMode(LineBreakMode.TailTruncation);

                    c.Paragraph("--- Casos de Uso Avanzados y Combinados ---").HorizontalOptions(LayoutAlignment.Center).FontSize(16).FontAttributes(FontAttributes.Bold).Margin(0, 20, 0, 10);

                    c.Paragraph("[P26] Caja centrada, texto a la derecha.")
                        .HorizontalOptions(LayoutAlignment.Center)
                        .HorizontalTextAlignment(TextAlignment.End)
                        .BackgroundColor(Colors.LightYellow)
                        .WidthRequest(300);

                    c.Paragraph("[P27] Este párrafo tiene padding y margin.")
                        .Padding(10, 20)
                        .Margin(20, 10)
                        .BackgroundColor(Colors.LightCoral);

                    c.Paragraph("[P28] Este texto tiene un ancho fijo y será truncado al final si no cabe en el espacio asignado por el WidthRequest que se le ha proporcionado.")
                        .WidthRequest(250)
                        .LineBreakMode(LineBreakMode.TailTruncation)
                        .BackgroundColor(Colors.LightSteelBlue);

                    c.Paragraph("[P29] Estilo Completo: Subrayado, Tachado, Negrita, Itálica y color.")
                        .FontSize(14)
                        .TextColor(Colors.DarkViolet)
                        .FontFamily(PdfFonts.Comic)
                        .FontAttributes(FontAttributes.Bold | FontAttributes.Italic)
                        .TextDecorations(TextDecorations.Underline | TextDecorations.Strikethrough);

                    c.Paragraph("[P30] Este párrafo\ntiene saltos de línea\nexplícitos en medio del texto.")
                        .HorizontalTextAlignment(TextAlignment.Center);

                    c.Paragraph("[P31] Párrafo con texto vacío (siguiente):");
                    c.Paragraph("");

                    c.Paragraph("[P32] Párrafo con solo espacios (siguiente):");
                    c.Paragraph("   ");
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

    private async void GeneratePdfOnlyImage_Clicked(object sender, EventArgs e)
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
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        data.Title("MauiPdfGenerator sample - Experimental Images");
                    });
                })
                .ContentPage()
                .Content(c =>
                {
                    c.Paragraph("--- Casos de Uso de Imágenes ---").FontSize(16).FontAttributes(FontAttributes.Bold).HorizontalOptions(LayoutAlignment.Center);

                    // Caso 1: Imagen simple, tamaño intrínseco
                    c.Paragraph("1. Imagen simple (tamaño intrínseco)");
                    c.Image(new MemoryStream(imageData));

                    // Caso 2: Imagen con WidthRequest y alineación
                    c.Paragraph("2. Imagen con WidthRequest(100) y HorizontalOptions(End)");
                    c.Image(new MemoryStream(imageData))
                        .WidthRequest(100)
                        .HorizontalOptions(LayoutAlignment.End);

                    // Caso 3: Aspect.Fill (estirada)
                    c.Paragraph("3. Aspect(Fill) con tamaño fijo (150x75)");
                    c.Image(new MemoryStream(imageData))
                        .WidthRequest(150)
                        .HeightRequest(75)
                        .Aspect(Aspect.Fill)
                        .HorizontalOptions(LayoutAlignment.Center);

                    // Caso 4: Aspect.AspectFill (recortada para llenar)
                    c.Paragraph("4. Aspect(AspectFill) con tamaño fijo (150x75)");
                    c.Image(new MemoryStream(imageData))
                        .WidthRequest(150)
                        .HeightRequest(75)
                        .Aspect(Aspect.AspectFill)
                        .HorizontalOptions(LayoutAlignment.Center)
                        .BackgroundColor(Colors.LightGray); // Fondo para ver el área

                    // Caso 5: Aspect.AspectFit (ajustada sin recortar)
                    c.Paragraph("5. Aspect(AspectFit) con tamaño fijo (150x75)");
                    c.Image(new MemoryStream(imageData))
                        .WidthRequest(150)
                        .HeightRequest(75)
                        .Aspect(Aspect.AspectFit)
                        .HorizontalOptions(LayoutAlignment.Center)
                        .BackgroundColor(Colors.LightBlue); // Fondo para ver el área

                    // Caso 6: Con Padding y BackgroundColor
                    c.Paragraph("6. Imagen con Padding(20) y BackgroundColor");
                    c.Image(new MemoryStream(imageData))
                        .WidthRequest(120)
                        .Padding(20)
                        .Margin(16)
                        .BackgroundColor(Colors.LightCoral)
                        .HorizontalOptions(LayoutAlignment.Center);

                    // Caso 7: Manejo de error (Stream cerrado)
                    //c.Paragraph("7. Manejo de error (Stream cerrado)");
                    //var closedStream = new MemoryStream();
                    //closedStream.Close();
                    //c.PdfImage(closedStream)
                    //    .WidthRequest(200)
                    //    .HeightRequest(50)
                    //    .HorizontalOptions(LayoutAlignment.Center);
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

    private async void GeneratePdfWithHorizontalLine_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        data.Title("MauiPdfGenerator sample - Experimental Horizontal Line");
                    });
                })
                .ContentPage()
                .Content(c =>
                {
                    c.Paragraph("--- Casos de Uso de HorizontalLine ---")
                        .FontSize(16)
                        .FontAttributes(FontAttributes.Bold)
                        .HorizontalOptions(LayoutAlignment.Center);

                    // Caso 1: Línea predeterminada
                    c.Paragraph("1. Línea con valores predeterminados (Thickness=1, Color=Black, HorizontalOptions=Fill)");
                    c.HorizontalLine();

                    // Caso 2: Grosor personalizado
                    c.Paragraph("2. Línea con Thickness(5)");
                    c.HorizontalLine()
                        .Thickness(5);

                    // Caso 3: Color personalizado
                    c.Paragraph("3. Línea con Color(Colors.Red)");
                    c.HorizontalLine()
                        .Color(Colors.Red);

                    // Caso 4: Grosor y Color combinados
                    c.Paragraph("4. Línea con Thickness(3) y Color(Colors.Green)");
                    c.HorizontalLine()
                        .Thickness(3)
                        .Color(Colors.Green);

                    // Caso 5: Ancho fijo y alineación
                    c.Paragraph("5. Línea con WidthRequest(200) y HorizontalOptions(Center)");
                    c.HorizontalLine()
                        .WidthRequest(200)
                        .HorizontalOptions(LayoutAlignment.Center)
                        .Color(Colors.Orange); // Color para que sea fácil de ver

                    // Caso 6: Alineación a la derecha
                    c.Paragraph("6. Línea con WidthRequest(100) y HorizontalOptions(End)");
                    c.HorizontalLine()
                        .WidthRequest(100)
                        .HorizontalOptions(LayoutAlignment.End)
                        .Color(Colors.Purple);

                    // Caso 7: Con margen vertical
                    c.Paragraph("7. Línea con Margin(0, 20) para crear espacio");
                    c.HorizontalLine()
                        .Margin(0, 20);

                    // Caso 8: Con margen horizontal (reducirá el ancho de la línea)
                    c.Paragraph("8. Línea con Margin(50, 0)");
                    c.HorizontalLine()
                        .Margin(50, 0);

                    // Caso 9: Con padding horizontal (no debería tener efecto visual en la línea)
                    c.Paragraph("9. Línea con Padding(50, 0) y fondo para ver el cajón");
                    c.HorizontalLine()
                        .Padding(50, 5, 10, 10)
                        .BackgroundColor(Colors.Gray) // Fondo para ver el área del elemento
                        .Color(Colors.Red);

                    // Caso 10: Combinación completa
                    c.Paragraph("10. Línea con todas las propiedades personalizadas");
                    c.HorizontalLine()
                        .Thickness(4)
                        .Color(Colors.DarkCyan)
                        .WidthRequest(300)
                        .HorizontalOptions(LayoutAlignment.Center)
                        .Margin(0, 10);

                    c.Paragraph("Fin de los casos de prueba para HorizontalLine.");
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

    private async void GeneratePdfWithStackLayout_Clicked(object sender, EventArgs e)
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
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        data.Title("MauiPdfGenerator sample - Experimental Horizontal Line");
                    });
                })
                .ContentPage()
                .Content(c =>
                {
                    c.VerticalStackLayout(vsl =>
                    {
                        vsl.HorizontalStackLayout(hsl =>
                        {
                            hsl.Paragraph("texto horizontal 1");
                            hsl.Paragraph("texto horizontal 2");
                        });

                        vsl.HorizontalStackLayout(hsl =>
                        {
                            hsl.Image(new MemoryStream(imageData)).WidthRequest(150);
                            hsl.Image(new MemoryStream(imageData)).WidthRequest(150);
                        });

                        vsl.HorizontalStackLayout(hsl =>
                        {
                            hsl.Image(new MemoryStream(imageData));
                            hsl.Image(new MemoryStream(imageData));
                        }).WidthRequest(300);

                        vsl.HorizontalStackLayout(hsl =>
                        {
                            hsl.Paragraph("texto horizontal 1").Margin(8f);
                            hsl.Paragraph("texto horizontal 2").Padding(8).BackgroundColor(Colors.LightPink);
                        }).Spacing(8).BackgroundColor(Colors.LightGoldenrodYellow);

                        vsl.HorizontalLine();
                    })
                    .Spacing(16);                    
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
