namespace MauiPdfGenerator.Common.Models;

internal abstract class PdfLayoutElementData : PdfElementData
{
    protected readonly List<PdfElementData> _children = [];

    internal float GetSpacing { get; private set; }

    internal IReadOnlyList<PdfElementData> GetChildren => _children.AsReadOnly();

    protected PdfLayoutElementData()
    {
    }

    internal PdfLayoutElementData(IEnumerable<PdfElementData> remainingChildren, PdfLayoutElementData originalStyleSource)
    {
        _children.AddRange(remainingChildren);
        Spacing(originalStyleSource.GetSpacing);
        BackgroundColor(originalStyleSource.GetBackgroundColor);
        HorizontalOptions(originalStyleSource.GetHorizontalOptions);
        VerticalOptions(originalStyleSource.GetVerticalOptions);
        Margin(originalStyleSource.GetMargin.Left, originalStyleSource.GetMargin.Top, originalStyleSource.GetMargin.Right, originalStyleSource.GetMargin.Bottom);
        Padding(originalStyleSource.GetPadding.Left, originalStyleSource.GetPadding.Top, originalStyleSource.GetPadding.Right, originalStyleSource.GetPadding.Bottom);
    }

    protected void AddChild(PdfElementData element)
    {
        element.Parent = this;
        _children.Add(element);
    }

    internal void Add(PdfElementData element)
    {
        AddChild(element);
    }

    public PdfLayoutElementData Spacing(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Spacing must be a non-negative value.");
        GetSpacing = value;
        return this;
    }

    public new PdfLayoutElementData BackgroundColor(Color? color) { base.BackgroundColor(color); return this; }
    public new PdfLayoutElementData HorizontalOptions(LayoutAlignment layoutAlignment) { base.HorizontalOptions(layoutAlignment); return this; }
    public new PdfLayoutElementData VerticalOptions(LayoutAlignment layoutAlignment) { base.VerticalOptions(layoutAlignment); return this; }
    public new PdfLayoutElementData Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfLayoutElementData Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfLayoutElementData Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfLayoutElementData Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfLayoutElementData Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfLayoutElementData Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfLayoutElementData WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfLayoutElementData HeightRequest(double height) { base.HeightRequest(height); return this; }
}
