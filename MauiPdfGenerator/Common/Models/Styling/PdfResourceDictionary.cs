using System.Diagnostics;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Styling;

internal class PdfResourceDictionary
{
    private readonly Dictionary<PdfStyleIdentifier, PdfStyle> _styles = [];

    public PdfResourceDictionary? Parent { get; set; }

    public void Add(PdfStyleIdentifier key, PdfStyle style)
    {
        if (!_styles.TryAdd(key, style))
        {
            Debug.WriteLine($"[PdfResourceDictionary] WARNING: A style with the key '{key}' has already been added. The previous definition will be kept.");
        }
    }

    public Action<object>? GetCombinedSetter(PdfStyleIdentifier key)
    {
        if (!_styles.TryGetValue(key, out var initialStyle))
        {
            return Parent?.GetCombinedSetter(key);
        }

        if (!initialStyle.BasedOnKey.HasValue)
        {
            return initialStyle.Setter;
        }

        var setters = new List<Action<object>>();
        var currentStyle = initialStyle;

        setters.Insert(0, currentStyle.Setter);

        while (currentStyle.BasedOnKey.HasValue)
        {
            var parentKey = currentStyle.BasedOnKey.Value;

            if (_styles.TryGetValue(parentKey, out var parentStyle))
            {
                currentStyle = parentStyle;
                setters.Insert(0, currentStyle.Setter);
            }
            else
            {

                if (Parent is null)
                {
                    throw new KeyNotFoundException($"The specified `BasedOn` style with key '{parentKey}' was not found in the local resource dictionary and no parent dictionary exists.");
                }

                var parentChainSetter = Parent.GetCombinedSetter(parentKey);

                if (parentChainSetter is null)
                {
                    throw new KeyNotFoundException($"The specified `BasedOn` style with key '{parentKey}' was not found in the parent resource dictionary.");
                }

                setters.Insert(0, parentChainSetter);

                break;
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
