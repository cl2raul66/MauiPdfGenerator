using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

public class PdfGrid : PdfLayoutElement, ILayoutElement
{
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private List<PdfGridLength>? _rowDefinitions;
    private List<PdfGridLength>? _columnDefinitions;

    internal IReadOnlyList<PdfGridLength> RowDefinitionsList => _rowDefinitions ??= [new PdfGridLength(1, GridUnitType.Star)];
    internal IReadOnlyList<PdfGridLength> ColumnDefinitionsList => _columnDefinitions ??= [new PdfGridLength(1, GridUnitType.Star)];

    internal PdfGrid(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    public PdfGrid RowDefinitions(Action<IGridDefinitionBuilder> config)
    {
        var builder = new GridDefinitionBuilder();
        config(builder);
        _rowDefinitions = [.. builder.GetDefinitions()];
        return this;
    }

    public PdfGrid ColumnDefinitions(Action<IGridDefinitionBuilder> config)
    {
        var builder = new GridDefinitionBuilder();
        config(builder);
        _columnDefinitions = [.. builder.GetDefinitions()];
        return this;
    }

    public IGridAfterChildren Children(Action<IGridChildrenBuilder> config)
    {
        var builder = new GridChildrenBuilder(this, _fontRegistry);
        config(builder);
        return new GridAfterChildren();
    }

    public new PdfGrid Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfGrid Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfGrid Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfGrid Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfGrid Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfGrid Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfGrid WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfGrid HeightRequest(double height) { base.HeightRequest(height); return this; }
    public new PdfGrid Spacing(float value) { base.Spacing(value); return this; }
    public new PdfGrid BackgroundColor(Color? color) { base.BackgroundColor(color); return this; }
    public new PdfGrid HorizontalOptions(LayoutAlignment layoutAlignment) { base.HorizontalOptions(layoutAlignment); return this; }
    public new PdfGrid VerticalOptions(LayoutAlignment layoutAlignment) { base.VerticalOptions(layoutAlignment); return this; }

    // ILayoutElement implementation
    IReadOnlyList<object> ILayoutElement.Children => _children.Cast<object>().ToList();
    LayoutType ILayoutElement.LayoutType => LayoutType.Grid;
    Thickness ILayoutElement.Margin => GetMargin;
    Thickness ILayoutElement.Padding => GetPadding;

    private class GridAfterChildren : IGridAfterChildren { }
}
