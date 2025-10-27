namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfImage<TSelf> : IPdfElement<TSelf> where TSelf : IPdfImage<TSelf>
{
    TSelf Aspect(Aspect aspect);
}

public interface IPdfImage : IPdfImage<IPdfImage> { }
