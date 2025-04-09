using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Implementation.Builders;

/// <summary>
/// Internal implementation for building PDF Grid Layouts.
/// Stores configuration and child positioning info.
/// </summary>
internal class GridBuilder : IPdfGridBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfResources _resources;

    // --- Configuration Storage ---
    private List<PdfGridLength> _columnDefinitions = [];
    private List<PdfGridLength> _rowDefinitions = [];
    private double _columnSpacing = 0;
    private double _rowSpacing = 0;
    private Thickness _padding = Thickness.Zero;

    // Store children along with their grid info
    private readonly List<GridChildInfo> _childrenInfo = [];

    // View Properties (from IPdfViewBuilder)
    private double? _explicitWidth;
    private double? _explicitHeight;
    private Thickness _margin = Thickness.Zero;
    private PdfHorizontalAlignment _horizontalOptions = PdfHorizontalAlignment.Fill; // Grids often fill space
    private PdfVerticalAlignment _verticalOptions = PdfVerticalAlignment.Fill;
    private Color? _backgroundColor;

    // Public properties for PageBuilder/Layout engine access
    public IReadOnlyList<PdfGridLength> ConfiguredColumnDefinitions => _columnDefinitions;
    public IReadOnlyList<PdfGridLength> ConfiguredRowDefinitions => _rowDefinitions;
    public double ConfiguredColumnSpacing => _columnSpacing;
    public double ConfiguredRowSpacing => _rowSpacing;
    public Thickness ConfiguredPadding => _padding;
    public IReadOnlyList<GridChildInfo> ConfiguredChildrenInfo => _childrenInfo;
    public double? ConfiguredWidth => _explicitWidth;
    public double? ConfiguredHeight => _explicitHeight;
    public Thickness ConfiguredMargin => _margin;
    public PdfHorizontalAlignment ConfiguredHorizontalOptions => _horizontalOptions;
    public PdfVerticalAlignment ConfiguredVerticalOptions => _verticalOptions;
    public Color? ConfiguredBackgroundColor => _backgroundColor;


    public GridBuilder(PdfDocument pdfDocument, PdfResources resources)
    {
        _pdfDocument = pdfDocument;
        _resources = resources;
    }

    // --- IPdfGridBuilder Implementation ---

    public IPdfGridBuilder ColumnDefinitions(params PdfGridLength[] widths)
    {
        _columnDefinitions = widths?.ToList() ?? new List<PdfGridLength>();
        // Ensure at least one column if children are added without definitions? Or handle in layout.
        // if (_columnDefinitions.Count == 0) _columnDefinitions.Add(PdfGridLength.Star(1)); // Default to single star column
        return this;
    }

    public IPdfGridBuilder RowDefinitions(params PdfGridLength[] heights)
    {
        _rowDefinitions = heights?.ToList() ?? new List<PdfGridLength>();
        // if (_rowDefinitions.Count == 0) _rowDefinitions.Add(PdfGridLength.Star(1)); // Default to single star row
        return this;
    }

    public IPdfGridBuilder ColumnSpacing(double value)
    {
        _columnSpacing = value >= 0 ? value : 0;
        return this;
    }

    public IPdfGridBuilder RowSpacing(double value)
    {
        _rowSpacing = value >= 0 ? value : 0;
        return this;
    }

    public IPdfGridBuilder Children(Action<IPdfContainerContentBuilder> childrenAction)
    {
        if (childrenAction == null) throw new ArgumentNullException(nameof(childrenAction));

        // Create a temporary content builder. The children added via this builder
        // will have their position info stored temporarily by the extension methods.
        var contentBuilder = new ContainerContentBuilder(_pdfDocument, _resources, this);
        childrenAction(contentBuilder);

        // Retrieve the builders added and their associated position info
        var addedBuilders = contentBuilder.GetAddedElements();
        foreach (var builderInstance in addedBuilders)
        {
            // Get the position info set by .Row(), .Column() extensions (or defaults)
            var positionInfo = GridChildInfo.GetAndRemovePositionInfo(builderInstance);
            // Ensure the builder instance is correct in the info object
            // (GetAndRemovePositionInfo creates a new info object if none was found)
            var childInfo = new GridChildInfo(builderInstance)
            {
                Row = positionInfo.Row,
                Column = positionInfo.Column,
                RowSpan = positionInfo.RowSpan,
                ColumnSpan = positionInfo.ColumnSpan
            };
            _childrenInfo.Add(childInfo);
        }
        return this;
    }


    // --- IPdfLayoutConfigurator<TBuilder> / IPdfViewBuilder Implementation ---

    public IPdfGridBuilder Padding(double uniformPadding)
    {
        _padding = new Thickness(uniformPadding);
        return this;
    }

    public IPdfGridBuilder Padding(double horizontal, double vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfGridBuilder Padding(double left, double top, double right, double bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfGridBuilder Width(double width)
    {
        _explicitWidth = width >= 0 ? width : null;
        return this;
    }

    public IPdfGridBuilder Height(double height)
    {
        _explicitHeight = height >= 0 ? height : null;
        return this;
    }

    public IPdfGridBuilder Margin(double uniformMargin)
    {
        _margin = new Thickness(uniformMargin);
        return this;
    }

    public IPdfGridBuilder Margin(double horizontal, double vertical)
    {
        _margin = new Thickness(horizontal, vertical);
        return this;
    }

    public IPdfGridBuilder Margin(double left, double top, double right, double bottom)
    {
        _margin = new Thickness(left, top, right, bottom);
        return this;
    }

    public IPdfGridBuilder HorizontalOptions(PdfHorizontalAlignment alignment)
    {
        _horizontalOptions = alignment;
        return this;
    }

    public IPdfGridBuilder VerticalOptions(PdfVerticalAlignment alignment)
    {
        _verticalOptions = alignment;
        return this;
    }

    public IPdfGridBuilder BackgroundColor(Color? color)
    {
        _backgroundColor = color;
        return this;
    }

    // --- Layout Logic Placeholder ---
    // The actual grid layout calculation (determining row heights, column widths based on content
    // and definitions like Auto/*, positioning children in cells respecting spans, spacing, padding)
    // is complex and will happen during the PageBuilder.FinalizePage phase.

} // Fin clase GridBuilder
// Fin namespace
