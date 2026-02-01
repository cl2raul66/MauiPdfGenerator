using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfLayoutChildHorizontalLine : 
    IPdfHorizontalLine<IPdfLayoutChildHorizontalLine>, 
    IPdfLayoutChild<IPdfLayoutChildHorizontalLine>,
    IPdfStylableElement<IPdfLayoutChildHorizontalLine>
{
}
