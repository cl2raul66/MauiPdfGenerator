namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IGridChildrenBuilder
{
    IGridCellChild Paragraph(string text);
    IGridCellChild PdfImage(Stream stream);
    IGridCellChild HorizontalLine();
    IGridCellChild VerticalStackLayout(Action<IStackLayoutBuilder> content);
    IGridCellChild HorizontalStackLayout(Action<IStackLayoutBuilder> content);
    IGridCellChild Grid(Action<IGridChildrenBuilder> content);
}
