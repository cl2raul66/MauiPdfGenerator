using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IGridChildrenBuilder
{
    IGridCellChild<PdfParagraph> Paragraph(string text);
    IGridCellChild<PdfImage> PdfImage(Stream stream);
    IGridCellChild<PdfHorizontalLine> HorizontalLine();
    IGridCellChild<PdfVerticalStackLayout> VerticalStackLayout(Action<IStackLayoutBuilder> content);
    IGridCellChild<PdfHorizontalStackLayout> HorizontalStackLayout(Action<IStackLayoutBuilder> content);
    IGridCellChild<PdfGrid> Grid(Action<IPageContentBuilder> content);
}
