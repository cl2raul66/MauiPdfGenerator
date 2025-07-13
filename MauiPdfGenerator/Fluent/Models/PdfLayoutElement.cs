using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Fluent.Models;

public abstract class PdfLayoutElement : PdfElement
{
    protected readonly List<PdfElement> _children = [];
    internal readonly PdfFontRegistryBuilder? _fontRegistry; // Cambiado de private a protected

    internal float GetSpacing { get; private set; }

    internal IReadOnlyList<PdfElement> GetChildren => _children.AsReadOnly();

    internal PdfLayoutElement(PdfFontRegistryBuilder? fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    internal PdfLayoutElement(IEnumerable<PdfElement> remainingChildren, PdfLayoutElement originalStyleSource)
    {
        _children.AddRange(remainingChildren);
        _fontRegistry = originalStyleSource._fontRegistry;
        Spacing(originalStyleSource.GetSpacing);
        BackgroundColor(originalStyleSource.GetBackgroundColor);
        HorizontalOptions(originalStyleSource.GetHorizontalOptions);
        VerticalOptions(originalStyleSource.GetVerticalOptions);
        Margin(originalStyleSource.GetMargin.Left, originalStyleSource.GetMargin.Top, originalStyleSource.GetMargin.Right, originalStyleSource.GetMargin.Bottom);
        Padding(originalStyleSource.GetPadding.Left, originalStyleSource.GetPadding.Top, originalStyleSource.GetPadding.Right, originalStyleSource.GetPadding.Bottom);
    }

    protected void AddChild(PdfElement element)
    {
        element.Parent = this;
        _children.Add(element);
    }

    internal void Add(PdfElement element)
    {
        AddChild(element);
    }

    public PdfLayoutElement Spacing(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Spacing must be a non-negative value.");
        GetSpacing = value;
        return this;
    }

    public new PdfLayoutElement BackgroundColor(Color? color) { base.BackgroundColor(color); return this; }
    public new PdfLayoutElement HorizontalOptions(LayoutAlignment layoutAlignment) { base.HorizontalOptions(layoutAlignment); return this; }
    public new PdfLayoutElement VerticalOptions(LayoutAlignment layoutAlignment) { base.VerticalOptions(layoutAlignment); return this; }
    public new PdfLayoutElement Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfLayoutElement Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfLayoutElement Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfLayoutElement Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfLayoutElement Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfLayoutElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfLayoutElement WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfLayoutElement HeightRequest(double height) { base.HeightRequest(height); return this; }
}
