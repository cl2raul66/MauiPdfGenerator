using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfFontRegistryBuilder : IPdfFontRegistry
{
    private readonly List<PdfFontRegistration> _mauiRegisteredFontsChronologically = [];
    private readonly Dictionary<string, PdfFontRegistration> _fonts = new(StringComparer.OrdinalIgnoreCase);
    private PdfFontIdentifier? _userConfiguredDefaultFontIdentifier;
    public PdfFontRegistryBuilder()
    {
    }

    internal PdfFontRegistration GetOrCreateFontRegistration(PdfFontIdentifier identifier, string? filePath = null, bool isFromMauiConfig = false)
    {
        if (!_fonts.TryGetValue(identifier.Alias, out var registration))
        {
            registration = new PdfFontRegistration(identifier, this, filePath);
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

    public PdfFontRegistration? GetFontRegistration(PdfFontIdentifier fontIdentifier) =>
        _fonts.GetValueOrDefault(fontIdentifier.Alias);

    public PdfFontRegistration? GetFontRegistration(string fontAlias) =>
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
            throw new InvalidOperationException($"Cannot set font '{fontIdentifier.Alias}' as default because it has not been registered.");
        }
    }

    public override string ToString() => $"Registered Fonts: {_fonts.Count}, User Configured Default: {_userConfiguredDefaultFontIdentifier?.Alias ?? "None"}, First MAUI: {GetFirstMauiRegisteredFontIdentifier()?.Alias ?? "None"}";
}
