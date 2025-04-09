using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using Microsoft.Maui.Graphics;

namespace MauiPdfGenerator.Implementation.Builders;

/// <summary>
/// Internal implementation for building PDF Vertical Stack Layouts.
/// Arranges children vertically.
/// </summary>
internal class VerticalStackLayoutBuilder : IPdfVerticalStackLayoutBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfResources _resources;

    // --- Configuration Storage ---
    private float _spacing = 0f; // Default spacing
    private Thickness _padding = Thickness.Zero;
    private readonly List<object> _children = new();

    // View Properties (from IPdfViewBuilder)
    private double? _explicitWidth;
    private double? _explicitHeight;
    private Thickness _margin = Thickness.Zero;
    private PdfHorizontalAlignment _horizontalOptions = PdfHorizontalAlignment.Fill;
    private PdfVerticalAlignment _verticalOptions = PdfVerticalAlignment.Start;
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

    public VerticalStackLayoutBuilder(PdfDocument pdfDocument, PdfResources resources)
    {
        _pdfDocument = pdfDocument ?? throw new ArgumentNullException(nameof(pdfDocument));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
    }

    // --- IPdfVerticalStackLayoutBuilder Implementation ---

    public IPdfVerticalStackLayoutBuilder Spacing(float value)
    {
        _spacing = value >= 0 ? value : 0f;
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Children(Action<IPdfContainerContentBuilder> childrenAction)
    {
        ArgumentNullException.ThrowIfNull(childrenAction);
        var contentBuilder = new ContainerContentBuilder(_pdfDocument, _resources, this);
        childrenAction(contentBuilder);
        _children.AddRange(contentBuilder.GetAddedElements());
        return this;
    }

    // --- IPdfLayoutConfigurator<TBuilder> / IPdfViewBuilder Implementation ---

    public IPdfVerticalStackLayoutBuilder Padding(double uniformPadding)
    {
        _padding = new Thickness(uniformPadding);
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Padding(double horizontal, double vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Padding(double left, double top, double right, double bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Width(double width)
    {
        _explicitWidth = width >= 0 ? width : null;
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Height(double height)
    {
        _explicitHeight = height >= 0 ? height : null;
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Margin(double uniformMargin)
    {
        _margin = new Thickness(uniformMargin);
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Margin(double horizontal, double vertical)
    {
        _margin = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Margin(double left, double top, double right, double bottom)
    {
        _margin = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfVerticalStackLayoutBuilder HorizontalOptions(PdfHorizontalAlignment alignment)
    {
        _horizontalOptions = alignment;
        return this;
    }

    public IPdfVerticalStackLayoutBuilder VerticalOptions(PdfVerticalAlignment alignment)
    {
        _verticalOptions = alignment;
        return this;
    }

    public IPdfVerticalStackLayoutBuilder BackgroundColor(Color? color)
    {
        _backgroundColor = color;
        return this;
    }

    public IPdfVerticalStackLayoutBuilder Spacing(double value)
    {
        throw new NotImplementedException();
    }

    // --- Layout Logic Placeholder ---
    // The actual calculation of child positions and the drawing will happen
    // during the PageBuilder.FinalizePage phase when this VSL builder is processed.
    // It will receive an available area (PdfRectangle) and need to position
    // its _children within that area based on spacing, padding, alignment, etc.
    // and recursively call the layout/draw logic for its children.

} // Fin clase VerticalStackLayoutBuilder
// Fin namespace
