namespace MauiPdfGenerator.Common;

internal interface ILayoutElement
{
    IReadOnlyList<object> Children { get; }
    LayoutType LayoutType { get; }
    Thickness Margin { get; }
    Thickness Padding { get; }
}

internal enum LayoutType
{
    Grid,
    VerticalStack,
    HorizontalStack
}
