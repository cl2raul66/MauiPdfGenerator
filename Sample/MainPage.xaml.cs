using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fonts;

namespace Sample;

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
                    c.Spacing(15);
                    c.Children(ch =>
                    {
                        // --- Propiedades básicas ---
                        ch.Paragraph("Título: Demostración de propiedades de párrafos")
                            .FontSize(24)
                            .FontAttributes(FontAttributes.Bold)
                            .TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .Margin(0, 0, 0, 20);

                        // --- Alineación de texto ---
                        ch.Paragraph("1. Texto alineado a la izquierda (predeterminado)")
                            .HorizontalTextAlignment(TextAlignment.Start);

                        ch.Paragraph("2. Texto alineado al centro")
                            .HorizontalTextAlignment(TextAlignment.Center);

                        ch.Paragraph("3. Texto alineado a la derecha")
                            .HorizontalTextAlignment(TextAlignment.End);

                        // --- Transformación de texto ---
                        ch.Paragraph("4. Texto sin transformación")
                            .TextTransform(TextTransform.None);

                        ch.Paragraph("5. Texto en mayúsculas")
                            .TextTransform(TextTransform.Uppercase);

                        ch.Paragraph("6. Texto en minúsculas")
                            .TextTransform(TextTransform.Lowercase);

                        // --- Atributos de fuente ---
                        ch.Paragraph("7. Texto en negrita")
                            .FontAttributes(FontAttributes.Bold);

                        ch.Paragraph("8. Texto en cursiva")
                            .FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("9. Texto en negrita y cursiva")
                            .FontAttributes(FontAttributes.Bold | FontAttributes.Italic);

                        // --- Decoración de texto ---
                        ch.Paragraph("10. Texto subrayado")
                            .TextDecorations(TextDecorations.Underline);

                        ch.Paragraph("11. Texto tachado")
                            .TextDecorations(TextDecorations.Strikethrough);

                        ch.Paragraph("12. Texto subrayado y tachado")
                            .TextDecorations(TextDecorations.Underline | TextDecorations.Strikethrough);

                        // --- Tamaños de fuente ---
                        ch.Paragraph("13. Texto pequeño (12pt)")
                            .FontSize(12);

                        ch.Paragraph("14. Texto mediano (16pt)")
                            .FontSize(16);

                        ch.Paragraph("15. Texto grande (22pt)")
                            .FontSize(22);

                        // --- Familias de fuente ---
                        ch.Paragraph("16. Texto con OpenSansRegular")
                            .FontFamily(PdfFonts.OpenSansRegular);

                        ch.Paragraph("17. Texto con OpenSansSemibold")
                            .FontFamily(PdfFonts.OpenSansSemibold);

                        ch.Paragraph("18. Texto con Comic")
                            .FontFamily(PdfFonts.Comic);

                        // --- Colores ---
                        ch.Paragraph("19. Texto en color rojo")
                            .TextColor(Colors.Red);

                        ch.Paragraph("20. Texto con fondo amarillo claro")
                            .BackgroundColor(Colors.LightYellow);

                        ch.Paragraph("21. Texto en color azul con fondo gris claro")
                            .TextColor(Colors.Blue)
                            .BackgroundColor(Colors.LightGray);

                        // --- Espaciado y márgenes ---
                        ch.Paragraph("22. Texto con 10 padding y fondo rosado claro")
                            .Padding(10)
                            .BackgroundColor(Colors.LightPink);

                        ch.Paragraph("23. Texto con 10 margin y fondo azul claro")
                            .Margin(10)
                            .BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("24. Texto con padding 5 horizontal, 10 de vertical y margin 10 horizontal, 5 vertical y fondo verde claro")
                            .Padding(5, 10)
                            .Margin(10, 5)
                            .BackgroundColor(Colors.LightGreen);

                        // --- Control de ancho ---
                        ch.Paragraph("25. Texto en ancho fijo de 200 y fondo amarillo dorado claro")
                            .WidthRequest(200)
                            .BackgroundColor(Colors.LightGoldenrodYellow);

                        // --- Control de altura ---
                        ch.Paragraph("26. Texto con altura fija de 50, fondo azul acerado claro con alineación vertical centrada")
                            .HeightRequest(50)
                            .BackgroundColor(Colors.LightSteelBlue)
                            .VerticalTextAlignment(TextAlignment.Center);

                        // --- Opciones de diseño ---
                        ch.Paragraph("27. Texto con alineación centrado horizontal y verticalmente")
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center);

                        // --- Control de salto de línea ---
                        ch.Paragraph("28. Texto largo con LineBreakMode WordWrap: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .LineBreakMode(LineBreakMode.WordWrap);

                        ch.Paragraph("29. Texto largo con LineBreakMode CharacterWrap: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .LineBreakMode(LineBreakMode.CharacterWrap);

                        ch.Paragraph("30. Texto largo con LineBreakMode HeadTruncation: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .LineBreakMode(LineBreakMode.HeadTruncation);

                        ch.Paragraph("31. Texto largo con LineBreakMode MiddleTruncation: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .LineBreakMode(LineBreakMode.MiddleTruncation);

                        ch.Paragraph("32. Texto largo con LineBreakMode TailTruncation: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .LineBreakMode(LineBreakMode.TailTruncation);

                        // --- Combinaciones complejas ---
                        ch.Paragraph("33. Párrafo con múltiples propiedades")
                            .FontSize(18)
                            .FontAttributes(FontAttributes.Bold | FontAttributes.Italic)
                            .TextColor(Colors.Purple)
                            .BackgroundColor(Colors.LightYellow)
                            .Padding(10)
                            .Margin(5)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .TextDecorations(TextDecorations.Underline);

                        ch.Paragraph("34. Párrafo con estilo de tarjeta")
                            .FontFamily(PdfFonts.Comic)
                            .FontSize(16)
                            .TextColor(Colors.DarkBlue)
                            .BackgroundColor(Colors.LightGrey)
                            .Padding(15)
                            .Margin(10)
                            .WidthRequest(300)
                            .HorizontalOptions(LayoutAlignment.Center);

                        ch.Paragraph("35. Párrafo con múltiples líneas y formato personalizado\nSegunda línea\nTercera línea")
                            .FontSize(14)
                            .TextColor(Colors.DarkGreen)
                            .BackgroundColor(Colors.LightGray)
                            .Padding(10)
                            .HorizontalTextAlignment(TextAlignment.Center);
                    });
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
                    c.Spacing(15)
                    .Children(ch =>
                    {
                        ch.Paragraph("--- Casos de Uso de Imágenes ---")
                        .FontSize(16)
                        .FontAttributes(FontAttributes.Bold)
                        .HorizontalTextAlignment(TextAlignment.Center);

                        // --- Propiedades básicas ---
                        ch.Paragraph("1. Imagen con tamaño intrínseco (sin modificaciones)");
                        ch.Image(new MemoryStream(imageData));

                        // --- Control de tamaño ---
                        ch.Paragraph("2. Imagen con WidthRequest(200)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200);

                        ch.Paragraph("3. Imagen con HeightRequest(75)");
                        ch.Image(new MemoryStream(imageData))
                            .HeightRequest(75);

                        ch.Paragraph("4. Imagen con 75 de ancho, 75 de altura y fondo gris claro");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(75)
                            .HeightRequest(75)
                            .BackgroundColor(Colors.LightGray);

                        // --- Aspectos ---
                        ch.Paragraph("5. Imagen con 75 de ancho, 75 de altura, fondo azul claro y Aspect.Fill (estira la imagen)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(75)
                            .HeightRequest(75)
                            .Aspect(Aspect.Fill)
                            .BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("6. Imagen con 75 de ancho, 75 de altura, fondo azul claro y Aspect.Center (mantiene centrada)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(75)
                            .HeightRequest(75)
                            .Aspect(Aspect.Center)
                            .BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("7. Imagen con 75 de ancho, 75 de altura, fondo azul claro y Aspect.AspectFit (mantiene proporción dentro del contenedor)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(75)
                            .HeightRequest(75)
                            .Aspect(Aspect.AspectFit)
                            .BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("8. Imagen con 75 de ancho, 75 de altura y Aspect.AspectFill (recorta manteniendo proporción)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(75)
                            .HeightRequest(75)
                            .Aspect(Aspect.AspectFill)
                            .BackgroundColor(Colors.LightBlue);

                        // --- Alineación ---
                        ch.Paragraph("9. Imagen con 200 de ancho y HorizontalOptions(Start)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .HorizontalOptions(LayoutAlignment.Start);

                        ch.Paragraph("10. Imagen con 200 de ancho y HorizontalOptions(Center)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .HorizontalOptions(LayoutAlignment.Center);

                        ch.Paragraph("11. Imagen con 200 de ancho y HorizontalOptions(End)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .HorizontalOptions(LayoutAlignment.End);

                        // --- Márgenes y Padding ---
                        ch.Paragraph("12. Imagen con Margin(20)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .Margin(20)
                            .BackgroundColor(Colors.LightYellow);

                        ch.Paragraph("13. Imagen con Padding(20)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .Padding(20)
                            .BackgroundColor(Colors.LightGreen);

                        ch.Paragraph("14. Imagen con Margin(10) y Padding(10)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .Margin(10)
                            .Padding(10)
                            .BackgroundColor(Colors.LightSalmon);

                        // --- Márgenes específicos ---
                        ch.Paragraph("15. Imagen con Margin(20, 10)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .Margin(20, 10)
                            .BackgroundColor(Colors.LightCyan);

                        ch.Paragraph("16. Imagen con Padding(20, 10)");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .Padding(20, 10)
                            .BackgroundColor(Colors.LightGoldenrodYellow);

                        // --- Combinaciones complejas ---
                        ch.Paragraph("17. Imagen con múltiples propiedades");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(300)
                            .HeightRequest(150)
                            .Aspect(Aspect.AspectFit)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .Margin(10)
                            .Padding(5)
                            .BackgroundColor(Colors.LightSteelBlue);

                        ch.Paragraph("18. Imagen estilo tarjeta");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(250)
                            .Aspect(Aspect.AspectFit)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .Margin(15)
                            .Padding(10)
                            .BackgroundColor(Colors.White);

                        ch.Paragraph("19. Imagen con efectos visuales combinados");
                        ch.Image(new MemoryStream(imageData))
                            .WidthRequest(200)
                            .HeightRequest(200)
                            .Aspect(Aspect.AspectFill)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .Margin(20, 10)
                            .Padding(15)
                            .BackgroundColor(Colors.LightCoral);

                        ch.Paragraph("--- Fin de los casos de uso de imágenes ---")
                            .FontSize(16)
                            .FontAttributes(FontAttributes.Bold)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .Margin(0, 20, 0, 0);
                    });
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
            var doc = pdfDocFactory.CreateDocument(targetFilePath);
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
                    c.Children(ch =>
                    {
                        ch.Paragraph("--- Casos de Uso de HorizontalLine ---")
                        .FontSize(16)
                        .FontAttributes(FontAttributes.Bold);

                        // Caso 1: Línea predeterminada
                        ch.Paragraph("1. Línea con valores predeterminados (Thickness=1, Color=Black, HorizontalOptions=Fill)");
                        ch.HorizontalLine();

                        // Caso 2: Grosor personalizado
                        ch.Paragraph("2. Línea con Thickness(5)");
                        ch.HorizontalLine()
                            .Thickness(5);

                        // Caso 3: Color personalizado
                        ch.Paragraph("3. Línea con Color(Colors.Red)");
                        ch.HorizontalLine()
                            .Color(Colors.Red);

                        // Caso 4: Grosor y Color combinados
                        ch.Paragraph("4. Línea con Thickness(3) y Color(Colors.Green)");
                        ch.HorizontalLine()
                            .Thickness(3)
                            .Color(Colors.Green);

                        // Caso 5: Ancho fijo y alineación
                        ch.Paragraph("5. Línea con WidthRequest(200) y HorizontalOptions(Center)");
                        ch.HorizontalLine()
                            .WidthRequest(200)
                            .Color(Colors.Orange); // Color para que sea fácil de ver

                        // Caso 6: Alineación a la derecha
                        ch.Paragraph("6. Línea con WidthRequest(100) y HorizontalOptions(End)");
                        ch.HorizontalLine()
                            .WidthRequest(100)
                            .Color(Colors.Purple);

                        // Caso 7: Con margen vertical
                        ch.Paragraph("7. Línea con Margin(0, 20) para crear espacio");
                        ch.HorizontalLine()
                            .Margin(0, 20);

                        // Caso 8: Con margen horizontal (reducirá el ancho de la línea)
                        ch.Paragraph("8. Línea con Margin(50, 0)");
                        ch.HorizontalLine()
                            .Margin(50, 0);

                        // Caso 9: Con padding horizontal (no debería tener efecto visual en la línea)
                        ch.Paragraph("9. Línea con Padding(50, 0) y fondo para ver el cajón");
                        ch.HorizontalLine()
                            .Padding(50, 5, 10, 10)
                            .BackgroundColor(Colors.Gray) // Fondo para ver el área del elemento
                            .Color(Colors.Red);

                        // Caso 10: Combinación completa
                        ch.Paragraph("10. Línea con todas las propiedades personalizadas");
                        ch.HorizontalLine()
                            .Thickness(4)
                            .Color(Colors.DarkCyan)
                            .WidthRequest(300)
                            .Margin(0, 10);

                        ch.Paragraph("Fin de los casos de prueba para HorizontalLine.");
                    });
                }).Build()
                .SaveAsync();

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
                    c.Spacing(15)
                    .Children(ch =>
                    {
                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Paragraph("texto horizontal 1");
                            hsl.Paragraph("texto horizontal 2");
                        });

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Image(new MemoryStream(imageData)).WidthRequest(150);
                            hsl.Image(new MemoryStream(imageData)).WidthRequest(150);
                        });

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Image(new MemoryStream(imageData));
                            hsl.Image(new MemoryStream(imageData));
                        }).WidthRequest(300);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Paragraph("texto horizontal 1").Margin(8f);
                            hsl.Paragraph("texto horizontal 2").Padding(8).BackgroundColor(Colors.LightPink);
                        }).Spacing(8).BackgroundColor(Colors.LightGoldenrodYellow);
                    });
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

    private async void GeneratePdfWithGrid_Clicked(object sender, EventArgs e)
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
                        data.Title("MauiPdfGenerator sample - Grid");
                    });
                })
                .ContentPage<IPdfGrid>()
                .Content(c =>
                {
                    c.BackgroundColor(Colors.LightSlateGray)
                    .Padding(10)
                    .RowSpacing(5)
                    .ColumnSpacing(5)
                    .RowDefinitions(rd =>
                    {
                        rd.GridLength(GridUnitType.Auto); // 0
                        rd.GridLength(GridUnitType.Auto); // 1
                        rd.GridLength(GridUnitType.Auto); // 2
                        rd.GridLength(GridUnitType.Auto); // 3
                        rd.GridLength(GridUnitType.Auto); // 4
                        rd.GridLength(GridUnitType.Auto); // 5
                    })
                    .ColumnDefinitions(cd =>
                    {
                        cd.GridLength(GridUnitType.Star);   // 0
                        cd.GridLength(GridUnitType.Auto);   // 1
                        cd.GridLength(GridUnitType.Auto);   // 2
                        cd.GridLength(GridUnitType.Auto);   // 3
                        cd.GridLength(100);                 // 4
                    })
                    .Children(ch =>
                    {
                        ch.Paragraph("Tabla de cuotas")
                            .FontAttributes(FontAttributes.Bold)
                            .TextTransform(TextTransform.Uppercase)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center)
                            .ColumnSpan(3);

                        ch.Image(new MemoryStream(imageData))
                            .HorizontalOptions(LayoutAlignment.End)
                            .WidthRequest(80)
                            .Column(3)
                            .RowSpan(2);

                        ch.Paragraph("Nombres")
                            .FontAttributes(FontAttributes.Bold)
                            .TextTransform(TextTransform.Uppercase)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center)
                            .Row(1);

                        ch.Paragraph("Cuotas")
                            .FontAttributes(FontAttributes.Bold)
                            .TextTransform(TextTransform.Uppercase)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center)
                            .Row(1)
                            .Column(1)
                            .ColumnSpan(3);

                        ch.Paragraph("Total de abonados")
                            .FontAttributes(FontAttributes.Bold)
                            .TextTransform(TextTransform.Uppercase)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center)
                            .Row(1)
                            .Column(4);

                        ch.Paragraph("1")
                            .FontAttributes(FontAttributes.Bold)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center)
                            .Row(2)
                            .Column(1);
                        ch.Paragraph("2")
                            .FontAttributes(FontAttributes.Bold)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center)
                            .Row(2)
                            .Column(2);

                        ch.Paragraph("3")
                            .Row(2)
                            .Column(3)
                            .FontAttributes(FontAttributes.Bold)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .VerticalOptions(LayoutAlignment.Center);

                        ch.Paragraph("María González").Row(3);
                        ch.Paragraph("120").Row(3).Column(1).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("150").Row(3).Column(2).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("130").Row(3).Column(3).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("400.00").Row(3).Column(4).HorizontalTextAlignment(TextAlignment.End);

                        ch.Paragraph("Luis Pérez").Row(4);
                        ch.Paragraph("100").Row(4).Column(1).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("110").Row(4).Column(2).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("105").Row(4).Column(3).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("315.00").Row(4).Column(4).HorizontalTextAlignment(TextAlignment.End);

                        ch.Paragraph("Carlos Gómez").Row(5);
                        ch.Paragraph("130").Row(5).Column(1).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("140").Row(5).Column(2).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("135").Row(5).Column(3).HorizontalTextAlignment(TextAlignment.End);
                        ch.Paragraph("405.00").Row(5).Column(4).HorizontalTextAlignment(TextAlignment.End);
                    });
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

    private async void GeneratePdfWithMiniGrid_Clicked(object sender, EventArgs e)
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
                        data.Title("MauiPdfGenerator sample - Grid");
                    });
                })
                .ContentPage<IPdfGrid>()
                .Content(c =>
                {
                    c.BackgroundColor(Colors.LightGray)
                    .Padding(5)
                    .WidthRequest(200)
                    .VerticalOptions(LayoutAlignment.End)
                    .Children(ch =>
                    {
                        ch.Paragraph("Hola mundo");
                    });
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
