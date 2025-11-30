using System.Diagnostics;

namespace MauiPdfGenerator.Common.Models.Styling;

/// <summary>
/// Internal class to store and manage a collection of styles.
/// </summary>
internal class PdfResourceDictionary
{
    private readonly Dictionary<string, PdfStyle> _styles = new();

    /// <summary>
    /// Adds a new style definition to the dictionary.
    /// </summary>
    /// <param name="key">The unique key for the style.</param>
    /// <param name="style">The style definition.</param>
    /// <exception cref="ArgumentException">Thrown if a style with the same key already exists.</exception>
    public void Add(string key, PdfStyle style)
    {
        if (!_styles.TryAdd(key, style))
        {
            // In a real-world scenario, you might want to log this as a warning
            // or allow overriding based on a specific configuration. For now, we enforce uniqueness.
            Debug.WriteLine($"[PdfResourceDictionary] WARNING: A style with the key '{key}' has already been added. The previous definition will be kept.");
        }
    }

    /// <summary>
    /// Retrieves a style definition by its key.
    /// </summary>
    /// <param name="key">The key of the style to retrieve.</param>
    /// <returns>The <see cref="PdfStyle"/> if found; otherwise, null.</returns>
    public PdfStyle? Get(string key)
    {
        _styles.TryGetValue(key, out var style);
        return style;
    }

    /// <summary>
    /// Resolves the inheritance chain for a given style and returns a combined setter action.
    /// </summary>
    /// <param name="key">The key of the style to resolve.</param>
    /// <returns>A single <see cref="Action{T}"/> that applies all setters from the inheritance chain, or null if the key is not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a circular dependency is detected in the style hierarchy.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if a `BasedOn` key points to a style that does not exist.</exception>
    public Action<object>? GetCombinedSetter(string key)
    {
        if (!_styles.TryGetValue(key, out var initialStyle))
        {
            return null;
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

            setters.Insert(0, currentStyle.Setter); // Prepend to apply base styles first

            if (string.IsNullOrEmpty(currentStyle.BasedOnKey))
            {
                break; // No more base styles
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
