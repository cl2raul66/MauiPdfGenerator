using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

public interface IPdfGridChildImage : 
    IPdfImage<IPdfGridChildImage>, 
    IPdfGridChild<IPdfGridChildImage>,
    IPdfStylableElement<IPdfGridChildImage>
{
}
