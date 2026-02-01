using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfPageChildImage : 
    IPdfImage<IPdfPageChildImage>, 
    IPdfPageChild<IPdfPageChildImage>,
    IPdfStylableElement<IPdfPageChildImage>
{
}
