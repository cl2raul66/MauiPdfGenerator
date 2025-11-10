using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Builders.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

internal class PdfGridBuilder : IPdfGrid, IPdfGridLayout, IBuildablePdfElement
{
    private readonly PdfGridData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly HashSet<(int, int)> _occupiedCells = [];
    private bool _hasRowDefinitions = false;
    private bool _hasColumnDefinitions = false;

    public PdfGridBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
        _model = new PdfGridData();
    }

    public PdfElementData GetModel() => _model;

    private void Add(IBuildablePdfElement element)
    {
        var elementModel = element.GetModel();
        elementModel.ApplyContextualDefaults(LayoutAlignment.Fill, LayoutAlignment.Fill);

        var cellInfo = (IPdfGridCellInfo)elementModel;
        var startRow = cellInfo.Row;
        var startCol = cellInfo.Column;
        var rowSpan = cellInfo.RowSpan;
        var colSpan = cellInfo.ColumnSpan;

        if (_model.GetRowDefinitions.Any() && (startRow + rowSpan > _model.GetRowDefinitions.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(element), $"The element's row position (Row={startRow}, RowSpan={rowSpan}) is outside the bounds of the {_model.GetRowDefinitions.Count} defined rows.");
        }

        if (_model.GetColumnDefinitions.Any() && (startCol + colSpan > _model.GetColumnDefinitions.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(element), $"The element's column position (Column={startCol}, ColumnSpan={colSpan}) is outside the bounds of the {_model.GetColumnDefinitions.Count} defined columns.");
        }

        for (int r = 0; r < rowSpan; r++)
        {
            for (int c = 0; c < colSpan; c++)
            {
                var cell = (startRow + r, startCol + c);
                if (!_occupiedCells.Add(cell))
                {
                    throw new InvalidOperationException($"The cell ({cell.Item1},{cell.Item2}) is already occupied. A cell in a PdfGrid cannot contain more than one direct child element. To compose multiple views, nest a layout (e.g., VerticalStackLayout) inside the cell.");
                }
            }
        }
        _model.Add(elementModel);
    }

    public void Children(Action<IPdfGridChildrenBuilder> builder)
    {
        var childrenBuilder = new PdfGridChildrenBuilder(_fontRegistry);
        builder(childrenBuilder);

        foreach (var child in childrenBuilder.Children)
        {
            this.Add(child);
        }
    }

    #region IPdfGrid implementation
    public IPdfGrid RowSpacing(double value) { _model.RowSpacing(value); return this; }
    public IPdfGrid ColumnSpacing(double value) { _model.ColumnSpacing(value); return this; }
    #endregion

    #region Shared Transitions & IPdfGridLayout implementation
    public IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder)
    {
        if (_hasRowDefinitions) throw new InvalidOperationException("RowDefinitions have already been set.");
        var defBuilder = new PdfRowDefinitionBuilder();
        builder(defBuilder);
        _model.SetRowDefinitions(defBuilder.Rows);
        _hasRowDefinitions = true;
        return this;
    }

    public IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder)
    {
        if (_hasColumnDefinitions) throw new InvalidOperationException("ColumnDefinitions have already been set.");
        var defBuilder = new PdfColumnDefinitionBuilder();
        builder(defBuilder);
        _model.SetColumnDefinitions(defBuilder.Columns);
        _hasColumnDefinitions = true;
        return this;
    }
    #endregion

    #region IPdfElement<IPdfGrid> implementation
    public IPdfGrid BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfGrid HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfGrid Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfGrid Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGrid Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGrid Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfGrid Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGrid Padding(double leftPadding, double topMargin, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topMargin, rightPadding, bottomMargin); return this; }
    public IPdfGrid WidthRequest(double width) { _model.WidthRequest(width); return this; }
    #endregion

    #region IPdfLayoutElement<IPdfGrid> implementation
    public IPdfGrid HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGrid VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    #endregion
}
