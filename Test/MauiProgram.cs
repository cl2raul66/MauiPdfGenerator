using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Maui.Controls;
using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Enums;

namespace Test;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiPdfGenerator()
            .PdfConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("comic.ttf", "Comic");
                fonts.AddFont("comicbd.ttf", "ComicBold");
                fonts.AddFont("comici.ttf", "ComicBoldItalica");
                fonts.AddFont("comicz.ttf", "ComicItalica");
            });

        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
