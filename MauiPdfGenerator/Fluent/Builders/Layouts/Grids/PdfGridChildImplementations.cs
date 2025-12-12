using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfGridChildParagraphBuilder : PdfGridChildBuilder<PdfParagraphBuilder, IPdfGridChildParagraph>, IPdfGridChildParagraph
{
    public PdfGridChildParagraphBuilder(PdfParagraphBuilder internalBuilder) : base(internalBuilder) { }

    public IPdfGridChildParagraph FontFamily(PdfFontIdentifier? family) { _internalBuilder.FontFamily(family); return this; }
    public IPdfGridChildParagraph FontSize(float size) { _internalBuilder.FontSize(size); return this; }
    public IPdfGridChildParagraph TextColor(Color color) { _internalBuilder.TextColor(color); return this; }
    public IPdfGridChildParagraph HorizontalTextAlignment(TextAlignment alignment) { _internalBuilder.HorizontalTextAlignment(alignment); return this; }
    public IPdfGridChildParagraph VerticalTextAlignment(TextAlignment alignment) { _internalBuilder.VerticalTextAlignment(alignment); return this; }
    public IPdfGridChildParagraph FontAttributes(FontAttributes attributes) { _internalBuilder.FontAttributes(attributes); return this; }
    public IPdfGridChildParagraph LineBreakMode(LineBreakMode mode) { _internalBuilder.LineBreakMode(mode); return this; }
    public IPdfGridChildParagraph TextDecorations(TextDecorations decorations) { _internalBuilder.TextDecorations(decorations); return this; }
    public IPdfGridChildParagraph TextTransform(TextTransform transform) { _internalBuilder.TextTransform(transform); return this; }
    public IPdfGridChildParagraph Margin(double uniformMargin) { _internalBuilder.Margin(uniformMargin); return this; }
    public IPdfGridChildParagraph Margin(double horizontalMargin, double verticalMargin) { _internalBuilder.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildParagraph Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _internalBuilder.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildParagraph Padding(double uniformPadding) { _internalBuilder.Padding(uniformPadding); return this; }
    public IPdfGridChildParagraph Padding(double horizontalPadding, double verticalMargin) { _internalBuilder.Padding(horizontalPadding, verticalMargin); return this; }
    public IPdfGridChildParagraph Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _internalBuilder.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildParagraph WidthRequest(double width) { _internalBuilder.WidthRequest(width); return this; }
    public IPdfGridChildParagraph HeightRequest(double height) { _internalBuilder.HeightRequest(height); return this; }
    public IPdfGridChildParagraph BackgroundColor(Color? color) { _internalBuilder.BackgroundColor(color); return this; }
    public IPdfGridChildParagraph HorizontalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildParagraph VerticalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.VerticalOptions(layoutAlignment); return this; }

    public IPdfGridChildParagraph Style(PdfStyleIdentifier key)
    {
        _internalBuilder.Style(key); return this;
    }
}

internal class PdfGridChildImageBuilder : PdfGridChildBuilder<PdfImageBuilder, IPdfGridChildImage>, IPdfGridChildImage
{
    public PdfGridChildImageBuilder(PdfImageBuilder internalBuilder) : base(internalBuilder) { }

