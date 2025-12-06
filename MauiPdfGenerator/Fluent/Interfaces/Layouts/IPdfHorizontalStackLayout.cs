using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfHorizontalStackLayout<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Spacing(float value);
    void Children(Action<IPdfStackLayoutBuilder> childrenSetup);
}

public interface IPdfHorizontalStackLayout : IPdfHorizontalStackLayout<IPdfHorizontalStackLayout>, IPdfLayoutChild<IPdfHorizontalStackLayout>
{

}
