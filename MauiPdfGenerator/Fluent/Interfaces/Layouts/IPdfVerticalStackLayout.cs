namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfVerticalStackLayout<TSelf> : IPdfLayoutElement<TSelf> where TSelf : IPdfVerticalStackLayout<TSelf> { }

public interface IPdfVerticalStackLayout : IPdfVerticalStackLayout<IPdfVerticalStackLayout> { }
