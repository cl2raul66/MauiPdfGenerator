using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Core.Exceptions;
internal class PdfGenerationException : Exception
{
    public PdfGenerationException() : base("Ocurrió un error durante la generación del PDF.") { }

    public PdfGenerationException(string message) : base(message) { }

    public PdfGenerationException(string message, Exception innerException) : base(message, innerException) { }
}
