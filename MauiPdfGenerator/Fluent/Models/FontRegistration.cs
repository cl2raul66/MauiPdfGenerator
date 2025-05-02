using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Fluent.Models;
internal class FontRegistration : IFontRegistrationOptions
{
    public IFontRegistrationOptions Default()
    {
        _registry.SetDefault(this.Alias);
        return this;
    }

    public IFontRegistrationOptions EmbeddedFont()
    {
        IsEmbedRequired = true;
        return this;
    }

    public string Alias { get; }
    public bool IsEmbedRequired { get; private set; } = false; 
    private readonly PdfFontRegistryBuilder _registry; 

    // Modificado: Aceptar el registro para llamar SetDefault
    public FontRegistration(string alias, PdfFontRegistryBuilder registry)
    {
        ArgumentException.ThrowIfNullOrEmpty(alias);
        ArgumentNullException.ThrowIfNull(registry);

        Alias = alias;
        _registry = registry;
    }
}
