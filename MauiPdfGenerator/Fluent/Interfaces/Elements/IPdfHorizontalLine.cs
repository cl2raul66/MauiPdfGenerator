namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfHorizontalLine<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Thickness(float value);
    TSelf Color(Color color);
}