    public IPdfGridChildImage Aspect(Aspect aspect) { _internalBuilder.Aspect(aspect); return this; }
    public IPdfGridChildImage Margin(double uniformMargin) { _internalBuilder.Margin(uniformMargin); return this; }
    public IPdfGridChildImage Margin(double horizontalMargin, double verticalMargin) { _internalBuilder.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildImage Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _internalBuilder.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildImage Padding(double uniformPadding) { _internalBuilder.Padding(uniformPadding); return this; }
    public IPdfGridChildImage Padding(double horizontalPadding, double verticalPadding) { _internalBuilder.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildImage Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _internalBuilder.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildImage WidthRequest(double width) { _internalBuilder.WidthRequest(width); return this; }
    public IPdfGridChildImage HeightRequest(double height) { _internalBuilder.HeightRequest(height); return this; }
    public IPdfGridChildImage BackgroundColor(Color? color) { _internalBuilder.BackgroundColor(color); return this; }
    public IPdfGridChildImage HorizontalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildImage VerticalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.VerticalOptions(layoutAlignment); return this; }

    IPdfGridChildImage IPdfImage<IPdfGridChildImage>.Aspect(Aspect aspect)
    {
        _internalBuilder.Aspect(aspect); return this;
    }

    IPdfGridChildImage IPdfGridChild<IPdfGridChildImage>.Row(int row)
    {
        _internalBuilder.Row(row); return this;
    }

    IPdfGridChildImage IPdfGridChild<IPdfGridChildImage>.Column(int column)
    {
        _internalBuilder.Column(column); return this;
    }

    IPdfGridChildImage IPdfGridChild<IPdfGridChildImage>.RowSpan(int span)
    {
        _internalBuilder.RowSpan(span); return this;
    }

    IPdfGridChildImage IPdfGridChild<IPdfGridChildImage>.ColumnSpan(int span)
    {
        _internalBuilder.ColumnSpan(span); return this; 
    }

    IPdfGridChildImage IPdfLayoutChild<IPdfGridChildImage>.HorizontalOptions(LayoutAlignment layoutAlignment)
    {
           _internalBuilder.HorizontalOptions(layoutAlignment); return this;
    }

    IPdfGridChildImage IPdfLayoutChild<IPdfGridChildImage>.VerticalOptions(LayoutAlignment layoutAlignment)
    {
        _internalBuilder.VerticalOptions(layoutAlignment); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Margin(double uniformMargin)
    {
        _internalBuilder.Margin(uniformMargin); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Margin(double horizontalMargin, double verticalMargin)
    {
        _internalBuilder.Margin(horizontalMargin, verticalMargin); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        _internalBuilder.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Padding(double uniformPadding)
    {
        _internalBuilder.Padding(uniformPadding); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Padding(double horizontalPadding, double verticalPadding)
    {
        _internalBuilder.Padding(horizontalPadding, verticalPadding); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        _internalBuilder.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.WidthRequest(double width)
    {
        _internalBuilder.WidthRequest(width); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.HeightRequest(double height)
    {
        _internalBuilder.HeightRequest(height); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.BackgroundColor(Color? color)
    {
        _internalBuilder.BackgroundColor(color); return this;
    }

    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Style(PdfStyleIdentifier key)
    {
        _internalBuilder.Style(key); return this;
    }
}

internal class PdfGridChildHorizontalLineBuilder : PdfGridChildBuilder<PdfHorizontalLineBuilder, IPdfGridChildHorizontalLine>, IPdfGridChildHorizontalLine
{
    public PdfGridChildHorizontalLineBuilder(PdfHorizontalLineBuilder internalBuilder) : base(internalBuilder) { }

    public IPdfGridChildHorizontalLine Thickness(float value) { _internalBuilder.Thickness(value); return this; }
    public IPdfGridChildHorizontalLine Color(Color color) { _internalBuilder.Color(color); return this; }
    public IPdfGridChildHorizontalLine Margin(double uniformMargin) { _internalBuilder.Margin(uniformMargin); return this; }
    public IPdfGridChildHorizontalLine Margin(double horizontalMargin, double verticalMargin) { _internalBuilder.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildHorizontalLine Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _internalBuilder.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildHorizontalLine Padding(double uniformPadding) { _internalBuilder.Padding(uniformPadding); return this; }
    public IPdfGridChildHorizontalLine Padding(double horizontalPadding, double verticalPadding) { _internalBuilder.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildHorizontalLine Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _internalBuilder.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildHorizontalLine WidthRequest(double width) { _internalBuilder.WidthRequest(width); return this; }
    public IPdfGridChildHorizontalLine HeightRequest(double height) { _internalBuilder.HeightRequest(height); return this; }
    public IPdfGridChildHorizontalLine BackgroundColor(Color? color) { _internalBuilder.BackgroundColor(color); return this; }
    public IPdfGridChildHorizontalLine HorizontalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildHorizontalLine VerticalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.VerticalOptions(layoutAlignment); return this; }

    public IPdfGridChildHorizontalLine Style(PdfStyleIdentifier key)
    {
       _internalBuilder.Style(key); return this;
    }
}

internal class PdfGridChildVerticalStackLayoutBuilder : PdfGridChildBuilder<PdfVerticalStackLayoutBuilder, IPdfGridChildVerticalStackLayout>, IPdfGridChildVerticalStackLayout
{
    public PdfGridChildVerticalStackLayoutBuilder(PdfVerticalStackLayoutBuilder internalBuilder) : base(internalBuilder) { }

    public IPdfGridChildVerticalStackLayout Spacing(float value) { _internalBuilder.Spacing(value); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double uniformMargin) { _internalBuilder.Margin(uniformMargin); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double horizontalMargin, double verticalMargin) { _internalBuilder.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _internalBuilder.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double uniformPadding) { _internalBuilder.Padding(uniformPadding); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double horizontalPadding, double verticalPadding) { _internalBuilder.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _internalBuilder.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildVerticalStackLayout WidthRequest(double width) { _internalBuilder.WidthRequest(width); return this; }
    public IPdfGridChildVerticalStackLayout HeightRequest(double height) { _internalBuilder.HeightRequest(height); return this; }
    public IPdfGridChildVerticalStackLayout BackgroundColor(Color? color) { _internalBuilder.BackgroundColor(color); return this; }
    public IPdfGridChildVerticalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildVerticalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.VerticalOptions(layoutAlignment); return this; }
    public void Children(Action<IPdfStackLayoutBuilder> childrenSetup){ _internalBuilder.Children(childrenSetup); }

    public IPdfGridChildVerticalStackLayout Style(PdfStyleIdentifier key)
    {
       _internalBuilder.Style(key); return this;
    }
}

internal class PdfGridChildHorizontalStackLayoutBuilder : PdfGridChildBuilder<PdfHorizontalStackLayoutBuilder, IPdfGridChildHorizontalStackLayout>, IPdfGridChildHorizontalStackLayout
{
    public PdfGridChildHorizontalStackLayoutBuilder(PdfHorizontalStackLayoutBuilder internalBuilder) : base(internalBuilder) { }

    public IPdfGridChildHorizontalStackLayout Spacing(float value) { _internalBuilder.Spacing(value); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double uniformMargin) { _internalBuilder.Margin(uniformMargin); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double horizontalMargin, double verticalMargin) { _internalBuilder.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _internalBuilder.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double uniformPadding) { _internalBuilder.Padding(uniformPadding); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double horizontalPadding, double verticalPadding) { _internalBuilder.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _internalBuilder.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildHorizontalStackLayout WidthRequest(double width) { _internalBuilder.WidthRequest(width); return this; }
    public IPdfGridChildHorizontalStackLayout HeightRequest(double height) { _internalBuilder.HeightRequest(height); return this; }
    public IPdfGridChildHorizontalStackLayout BackgroundColor(Color? color) { _internalBuilder.BackgroundColor(color); return this; }
    public IPdfGridChildHorizontalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildHorizontalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { _internalBuilder.VerticalOptions(layoutAlignment); return this; }
    public void Children(Action<IPdfStackLayoutBuilder> childrenSetup){ _internalBuilder.Children(childrenSetup); }

    public IPdfGridChildHorizontalStackLayout Style(PdfStyleIdentifier key)
    {
        _internalBuilder.Style(key); return this;
    }
}

internal class PdfGridChildGridBuilder : PdfGridChildBuilder<PdfGridBuilder, IPdfGridChildGrid>, IPdfGridChildGrid
{
    public PdfGridChildGridBuilder(PdfGridBuilder internalBuilder) : base(internalBuilder) { }

    public IPdfGridChildGrid RowSpacing(double value) { ((IPdfGrid)_internalBuilder).RowSpacing(value); return this; }
    public IPdfGridChildGrid ColumnSpacing(double value) { ((IPdfGrid)_internalBuilder).ColumnSpacing(value); return this; }
    public IPdfGridLayout RowDefinitions(Action<IPdfRowDefinitionBuilder> builder) => ((IPdfGrid)_internalBuilder).RowDefinitions(builder);
    public IPdfGridLayout ColumnDefinitions(Action<IPdfColumnDefinitionBuilder> builder) => ((IPdfGrid)_internalBuilder).ColumnDefinitions(builder);
    public void Children(Action<IPdfGridChildrenBuilder> builder) => ((IPdfGrid)_internalBuilder).Children(builder);
    public IPdfGridChildGrid Margin(double uniformMargin) { ((IPdfGrid)_internalBuilder).Margin(uniformMargin); return this; }
    public IPdfGridChildGrid Margin(double horizontalMargin, double verticalMargin) { ((IPdfGrid)_internalBuilder).Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildGrid Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { ((IPdfGrid)_internalBuilder).Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildGrid Padding(double uniformPadding) { ((IPdfGrid)_internalBuilder).Padding(uniformPadding); return this; }
    public IPdfGridChildGrid Padding(double horizontalPadding, double verticalPadding) { ((IPdfGrid)_internalBuilder).Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildGrid Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { ((IPdfGrid)_internalBuilder).Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfGridChildGrid WidthRequest(double width) { ((IPdfGrid)_internalBuilder).WidthRequest(width); return this; }
    public IPdfGridChildGrid HeightRequest(double height) { ((IPdfGrid)_internalBuilder).HeightRequest(height); return this; }
    public IPdfGridChildGrid BackgroundColor(Color? color) { ((IPdfGrid)_internalBuilder).BackgroundColor(color); return this; }
    public IPdfGridChildGrid HorizontalOptions(LayoutAlignment layoutAlignment) { ((IPdfGrid)_internalBuilder).HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildGrid VerticalOptions(LayoutAlignment layoutAlignment) { ((IPdfGrid)_internalBuilder).VerticalOptions(layoutAlignment); return this; }

    public IPdfGridChildGrid Style(PdfStyleIdentifier key)
    {
        _internalBuilder.Style(key); return this;
    }
}