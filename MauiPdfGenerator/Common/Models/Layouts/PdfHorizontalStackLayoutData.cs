namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfHorizontalStackLayoutData : PdfLayoutElementData, ILayoutElement
{
    internal PdfHorizontalStackLayoutData() : base() { }

    internal PdfHorizontalStackLayoutData(IEnumerable<PdfElementData> remainingChildren, PdfHorizontalStackLayoutData originalStyleSource)
        : base(remainingChildren, originalStyleSource) { }

    IReadOnlyList<object> ILayoutElement.Children => [.. _children.Cast<object>()];
    LayoutType ILayoutElement.LayoutType => LayoutType.HorizontalStack;
    Thickness ILayoutElement.Margin => GetMargin;
    Thickness ILayoutElement.Padding => GetPadding;
}
