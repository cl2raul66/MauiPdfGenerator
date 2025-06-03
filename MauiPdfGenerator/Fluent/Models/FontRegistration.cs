using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using System.Diagnostics;

namespace MauiPdfGenerator.Fluent.Models;

public class FontRegistration : IFontRegistrationOptions
{
    public PdfFontIdentifier Identifier { get; }
    public bool ShouldEmbed { get; private set; } = false;
    public string? FilePath { get; internal set; }
    private readonly PdfFontRegistryBuilder _registryBuilder;

    internal FontRegistration(PdfFontIdentifier identifier, PdfFontRegistryBuilder registryBuilder, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(registryBuilder);
        Identifier = identifier;
        _registryBuilder = registryBuilder;
        FilePath = filePath;
    }

    public IFontRegistrationOptions Default()
    {
        _registryBuilder.SetUserConfiguredDefault(Identifier);
        return this;
    }

    public IFontRegistrationOptions EmbeddedFont()
    {
        if (FilePath is not null && !string.IsNullOrEmpty(FilePath))
        {
            ShouldEmbed = true;
            Debug.WriteLine($"[FontRegistration] Fuente '{Identifier.Alias}' (Archivo: '{FilePath}') marcada para incrustar.");
        }
        else
        {
            ShouldEmbed = false;
            Debug.WriteLine($"[FontRegistration] Fuente '{Identifier.Alias}' no se puede marcar para incrustar: FilePath no está establecido. Se referenciará por nombre si está disponible en el sistema.");
        }
        return this;
    }
    public override string ToString() => $"Identifier: {Identifier.Alias}, Embed: {ShouldEmbed}, FilePath: {FilePath ?? "N/A"}";
}
