using MauiPdfGenerator.Common;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Core.Integration;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Enums;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator;

public static class MauiPdfGeneratorExtensions
{
    public static MauiAppBuilder UseMauiPdfGenerator(this MauiAppBuilder builder)
    {
        if (builder.Services.Any(sd => sd.ServiceType == typeof(PdfFontConfigurationContext)))
        {
            return builder;
        }

        var fontRegistry = new PdfFontRegistryBuilder();
        builder.Services.AddSingleton(fontRegistry);

        var configContext = new PdfFontConfigurationContext(fontRegistry);
        builder.Services.AddSingleton(configContext);

        builder.Services.AddSingleton<IPdfDocumentFactory>(sp =>
            new PdfDocumentFactory(
                sp.GetRequiredService<PdfFontRegistryBuilder>(),
                sp.GetRequiredService<ILoggerFactory>()
            )
        );

        return builder;
    }

    public static MauiAppBuilder PdfConfigureFonts(
    this MauiAppBuilder builder,
    Action<IFontCollection> configureFontsAction,
    FontDestinationType destination = FontDestinationType.Both)
    {
        builder.ConfigureFonts(originalMauiFonts =>
        {
            PdfFontConfigurationContext? configContext = null;
            var contextDescriptor = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(PdfFontConfigurationContext));
            if (contextDescriptor?.ImplementationInstance is PdfFontConfigurationContext)
            {
                configContext = (PdfFontConfigurationContext)contextDescriptor.ImplementationInstance;
            }

            if (configContext is null)
            {
                var tempServices = new ServiceCollection();
                foreach (var s in builder.Services) tempServices.Add(s);
                var tempSp = tempServices.BuildServiceProvider();
                configContext = tempSp.GetService<PdfFontConfigurationContext>();
            }

            if (configContext is null)
            {
                throw new InvalidOperationException(
                    "MauiPdfGenerator has not been initialized correctly or PdfFontConfigurationContext could not be resolved. " +
                    "Make sure to call .UseMauiPdfGenerator() on MauiAppBuilder before calling the overload of .ConfigureFonts(..., FontDestinationType).");
            }

            PdfFontRegistryBuilder pdfFontRegistry = configContext.FontRegistryBuilder;

            IFontCollection collectionForUserAction;
            if (destination is FontDestinationType.OnlyUI)
            {
                collectionForUserAction = originalMauiFonts;
            }
            else
            {
                collectionForUserAction = new PdfAwareFontCollection(originalMauiFonts, pdfFontRegistry, destination);
            }
            configureFontsAction(collectionForUserAction);
        });

        return builder;
    }
}
