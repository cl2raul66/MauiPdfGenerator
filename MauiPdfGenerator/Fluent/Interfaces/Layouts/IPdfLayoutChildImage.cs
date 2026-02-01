using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfLayoutChildImage : 
    IPdfImage<IPdfLayoutChildImage>, 
    IPdfLayoutChild<IPdfLayoutChildImage>,
    IPdfStylableElement<IPdfLayoutChildImage>
{
}
