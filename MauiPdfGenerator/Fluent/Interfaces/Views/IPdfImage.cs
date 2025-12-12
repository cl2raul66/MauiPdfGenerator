namespace MauiPdfGenerator.Fluent.Interfaces.Views;

public interface IPdfImage<TSelf> : IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Aspect(Aspect aspect);
}

public interface IPdfImage : IPdfImage<IPdfImage>
{
}
