namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfHorizontalStackLayout<TSelf> : IPdfLayoutElement<TSelf> where TSelf : IPdfHorizontalStackLayout<TSelf> { }

public interface IPdfHorizontalStackLayout : IPdfHorizontalStackLayout<IPdfHorizontalStackLayout> { }
