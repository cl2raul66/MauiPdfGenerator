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

    private async void GeneratePdfParagraphs_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Paragraphs.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data => data.Title("MauiPdfGenerator - Paragraph Showcase"));
                })
                .ContentPage()
                .Content(c =>
                {
                    c.Spacing(10).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("Paragraph Showcase")
                            .FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                        ch.Paragraph("Alineación de Texto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Alineado a la Izquierda (Start).").HorizontalTextAlignment(TextAlignment.Start);
                        ch.Paragraph("Alineado al Centro.").HorizontalTextAlignment(TextAlignment.Center);
                        ch.Paragraph("Alineado a la Derecha (End).").HorizontalTextAlignment(TextAlignment.End);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Estilos y Decoraciones").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Texto en Negrita.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Texto en Cursiva.").FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Texto en Negrita y Cursiva.").FontAttributes(FontAttributes.Bold | FontAttributes.Italic);
                        ch.Paragraph("Texto Subrayado.").TextDecorations(TextDecorations.Underline);
                        ch.Paragraph("Texto Tachado.").TextDecorations(TextDecorations.Strikethrough);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Transformaciones de Texto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Este texto está en MAYÚSCULAS.").TextTransform(TextTransform.Uppercase);
                        ch.Paragraph("Este Texto Está En Minúsculas.").TextTransform(TextTransform.Lowercase);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Familias de Fuente").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Fuente OpenSans Regular.").FontFamily(PdfFonts.OpenSansRegular);
                        ch.Paragraph("Fuente OpenSans Semibold.").FontFamily(PdfFonts.OpenSansSemibold);
                        ch.Paragraph("Fuente Comic (si está registrada).").FontFamily(PdfFonts.Comic);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Tamaño y Color").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Texto con FontSize 22.").FontSize(22);
                        ch.Paragraph("Texto con color Rojo.").TextColor(Colors.Red);
                        ch.Paragraph("Texto azul sobre fondo amarillo.").TextColor(Colors.DarkBlue).BackgroundColor(Colors.LightYellow);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Caja y Espaciado").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Este párrafo tiene un Padding de 10. El fondo se extiende por el área de padding.")
                            .Padding(10).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("Este párrafo tiene un Margin de 10. El fondo NO se extiende por el margen.")
                            .Margin(10).BackgroundColor(Colors.LightBlue);
                        ch.Paragraph("Este párrafo tiene un WidthRequest de 300 y está centrado horizontalmente.")
                            .WidthRequest(300).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("Este párrafo tiene un HeightRequest de 50 y el texto está centrado verticalmente.")
                            .HeightRequest(50).BackgroundColor(Colors.LightSteelBlue).VerticalTextAlignment(TextAlignment.Center);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Ajuste de Línea (en una caja de 400pt)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.VerticalStackLayout(vsl_inner =>
                        {
                            vsl_inner.WidthRequest(400).HorizontalOptions(LayoutAlignment.Center).Padding(5).Spacing(5).BackgroundColor(Colors.LightGray);
                            vsl_inner.Children(vsl_ch =>
                            {
                                vsl_ch.Paragraph("WordWrap:").FontAttributes(FontAttributes.Bold);
                                vsl_ch.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                                    .LineBreakMode(LineBreakMode.WordWrap);
                                vsl_ch.Paragraph("CharacterWrap:").FontAttributes(FontAttributes.Bold);
                                vsl_ch.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                                    .LineBreakMode(LineBreakMode.CharacterWrap);
                                vsl_ch.Paragraph("HeadTruncation:").FontAttributes(FontAttributes.Bold);
                                vsl_ch.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                                    .LineBreakMode(LineBreakMode.HeadTruncation);
                                vsl_ch.Paragraph("MiddleTruncation:").FontAttributes(FontAttributes.Bold);
                                vsl_ch.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                                    .LineBreakMode(LineBreakMode.MiddleTruncation);
                                vsl_ch.Paragraph("TailTruncation:").FontAttributes(FontAttributes.Bold);
                                vsl_ch.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                                    .LineBreakMode(LineBreakMode.TailTruncation);
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

    private async void GeneratePdfImages_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Images.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data => data.Title("MauiPdfGenerator - Image Showcase"));
                })
                .ContentPage()
                .Content(c =>
                {
                    c.Spacing(10).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("Image Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                        ch.Paragraph("1. Comportamiento por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("En un VerticalStackLayout, el HorizontalOptions por defecto es 'Fill'. El fondo (BackgroundColor) revela que la caja de la imagen ocupa todo el ancho. La imagen (el contenido) se centra por defecto dentro de esa caja gracias a AspectFit.")
                            .FontSize(10);
                        ch.Image(imageStream).BackgroundColor(Colors.LightGray);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. Con Tamaño Explícito").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Cuando se usa Width/HeightRequest, el HorizontalOptions por defecto cambia a 'Start' para respetar el tamaño solicitado. El fondo revela el tamaño real de la caja.")
                            .FontSize(10);
                        ch.Image(imageStream).WidthRequest(200).BackgroundColor(Colors.LightPink);
                        ch.Image(imageStream).HeightRequest(50).BackgroundColor(Colors.LightBlue);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("3. Alineación Explícita").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("HorizontalOptions puede anular el comportamiento por defecto para posicionar la caja.")
                            .FontSize(10);
                        ch.Image(imageStream).WidthRequest(200).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Image(imageStream).WidthRequest(200).HorizontalOptions(LayoutAlignment.End).BackgroundColor(Colors.LightYellow);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("4. Modos de Aspecto (en una caja de 150x75)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("AspectFit (defecto): cabe completa, puede dejar espacio.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("AspectFill: llena la caja, puede recortar.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.AspectFill).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Fill: estira para llenar la caja, deforma la imagen.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.Fill).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Center: no escala, centra y recorta.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).HeightRequest(75).Aspect(Aspect.Center).BackgroundColor(Colors.LightSteelBlue);
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

    private async void GeneratePdfHorizontalLine_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Lines.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg =>
            {
                cfg.MetaData(data => data.Title("MauiPdfGenerator - HorizontalLine Showcase"));
            })
            .ContentPage()
            .Content(c =>
            {
                c.Spacing(10).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("HorizontalLine Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                        .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                    ch.Paragraph("1. Línea por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Por defecto, una línea ocupa todo el ancho disponible (HorizontalOptions='Fill').").FontSize(10);
                    ch.HorizontalLine();
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("2. Grosor y Color").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.HorizontalLine().Thickness(5).Color(Colors.Red);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("3. Ancho y Alineación").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Con WidthRequest, el HorizontalOptions por defecto cambia a 'Start'.").FontSize(10);
                    ch.HorizontalLine().WidthRequest(200).Color(Colors.Green);
                    ch.Paragraph("Se puede anular con HorizontalOptions explícito:").FontSize(10);
                    ch.HorizontalLine().WidthRequest(200).HorizontalOptions(LayoutAlignment.Center).Color(Colors.Green);
                    ch.HorizontalLine().WidthRequest(200).HorizontalOptions(LayoutAlignment.End).Color(Colors.Green);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("4. Padding y BackgroundColor").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("El padding crea espacio vertical dentro de la caja de la línea, entre el fondo y la línea misma.").FontSize(10);
                    ch.HorizontalLine().Thickness(2).Padding(0, 10).BackgroundColor(Colors.LightGray).Color(Colors.DarkBlue);
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

    private async void GeneratePdfLayouts_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Layouts.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data => data.Title("MauiPdfGenerator - Layout Showcase"));
                })
                .ContentPage()
                .Content(c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("Layout Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center).Margin(0, 0, 0, 20);

                        ch.Paragraph("1. HorizontalStackLayout (Comportamiento por Defecto)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Por defecto, un HSL se ajusta al ancho de su contenido (HorizontalOptions='Start'). Sus hijos se expanden verticalmente (VerticalOptions='Fill').")
                            .FontSize(10);
                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(10).Padding(5).BackgroundColor(Colors.LightYellow);
                            hsl.Children(hsl_ch =>
                            {
                                hsl_ch.Paragraph("Texto 1");
                                hsl_ch.Image(imageStream).HeightRequest(20); // HeightRequest anula el 'Fill'
                                hsl_ch.Paragraph("Texto 2");
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. Anidamiento Complejo").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.VerticalStackLayout(vsl_outer =>
                        {
                            vsl_outer.Padding(10).BackgroundColor(Colors.LightGray);
                            vsl_outer.Children(vsl_outer_ch =>
                            {
                                vsl_outer_ch.Paragraph("Contenedor Principal (VSL)");
                                vsl_outer_ch.HorizontalStackLayout(hsl_inner =>
                                {
                                    hsl_inner.Spacing(10).Padding(5).BackgroundColor(Colors.LightBlue);
                                    hsl_inner.Children(hsl_inner_ch =>
                                    {
                                        hsl_inner_ch.Image(imageStream); // Sin HeightRequest, se expande a la altura del HSL
                                        hsl_inner_ch.VerticalStackLayout(vsl_inner =>
                                        {
                                            vsl_inner.Spacing(2).BackgroundColor(Colors.LightGreen);
                                            vsl_inner.Children(vsl_inner_ch =>
                                            {
                                                vsl_inner_ch.Paragraph("Texto Anidado 1").FontSize(10);
                                                vsl_inner_ch.Paragraph("Texto Anidado 2").FontSize(10);
                                            });
                                        });
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

    private async void GeneratePdfGrid_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Grid.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg =>
            {
                cfg.MetaData(data => data.Title("MauiPdfGenerator - Grid Showcase"));
            })
            .ContentPage<IPdfGrid>()
            .Content(c =>
            {
                // Usamos un Grid como layout raíz de la página
                c.Margin(20)
                .BackgroundColor(Colors.WhiteSmoke)
                .RowSpacing(5).ColumnSpacing(10)
                .ColumnDefinitions(cd =>
                {
                    cd.GridLength(GridLength.Auto); // Col 0: Se ajusta al contenido más ancho
                    cd.GridLength(GridLength.Star); // Col 1: Ocupa el espacio restante
                    cd.GridLength(100);             // Col 2: Ancho fijo
                })
                .RowDefinitions(rd =>
                {
                    rd.GridLength(GridLength.Auto); // Fila 0: Se ajusta al contenido más alto
                    rd.GridLength(GridLength.Auto); // Fila 1
                    rd.GridLength(50);              // Fila 2: Alto fijo
                })
                .Children(ch =>
                {
                    // Fila 0
                    ch.Paragraph("Header 1 (Auto)").FontAttributes(FontAttributes.Bold).Row(0).Column(0);
                    ch.Paragraph("Header 2 (Star)").FontAttributes(FontAttributes.Bold).Row(0).Column(1);
                    ch.Paragraph("Header 3 (Fixed)").FontAttributes(FontAttributes.Bold).Row(0).Column(2);

                    // Fila 1
                    ch.Image(imageStream).Row(1).Column(0).HeightRequest(40);
                    ch.Paragraph("Este texto está en una columna Star, por lo que se ajustará para llenar el espacio disponible. Lorem ipsum dolor sit amet, consectetur adipiscing elit.")
                        .Row(1).Column(1);
                    ch.Paragraph("Contenido de ancho fijo, alineado a la derecha.")
                        .Row(1).Column(2).HorizontalTextAlignment(TextAlignment.End);

                    // Fila 2
                    ch.Paragraph("Celda con ColumnSpan=2 y centrada")
                        .Row(2).Column(0).ColumnSpan(2)
                        .HorizontalOptions(LayoutAlignment.Center)
                        .VerticalOptions(LayoutAlignment.Center)
                        .BackgroundColor(Colors.LightBlue);

                    ch.Paragraph("Celda (2,2)")
                        .Row(2).Column(2)
                        .VerticalOptions(LayoutAlignment.End)
                        .HorizontalTextAlignment(TextAlignment.End)
                        .BackgroundColor(Colors.LightGreen);
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

    private async void GeneratePdfAdvancedLayout_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-AdvancedLayout.pdf");

        var doc = pdfDocFactory.CreateDocument();
        await doc.Configuration(cfg => cfg.MetaData(m => m.Title("Advanced Layouts")))
            .ContentPage()
            .Content(c =>
            {
                c.Spacing(15).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("Layouts Avanzados: Overflow y Reparto de Espacio").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.HorizontalLine();

                    ch.Paragraph("0. HSL con un solo hijo (Comportamiento por defecto)").FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Por defecto, el HSL se ajusta al ancho de su contenido (HorizontalOptions='Start').")
                        .FontSize(10);
                    ch.HorizontalStackLayout(hsl =>
                    {
                        hsl.BackgroundColor(Colors.LightCoral);
                        hsl.Children(hsl_ch =>
                        {
                            hsl_ch.Image(imageStream);
                        });
                    });
                    ch.HorizontalLine();

                    ch.Paragraph("0.1 Solución: Forzar Expansión con HorizontalOptions.Fill").FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Para que el HSL ocupe todo el ancho disponible, como en MAUI, se debe especificar 'Fill' explícitamente.")
                        .FontSize(10);
                    ch.HorizontalStackLayout(hsl =>
                    {
                        hsl.HorizontalOptions(LayoutAlignment.Fill).BackgroundColor(Colors.LightBlue);
                        hsl.Children(hsl_ch =>
                        {
                            hsl_ch.Image(imageStream);
                        });
                    });
                    ch.HorizontalLine();

                    ch.Paragraph("1. Desbordamiento (Overflow) en HorizontalStackLayout").FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Un HSL crece con su contenido. Si los hijos son más anchos que la página, se desbordarán y serán recortados. El sistema de diagnóstico (si está habilitado) advertirá sobre esto.")
                        .FontSize(10);
                    ch.HorizontalStackLayout(hsl =>
                    {
                        hsl.BackgroundColor(Colors.LightCoral);
                        hsl.Children(hsl_ch =>
                        {
                            hsl_ch.Image(imageStream);
                            hsl_ch.Image(imageStream);
                            hsl_ch.Image(imageStream); // Esta imagen probablemente se cortará
                        });
                    });
                    ch.HorizontalLine();

                    ch.Paragraph("2. Solución: Reparto de Espacio con Grid y Columnas 'Star'").FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Para repartir el espacio disponible equitativamente, la herramienta correcta es un Grid con columnas 'Star'. Cada columna 'Star' recibe una porción del espacio restante.")
                        .FontSize(10);
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

    async Task<Stream> GetSampleImageStream()
    {
        using var httpClient = new HttpClient();
        var uri = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
        var imageData = await httpClient.GetByteArrayAsync(uri);
        return new MemoryStream(imageData);
    }
}
