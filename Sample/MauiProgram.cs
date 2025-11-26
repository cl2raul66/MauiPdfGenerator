using Microsoft.Extensions.Logging;
using MauiPdfGenerator;

namespace Sample;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
