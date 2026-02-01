using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

public interface IPdfGridChildParagraph : 
    IPdfParagraph<IPdfGridChildParagraph>, 
    IPdfGridChild<IPdfGridChildParagraph>,
    IPdfStylableElement<IPdfGridChildParagraph>
{
}
