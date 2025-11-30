using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Styles;

/// <summary>
/// Represents a style for an Image element.
/// This interface closes the generic type chain for styling purposes.
/// </summary>
public interface IPdfImageStyle : IPdfImage<IPdfImageStyle>
{
}
