using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfLayoutChild<TSelf> : IPdfPageChild<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf HorizontalOptions(LayoutAlignment layoutAlignment);
    TSelf VerticalOptions(LayoutAlignment layoutAlignment);
}
