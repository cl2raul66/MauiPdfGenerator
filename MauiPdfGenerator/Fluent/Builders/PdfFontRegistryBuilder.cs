using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfFontRegistryBuilder : IPdfFontRegistry
{
    private readonly List<FontRegistration> _mauiRegisteredFontsChronologically = new();
    private readonly Dictionary<string, FontRegistration> _fonts = new(StringComparer.OrdinalIgnoreCase);
    private PdfFontIdentifier? _userConfiguredDefaultFontIdentifier;

    public PdfFontRegistryBuilder()
    {
    }

    internal FontRegistration GetOrCreateFontRegistration(PdfFontIdentifier identifier, string? filePath = null, bool isFromMauiConfig = false)
    {
        if (!_fonts.TryGetValue(identifier.Alias, out var registration))
        {
            registration = new FontRegistration(identifier, this, filePath);
            _fonts[identifier.Alias] = registration;
            if (isFromMauiConfig && !string.IsNullOrEmpty(filePath))
            {
                if (!_mauiRegisteredFontsChronologically.Any(fr => fr.Identifier.Equals(identifier)))
                {
                    _mauiRegisteredFontsChronologically.Add(registration);
                }
            }
        }
        else
        {
            if (filePath is not null && registration.FilePath is null) 
            {
                registration.FilePath = filePath;
                if (isFromMauiConfig && !string.IsNullOrEmpty(filePath) && !_mauiRegisteredFontsChronologically.Any(fr => fr.Identifier.Equals(identifier)))
                {
                    _mauiRegisteredFontsChronologically.Add(registration);
                }
            }
        }
        return registration;
    }

    public IFontRegistrationOptions Font(PdfFontIdentifier fontIdentifier)
    {
        return GetOrCreateFontRegistration(fontIdentifier);
    }

    public PdfFontIdentifier? GetUserConfiguredDefaultFontIdentifier() => _userConfiguredDefaultFontIdentifier;

    public PdfFontIdentifier? GetFirstMauiRegisteredFontIdentifier()
    {
        return _mauiRegisteredFontsChronologically.FirstOrDefault()?.Identifier;
    }

    public FontRegistration? GetFontRegistration(PdfFontIdentifier fontIdentifier) =>
        _fonts.GetValueOrDefault(fontIdentifier.Alias);

    public FontRegistration? GetFontRegistration(string fontAlias) =>
        _fonts.GetValueOrDefault(fontAlias);

    internal void SetUserConfiguredDefault(PdfFontIdentifier fontIdentifier)
    {
        var reg = GetFontRegistration(fontIdentifier);
        if (reg is not null)
        {
            _userConfiguredDefaultFontIdentifier = fontIdentifier;
        }
        else
        {
            throw new InvalidOperationException($"No se puede establecer la fuente '{fontIdentifier.Alias}' como predeterminada porque no ha sido registrada.");
        }
    }

    public override string ToString() => $"Fuentes Registradas: {_fonts.Count}, Predeterminada Configurada por Usuario: {_userConfiguredDefaultFontIdentifier?.Alias ?? "Ninguna"}, Primera de MAUI: {GetFirstMauiRegisteredFontIdentifier()?.Alias ?? "Ninguna"}";
}
