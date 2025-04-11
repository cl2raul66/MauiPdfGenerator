using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Implementation.Layout.Managers;
using Microsoft.Maui.Graphics;

namespace MauiPdfGenerator.Implementation.Builders;

/// <summary>
/// Internal implementation for building PDF Horizontal Stack Layouts.
/// Arranges children horizontally.
/// </summary>
internal class HorizontalStackLayoutBuilder : IPdfHorizontalStackLayoutBuilder, IInternalViewBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfResources _resources;

    // --- Configuration Storage ---
    private float _spacing = 0f; // Default spacing
    private Thickness _padding = Thickness.Zero;
    private readonly List<object> _children = [];

    // View Properties (from IPdfViewBuilder)
    private double? _explicitWidth;
    private double? _explicitHeight;
    private Thickness _margin = Thickness.Zero;
    private PdfHorizontalAlignment _horizontalOptions = PdfHorizontalAlignment.Start;
    private PdfVerticalAlignment _verticalOptions = PdfVerticalAlignment.Fill;
    private Color? _backgroundColor;

    // Public properties for PageBuilder/Layout engine access
    public float ConfiguredSpacing => _spacing;
    public Thickness ConfiguredPadding => _padding;
    public IReadOnlyList<object> ConfiguredChildren => _children;
    public double? ConfiguredWidth => _explicitWidth;
    public double? ConfiguredHeight => _explicitHeight;
    public Thickness ConfiguredMargin => _margin;
    public PdfHorizontalAlignment ConfiguredHorizontalOptions => _horizontalOptions;
    public PdfVerticalAlignment ConfiguredVerticalOptions => _verticalOptions;
    public Color? ConfiguredBackgroundColor => _backgroundColor;

    public HorizontalStackLayoutBuilder(PdfDocument pdfDocument, PdfResources resources)
    {
        _pdfDocument = pdfDocument ?? throw new ArgumentNullException(nameof(pdfDocument));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
    }

    // --- IPdfHorizontalStackLayoutBuilder Implementation ---

    public IPdfHorizontalStackLayoutBuilder Spacing(float value)
    {
        _spacing = value >= 0 ? value : 0f;
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Children(Action<IPdfContainerContentBuilder> childrenAction)
    {
        ArgumentNullException.ThrowIfNull(childrenAction);
        var contentBuilder = new ContainerContentBuilder(_pdfDocument, _resources, this);
        childrenAction(contentBuilder);
        _children.AddRange(contentBuilder.GetAddedElements());
        return this;
    }

    // --- IPdfLayoutConfigurator<TBuilder> / IPdfViewBuilder Implementation ---

    public IPdfHorizontalStackLayoutBuilder Padding(double uniformPadding)
    {
        _padding = new Thickness(uniformPadding);
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Padding(double horizontal, double vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Padding(double left, double top, double right, double bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Width(double width)
    {
        _explicitWidth = width >= 0 ? width : null;
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Height(double height)
    {
        _explicitHeight = height >= 0 ? height : null;
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Margin(double uniformMargin)
    {
        _margin = new Thickness(uniformMargin);
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Margin(double horizontal, double vertical)
    {
        _margin = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Margin(double left, double top, double right, double bottom)
    {
        _margin = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder HorizontalOptions(PdfHorizontalAlignment alignment)
    {
        _horizontalOptions = alignment;
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder VerticalOptions(PdfVerticalAlignment alignment)
    {
        _verticalOptions = alignment;
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder BackgroundColor(Color? color)
    {
        _backgroundColor = color;
        return this;
    }

    public IPdfHorizontalStackLayoutBuilder Spacing(double value)
    {
        throw new NotImplementedException();
    }
}
