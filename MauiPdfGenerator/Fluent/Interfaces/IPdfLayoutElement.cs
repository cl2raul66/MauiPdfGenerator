namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfLayoutElement<TSelf> : IPdfElement<TSelf> where TSelf : IPdfLayoutElement<TSelf>
{
    TSelf Spacing(float value);
}
