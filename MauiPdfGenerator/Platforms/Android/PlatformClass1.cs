using static Android.Graphics.Pdf.PdfDocument;
using Android.Graphics.Pdf;

namespace MauiPdfGenerator;

// All the code in this file is only included on Android.
public class PlatformClass1
{
    public void Generate()
    {
        string filePath = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath, "test.pdf");
        using PdfDocument pdfDocument = new();
        PdfDocument.PageInfo pageInfo = new PageInfo.Builder(612, 792, 1).Create();
        PdfDocument.Page page = pdfDocument.StartPage(pageInfo);

        var canvas = page.Canvas;
        canvas.DrawText("Hola, mundo PDF en Android", 100, 100, new Android.Graphics.Paint());

        pdfDocument.FinishPage(page);
        using var stream = new FileStream(filePath, FileMode.Create);
        pdfDocument.WriteTo(stream);
    }
}




