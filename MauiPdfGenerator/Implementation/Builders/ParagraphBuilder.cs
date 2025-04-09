using MauiPdfGenerator.Core.Content; // Necesario para Color? No directamente aquí.
using MauiPdfGenerator.Core.Structure; // Necesario para PdfDocument, PdfResources
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Implementation.Layout.Managers; // Para IInternalViewBuilder

namespace MauiPdfGenerator.Implementation.Builders // Namespace ajustado
{
    /// <summary>
    /// Internal implementation for building PDF Paragraphs.
    /// Stores configuration set via the fluent API.
    /// </summary>
    internal class ParagraphBuilder : IPdfParagraphBuilder, IInternalViewBuilder // Implementa IInternalViewBuilder
    {
        private readonly PdfDocument _pdfDocument;
        private readonly PdfResources _resources;

        // --- Configuration Storage ---

        // Content
        private string? _text;
        private FormattedTextBuilder? _formattedText; // Ahora es una clase separada

        // Formatting Defaults
        private string? _fontFamily;
        private double _fontSize = 12;
        private Color? _textColor;
        private PdfFontAttributes _fontAttributes = PdfFontAttributes.None;
        private PdfTextDecorations _textDecorations = PdfTextDecorations.None;
        private PdfTextAlignment _horizontalTextAlignment = PdfTextAlignment.Start;
        private PdfTextAlignment _verticalTextAlignment = PdfTextAlignment.Start;
        private double _lineHeight = 1.2;
        private Thickness _padding = Thickness.Zero; // Usa Microsoft.Maui.Thickness

        // View Properties
        private double? _explicitWidth;
        private double? _explicitHeight;
        private Thickness _margin = Thickness.Zero; // Usa Microsoft.Maui.Thickness
        private PdfHorizontalAlignment _horizontalOptions = PdfHorizontalAlignment.Start;
        private PdfVerticalAlignment _verticalOptions = PdfVerticalAlignment.Start;
        private Color? _backgroundColor;

        // Public properties for Layout engine access (read-only view)
        public string? ConfiguredText => _text;
        public FormattedTextBuilder? ConfiguredFormattedText => _formattedText;
        public string? ConfiguredFontFamily => _fontFamily;
        public double ConfiguredFontSize => _fontSize;
        public Color? ConfiguredTextColor => _textColor;
        public PdfFontAttributes ConfiguredFontAttributes => _fontAttributes;
        public PdfTextDecorations ConfiguredTextDecorations => _textDecorations;
        public PdfTextAlignment ConfiguredHorizontalTextAlignment => _horizontalTextAlignment;
        public PdfTextAlignment ConfiguredVerticalTextAlignment => _verticalTextAlignment;
        public double ConfiguredLineHeight => _lineHeight;
        public Thickness ConfiguredPadding => _padding; // Usa Thickness
        public double? ConfiguredWidth => _explicitWidth;
        public double? ConfiguredHeight => _explicitHeight;
        public Thickness ConfiguredMargin => _margin; // Usa Thickness
        public PdfHorizontalAlignment ConfiguredHorizontalOptions => _horizontalOptions;
        public PdfVerticalAlignment ConfiguredVerticalOptions => _verticalOptions;
        public Color? ConfiguredBackgroundColor => _backgroundColor;

        // Implementación explícita de IInternalViewBuilder
        PdfHorizontalAlignment IInternalViewBuilder.ConfiguredHorizontalOptions => this.ConfiguredHorizontalOptions;
        PdfVerticalAlignment IInternalViewBuilder.ConfiguredVerticalOptions => this.ConfiguredVerticalOptions;


        public ParagraphBuilder(PdfDocument pdfDocument, PdfResources resources)
        {
            _pdfDocument = pdfDocument;
            _resources = resources;
        }

        // --- IPdfParagraphBuilder Implementation ---

        public IPdfParagraphBuilder Text(string text)
        {
            _text = text;
            _formattedText = null;
            return this;
        }

