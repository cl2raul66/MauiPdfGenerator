using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models;

namespace MauiPdfGenerator.Common.Models.Layouts;

internal class PdfGridData : PdfLayoutElementData, IPdfLayoutElement
{
internal IReadOnlyList<PdfRowDefinition> GetRowDefinitions { get; private set; } = [];
    internal IReadOnlyList<PdfColumnDefinition> GetColumnDefinitions { get; private set; } = [];

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

    internal void SetRowDefinitions(IReadOnlyList<PdfRowDefinition> definitions) => GetRowDefinitions = definitions;
    internal void SetColumnDefinitions(IReadOnlyList<PdfColumnDefinition> definitions) => GetColumnDefinitions = definitions;

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
