using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders.Layouts.Grids;

internal class PdfGridChildBuilder<TBuilder, TInterface>
    where TBuilder : class, IBuildablePdfElement
    where TInterface : class
{
    protected readonly TBuilder _internalBuilder;
    private readonly PdfElementData _model;

    public PdfGridChildBuilder(TBuilder internalBuilder)
    {
        _internalBuilder = internalBuilder;
        _model = _internalBuilder.GetModel();
    }

    public TInterface Row(int row)
    {
        _model.SetRow(row);
        return this as TInterface ?? throw new InvalidCastException();
    }

    public TInterface Column(int column)
    {
        _model.SetColumn(column);
        return this as TInterface ?? throw new InvalidCastException();
    }

    public TInterface RowSpan(int span)
    {
        _model.SetRowSpan(span);
        return this as TInterface ?? throw new InvalidCastException();
    }

    public TInterface ColumnSpan(int span)
    {
        _model.SetColumnSpan(span);
        return this as TInterface ?? throw new InvalidCastException();
    }
}