        public IPdfParagraphBuilder FormattedText(Action<IPdfFormattedTextBuilder> formattedTextAction)
        {
            if (formattedTextAction == null) throw new ArgumentNullException(nameof(formattedTextAction));
            // Asume que FormattedTextBuilder y SpanBuilder están en el mismo namespace o accesible
            _formattedText = new FormattedTextBuilder();
            formattedTextAction(_formattedText);
            _text = null;
            return this;
        }

        public IPdfParagraphBuilder FontFamily(string fontFamily)
        {
            _fontFamily = fontFamily;
            return this;
        }

        public IPdfParagraphBuilder FontSize(double size)
        {
            _fontSize = size > 0 ? size : 1;
            return this;
        }

        public IPdfParagraphBuilder TextColor(Color? color)
        {
            _textColor = color;
            return this;
        }

        public IPdfParagraphBuilder FontAttributes(PdfFontAttributes attributes)
        {
            _fontAttributes = attributes;
            return this;
        }

        public IPdfParagraphBuilder TextDecorations(PdfTextDecorations decorations)
        {
            _textDecorations = decorations;
            return this;
        }

        public IPdfParagraphBuilder HorizontalTextAlignment(PdfTextAlignment alignment)
        {
            _horizontalTextAlignment = alignment;
            return this;
        }

        public IPdfParagraphBuilder VerticalTextAlignment(PdfTextAlignment alignment)
        {
            _verticalTextAlignment = alignment;
            return this;
        }

        public IPdfParagraphBuilder LineHeight(double multiplier)
        {
            _lineHeight = multiplier > 0 ? multiplier : 1.0;
            return this;
        }

        // Métodos Padding actualizados para usar Thickness
        public IPdfParagraphBuilder Padding(double uniformPadding)
        {
            _padding = new Thickness(uniformPadding);
            return this;
        }

        public IPdfParagraphBuilder Padding(double horizontal, double vertical)
        {
            _padding = new Thickness(horizontal, vertical);
            return this;
        }

        public IPdfParagraphBuilder Padding(double left, double top, double right, double bottom)
        {
            _padding = new Thickness(left, top, right, bottom);
            return this;
        }

        // --- IPdfViewBuilder Implementation ---

        public IPdfParagraphBuilder Width(double width)
        {
            _explicitWidth = width >= 0 ? width : (double?)null;
            return this;
        }

        public IPdfParagraphBuilder Height(double height)
        {
            _explicitHeight = height >= 0 ? height : (double?)null;
            return this;
        }

        // Métodos Margin actualizados para usar Thickness
        public IPdfParagraphBuilder Margin(double uniformMargin)
        {
            _margin = new Thickness(uniformMargin);
            return this;
        }

        public IPdfParagraphBuilder Margin(double horizontal, double vertical)
        {
            _margin = new Thickness(horizontal, vertical);
            return this;
        }

        public IPdfParagraphBuilder Margin(double left, double top, double right, double bottom)
        {
            _margin = new Thickness(left, top, right, bottom);
            return this;
        }

        public IPdfParagraphBuilder HorizontalOptions(PdfHorizontalAlignment alignment)
        {
            _horizontalOptions = alignment;
            return this;
        }

        public IPdfParagraphBuilder VerticalOptions(PdfVerticalAlignment alignment)
        {
            _verticalOptions = alignment;
            return this;
        }

        public IPdfParagraphBuilder BackgroundColor(Color? color)
        {
            _backgroundColor = color;
            return this;
        }

        // --- MÉTODO RenderContent ELIMINADO ---
        // internal void RenderContent(PdfContentStream contentStream, PdfRectangle contentArea)
        // { ... } // <- ¡¡Este método ya no debe existir aquí!!

    } // Fin clase ParagraphBuilder

    // --- Implementations for Formatted Text / Spans ---
    // Si no moviste estas clases a sus propios archivos, deberían estar aquí.
    // Si las moviste, elimina estas definiciones de este archivo.

    /* Si están en archivos separados, eliminar esto:
    internal class FormattedTextBuilder : IPdfFormattedTextBuilder
    {
        internal List<SpanBuilder> Spans { get; } = new List<SpanBuilder>();
        // ... resto de FormattedTextBuilder ...
    }

    internal class SpanBuilder : IPdfSpanBuilder
    {
        // ... resto de SpanBuilder ...
    }
    */

} // Fin namespace