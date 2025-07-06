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

                    c.Paragraph("[P27] Este párrafo tiene padding interno.")
                        .Padding(10, 20)
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
}
