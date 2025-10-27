namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfGridData : PdfLayoutElementData, IPdfLayoutElement
{
    internal IReadOnlyList<RowDefinition> GetRowDefinitions { get; private set; } = [];
    internal IReadOnlyList<ColumnDefinition> GetColumnDefinitions { get; private set; } = [];
    internal double GetRowSpacing { get; private set; }
    internal double GetColumnSpacing { get; private set; }

    internal PdfGridData() : base() { }

    internal PdfGridData(IEnumerable<PdfElementData> remainingChildren, PdfGridData originalStyleSource)
        : base(remainingChildren, originalStyleSource)
    {
        GetRowDefinitions = originalStyleSource.GetRowDefinitions;
        GetColumnDefinitions = originalStyleSource.GetColumnDefinitions;
        GetRowSpacing = originalStyleSource.GetRowSpacing;
        GetColumnSpacing = originalStyleSource.GetColumnSpacing;
    }

    internal void SetRowDefinitions(IReadOnlyList<RowDefinition> definitions)
    {
        GetRowDefinitions = definitions;
    }

    internal void SetColumnDefinitions(IReadOnlyList<ColumnDefinition> definitions)
    {
        GetColumnDefinitions = definitions;
    }

    internal void RowSpacing(double value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "RowSpacing must be a non-negative value.");
        GetRowSpacing = value;
    }

    internal void ColumnSpacing(double value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "ColumnSpacing must be a non-negative value.");
        GetColumnSpacing = value;
    }

    IReadOnlyList<object> IPdfLayoutElement.Children => [.. _children.Cast<object>()];
    LayoutType IPdfLayoutElement.LayoutType => LayoutType.Grid;
    Thickness IPdfLayoutElement.Margin => GetMargin;
    Thickness IPdfLayoutElement.Padding => GetPadding;
}
