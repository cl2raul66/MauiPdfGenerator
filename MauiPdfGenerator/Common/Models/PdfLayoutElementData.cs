using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;

namespace MauiPdfGenerator.Common.Models;

internal abstract class PdfLayoutElementData : PdfElementData
{
    protected readonly List<PdfElementData> _children = [];

    internal PdfStyledProperty<float> SpacingProp { get; } = new(0f);
    internal float GetSpacing => SpacingProp.Value;

    internal IReadOnlyList<PdfElementData> GetChildren => _children.AsReadOnly();

    protected PdfLayoutElementData() : base() { }

    internal PdfLayoutElementData(IEnumerable<PdfElementData> remainingChildren, PdfLayoutElementData original) : base()
    {
        _children.AddRange(remainingChildren);
        SpacingProp.Set(original.SpacingProp.Value, PdfPropertyPriority.Local);
        BackgroundColorProp.Set(original.BackgroundColorProp.Value, PdfPropertyPriority.Local);
        HorizontalOptionsProp.Set(original.HorizontalOptionsProp.Value, PdfPropertyPriority.Local);
        VerticalOptionsProp.Set(original.VerticalOptionsProp.Value, PdfPropertyPriority.Local);
        MarginProp.Set(original.MarginProp.Value, PdfPropertyPriority.Local);
        PaddingProp.Set(original.PaddingProp.Value, PdfPropertyPriority.Local);
    }

    internal void Add(PdfElementData element)
    {
        element.Parent = this;
        _children.Add(element);
    }

    public PdfLayoutElementData SetSpacing(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SpacingProp.Set(value, PdfPropertyPriority.Local);
        return this;
    }
}
