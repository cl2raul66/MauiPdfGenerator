namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

// Placeholder básico
public interface IImageBuilder
{
    IImageBuilder Source(string pathOrUrl); // O byte[], Stream
                                            // ... más configuraciones (Aspect, Width, Height, Alignment, etc.)
}
