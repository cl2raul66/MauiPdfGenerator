namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfVerticalStackLayoutData : PdfLayoutElementData, IPdfLayoutElement
{
    internal PdfVerticalStackLayoutData() : base()
    {
    }

    internal PdfVerticalStackLayoutData(IEnumerable<PdfElementData> remainingChildren, PdfVerticalStackLayoutData originalStyleSource)
        : base(remainingChildren, originalStyleSource)
    {
    }

    IReadOnlyList<object> IPdfLayoutElement.Children => _children.Cast<object>().ToList();
    LayoutType IPdfLayoutElement.LayoutType => LayoutType.VerticalStack;
    Thickness IPdfLayoutElement.Margin => GetMargin;
    Thickness IPdfLayoutElement.Padding => GetPadding;
}
