using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfVerticalStackLayout<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Spacing(float value);
}

public interface IPdfVerticalStackLayout : IPdfVerticalStackLayout<IPdfVerticalStackLayout>, IPdfLayoutChild<IPdfVerticalStackLayout>
{
    void Children(Action<IPdfStackLayoutBuilder> childrenSetup);
}
