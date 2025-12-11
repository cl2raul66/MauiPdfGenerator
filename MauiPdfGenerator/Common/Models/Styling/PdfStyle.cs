using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models.Styling;

internal record PdfStyle(Type TargetType, PdfStyleIdentifier? BasedOnKey, Action<object> Setter);
