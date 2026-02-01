using MauiPdfGenerator.Common;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Builders.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

internal class PdfGridBuilder : IPdfGrid, IPdfGridLayout, IBuildablePdfElement
{
    private readonly PdfGridData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly PdfResourceDictionary? _resourceDictionary;
    private readonly HashSet<(int, int)> _occupiedCells = [];
    private bool _hasRowDefinitions = false;
    private bool _hasColumnDefinitions = false;

    public PdfGridBuilder(PdfFontRegistryBuilder fontRegistry, PdfResourceDictionary? resourceDictionary = null)
    {
        _fontRegistry = fontRegistry;
        _resourceDictionary = resourceDictionary;
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
        var childrenBuilder = new PdfGridChildrenBuilder(_fontRegistry, _resourceDictionary);
        builder(childrenBuilder);
        foreach (var child in childrenBuilder.Children) this.Add(child);
    }

    #region IPdfGrid implementation
    public IPdfGrid RowSpacing(double value) { _model.SetRowSpacing(value); return this; }
    public IPdfGrid ColumnSpacing(double value) { _model.SetColumnSpacing(value); return this; }

    IPdfGrid IPdfGrid<IPdfGrid>.RowSpacing(double value) { RowSpacing(value); return this; }
    IPdfGrid IPdfGrid<IPdfGrid>.ColumnSpacing(double value) { ColumnSpacing(value); return this; }
    #endregion

    #region Shared Transitions
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

    IPdfGridLayout IPdfGrid<IPdfGrid>.RowDefinitions(Action<IPdfRowDefinitionBuilder> builder) => RowDefinitions(builder);
    IPdfGridLayout IPdfGrid<IPdfGrid>.ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder) => ColumnDefinitions(builder);
    void IPdfGrid<IPdfGrid>.Children(Action<IPdfGridChildrenBuilder> builder) => Children(builder);
    #endregion

    #region IPdfElement<IPdfGrid> implementation
    public IPdfGrid BackgroundColor(Color? c) { _model.SetBackgroundColor(c); return this; }
    public IPdfGrid HeightRequest(double h) { _model.SetHeightRequest(h); return this; }
    public IPdfGrid Margin(double u) { _model.SetMargin(u); return this; }
    public IPdfGrid Margin(double h, double v) { _model.SetMargin(h, v); return this; }
    public IPdfGrid Margin(double l, double t, double r, double b) { _model.SetMargin(l, t, r, b); return this; }
    public IPdfGrid Padding(double u) { _model.SetPadding(u); return this; }
    public IPdfGrid Padding(double h, double v) { _model.SetPadding(h, v); return this; }
    public IPdfGrid Padding(double l, double t, double r, double b) { _model.SetPadding(l, t, r, b); return this; }
    public IPdfGrid WidthRequest(double w) { _model.SetWidthRequest(w); return this; }
    #endregion

    #region IPdfLayoutElement<IPdfGrid> implementation
    public IPdfGrid HorizontalOptions(LayoutAlignment a) { _model.SetHorizontalOptions(a); return this; }
    public IPdfGrid VerticalOptions(LayoutAlignment a) { _model.SetVerticalOptions(a); return this; }
    public IPdfGrid Style(PdfStyleIdentifier key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations (IPdfGrid)
    // IPdfElement<IPdfGrid>.Style(...) -> ELIMINADO
    IPdfGrid IPdfElement<IPdfGrid>.Margin(double u) { Margin(u); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.Padding(double u) { Padding(u); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfGrid IPdfElement<IPdfGrid>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfGrid IPdfLayoutChild<IPdfGrid>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfGrid IPdfLayoutChild<IPdfGrid>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }
    #endregion
}
