namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfHorizontalStackLayoutData : PdfLayoutElementData, IPdfLayoutElement
{
    internal PdfHorizontalStackLayoutData() : base() { }

    internal PdfHorizontalStackLayoutData(IEnumerable<PdfElementData> remainingChildren, PdfHorizontalStackLayoutData originalStyleSource)
        : base(remainingChildren, originalStyleSource) { }

    IReadOnlyList<object> IPdfLayoutElement.Children => [.. _children.Cast<object>()];
    LayoutType IPdfLayoutElement.LayoutType => LayoutType.HorizontalStack;
    Thickness IPdfLayoutElement.Margin => GetMargin;
    Thickness IPdfLayoutElement.Padding => GetPadding;
}
