using MauiPdfGenerator;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;

namespace Test;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void GeneratePdf_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");
        try
        { 
            var doc = PdfGenerator.CreateDocument();
            doc.Configuration(config =>
            {
                config.PageSize(Letter);
                config.Margins(20);
                config.PdfFontRegistry(f =>
                {
                    f.Font("Helvetica").Default();
                    f.Font("Arial").IsEmbeddedFont();
                });
                config.MetaData(md =>
                {
                    md.CreationDate(DateTime.Now);
                    md.Title("Sample");
                    md.Author("Test");
                    md.Creator("Test");
                });
            });
            
            //doc.Page(pg =>
            //{
            //    pg.Paragraph("¡Hola Mundo!").FontFamily(Helvetica).TextColors(Colors.Blue);
            //    pg.Paragraph(p =>
            //    {
            //        p.Text("¡Hola Mundo!").FontSize(16).FontAttributes(FontAttributes.Bold);
            //        p.Text("¡Hola Mundo!").FontSize(14).TextTransform(TextTransform.Uppercase);
            //    }).FontFamily(Arial);
            //});
            
            await doc.SaveAsync(targetFilePath);
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
