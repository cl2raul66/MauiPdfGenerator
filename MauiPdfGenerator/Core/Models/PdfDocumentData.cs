namespace MauiPdfGenerator.Core.Models;

internal record PdfDocumentData(
    IReadOnlyList<PdfPageData> Pages,
    string? Title,
    string? Author,
    string? Subject,
    string? Keywords,
    string? Creator,
    string? Producer,
    DateTime? CreationDate,
    IReadOnlyDictionary<string, string>? CustomProperties
);
