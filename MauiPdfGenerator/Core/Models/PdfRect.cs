namespace MauiPdfGenerator.Core.Models;

internal readonly record struct PdfRect(float X, float Y, float Width, float Height)
{
    public static readonly PdfRect Empty = new(0, 0, 0, 0);

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
}
