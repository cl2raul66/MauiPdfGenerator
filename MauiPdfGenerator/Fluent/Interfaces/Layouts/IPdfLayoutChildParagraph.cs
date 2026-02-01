using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfLayoutChildParagraph : 
    IPdfParagraph<IPdfLayoutChildParagraph>, 
    IPdfLayoutChild<IPdfLayoutChildParagraph>,
    IPdfStylableElement<IPdfLayoutChildParagraph>
{
}
