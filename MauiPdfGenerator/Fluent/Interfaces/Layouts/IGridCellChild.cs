using Microsoft.Maui.Graphics;
using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IGridCellChild<TElement> where TElement : class
{
    IGridCellChild<TElement> Row(int row);
    IGridCellChild<TElement> Column(int column);
    IGridCellChild<TElement> RowSpan(int span);
    IGridCellChild<TElement> ColumnSpan(int span);
    IGridCellChild<TElement> BackgroundColor(Color? color);
    IGridCellChild<TElement> HorizontalOptions(LayoutAlignment alignment);
    IGridCellChild<TElement> VerticalOptions(LayoutAlignment alignment);
    // Métodos de estilo específicos para tipos
    IGridCellChild<TElement> TextColor(Color color); // Para PdfParagraph
    TElement Element { get; }
}
