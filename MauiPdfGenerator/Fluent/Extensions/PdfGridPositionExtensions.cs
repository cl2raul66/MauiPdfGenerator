using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Builders; // Namespace for GridChildInfo
using System;

namespace MauiPdfGenerator.Fluent.Extensions;

public static class PdfGridPositionExtensions
{
    // No necesitamos las Keys si usamos ConditionalWeakTable

    public static TBuilder Row<TBuilder>(this TBuilder builder, int row)
        where TBuilder : IPdfViewBuilder<TBuilder> // O la interfaz base común que usen los builders
    {
        if (row < 0) throw new ArgumentOutOfRangeException(nameof(row), "Row index must be non-negative.");
        // Get existing info or create new, then update row
        var info = GridChildInfo.GetAndRemovePositionInfo(builder); // Get (potentially default)
        info.Row = row;
        GridChildInfo.SetPositionInfo(builder, info.Row, info.Column, info.RowSpan, info.ColumnSpan); // Set updated info
        return builder;
    }

    public static TBuilder Column<TBuilder>(this TBuilder builder, int column)
        where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (column < 0) throw new ArgumentOutOfRangeException(nameof(column), "Column index must be non-negative.");
        var info = GridChildInfo.GetAndRemovePositionInfo(builder);
        info.Column = column;
        GridChildInfo.SetPositionInfo(builder, info.Row, info.Column, info.RowSpan, info.ColumnSpan);
        return builder;
    }

    public static TBuilder RowSpan<TBuilder>(this TBuilder builder, int span)
        where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (span < 1) throw new ArgumentOutOfRangeException(nameof(span), "Row span must be 1 or greater.");
        var info = GridChildInfo.GetAndRemovePositionInfo(builder);
        info.RowSpan = span;
        GridChildInfo.SetPositionInfo(builder, info.Row, info.Column, info.RowSpan, info.ColumnSpan);
        return builder;
    }

    public static TBuilder ColumnSpan<TBuilder>(this TBuilder builder, int span)
         where TBuilder : IPdfViewBuilder<TBuilder>
    {
        if (span < 1) throw new ArgumentOutOfRangeException(nameof(span), "Column span must be 1 or greater.");
        var info = GridChildInfo.GetAndRemovePositionInfo(builder);
        info.ColumnSpan = span;
        GridChildInfo.SetPositionInfo(builder, info.Row, info.Column, info.RowSpan, info.ColumnSpan);
        return builder;
    }

    // Helper para simplificar la asignación conjunta (opcional)
    public static TBuilder GridPosition<TBuilder>(this TBuilder builder, int row, int column, int rowSpan = 1, int columnSpan = 1)
        where TBuilder : IPdfViewBuilder<TBuilder>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfLessThan(rowSpan, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columnSpan, 1);
        GridChildInfo.SetPositionInfo(builder, row, column, rowSpan, columnSpan);
        return builder;
    }
}
