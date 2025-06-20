using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfGrid : PdfLayoutElement
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

    public PdfGrid Children(Action<IPageContentBuilder> config)
    {
        var builder = new PageContentBuilder(_fontRegistry);
        config(builder);
        _children.AddRange(builder.GetChildren());
        return this;
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
}
