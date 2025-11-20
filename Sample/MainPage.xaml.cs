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
                    c.Spacing(10).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("Image Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                        ch.Paragraph("1. Comportamiento por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Por defecto, la caja de la imagen ocupa todo el ancho (Fill) y la imagen se ajusta dentro (AspectFit). El fondo revela el tamaño de la caja.")
                            .FontSize(10);
                        ch.Image(imageStream).BackgroundColor(Colors.LightGray);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. Tamaño y Posicionamiento").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("WidthRequest(200) cambia el HorizontalOptions implícito a 'Start'.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(200).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("Se puede centrar la caja explícitamente con HorizontalOptions.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(200).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("3. Modos de Aspecto (en una caja de 150x75)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("AspectFit (defecto): cabe completa, puede dejar espacio.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("AspectFill: llena la caja, puede recortar.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.AspectFill).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Fill: estira para llenar la caja, deforma la imagen.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.Fill).BackgroundColor(Colors.LightSteelBlue);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("4. Composición Final").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Imagen de 80x80, alineada al final, con margen y padding, usando AspectFill.").FontSize(10);
                        ch.Image(imageStream)
                            .WidthRequest(80).HeightRequest(80)
                            .HorizontalOptions(LayoutAlignment.End)
                            .Margin(0, 0, 20, 0)
                            .Padding(5)
                            .Aspect(Aspect.AspectFill)
                            .BackgroundColor(Colors.LightGoldenrodYellow);
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
                c.Spacing(10).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("HorizontalLine Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                        .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                    ch.Paragraph("1. Línea por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Ocupa todo el ancho (Fill), grosor 1, color negro.").FontSize(10);
                    ch.HorizontalLine().BackgroundColor(Colors.LightGray);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("2. Propiedades: Grosor y Color").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.HorizontalLine().Thickness(5).Color(Colors.Red);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("3. Tamaño y Posicionamiento").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("WidthRequest(200) cambia el HorizontalOptions a 'Start'.").FontSize(10);
                    ch.HorizontalLine().WidthRequest(200).Color(Colors.Green).Thickness(2);
                    ch.Paragraph("Centrada explícitamente.").FontSize(10);
                    ch.HorizontalLine().WidthRequest(200).HorizontalOptions(LayoutAlignment.Center).Color(Colors.Green).Thickness(2);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("4. Composición Final").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Línea azul de 3pt de grosor, centrada, con padding vertical de 10 y fondo gris.").FontSize(10);
                    ch.HorizontalLine().Thickness(3).Color(Colors.DarkBlue)
                        .HorizontalOptions(LayoutAlignment.Center).WidthRequest(300)
                        .Padding(0, 10).BackgroundColor(Colors.LightGray);
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
                        ch.Paragraph("VerticalStackLayout Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                        ch.Paragraph("1. Comportamiento por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Un VSL es un contenedor que apila hijos verticalmente. Por defecto, se expande horizontalmente (Fill) y se ajusta a su contenido verticalmente (Start).").FontSize(10);
                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.Padding(5).BackgroundColor(Colors.LightGray);
                            vsl.Children(vsl_ch =>
                            {
                                vsl_ch.Paragraph("Hijo 1");
                                vsl_ch.Paragraph("Hijo 2");
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. Propiedades del Layout").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("'Spacing' añade espacio entre hijos. 'Padding' crea un borde interno.").FontSize(10);
                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.Spacing(10).Padding(10).BackgroundColor(Colors.LightBlue);
                            vsl.Children(vsl_ch =>
                            {
                                vsl_ch.Paragraph("Item 1").BackgroundColor(Colors.White);
                                vsl_ch.Paragraph("Item 2").BackgroundColor(Colors.White);
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("3. Composición: Anidamiento").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Anidar layouts permite crear estructuras complejas. Aquí, un HSL dentro de un VSL.").FontSize(10);
                        ch.VerticalStackLayout(vsl_outer =>
                        {
                            vsl_outer.Padding(10).BackgroundColor(Colors.LightSteelBlue).Spacing(5);
                            vsl_outer.Children(vsl_outer_ch =>
                            {
                                vsl_outer_ch.Paragraph("Contenedor Principal (VSL)");
                                vsl_outer_ch.HorizontalStackLayout(hsl_inner =>
                                {
                                    hsl_inner.Spacing(10).Padding(5).BackgroundColor(Colors.White);
                                    hsl_inner.Children(hsl_inner_ch =>
                                    {
                                        hsl_inner_ch.Image(imageStream).HeightRequest(30);
                                        hsl_inner_ch.Paragraph("Texto junto a la imagen");
                                    });
                                });
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
                        ch.Paragraph("HorizontalStackLayout Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                        ch.Paragraph("1. Comportamiento por Defecto ('Start')").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Por defecto, un HSL se ajusta al ancho de su contenido. El fondo lo demuestra.")
                            .FontSize(10);
                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(10).Padding(5).BackgroundColor(Colors.LightYellow);
                            hsl.Children(hsl_ch =>
                            {
                                hsl_ch.Paragraph("Texto 1");
                                hsl_ch.Image(imageStream).HeightRequest(20);
                                hsl_ch.Paragraph("Texto 2");
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. Forzar Expansión ('Fill')").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Para que el HSL ocupe todo el ancho disponible, se debe especificar 'Fill' explícitamente.")
                            .FontSize(10);
                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.HorizontalOptions(LayoutAlignment.Fill).BackgroundColor(Colors.LightBlue).Padding(5);
                            hsl.Children(hsl_ch =>
                            {
                                hsl_ch.Image(imageStream).HeightRequest(30);
                                hsl_ch.Paragraph("Este texto está en un HSL que ocupa todo el ancho.");
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("3. Desbordamiento (Overflow)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Si los hijos son más anchos que la página, se desbordarán y serán recortados. El sistema de diagnóstico advertirá sobre esto.")
                            .FontSize(10);
                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.BackgroundColor(Colors.LightCoral);
                            hsl.Children(hsl_ch =>
                            {
                                hsl_ch.Image(imageStream);
                                hsl_ch.Image(imageStream);
                                hsl_ch.Image(imageStream);
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
                        .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                    ch.Paragraph("1. Grid Básico").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Definición de filas y columnas con tamaños 'Auto', 'Star' (proporcional) y Fijo. Los hijos se posicionan con .Row() y .Column().").FontSize(10);
                    ch.Grid(g =>
                    {
                        g.BackgroundColor(Colors.WhiteSmoke).RowSpacing(5).ColumnSpacing(10).Padding(5);
                        g.ColumnDefinitions(cd =>
                        {
                            cd.GridLength(GridLength.Auto);
                            cd.GridLength(GridLength.Star);
                            cd.GridLength(100);
                        });
                        g.RowDefinitions(rd =>
                        {
                            rd.GridLength(GridLength.Auto);
                            rd.GridLength(50);
                        });
                        g.Children(g_ch =>
                        {
                            g_ch.Paragraph("Col 0 (Auto)").Row(0).Column(0).BackgroundColor(Colors.LightGray);
                            g_ch.Paragraph("Col 1 (Star)").Row(0).Column(1).BackgroundColor(Colors.LightGray);
                            g_ch.Paragraph("Col 2 (100)").Row(0).Column(2).BackgroundColor(Colors.LightGray);
                        });
                    });
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("2. RowSpan y ColumnSpan").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Un hijo puede ocupar múltiples celdas.").FontSize(10);
                    ch.Grid(g =>
                    {
                        g.BackgroundColor(Colors.WhiteSmoke).RowSpacing(5).ColumnSpacing(5).Padding(5);
                        g.ColumnDefinitions(cd => { cd.GridLength(GridUnitType.Star); cd.GridLength(GridUnitType.Star); });
                        g.RowDefinitions(rd => { rd.GridLength(GridUnitType.Auto); rd.GridLength(GridUnitType.Auto); });
                        g.Children(g_ch =>
                        {
                            g_ch.Paragraph("Celda (0,0)").Row(0).Column(0).BackgroundColor(Colors.LightBlue);
                            g_ch.Paragraph("Celda (0,1)").Row(0).Column(1).BackgroundColor(Colors.LightBlue);
                            g_ch.Paragraph("Esta celda ocupa dos columnas (ColumnSpan=2)")
                                .Row(1).Column(0).ColumnSpan(2)
                                .HorizontalTextAlignment(TextAlignment.Center)
                                .BackgroundColor(Colors.LightGreen);
                        });
                    });
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("3. Caso de Uso: Reparto de Espacio").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("El Grid es la solución correcta para repartir el espacio disponible equitativamente.").FontSize(10);
                    ch.Grid(g =>
                    {
                        g.ColumnSpacing(5);
                        g.ColumnDefinitions(cd =>
                        {
                            cd.GridLength(GridUnitType.Star);
                            cd.GridLength(GridUnitType.Star);
                            cd.GridLength(GridUnitType.Star);
                        });
                        g.Children(gch =>
                        {
                            gch.Image(imageStream).Column(0).BackgroundColor(Colors.LightCyan);
                            gch.Image(imageStream).Column(1).BackgroundColor(Colors.LightCyan);
                            gch.Image(imageStream).Column(2).BackgroundColor(Colors.LightCyan);
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

    async Task<Stream> GetSampleImageStream()
    {
        using var httpClient = new HttpClient();
        var uri = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
        var imageData = await httpClient.GetByteArrayAsync(uri);
        return new MemoryStream(imageData);
    }
}
