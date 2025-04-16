namespace MauiPdfGenerator.Fluent.Styles;

/// <summary>
/// Defines a reusable set of style properties for text elements.
/// Properties set here will override defaults or document/page level settings.
/// </summary>
public class TextStyle // Renamed from TextOptions
{
    /// <summary>
    /// Specifies the text color.
    /// </summary>
    public Color? TextColor { get; set; }

    /// <summary>
    /// Specifies the font attributes (Bold, Italic).
    /// </summary>
    public FontAttributes? Attributes { get; set; }

    /// <summary>
    /// Specifies the font family name.
    /// </summary>
    public string FontFamily { get; set; }

    /// <summary>
    /// Specifies the font size (unit depends on context, often points).
    /// </summary>
    public float? FontSize { get; set; }

    // Add other style properties as needed:
    // public double? LineHeight { get; set; }
    // ...

    public TextStyle() { }
}
