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
                    c.Spacing(10);
                    c.Padding(20);
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
                        ch.Paragraph("Este párrafo tiene un Padding de 10.").Padding(10).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("Este párrafo tiene un Margin de 10.").Margin(10).BackgroundColor(Colors.LightBlue);
                        ch.Paragraph("Este párrafo tiene un WidthRequest de 300 y está centrado.")
                            .WidthRequest(300).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("Este párrafo tiene un HeightRequest de 50 y el texto está centrado verticalmente.")
                            .HeightRequest(50).BackgroundColor(Colors.LightSteelBlue).VerticalTextAlignment(TextAlignment.Center);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("Ajuste de Línea (en una caja de 400pt)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.WidthRequest(400).HorizontalOptions(LayoutAlignment.Center).Padding(5).Spacing(5).BackgroundColor(Colors.LightGray);
                            vsl.Children(vsl_ch =>
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
                        ch.Paragraph("Sin tamaño ni alineación, la imagen se centra (porque su caja es 'Fill' y el contenido se centra en la caja).");
                        ch.Image(new MemoryStream(imageData)).BackgroundColor(Colors.LightGray);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. Con Tamaño Explícito").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Con WidthRequest o HeightRequest, la alineación por defecto es 'Start' (izquierda).");
                        ch.Image(new MemoryStream(imageData)).WidthRequest(200).BackgroundColor(Colors.LightPink);
                        ch.Image(new MemoryStream(imageData)).HeightRequest(50).BackgroundColor(Colors.LightBlue);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("3. Alineación Explícita").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("HorizontalOptions anula el comportamiento por defecto.");
                        ch.Image(new MemoryStream(imageData)).WidthRequest(200).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Image(new MemoryStream(imageData)).WidthRequest(200).HorizontalOptions(LayoutAlignment.End).BackgroundColor(Colors.LightYellow);
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("4. Modos de Aspecto (en una caja de 150x75)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("AspectFit (defecto): cabe completa, puede dejar espacio.");
                        ch.Image(new MemoryStream(imageData)).WidthRequest(150).HeightRequest(75).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("AspectFill: llena la caja, puede recortar.");
                        ch.Image(new MemoryStream(imageData)).WidthRequest(150).HeightRequest(75).Aspect(Aspect.AspectFill).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Fill: estira para llenar la caja, deforma la imagen.");
                        ch.Image(new MemoryStream(imageData)).WidthRequest(150).HeightRequest(75).Aspect(Aspect.Fill).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Center: no escala, centra y recorta.");
                        ch.Image(new MemoryStream(imageData)).WidthRequest(150).HeightRequest(75).Aspect(Aspect.Center).BackgroundColor(Colors.LightSteelBlue);
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    private async void GeneratePdfWithHorizontalLine_Clicked(object sender, EventArgs e)
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
                    ch.HorizontalLine();
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("2. Grosor y Color").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.HorizontalLine().Thickness(5).Color(Colors.Red);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("3. Ancho y Alineación").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.HorizontalLine().WidthRequest(200).HorizontalOptions(LayoutAlignment.Start).Color(Colors.Green);
                    ch.HorizontalLine().WidthRequest(200).HorizontalOptions(LayoutAlignment.Center).Color(Colors.Green);
                    ch.HorizontalLine().WidthRequest(200).HorizontalOptions(LayoutAlignment.End).Color(Colors.Green);
                    ch.HorizontalLine().Margin(0, 10);

                    ch.Paragraph("4. Padding y BackgroundColor").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("El padding crea espacio dentro de la caja de la línea.");
                    ch.HorizontalLine().Thickness(2).Padding(0, 10).BackgroundColor(Colors.LightGray).Color(Colors.DarkBlue);
                });
            })
            .Build()
            .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
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

                        ch.Paragraph("1. HorizontalStackLayout").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Se ajusta al ancho de su contenido (`Start`) y alinea a sus hijos en una fila.");
                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(10).Padding(5).BackgroundColor(Colors.LightYellow);
                            hsl.Children(hsl_ch =>
                            {
                                hsl_ch.Paragraph("Texto 1");
                                hsl_ch.Image(new MemoryStream(imageData)).HeightRequest(20);
                                hsl_ch.Paragraph("Texto 2");
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("2. VerticalStackLayout Anidado (Atómico)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Este bloque es atómico. Si no cabe, se moverá completo a la siguiente página.");
                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.Spacing(5).Padding(10).BackgroundColor(Colors.LightPink)
                               .HorizontalOptions(LayoutAlignment.Center).WidthRequest(400); 
                            vsl.Children(vsl_ch =>
                            {
                                vsl_ch.Paragraph("Título del Bloque Atómico").FontAttributes(FontAttributes.Bold);
                                vsl_ch.Paragraph("Este texto no se separará del título por un salto de página.");
                            });
                        });
                        ch.HorizontalLine().Margin(0, 10);

                        ch.Paragraph("3. Anidamiento Complejo").FontSize(18).FontAttributes(FontAttributes.Bold);
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
                                        hsl_inner_ch.Image(new MemoryStream(imageData)).HeightRequest(40);
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

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Grid.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg =>
            {
                cfg.MetaData(data => data.Title("MauiPdfGenerator - Grid Showcase"));
            })
            .ContentPage<IPdfGrid>() 
            .Content(grid =>
            {
                grid.Margin(20).BackgroundColor(Colors.WhiteSmoke)
                    .RowSpacing(5).ColumnSpacing(10)
                    .ColumnDefinitions(cd =>
                    {
                        cd.GridLength(GridLength.Auto); 
                        cd.GridLength(GridLength.Star); 
                        cd.GridLength(100);             
                    })
                    .RowDefinitions(rd =>
                    {
                        rd.GridLength(GridLength.Auto); 
                        rd.GridLength(GridLength.Auto); 
                        rd.GridLength(50);              
                    })
                    .Children(ch =>
                    {
                        ch.Paragraph("Header 1").FontAttributes(FontAttributes.Bold).Row(0).Column(0);
                        ch.Paragraph("Header 2 (Star Column)").FontAttributes(FontAttributes.Bold).Row(0).Column(1);
                        ch.Paragraph("Header 3 (Fixed)").FontAttributes(FontAttributes.Bold).Row(0).Column(2);

                        ch.Image(new MemoryStream(imageData)).Row(1).Column(0).HeightRequest(40);
                        ch.Paragraph("Este texto está en una columna Star, por lo que se ajustará para llenar el espacio disponible. Lorem ipsum dolor sit amet, consectetur adipiscing elit.")
                            .Row(1).Column(1);
                        ch.Paragraph("Contenido de ancho fijo, alineado a la derecha.")
                            .Row(1).Column(2).HorizontalTextAlignment(TextAlignment.End);

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
            await DisplayAlert("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    private async void GeneratePdfWithGrid1_Clicked(object sender, EventArgs e)
    {
        byte[] imageData;
        using (var httpClient = new HttpClient())
        {
            var uri = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
            imageData = await httpClient.GetByteArrayAsync(uri);
        }

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");

        var doc = pdfDocFactory.CreateDocument();
        await doc.ContentPage<IPdfGrid>()
            .Content(c =>
            {
                c.BackgroundColor(Colors.Snow);
                c.RowDefinitions(rd =>
                {
                    rd.GridLength(GridLength.Auto);
                    rd.GridLength(GridLength.Star);
                    rd.GridLength(GridLength.Auto);
                })
                .Children(ch =>
                {
                    ch.Paragraph("Titulo")
                    .BackgroundColor(Colors.LightCoral)
                    .HorizontalTextAlignment(TextAlignment.Center)
                    .Padding(5);
                    ch.Paragraph("Cuerpo")
                    .VerticalTextAlignment(TextAlignment.Center)
                    .HorizontalTextAlignment(TextAlignment.Center)
                    .BackgroundColor(Colors.LightCyan)
                    .Row(1);
                    ch.Paragraph("Pie de pagina")
                    .HorizontalTextAlignment(TextAlignment.Center)
                    .BackgroundColor(Colors.LightCoral)
                    .Padding(5)
                    .Row(2);
                });
            })
            .Build()
            .SaveAsync(targetFilePath);

        await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
    }

}
