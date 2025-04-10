using System.Text;
using MauiPdfGenerator.Core.Fonts;
using MauiPdfGenerator.Core.Images;
using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Common.Geometry;
using System.Globalization;

namespace MauiPdfGenerator.Core.Content
{
    internal class PdfContentStream : PdfStream
    {
        private readonly PdfDocument _document;
        private readonly PdfResources _resources;
        private readonly MemoryStream _contentBytes = new();
        private readonly StreamWriter _writer;
        private readonly Encoding _pdfEncoding = Encoding.ASCII;

        private PdfGraphicsState CurrentGraphicsState { get; } = new();

        public PdfContentStream(PdfDocument document, PdfResources resources) : base()
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
            _writer = new StreamWriter(_contentBytes, Encoding.ASCII, leaveOpen: true);
            //Dictionary.Add(PdfName.Filter, PdfName.FlateDecode);
        }

        // --- Operadores básicos de texto ---
        public void BeginText() => AppendOperator("BT");
        public void EndText() => AppendOperator("ET");        

        public void ShowText(string text, PdfFontBase font)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Solo usar codificación hex para caracteres no ASCII
            bool needsHexEncoding = text.Any(c => c < 32 || c > 126);
            
            if (needsHexEncoding)
            {
                byte[] encodedText = font.EncodeText(text);
                StringBuilder hexString = new StringBuilder("<");
                foreach (byte b in encodedText)
                {
                    hexString.Append(b.ToString("X2"));
                }
                hexString.Append(">");
                AppendOperator($"{hexString} Tj");
            }
            else
            {
                //ShowText(text);
            }
        }

        // --- Font y Text State ---
        public void SetFont(PdfFontBase font, double size)
        {
            PdfName fontName = _resources.GetResourceName(font);
            AppendOperator($"{fontName} {FormatDouble(size)} Tf");
            CurrentGraphicsState.CurrentFont = font;
            CurrentGraphicsState.CurrentFontSize = size;
        }

        public void SetFontAndSize(PdfFontBase font, double size)
        {
            SetFont(font, size);
        }

        public void SetTextMatrix(double a, double b, double c, double d, double e, double f) => 
            AppendOperator($"{Fd(a)} {Fd(b)} {Fd(c)} {Fd(d)} {Fd(e)} {Fd(f)} Tm");

        public void MoveTextPosition(double tx, double ty) => 
            AppendOperator($"{Fd(tx)} {Fd(ty)} Td");

        // --- Color ---
        public void SetTextColor(double r, double g, double b) =>
            AppendOperator($"{FormatDouble(r)} {FormatDouble(g)} {FormatDouble(b)} rg");

        // --- Color Operations ---
        public void SetFillColor(double r, double g, double b)
        {
            AppendOperator($"{FormatDouble(r)} {FormatDouble(g)} {FormatDouble(b)} rg");
        }

        public void SetStrokeColor(double r, double g, double b)
        {
            AppendOperator($"{FormatDouble(r)} {FormatDouble(g)} {FormatDouble(b)} RG");
        }

        // --- Graphics State Stack Operations ---
        public void SaveGraphicsState()
        {
            AppendOperator("q");
        }

        public void RestoreGraphicsState()
        {
            AppendOperator("Q");
        }

        // --- XObject Drawing ---
        public void DrawXObject(PdfImageXObject image)
        {
            var name = _resources.GetResourceName(image);
            AppendOperator($"{name} Do");
        }

        public void DrawXObject(PdfImageXObject image, double x, double y, double width, double height)
        {
            SaveGraphicsState(); // q
                                 // Configurar la matriz de transformación para posicionar y escalar la imagen
                                 // La matriz para 'cm' es [a b c d e f]
                                 // Para escalar y trasladar una imagen en x,y con tamaño width,height:
                                 // a=width, b=0, c=0, d=height, e=x, f=y
            AppendOperator($"{Fd(width)} 0 0 {Fd(height)} {Fd(x)} {Fd(y)} cm"); // <-- Cambio clave
            DrawXObject(image); // Llama al método base que hace "/ImName Do"
            RestoreGraphicsState(); // Q
        }

        // --- Path Construction and Painting ---
        public void Rectangle(double x, double y, double width, double height)
        {
            AppendOperator($"{Fd(x)} {Fd(y)} {Fd(width)} {Fd(height)} re");
        }

        public void Rectangle(PdfRectangle rect)
        {
            Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void Fill()
        {
            AppendOperator("f");
        }

        public void FillAndStroke()
        {
            AppendOperator("B");
        }

        public void Stroke()
        {
            AppendOperator("S");
        }

        // --- Helper para convertir puntos y rectángulos ---
        public void MoveTo(double x, double y)
        {
            AppendOperator($"{Fd(x)} {Fd(y)} m");
        }

        public void LineTo(double x, double y)
        {
            AppendOperator($"{Fd(x)} {Fd(y)} l");
        }

        public void ClosePath()
        {
            AppendOperator("h");
        }

        // --- Helpers ---
        private static string FormatDouble(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);

        private static string Fd(double value) => FormatDouble(value);

        internal void AppendOperator(string op)
        {
            _writer.Write(op);
            _writer.Write('\n');
        }

        // --- Stream Management ---
        internal byte[] GetContentBytes()
        {
            _writer.Flush();
            var bytes = _contentBytes.ToArray();
            UnfilteredData = bytes;
            return bytes;
        }

        protected override byte[] ApplyFilters()
        {
            if (UnfilteredData == null || UnfilteredData.Length == 0)
            {
                GetContentBytes();
            }
            return base.ApplyFilters();
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _contentBytes?.Dispose();
        }
    }
}
