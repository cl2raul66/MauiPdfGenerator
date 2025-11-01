using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfHorizontalStackLayout<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Spacing(float value);
}

public interface IPdfHorizontalStackLayout : IPdfHorizontalStackLayout<IPdfHorizontalStackLayout>, IPdfLayoutChild<IPdfHorizontalStackLayout>
{
    void Children(Action<IPdfStackLayoutBuilder> childrenSetup);
}
