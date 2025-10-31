namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfVerticalStackLayout<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Spacing(float value);
}

// Interfaz final para uso en Page y otros Layouts
public interface IPdfVerticalStackLayout : IPdfVerticalStackLayout<IPdfVerticalStackLayout>, IPdfLayoutChild<IPdfVerticalStackLayout>
{
}
