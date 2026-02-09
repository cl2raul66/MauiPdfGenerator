namespace MauiPdfGenerator.Fluent.Interfaces.Views;

public interface IPdfSpan : IPdfElement<IPdfSpan>, IPdfSpanStyles
{
    IPdfSpan Text(string text);
}

public interface IPdfBuildableSpan : 
    IPdfSpan, 
    IPdfStylableElement<IPdfBuildableSpan>,
    IPdfElement<IPdfBuildableSpan>
{
    new IPdfBuildableSpan Text(string text);
    new IPdfBuildableSpan Culture(string culture);
}
