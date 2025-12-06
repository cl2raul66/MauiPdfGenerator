using MauiPdfGenerator.Common.Enums;

namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfHorizontalStackLayoutData : PdfLayoutElementData, IPdfLayoutElement
{
    internal PdfHorizontalStackLayoutData() : base()
    {
        base.HorizontalOptionsProp.Set(LayoutAlignment.Start, PdfPropertyPriority.Default);
    }

    internal PdfHorizontalStackLayoutData(IEnumerable<PdfElementData> remainingChildren, PdfHorizontalStackLayoutData original)
        : base(remainingChildren, original) { }

    IReadOnlyList<object> IPdfLayoutElement.Children => [.. _children.Cast<object>()];
    LayoutType IPdfLayoutElement.LayoutType => LayoutType.HorizontalStack;
    Thickness IPdfLayoutElement.Margin => GetMargin;
    Thickness IPdfLayoutElement.Padding => GetPadding;
}
