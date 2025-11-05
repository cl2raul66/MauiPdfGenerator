namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfImage<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Aspect(Aspect aspect);
}
