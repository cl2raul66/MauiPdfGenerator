namespace MauiPdfGenerator.Fluent.Interfaces.Views;

public interface IPdfHorizontalLine<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Thickness(float value);
    TSelf Color(Color color);
}

public interface IPdfHorizontalLine : IPdfHorizontalLine<IPdfHorizontalLine>
{
}
