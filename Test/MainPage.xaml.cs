using MauiPdfGenerator;
using static Microsoft.Maui.Graphics.Colors;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;
using static MauiPdfGenerator.Fluent.Enums.DefaultMarginType;
using static MauiPdfGenerator.SourceGenerators.MauiFontAliases;
using static Microsoft.Maui.Controls.FontAttributes;

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

            await doc.ContentPage().Content(c =>
                {
                    c.Paragraph("Hola mundo");
                    c.HorizontalLine();
                    c.Paragraph("Cruel");
                    c.Paragraph("Lorem ipsum dolor sit amet velit tincidunt qui tempor magna quod laoreet et odio. Molestie dolores invidunt sit magna sit dolore ea vulputate gubergren est. Option iriure dolor no praesent eos consequat dolor ut. Amet aliquyam eirmod dolore. Kasd elitr takimata accusam sed in eirmod gubergren. No diam invidunt sea consequat stet et stet.");
                })
                .Spacing(8)
                .Build()
                .ContentPage().Content(c =>
                {
                    c.Paragraph("Pagina 2").TextColor(Green).FontSize(22).FontAttribute(Bold);
                    c.HorizontalLine().Color(Red).Thickness(2);
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
