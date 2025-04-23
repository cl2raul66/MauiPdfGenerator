namespace MauiPdfGenerator.Common;

internal abstract class PdfElement
{
    public double WidthRequest { get; set; }
    public double HeightRequest { get; set; }
    public Thickness Margin { get; set; }
    public Thickness Padding { get; set; }
    public LayoutOptions HorizontalOptions { get; set; }
    public LayoutOptions VerticalOptions { get; set; }
}
