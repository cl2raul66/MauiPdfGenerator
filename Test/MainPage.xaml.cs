using MauiPdfGenerator;
using static Microsoft.Maui.Graphics.Colors;
using static Microsoft.Maui.Controls.FontAttributes;
using static Microsoft.Maui.LineBreakMode;
using static Microsoft.Maui.TextAlignment;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;
using static MauiPdfGenerator.Fluent.Enums.DefaultMarginType;
using static MauiPdfGenerator.SourceGenerators.MauiFontAliases;
using static MauiPdfGenerator.Fluent.Enums.PageOrientationType;
using static MauiPdfGenerator.Fluent.Enums.PdfImageSourceType;

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

            doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        Title = "MauiPdfGenerator sample";
                    });
                })
                .ContentPage()
                .DefaultFont(f => f.Size(10))
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph("Text Wrapping Demonstration")
                         .FontSize(16f)
                         .FontAttributes(Bold)
                         .Alignment(Center);
                    c.HorizontalLine();
                    c.Paragraph("Default (WordWrap): This is a relatively long sentence designed to test the default word wrapping behavior which should break lines at spaces.");
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(CharacterWrap);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(HeadTruncation);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(MiddleTruncation);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(TailTruncation);
                    c.Paragraph("NoWrap (behaves like WordWrap but allows clipping): This text is set to NoWrap. If it exceeds the available width, it might get clipped by SkiaSharp depending on the exact rendering path, although the breaking algorithm currently treats it like WordWrap internally for line division.")
                         .LineBreakMode(NoWrap);
                    c.HorizontalLine();
                    c.PdfImage("dotnet_bot.png", IsMauiSource)
                         .WidthRequest(100)
                         .Aspect(Aspect.AspectFit);
                })
                .Build();
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
