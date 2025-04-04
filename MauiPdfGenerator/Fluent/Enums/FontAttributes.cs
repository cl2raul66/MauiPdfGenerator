namespace MauiPdfGenerator.Fluent.Enums;

[Flags]
public enum FontAttributes
{
    None = 0,
    Bold = 1 << 0,
    Italic = 1 << 1
    // Underline, Strikethrough?
}
