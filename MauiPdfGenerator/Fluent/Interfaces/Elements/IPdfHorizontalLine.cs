namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfHorizontalLine : IPdfElement<IPdfHorizontalLine>
{
    IPdfHorizontalLine Thickness(float value);
    IPdfHorizontalLine Color(Color color);
}
