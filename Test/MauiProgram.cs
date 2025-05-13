using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Maui.Controls;

namespace Test;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
