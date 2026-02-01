using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

public interface IPdfLayoutChildVerticalStackLayout : 
    IPdfVerticalStackLayout<IPdfLayoutChildVerticalStackLayout>, 
    IPdfLayoutChild<IPdfLayoutChildVerticalStackLayout>,
    IPdfStylableElement<IPdfLayoutChildVerticalStackLayout>
{
}