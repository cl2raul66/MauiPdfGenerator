using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfPageChildParagraph : 
    IPdfParagraph<IPdfPageChildParagraph>, 
    IPdfPageChild<IPdfPageChildParagraph>,
    IPdfStylableElement<IPdfPageChildParagraph>
{
}
