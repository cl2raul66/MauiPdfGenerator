namespace MauiPdfGenerator.Fluent.Models.Elements;

public abstract class PdfLayoutElement<TSelf> : PdfElement where TSelf : PdfLayoutElement<TSelf>
{
    internal protected float GetSpacing { get; protected set; }
    internal protected Color? GetBackgroundColor { get; protected set; }
    internal protected LayoutAlignment GetHorizontalOptions { get; protected set; }
    internal protected LayoutAlignment GetVerticalOptions { get; protected set; }
   
    public TSelf Spacing(float value)
    {
        GetSpacing = value >= 0 ? value : 0;
        return (TSelf)this;
    }
       
    public TSelf BackgroundColor(Color color)
    {
        GetBackgroundColor = color;
        return (TSelf)this;
    }

    public TSelf HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        GetHorizontalOptions = layoutAlignment;
        return (TSelf)this;
    }

    public TSelf VerticalOptions(LayoutAlignment layoutAlignment)
    {
        GetVerticalOptions = layoutAlignment;
        return (TSelf)this;
    }
}
