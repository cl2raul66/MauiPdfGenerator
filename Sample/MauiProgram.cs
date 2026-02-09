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
                fonts.AddFont("OpenSans.ttf", "OpenSans");
                fonts.AddFont("comic.ttf", "Comic");

                fonts.AddFont("NotoSansArabic.ttf", "NotoSansArabic");
                fonts.AddFont("Harmattan.ttf", "Harmattan");

                fonts.AddFont("NotoSansHebrew.ttf", "NotoSansHebrew");
                fonts.AddFont("VarelaRound.ttf", "VarelaRound");
            });

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
