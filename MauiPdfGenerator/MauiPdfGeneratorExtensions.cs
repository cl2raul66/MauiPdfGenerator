using MauiPdfGenerator.Core;
using MauiPdfGenerator.Core.Integration;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Enums;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MauiPdfGenerator;

public static class MauiPdfGeneratorExtensions
{
    public static MauiAppBuilder UseMauiPdfGenerator(this MauiAppBuilder builder)
    {
        // Evitar doble inicialización
        if (builder.Services.Any(sd => sd.ServiceType == typeof(PdfFontConfigurationContext)))
        {
            return builder;
        }

        var fontRegistry = new PdfFontRegistryBuilder();
        // Registrar PdfFontRegistryBuilder como singleton para uso posterior por PdfDocumentFactory, etc.
        builder.Services.AddSingleton(fontRegistry);

        // Registrar el contexto de configuración que contiene el PdfFontRegistryBuilder.
        // Este contexto se usará en la sobrecarga de ConfigureFonts.
        var configContext = new PdfFontConfigurationContext(fontRegistry);
        builder.Services.AddSingleton(configContext); // Registrar la instancia

        builder.Services.AddSingleton<IPdfDocumentFactory, PdfDocumentFactory>();

        return builder;
    }

    public static MauiAppBuilder ConfigureFonts(
        this MauiAppBuilder builder,
        Action<IFontCollection> configureFontsAction,
        FontDestinationType destination)
    {
        // Esta sobrecarga depende de que UseMauiPdfGenerator haya registrado PdfFontConfigurationContext.
        // Cuando MAUI ejecute las acciones de ConfigureServices, podremos resolverlo.

        // La clave es cómo obtener PdfFontConfigurationContext *aquí*, antes de Build().
        // El propio MauiAppBuilder tiene acceso a los servicios que está construyendo.
        // Sin embargo, resolver un servicio directamente aquí es complicado.

        // Solución: Envolvemos la acción del usuario en otra acción que SÍ tendrá acceso
        // al IServiceProvider cuando MAUI procese la configuración de fuentes.
        // Esto es un poco indirecto pero evita construir SPs prematuramente.

        // Este enfoque es más seguro: registramos una acción que MAUI llamará
        // pasándole el IFontCollection. Dentro de esa acción, resolvemos nuestro contexto.
        builder.ConfigureFonts(originalMauiFonts => // Esto registra una acción con MAUI.
        {
            // Cuando MAUI ejecuta esta lambda, el contenedor de servicios está en un estado
            // más maduro, o al menos podemos construir uno temporalmente para obtener
            // nuestro PdfFontConfigurationContext si no hay otra forma.

            // Intento 1: Ver si podemos obtener los servicios sin construir todo el SP.
            // (Esto es altamente dependiente de la implementación interna de MauiAppBuilder)
            // Si esto no funciona, el fallback es construir un SP temporal.

            PdfFontConfigurationContext? configContext = null;
            var contextDescriptor = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(PdfFontConfigurationContext));
            if (contextDescriptor?.ImplementationInstance is PdfFontConfigurationContext)
            {
                configContext = (PdfFontConfigurationContext)contextDescriptor.ImplementationInstance;
            }

            if (configContext == null)
            {
                // Fallback: construir un SP temporal SOLAMENTE si es estrictamente necesario.
                // Esto no es lo ideal, pero es mejor que un error de variable no asignada.
                var tempServices = new ServiceCollection();
                foreach (var s in builder.Services) tempServices.Add(s); // Copiar descriptores
                var tempSp = tempServices.BuildServiceProvider();
                configContext = tempSp.GetService<PdfFontConfigurationContext>();
            }

            if (configContext == null)
            {
                throw new InvalidOperationException(
                    "MauiPdfGenerator no ha sido inicializado correctamente o PdfFontConfigurationContext no se pudo resolver. " +
                    "Asegúrese de llamar a .UseMauiPdfGenerator() en MauiAppBuilder antes de llamar a " +
                    "la sobrecarga de .ConfigureFonts(..., FontDestinationType).");
            }

            PdfFontRegistryBuilder pdfFontRegistry = configContext.FontRegistryBuilder;

            IFontCollection collectionForUserAction;
            if (destination == FontDestinationType.OnlyUI)
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
