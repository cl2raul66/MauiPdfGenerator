using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfResourceBuilder : IPdfResourceBuilder
{
    private readonly PdfResourceDictionary _resourceDictionary;

    public PdfResourceBuilder(PdfResourceDictionary resourceDictionary)
    {
        _resourceDictionary = resourceDictionary;
    }

    public IPdfResourceBuilder Style<TElement>(Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>
    {
        // Para estilos implícitos, usamos el FullName del tipo como clave.
        // Creamos un PdfStyleIdentifier a partir de ese nombre.
        string typeName = typeof(TElement).FullName ?? typeof(TElement).Name;
        var implicitKey = new PdfStyleIdentifier(typeName);

        return Style(implicitKey, null, setup);
    }

    // CAMBIO: string key -> PdfStyleIdentifier key
    public IPdfResourceBuilder Style<TElement>(PdfStyleIdentifier key, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>
    {
        return Style(key, null, setup);
    }

    // CAMBIO: string key -> PdfStyleIdentifier key, string basedOn -> PdfStyleIdentifier basedOn
    public IPdfResourceBuilder Style<TElement>(PdfStyleIdentifier key, PdfStyleIdentifier? basedOn, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>
    {
        // La validación de string.IsNullOrWhiteSpace ya la hace el struct PdfStyleIdentifier en su ctor,
        // pero aquí validamos que el struct no esté "default" (vacío) si se pasó por valor.
        if (string.IsNullOrWhiteSpace(key.Key)) throw new ArgumentException("Style key cannot be empty.", nameof(key));
        ArgumentNullException.ThrowIfNull(setup);

        Action<object> safeSetter = (target) =>
        {
            if (target is TElement typedTarget)
            {
                setup(typedTarget);
            }
        };

        // CAMBIO: Pasamos PdfStyleIdentifier? para basedOn
        var style = new PdfStyle(typeof(TElement), basedOn, safeSetter);
        _resourceDictionary.Add(key, style);

        return this;
    }

    // Implementación explícita para cumplir con la interfaz que usa PdfStyleIdentifier (no nullable en la firma de interfaz para basedOn, pero aquí manejamos la lógica interna)
    IPdfResourceBuilder IPdfResourceBuilder.Style<TElement>(PdfStyleIdentifier key, PdfStyleIdentifier basedOn, Action<TElement> setup)
    {
        return Style(key, (PdfStyleIdentifier?)basedOn, setup);
    }
}
