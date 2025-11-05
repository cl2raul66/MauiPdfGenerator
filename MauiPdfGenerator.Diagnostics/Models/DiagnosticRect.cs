namespace MauiPdfGenerator.Diagnostics.Models;

public readonly record struct DiagnosticRect(float X, float Y, float Width, float Height)
{
    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
}
