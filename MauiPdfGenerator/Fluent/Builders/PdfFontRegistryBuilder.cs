using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfFontRegistryBuilder : IPdfFontRegistry
{
    private readonly Dictionary<string, FontRegistration> _fonts = new(StringComparer.OrdinalIgnoreCase);
    private string? _defaultFontAlias;

    public IFontRegistrationOptions Font(string fontAlias)
    {
        ArgumentException.ThrowIfNullOrEmpty(fontAlias);

        if (!_fonts.TryGetValue(fontAlias, out var registration))
        {
            registration = new FontRegistration(fontAlias, this);
            _fonts[fontAlias] = registration;
        }
        return registration;
    }

    public string? GetDefaultFontAlias() => _defaultFontAlias;

    public FontRegistration? GetFontRegistration(string fontAlias) => _fonts.GetValueOrDefault(fontAlias);

    internal void SetDefault(string fontAlias)
    {
        if (_fonts.ContainsKey(fontAlias))
        {
            _defaultFontAlias = fontAlias;
        }
        else
        {
            throw new InvalidOperationException($"Cannot set font '{fontAlias}' as default because it has not been registered.");
        }
    }

    public override string ToString() => $"Registered Fonts: {_fonts.Count}, Default: {_defaultFontAlias ?? "None"}";
}
