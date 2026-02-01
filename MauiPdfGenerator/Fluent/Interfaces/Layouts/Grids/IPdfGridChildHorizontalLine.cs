using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

public interface IPdfGridChildHorizontalLine : 
    IPdfHorizontalLine<IPdfGridChildHorizontalLine>, 
    IPdfGridChild<IPdfGridChildHorizontalLine>,
    IPdfStylableElement<IPdfGridChildHorizontalLine>
{
}
