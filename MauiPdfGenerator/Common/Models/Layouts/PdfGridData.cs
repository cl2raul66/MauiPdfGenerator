using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;

namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfGridData : PdfLayoutElementData, IPdfLayoutElement
{
    internal IReadOnlyList<RowDefinition> GetRowDefinitions { get; private set; } = [];
    internal IReadOnlyList<ColumnDefinition> GetColumnDefinitions { get; private set; } = [];

    internal PdfStyledProperty<double> RowSpacingProp { get; } = new(0);
    internal PdfStyledProperty<double> ColumnSpacingProp { get; } = new(0);

    internal double GetRowSpacing => RowSpacingProp.Value;
    internal double GetColumnSpacing => ColumnSpacingProp.Value;

    internal PdfGridData() : base() { }

    internal PdfGridData(IEnumerable<PdfElementData> remainingChildren, PdfGridData original)
        : base(remainingChildren, original)
    {
        GetRowDefinitions = original.GetRowDefinitions;
        GetColumnDefinitions = original.GetColumnDefinitions;
        RowSpacingProp.Set(original.RowSpacingProp.Value, PdfPropertyPriority.Local);
        ColumnSpacingProp.Set(original.ColumnSpacingProp.Value, PdfPropertyPriority.Local);
    }

    internal void SetRowDefinitions(IReadOnlyList<RowDefinition> definitions) => GetRowDefinitions = definitions;
    internal void SetColumnDefinitions(IReadOnlyList<ColumnDefinition> definitions) => GetColumnDefinitions = definitions;

    internal void SetRowSpacing(double value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        RowSpacingProp.Set(value, PdfPropertyPriority.Local);
    }

    internal void SetColumnSpacing(double value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        ColumnSpacingProp.Set(value, PdfPropertyPriority.Local);
    }

    IReadOnlyList<object> IPdfLayoutElement.Children => [.. _children.Cast<object>()];
    LayoutType IPdfLayoutElement.LayoutType => LayoutType.Grid;
    Thickness IPdfLayoutElement.Margin => GetMargin;
    Thickness IPdfLayoutElement.Padding => GetPadding;
}
