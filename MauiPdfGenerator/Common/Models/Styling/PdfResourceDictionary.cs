using System.Diagnostics;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Styling;

internal class PdfResourceDictionary
{
    // CAMBIO: Dictionary<string, ...> -> Dictionary<PdfStyleIdentifier, ...>
    private readonly Dictionary<PdfStyleIdentifier, PdfStyle> _styles = [];

    // CAMBIO: string key -> PdfStyleIdentifier key
    public void Add(PdfStyleIdentifier key, PdfStyle style)
    {
        if (!_styles.TryAdd(key, style))
        {
            Debug.WriteLine($"[PdfResourceDictionary] WARNING: A style with the key '{key}' has already been added. The previous definition will be kept.");
        }
    }

    // CAMBIO: string key -> PdfStyleIdentifier key
    public Action<object>? GetCombinedSetter(PdfStyleIdentifier key)
    {
        if (!_styles.TryGetValue(key, out var initialStyle))
        {
            return null;
        }

        if (!initialStyle.BasedOnKey.HasValue)
        {
            return initialStyle.Setter;
        }

        var setters = new List<Action<object>>();
        var currentStyle = initialStyle;
        var currentKey = key;
        var visitedKeys = new HashSet<PdfStyleIdentifier>();

        while (currentStyle != null)
        {
            if (!visitedKeys.Add(currentKey))
            {
                throw new InvalidOperationException($"Circular dependency detected in style inheritance involving key '{currentKey}'.");
            }

            setters.Insert(0, currentStyle.Setter);

            if (!currentStyle.BasedOnKey.HasValue)
            {
                break;
            }

            currentKey = currentStyle.BasedOnKey.Value;
            if (!_styles.TryGetValue(currentKey, out currentStyle))
            {
                throw new KeyNotFoundException($"The specified `BasedOn` style with key '{currentKey}' was not found in the resource dictionary.");
            }
        }

        return target =>
        {
            foreach (var setter in setters)
            {
                setter(target);
            }
        };
    }
}
