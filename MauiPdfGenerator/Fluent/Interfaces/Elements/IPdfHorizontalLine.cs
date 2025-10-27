namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfHorizontalLine<TSelf> : IPdfElement<TSelf> where TSelf : IPdfHorizontalLine<TSelf>
{
    TSelf Thickness(float value);
    TSelf Color(Color color);
}

public interface IPdfHorizontalLine : IPdfHorizontalLine<IPdfHorizontalLine> { }
