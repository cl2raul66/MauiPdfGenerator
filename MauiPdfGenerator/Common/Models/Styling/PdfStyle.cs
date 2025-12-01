namespace MauiPdfGenerator.Common.Models.Styling;

/// <summary>
/// Internal data contract representing a defined style.
/// </summary>
/// <param name="TargetType">The interface type of the element this style targets (e.g., typeof(IPdfParagraph)).</param>
/// <param name="BasedOnKey">The key of the style this style inherits from, if any.</param>
/// <param name="Setter">The action that applies the style's properties to a target object.</param>
internal record PdfStyle(Type TargetType, string? BasedOnKey, Action<object> Setter);
