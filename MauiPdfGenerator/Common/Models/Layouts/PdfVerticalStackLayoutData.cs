namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfVerticalStackLayoutData : PdfLayoutElementData, ILayoutElement
{
    internal PdfVerticalStackLayoutData() : base()
    {
    }

    internal PdfVerticalStackLayoutData(IEnumerable<PdfElementData> remainingChildren, PdfVerticalStackLayoutData originalStyleSource)
        : base(remainingChildren, originalStyleSource)
    {
    }

    IReadOnlyList<object> ILayoutElement.Children => _children.Cast<object>().ToList();
    LayoutType ILayoutElement.LayoutType => LayoutType.VerticalStack;
    Thickness ILayoutElement.Margin => GetMargin;
    Thickness ILayoutElement.Padding => GetPadding;
}
