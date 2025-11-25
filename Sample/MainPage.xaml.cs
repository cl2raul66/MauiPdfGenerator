using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Enums;
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

    private async void GenerateParagraphShowcase_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Paragraphs.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - Paragraph Showcase")))
                .ContentPage()
                .Content(c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        // --- TÍTULO ---
                        ch.Paragraph("Paragraph Showcase")
                            .FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Comportamiento por Defecto ---
                        ch.Paragraph("1. Comportamiento por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Un párrafo sin ninguna configuración. Hereda los estilos de la página y se ajusta al ancho disponible.").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Este es un párrafo con sus valores por defecto. El texto fluye naturalmente en múltiples líneas para ajustarse al ancho.");
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Propiedades de Fuente y Estilo ---
                        ch.Paragraph("2. Propiedades de Fuente y Estilo").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Estas propiedades modifican la apariencia del texto en sí.").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Fuente (FontFamily) Comic Semibold.").FontFamily(PdfFonts.Comic);
                        ch.Paragraph("Tamaño de fuente (FontSize) de 16 puntos.").FontSize(16);
                        ch.Paragraph("Estilo (FontAttributes) en Negrita.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Estilo (FontAttributes) en Cursiva.").FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Estilo (FontAttributes) en Negrita y Cursiva.").FontAttributes(FontAttributes.Bold | FontAttributes.Italic);
                        ch.Paragraph("Decoración (TextDecorations) Subrayado.").TextDecorations(TextDecorations.Underline);
                        ch.Paragraph("Decoración (TextDecorations) Tachado.").TextDecorations(TextDecorations.Strikethrough);
                        ch.Paragraph("Decoración (TextDecorations) Subrayado y Tachado.").TextDecorations(TextDecorations.Underline | TextDecorations.Strikethrough);
                        ch.Paragraph("Transformación (TextTransform) a MAYÚSCULAS.").TextTransform(TextTransform.Uppercase);
                        ch.Paragraph("Transformación (TextTransform) a minúsculas.").TextTransform(TextTransform.Lowercase);
                        ch.Paragraph("Color de texto (TextColor) Rojo.").TextColor(Colors.Red);
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Alineación del Contenido (TextAlignment) ---
                        ch.Paragraph("3. Alineación del Contenido (TextAlignment)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Estas propiedades alinean el texto HORIZONTALMENTE DENTRO de la caja del párrafo. Usamos 'Start' y 'End' en lugar de 'Left' y 'Right' para dar soporte a la internacionalización (se adaptan automáticamente a idiomas de escritura de izquierda a derecha (LTR) y de derecha a izquierda (RTL)). La alineación vertical se demuestra en la siguiente sección.").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Este texto está alineado al Inicio (Start).")
                            .HorizontalTextAlignment(TextAlignment.Start).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("Este texto está alineado al Centro (Center).")
                            .HorizontalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("Este texto está alineado al Final (End).")
                            .HorizontalTextAlignment(TextAlignment.End).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("El texto justificado (Justify) se alinea en ambos bordes, ideal para párrafos largos. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .HorizontalTextAlignment(TextAlignment.Justify).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("Nota: Se usó 'BackgroundColor' para visualizar los límites de la caja del párrafo.")
                            .FontSize(10).FontAttributes(FontAttributes.Italic).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        // --- SECCIÓN 4: Modelo de Caja y Posicionamiento ---
                        ch.Paragraph("4. Modelo de Caja y Posicionamiento").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Estas propiedades afectan a la CAJA del párrafo (su tamaño, espaciado y posición). La alineación vertical del texto ('VerticalTextAlignment') solo es visible cuando la caja tiene una altura explícita.").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Padding: Espacio interior de 10pt.")
                            .Padding(10).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("Margin: Espacio exterior de 10pt.")
                            .Margin(10).BackgroundColor(Colors.LightBlue);
                        ch.Paragraph("WidthRequest y HorizontalOptions: Caja de 300pt de ancho, centrada.")
                            .WidthRequest(300).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("HeightRequest y VerticalTextAlignment: Caja de 50pt de alto, con texto centrado verticalmente.")
                            .HeightRequest(50).VerticalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Nota: Se usó 'BackgroundColor' para hacer visible la caja y entender cómo las propiedades afectan su tamaño y posición.")
                            .FontSize(10).FontAttributes(FontAttributes.Italic).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        // --- SECCIÓN 5: Ajuste de Línea (LineBreakMode) ---
                        ch.Paragraph("5. Ajuste de Línea (LineBreakMode)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Controla cómo se ajusta o trunca el texto cuando es más largo que el ancho de su contenedor.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        var longText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
                        ch.Paragraph("WordWrap (defecto): El texto se ajusta por palabras.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText);
                        ch.Paragraph("NoWrap: El texto no se ajusta y se desborda.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.NoWrap);
                        ch.Paragraph("CharacterWrap: El texto se ajusta por caracteres.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.CharacterWrap);
                        ch.Paragraph("HeadTruncation: Se trunca al inicio.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.HeadTruncation);
                        ch.Paragraph("MiddleTruncation: Se trunca en el medio.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.MiddleTruncation);
                        ch.Paragraph("TailTruncation: Se trunca al final.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.TailTruncation);
                        ch.HorizontalLine();

                        // --- SECCIÓN 6: Composición Final ---
                        ch.Paragraph("6. Composición Final").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Ejemplo que combina múltiples propiedades para un resultado específico.").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Fuente Comic, tamaño 12, color rojo, texto centrado horizontalmente en una caja de 375pt que a su vez está centrada en la página horizontalmente, con fondo amarillo y 15 de padding.")
                            .FontFamily(PdfFonts.Comic).FontSize(12).TextColor(Colors.DarkRed)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .WidthRequest(375)
                            .BackgroundColor(Colors.LightYellow)
                            .Padding(15);
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    private async void GenerateImageShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Images.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - Image Showcase")))
                .ContentPage()
                .Content(async c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        // --- TÍTULO ---
                        ch.Paragraph("Image Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Comportamiento por Defecto ---
                        ch.Paragraph("1. Comportamiento por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Al pasar solo el stream, la imagen ocupa todo el ancho disponible (Fill) y ajusta su altura proporcionalmente para mostrarse completa (AspectFit).").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Image(imageStream);
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Modos de Aspecto (Aspect) ---
                        ch.Paragraph("2. Modos de Aspecto (Aspect)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Controla cómo se dibuja la imagen dentro de su caja asignada. Para visualizarlo, forzamos una caja fija de 200x80 con fondo azul claro.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("AspectFit (Defecto): La imagen se escala para caber completa sin deformarse. Pueden sobrar espacios (letterboxing).").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("AspectFill: La imagen llena la caja manteniendo proporción, recortando lo que sobre (zoom).").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.AspectFill).BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("Fill: La imagen se estira para llenar la caja exactamente (se deforma).").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.Fill).BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("Center: La imagen no se escala, se centra en la caja. Si es más grande, se recorta.").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.Center).BackgroundColor(Colors.LightBlue);
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Dimensiones Automáticas ---
                        ch.Paragraph("3. Dimensiones Automáticas").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Si defines solo una dimensión, la otra se calcula automáticamente manteniendo la proporción.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("Solo WidthRequest(100): La altura se ajusta sola.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).BackgroundColor(Colors.LightGreen);

                        ch.Paragraph("Solo HeightRequest(40): El ancho se ajusta solo.").FontSize(10);
                        ch.Image(imageStream).HeightRequest(40).HorizontalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightGreen);
                        ch.HorizontalLine();

                        // --- SECCIÓN 4: Alineación Horizontal ---
                        ch.Paragraph("4. Alineación Horizontal").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Si la imagen (o su caja) es más pequeña que el ancho de la página, 'HorizontalOptions' define su posición.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("Start (Inicio)").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).HorizontalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightPink);

                        ch.Paragraph("Center (Centro)").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightPink);

                        ch.Paragraph("End (Final)").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).HorizontalOptions(LayoutAlignment.End).BackgroundColor(Colors.LightPink);
                        ch.HorizontalLine();

                        // --- SECCIÓN 5: Modelo de Caja (Padding y Margin) ---
                        ch.Paragraph("5. Modelo de Caja").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Padding: Espacio entre el borde de la caja (fondo) y la imagen.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).Padding(10).BackgroundColor(Colors.Orange);

                        ch.Paragraph("Margin: Espacio fuera de la caja.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).Margin(20, 0, 0, 0).BackgroundColor(Colors.Orange);
                        ch.HorizontalLine();

                        // --- SECCIÓN 6: Composición Final (Estilo Polaroid) ---
                        ch.Paragraph("6. Ejemplo Complejo: Estilo Polaroid").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Combinando Padding, BackgroundColor y Alineación para crear un marco.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(card =>
                        {
                            card.WidthRequest(200)
                                .HorizontalOptions(LayoutAlignment.Center)
                                .BackgroundColor(Colors.WhiteSmoke)
                                .Padding(10) // Marco blanco
                                .Spacing(5);

                            card.Children(cardContent =>
                            {
                                // Imagen cuadrada
                                cardContent.Image(imageStream)
                                    .HeightRequest(180)
                                    .BackgroundColor(Colors.LightGray);

                                // Texto al pie
                                cardContent.Paragraph("Microsoft Logo")
                                    .HorizontalTextAlignment(TextAlignment.Center)
                                    .FontFamily(PdfFonts.Comic)
                                    .FontSize(14)
                                    .TextColor(Colors.Black);

                                cardContent.Paragraph("Ejemplo de composición")
                                    .HorizontalTextAlignment(TextAlignment.Center)
                                    .FontSize(8)
                                    .TextColor(Colors.Gray);
                            });
                        });
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    private async void GenerateHorizontalLineShowcase_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Lines.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - HorizontalLine Showcase")))
            .ContentPage()
            .Content(c =>
            {
                c.Spacing(15).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("HorizontalLine Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                        .HorizontalTextAlignment(TextAlignment.Center);

                    // --- SECCIÓN 1: Básicos ---
                    ch.Paragraph("1. Línea Básica").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Por defecto ocupa todo el ancho (Fill), grosor 1pt, color negro.").FontSize(10).FontAttributes(FontAttributes.Italic);
                    ch.HorizontalLine();

                    ch.Paragraph("Con Padding vertical para separar del texto:").FontSize(10);
                    ch.HorizontalLine().Padding(0, 10);

                    // --- SECCIÓN 2: Estilizado ---
                    ch.Paragraph("2. Estilizado (Grosor y Color)").FontSize(18).FontAttributes(FontAttributes.Bold);

                    ch.Paragraph("Grosor 5pt, Color Rojo:").FontSize(10);
                    ch.HorizontalLine().Thickness(5).Color(Colors.Red);

                    ch.Paragraph("Grosor 0.5pt (Línea fina), Color Gris:").FontSize(10);
                    ch.HorizontalLine().Thickness(0.5f).Color(Colors.Gray);

                    ch.Paragraph("Nota: Útil para separadores sutiles en listas.").FontSize(9).FontAttributes(FontAttributes.Italic);
                    ch.HorizontalLine();

                    // --- SECCIÓN 3: Control de Ancho y Alineación ---
                    ch.Paragraph("3. Ancho y Alineación").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Las líneas no tienen que ocupar todo el ancho. Pueden usarse como adornos.").FontSize(10);

                    ch.Paragraph("Ancho 100pt, Alineada al Inicio (Start):").FontSize(10);
                    ch.HorizontalLine().WidthRequest(100).Color(Colors.Blue).Thickness(2);

                    ch.Paragraph("Ancho 50%, Alineada al Centro (Center):").FontSize(10);
                    // Simulamos 50% de una página A4 (~500pt de contenido) -> 250pt
                    ch.HorizontalLine().WidthRequest(250).HorizontalOptions(LayoutAlignment.Center).Color(Colors.Blue).Thickness(2);

                    ch.Paragraph("Ancho 100pt, Alineada al Final (End):").FontSize(10);
                    ch.HorizontalLine().WidthRequest(100).HorizontalOptions(LayoutAlignment.End).Color(Colors.Blue).Thickness(2);
                    ch.HorizontalLine();

                    // --- SECCIÓN 4: Uso en Contexto ---
                    ch.Paragraph("4. Ejemplo en Contexto (Título con Adornos)").FontSize(18).FontAttributes(FontAttributes.Bold);

                    ch.HorizontalStackLayout(hsl =>
                    {
                        hsl.HorizontalOptions(LayoutAlignment.Center).Spacing(10);
                        hsl.Children(h =>
                        {
                            h.HorizontalLine().WidthRequest(50).Thickness(2).Color(Colors.DarkGoldenrod).VerticalOptions(LayoutAlignment.Center);
                            h.Paragraph("CAPÍTULO 1").FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkGoldenrod);
                            h.HorizontalLine().WidthRequest(50).Thickness(2).Color(Colors.DarkGoldenrod).VerticalOptions(LayoutAlignment.Center);
                        });
                    });
                });
            })
            .Build()
            .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateVerticalStackLayoutShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-VSL.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(m => m.Title("VerticalStackLayout Showcase")))
                .ContentPage()
                .Content(async c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("VerticalStackLayout (VSL)").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Mezcla de Vistas (Texto + Línea + Imagen) ---
                        ch.Paragraph("1. Mezcla de Vistas").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("El VSL maneja elementos de distinta naturaleza. Aquí vemos Texto, Línea e Imagen apilados.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.BackgroundColor(Colors.WhiteSmoke).Padding(15).Spacing(10);
                            vsl.Children(items =>
                            {
                                items.Paragraph("Encabezado del Reporte").FontAttributes(FontAttributes.Bold).HorizontalTextAlignment(TextAlignment.Center);
                                items.HorizontalLine().Color(Colors.DarkGray).Thickness(2);
                                items.Image(imageStream).HeightRequest(60).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightGray);
                                items.Paragraph("Pie de foto descriptivo.").FontSize(9).TextColor(Colors.Gray).HorizontalTextAlignment(TextAlignment.Center);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Alineación Transversal (Cross-Axis) con Imágenes ---
                        ch.Paragraph("2. Alineación de Imágenes").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Las imágenes suelen ser más estrechas que el contenedor. 'HorizontalOptions' es vital aquí.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.BackgroundColor(Colors.AliceBlue).Padding(10).Spacing(5);
                            vsl.Children(items =>
                            {
                                items.Paragraph("Start (Izquierda):").HorizontalOptions(LayoutAlignment.Start);
                                items.Image(imageStream).WidthRequest(50).HorizontalOptions(LayoutAlignment.Start);

                                items.Paragraph("Center (Centro):").HorizontalOptions(LayoutAlignment.Center);
                                items.Image(imageStream).WidthRequest(50).HorizontalOptions(LayoutAlignment.Center);

                                items.Paragraph("End (Derecha):").HorizontalOptions(LayoutAlignment.End);
                                items.Image(imageStream).WidthRequest(50).HorizontalOptions(LayoutAlignment.End);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Caso Real (Tarjeta de Perfil) ---
                        ch.Paragraph("3. Caso Real: Tarjeta de Perfil").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Combinando Padding, Bordes (simulados con líneas) y Alineación.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(card =>
                        {
                            card.WidthRequest(250).HorizontalOptions(LayoutAlignment.Center)
                                .BackgroundColor(Colors.White).Padding(0); // Sin padding externo para simular borde

                            // Borde Superior
                            card.Children(content =>
                            {
                                content.HorizontalLine().Color(Colors.DarkBlue).Thickness(5);

                                content.VerticalStackLayout(inner =>
                                {
                                    inner.Padding(15).Spacing(10);
                                    inner.Children(c =>
                                    {
                                        // Avatar Centrado
                                        c.Image(imageStream).WidthRequest(80).HeightRequest(80).HorizontalOptions(LayoutAlignment.Center);

                                        // Nombre y Cargo
                                        c.Paragraph("Jane Doe").FontSize(16).FontAttributes(FontAttributes.Bold).HorizontalTextAlignment(TextAlignment.Center);
                                        c.Paragraph("Senior Architect").TextColor(Colors.Gray).HorizontalTextAlignment(TextAlignment.Center);

                                        // Separador interno
                                        c.HorizontalLine().Color(Colors.LightGray).Padding(20, 5);

                                        // Detalles
                                        c.Paragraph("ID: 8493-22").FontSize(10).HorizontalTextAlignment(TextAlignment.Center);
                                    });
                                });

                                // Borde Inferior
                                content.HorizontalLine().Color(Colors.DarkBlue).Thickness(2);
                            });
                        });
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateHorizontalStackLayoutShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-HSL.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(m => m.Title("HorizontalStackLayout Showcase")))
                .ContentPage()
                .Content(async c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("HorizontalStackLayout (HSL)").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Patrón Icono + Texto ---
                        ch.Paragraph("1. Patrón Icono + Texto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("El uso más común del HSL. Alineamos verticalmente al centro para que el texto cuadre con el icono.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(10).Padding(10).BackgroundColor(Colors.WhiteSmoke);
                            hsl.Children(items =>
                            {
                                // Icono
                                items.Image(imageStream).WidthRequest(24).HeightRequest(24).VerticalOptions(LayoutAlignment.Center);
                                // Texto
                                items.Paragraph("Configuración del Sistema").FontSize(14).VerticalTextAlignment(TextAlignment.Center);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Separadores Verticales (Simulados) ---
                        ch.Paragraph("2. Separadores Verticales").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Usamos HorizontalLine con un ancho pequeño para simular guiones o separadores entre elementos.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(5).HorizontalOptions(LayoutAlignment.Center);
                            hsl.Children(items =>
                            {
                                items.Paragraph("Inicio").TextColor(Colors.Blue);
                                items.HorizontalLine().WidthRequest(10).Color(Colors.Black).VerticalOptions(LayoutAlignment.Center); // Guión
                                items.Paragraph("Productos").TextColor(Colors.Blue);
                                items.HorizontalLine().WidthRequest(10).Color(Colors.Black).VerticalOptions(LayoutAlignment.Center); // Guión
                                items.Paragraph("Detalle").FontAttributes(FontAttributes.Bold);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Alineación Vertical Mixta ---
                        ch.Paragraph("3. Alineación Vertical Mixta").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Elementos de distinta altura (Imagen alta vs Texto bajo) alineados de formas diferentes.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.BackgroundColor(Colors.AliceBlue).HeightRequest(80).Padding(5);
                            hsl.Children(items =>
                            {
                                items.Image(imageStream).WidthRequest(60).HeightRequest(60).VerticalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightPink);
                                items.Paragraph("Top").VerticalOptions(LayoutAlignment.Start);

                                items.Image(imageStream).WidthRequest(60).HeightRequest(60).VerticalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                                items.Paragraph("Center").VerticalOptions(LayoutAlignment.Center);

                                items.Image(imageStream).WidthRequest(60).HeightRequest(60).VerticalOptions(LayoutAlignment.End).BackgroundColor(Colors.LightBlue);
                                items.Paragraph("Bottom").VerticalOptions(LayoutAlignment.End);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 4: Galería de Imágenes (Overflow) ---
                        ch.Paragraph("4. Galería de Imágenes (Overflow)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Varias imágenes en fila. Si exceden el ancho, se recortan (clipping).").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(2);
                            hsl.Children(items =>
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    items.Image(imageStream).WidthRequest(100).HeightRequest(80).Aspect(Aspect.AspectFill);
                                }
                            });
                        });
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateGridShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Grid.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - Grid Showcase")))
            .ContentPage()
            .Content(async c =>
            {
                c.Spacing(15).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("Grid Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                        .HorizontalTextAlignment(TextAlignment.Center);

                    // --- SECCIÓN 1: Media Object (Imagen + Texto) ---
                    ch.Paragraph("1. Patrón Media Object").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Columna Auto para la imagen, Star para el texto. Típico en listas de noticias.").FontSize(10).FontAttributes(FontAttributes.Italic);

                    ch.Grid(g =>
                    {
                        g.ColumnSpacing(10).Padding(10).BackgroundColor(Colors.WhiteSmoke);
                        g.ColumnDefinitions(cd => { cd.GridLength(GridLength.Auto); cd.GridLength(GridLength.Star); });

                        g.Children(cells =>
                        {
                            // Imagen (Auto)
                            cells.Image(imageStream).WidthRequest(80).HeightRequest(60).Aspect(Aspect.AspectFill).VerticalOptions(LayoutAlignment.Start);

                            // Contenido (Star)
                            cells.VerticalStackLayout(vsl =>
                            {
                                vsl.Column(1);
                                vsl.Children(t =>
                                {
                                    t.Paragraph("Título de la Noticia").FontAttributes(FontAttributes.Bold);
                                    t.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor.").FontSize(10).TextColor(Colors.Gray);
                                });
                            });
                        });
                    });
                    ch.HorizontalLine();

                    // --- SECCIÓN 2: Alineación en Celdas (Imagen vs Texto) ---
                    ch.Paragraph("2. Alineación en Celdas").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Centrando una imagen pequeña en una celda grande.").FontSize(10).FontAttributes(FontAttributes.Italic);

                    ch.Grid(g =>
                    {
                        g.BackgroundColor(Colors.AliceBlue).HeightRequest(100);
                        g.ColumnDefinitions(cd => { cd.GridLength(GridUnitType.Star); cd.GridLength(GridUnitType.Star); });

                        g.Children(cells =>
                        {
                            // Celda 1: Texto
                            cells.Paragraph("Texto Centrado").HorizontalTextAlignment(TextAlignment.Center).VerticalTextAlignment(TextAlignment.Center)
                                .Column(0).BackgroundColor(Colors.LightBlue);

                            // Celda 2: Imagen
                            cells.Image(imageStream).WidthRequest(40).HeightRequest(40)
                                .HorizontalOptions(LayoutAlignment.Center).VerticalOptions(LayoutAlignment.Center)
                                .Column(1).BackgroundColor(Colors.LightGreen);
                        });
                    });
                    ch.HorizontalLine();

                    // --- SECCIÓN 3: Tabla Compleja (Factura) ---
                    ch.Paragraph("3. Ejemplo Real: Detalle de Factura").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Mezcla de Texto, Líneas (bordes) e Imágenes (miniaturas de producto).").FontSize(10).FontAttributes(FontAttributes.Italic);

                    ch.Grid(table =>
                    {
                        table.RowSpacing(5);
                        table.ColumnDefinitions(cd =>
                        {
                            cd.GridLength(40);              // Miniatura
                            cd.GridLength(GridUnitType.Star); // Descripción
                            cd.GridLength(60);              // Total
                        });

                        // Encabezados
                        table.Children(c =>
                        {
                            c.Paragraph("Img").FontAttributes(FontAttributes.Bold).Column(0);
                            c.Paragraph("Producto").FontAttributes(FontAttributes.Bold).Column(1);
                            c.Paragraph("Total").FontAttributes(FontAttributes.Bold).HorizontalTextAlignment(TextAlignment.End).Column(2);

                            // Línea separadora debajo de encabezados (Span 3 columnas)
                            c.HorizontalLine().Color(Colors.Black).Thickness(1).Row(1).Column(0).ColumnSpan(3);
                        });

                        // Fila de Datos 1
                        table.Children(c =>
                        {
                            c.Image(imageStream).WidthRequest(30).HeightRequest(30).Aspect(Aspect.AspectFill).Row(2).Column(0);
                            c.Paragraph("Laptop Gamer X500\nIntel i9, 32GB RAM").FontSize(10).Row(2).Column(1).VerticalTextAlignment(TextAlignment.Center);
                            c.Paragraph("$1200.00").HorizontalTextAlignment(TextAlignment.End).VerticalTextAlignment(TextAlignment.Center).Row(2).Column(2);
                        });

                        // Fila de Datos 2
                        table.Children(c =>
                        {
                            c.Image(imageStream).WidthRequest(30).HeightRequest(30).Aspect(Aspect.AspectFill).Row(3).Column(0);
                            c.Paragraph("Mouse Inalámbrico\nErgonómico").FontSize(10).Row(3).Column(1).VerticalTextAlignment(TextAlignment.Center);
                            c.Paragraph("$25.50").HorizontalTextAlignment(TextAlignment.End).VerticalTextAlignment(TextAlignment.Center).Row(3).Column(2);
                        });

                        // Línea final
                        table.Children(c => c.HorizontalLine().Color(Colors.Black).Thickness(1).Row(4).Column(0).ColumnSpan(3));
                    });
                });
            })
            .Build()
            .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateIssue(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Issue.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg =>
            {
                cfg.PageSize(PageSizeType.Letter); 
                cfg.PageOrientation(PageOrientationType.Landscape);
                cfg.Padding(100);
                cfg.MetaData(data => data.Title("MauiPdfGenerator - Issue Showcase"));
            })
            .ContentPage()
            .Content(async c =>
            {
                c.BackgroundColor(Colors.WhiteSmoke);
                c.Children(ch =>
                {
                    ch.Paragraph("Lorem ipsum dolor sit amet Justo magna rebum et gubergren molestie ipsum sit lorem commodo hendrerit eirmod at. Et ad sed invidunt magna. Nulla dolore commodo duis ex kasd. Vero iusto ipsum et ipsum no takimata accusam eirmod at sed nam magna ut dolore veniam qui in sit. Exerci vero vel ea labore dolor elitr tation elitr. Dolore molestie ipsum odio erat ipsum voluptua iriure exerci. Dolor magna labore lorem sit commodo. Et sit dolor ipsum no ex amet eos duis invidunt gubergren eirmod congue dolores magna erat nulla rebum. Facilisi sed est illum elitr eos. Et clita amet diam diam ea est velit et veniam sed luptatum eos labore enim et lorem.\r\n\r\nFacilisi no aliquyam. Ipsum consetetur nulla voluptua diam ipsum dolores amet tincidunt vero at sed tempor diam suscipit invidunt aliquip. No dolor esse magna sea diam diam. Sit laoreet et ipsum. Vero at sed sadipscing feugiat eos et rebum lorem ipsum magna lorem in commodo laoreet. Ipsum accumsan tempor ullamcorper diam sit et erat. Invidunt praesent justo sit id gubergren rebum accusam facilisi amet ut. Consectetuer sadipscing dolores diam stet gubergren. Lorem eu dolores veniam amet erat at sit et sit sea vero elitr. Kasd stet tincidunt eirmod rebum vero exerci sit enim labore. Eos dolor lorem diam et at. Sed facilisi sit illum clita takimata invidunt est. Takimata dolor at ea ad tation consetetur sea congue exerci suscipit sanctus ex dolore cum lorem et zzril. Eos accusam eirmod. Diam nam et lorem tempor eros consetetur duo justo ullamcorper est tempor esse.\r\n\r\nGubergren elit sit veniam et consetetur et dolor sanctus. Vel volutpat eirmod at sadipscing. Ad et vel et no erat diam sea eu suscipit consetetur. Dolore et ea ea accumsan rebum dolores ea at. Est ipsum et et gubergren ea. Sanctus diam sadipscing no dolores iusto ut justo odio in invidunt diam. Dolor iriure lorem lorem consequat et iriure dolore. Eos sea sed rebum at dolore ex tincidunt labore et at soluta at minim sit sit et kasd. Dolor lorem volutpat dolore et. No dolor ipsum dolores. Tempor et in gubergren dolores ut magna diam duis dolore tempor dolor dolor dolor rebum sed eos aliquyam.\r\n\r\nVel clita in vel sadipscing eirmod duis dolore et dolores nonumy. Tation at consectetuer lorem kasd ea duis dolores ea duis lorem et euismod sanctus diam amet diam. Molestie quis cum consectetuer iriure eros. Accusam duo sed nisl feugiat et nonummy elitr nonumy nulla qui ipsum gubergren dolores. Lorem dolore esse eirmod rebum ut elitr tempor clita nobis consetetur dolor no amet duis vero eleifend. Laoreet diam ipsum consequat sed est.\r\n\r\nAt eos et facilisis sadipscing et molestie delenit ipsum invidunt consetetur tempor eum. Erat ad dolores sanctus labore diam amet luptatum tation. Justo aliquyam nisl diam gubergren augue eos et doming. Ea molestie lorem aliquip lobortis nulla nonummy. Sit in aliquyam sit gubergren accusam consetetur ea. Justo accusam eirmod. Facilisis clita dolore. Augue vel veniam tempor vel vel duis no dolor dolor diam sed. Et accumsan nonummy luptatum dignissim ut cum eos et vel et diam dolor sit amet sea et. Diam accusam magna gubergren amet dolores aliquyam amet sed dolor tempor elitr et labore odio accusam. Vel at est sanctus diam. Ut tincidunt lorem dolor voluptua voluptua magna dolores qui erat at et takimata amet ullamcorper facilisis veniam. No et invidunt invidunt kasd stet et takimata eirmod. Lorem et vulputate lorem stet takimata sit sit sit sed lorem ut sit esse invidunt aliquyam.\r\n\r\nNulla et justo ipsum gubergren enim wisi clita. Amet magna tempor voluptua. Labore lorem justo consequat et in sanctus lorem est nonumy eu ut volutpat sed nibh sit enim eu. Invidunt diam dolores no sit eos et nonumy nostrud sit wisi justo gubergren delenit ut dolor. Ad amet aliquyam esse sed feugiat ea. Assum dolore sit et accumsan kasd enim amet justo laoreet elitr lorem sed et stet est consequat. Lorem nulla justo eos euismod consectetuer sed. Justo dolore kasd vel. Eu dolor facilisis voluptua ut. Amet eirmod vel dolor ut elitr eirmod sadipscing veniam. No est labore et wisi nostrud elitr consetetur et sadipscing molestie sit. Elitr amet et tempor magna illum duo congue. Dolor clita duo facilisi dolor eros ut magna est hendrerit et. Consetetur doming diam gubergren stet dolores gubergren nisl kasd elitr dolores consequat. Euismod ipsum duis est sea nulla sadipscing ut rebum dolore ea lorem vero. Ea feugait ut nonumy ipsum magna invidunt eu diam nonummy doming vulputate quis elitr.\r\n\r\nSit ipsum ex elitr suscipit quis eos at diam sea mazim. Dolore et clita sanctus assum dolor dolore consetetur gubergren lorem. Nam stet kasd sea ea. Velit et dolor diam sea tation amet laoreet ex. Invidunt invidunt takimata at invidunt tation ea adipiscing. Dolore nonumy ipsum eos clita sit te ipsum labore consetetur sit duis accusam elitr et. Elitr in vero vero elitr duo labore eirmod et in nibh aliquam dolor augue amet diam amet consequat. Nonumy lorem dolor invidunt magna lorem minim eu vero est vero ea consetetur elitr eos magna dolores sed clita. Consequat eirmod gubergren invidunt. In at illum accusam amet vel esse et gubergren diam accusam lorem dolores eirmod vero aliquyam lobortis. Praesent sit facilisis adipiscing eros et dolore et consequat et dolor amet sit amet voluptua tempor. Eos in suscipit autem qui tation amet ex ea kasd et lorem lorem nonumy justo sadipscing stet consetetur facer. Clita praesent ex et clita ipsum ea ea gubergren voluptua. Sanctus aliquyam takimata duo tempor ipsum feugiat dolor takimata duis sed. Lorem dolore et autem augue dignissim takimata eirmod. Invidunt augue duo et at invidunt ut odio. Rebum nulla nonumy est nulla. Dolor lorem dolore consetetur qui dolor.\r\n\r\nLabore sanctus ex vulputate augue ut. Erat dolore eirmod nonumy et esse eirmod gubergren clita stet iriure sed ut ipsum et erat dolore tincidunt. Amet suscipit assum clita et possim molestie accusam vel justo in rebum diam. Et esse et sit luptatum ullamcorper ipsum no amet et dolore. Et zzril eos rebum ipsum dolore et et duo lorem gubergren gubergren duo et dolor. Diam ullamcorper diam erat ipsum est consectetuer duo. Ut sit justo. Sed facilisis duo magna no ut eirmod et iusto et at ipsum diam accusam ut at clita et. Lorem dolore dolor est et consectetuer sea ea rebum est ea dolores luptatum dolor. No volutpat sed augue no eu stet dolore zzril quod clita et clita qui dolores sadipscing tempor ipsum. Rebum eirmod wisi iriure commodo. Amet liber at clita facilisi. Sed dolor justo stet delenit et et ullamcorper. Et nulla voluptua dolore eos clita nonumy enim. Dolore sed ea nulla dolore praesent exerci. Takimata rebum dolor sed assum invidunt eos voluptua labore dolor dolor facilisi dignissim iriure laoreet kasd.").BackgroundColor(Colors.LightGray);
                });
            })
            .Build()
            .SaveAsync(targetFilePath);
            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task<Stream> GetSampleImageStream()
    {
        using var httpClient = new HttpClient();
        var uri = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
        var imageData = await httpClient.GetByteArrayAsync(uri);
        return new MemoryStream(imageData);
    }
}
