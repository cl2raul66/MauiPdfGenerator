namespace MauiPdfGenerator.Fluent.Models.Elements;

/// <summary>
/// Represents a base class for layout elements that can contain other elements
/// and have common layout properties like Spacing, Padding, and Alignment.
/// </summary>
/// <typeparam name="TSelf">The type of the inheriting layout class, for fluent chaining.</typeparam>
public abstract class PdfLayoutElement<TSelf> : PdfElement where TSelf : PdfLayoutElement<TSelf>
{
    internal protected float GetSpacing { get; protected set; }
    internal protected Color? GetBackgroundColor { get; protected set; }
    internal protected LayoutAlignment GetHorizontalOptions { get; protected set; }
    internal protected LayoutAlignment GetVerticalOptions { get; protected set; }

    /// <summary>
    /// Sets the space between child elements.
    /// </summary>
    public TSelf Spacing(float value)
    {
        GetSpacing = value >= 0 ? value : 0;
        return (TSelf)this;
    }

    /// <summary>
    /// Sets the background color of the layout. The color covers the area including Padding, but not Margin.
    /// </summary>
    public TSelf BackgroundColor(Color color)
    {
        GetBackgroundColor = color;
        return (TSelf)this;
    }

    /// <summary>
    /// Sets the horizontal alignment of the layout within its parent's allocated space.
    /// </summary>
    public TSelf HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        GetHorizontalOptions = layoutAlignment;
        return (TSelf)this;
    }

    /// <summary>
    /// Sets the vertical alignment of the layout within its parent's allocated space.
    /// </summary>
    public TSelf VerticalOptions(LayoutAlignment layoutAlignment)
    {
        GetVerticalOptions = layoutAlignment;
        return (TSelf)this;
    }
}
