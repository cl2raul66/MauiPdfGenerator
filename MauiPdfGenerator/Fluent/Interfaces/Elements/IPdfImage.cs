namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IPdfImage : IPdfElement<IPdfImage>
{
    IPdfImage Aspect(Aspect aspect);
}
