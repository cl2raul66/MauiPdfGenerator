using System.Diagnostics;

namespace MauiPdfGenerator.Common.Models.Styling;

internal class PdfResourceDictionary
{
    private readonly Dictionary<string, PdfStyle> _styles = [];

    public void Add(string key, PdfStyle style)
    {
        if (!_styles.TryAdd(key, style))
        {
            Debug.WriteLine($"[PdfResourceDictionary] WARNING: A style with the key '{key}' has already been added. The previous definition will be kept.");
        }
    }

    public Action<object>? GetCombinedSetter(string key)
    {
        if (!_styles.TryGetValue(key, out var initialStyle))
        {
            return null;
        }

        if (string.IsNullOrEmpty(initialStyle.BasedOnKey))
        {
            return initialStyle.Setter;
        }

        var setters = new List<Action<object>>();
        var currentStyle = initialStyle;
        var currentKey = key;
        var visitedKeys = new HashSet<string>();

        while (currentStyle != null)
        {
            if (!visitedKeys.Add(currentKey))
            {
                throw new InvalidOperationException($"Circular dependency detected in style inheritance involving key '{currentKey}'.");
            }

            setters.Insert(0, currentStyle.Setter);

            if (string.IsNullOrEmpty(currentStyle.BasedOnKey))
            {
                break; 
            }

            currentKey = currentStyle.BasedOnKey;
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
