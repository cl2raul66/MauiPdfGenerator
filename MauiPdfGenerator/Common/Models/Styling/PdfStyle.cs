namespace MauiPdfGenerator.Common.Models.Styling;

internal record PdfStyle(Type TargetType, string? BasedOnKey, Action<object> Setter);
