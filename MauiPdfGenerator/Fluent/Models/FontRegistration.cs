using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Fluent.Models;
internal class FontRegistration : IFontRegistrationOptions
{
    private readonly PdfFontRegistryBuilder _registry; 

    public string Alias { get; }
    public bool IsEmbedRequired { get; private set; } = false; 

    public FontRegistration(string alias, PdfFontRegistryBuilder registry)
    {
        ArgumentException.ThrowIfNullOrEmpty(alias);
        ArgumentNullException.ThrowIfNull(registry);

        Alias = alias;
        _registry = registry;
    }

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

    public override string ToString()
    {
        return $"Font: {Alias}, Embed: {IsEmbedRequired}";
    }
}
