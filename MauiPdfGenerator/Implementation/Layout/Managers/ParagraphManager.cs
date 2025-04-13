using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Implementation.Builders;
using MauiPdfGenerator.Implementation.Layout.Models;
using MauiPdfGenerator.Core.Fonts;
using MauiPdfGenerator.Fluent.Enums;
using System.Text;
using MauiPdfGenerator.Implementation.Extensions;
using System.Diagnostics;

namespace MauiPdfGenerator.Implementation.Layout.Managers; 

/// <summary>
/// Gestiona el layout y renderizado de párrafos de texto.
/// </summary>
internal class ParagraphManager
{
    // Cache simple para instancias de fuentes estándar
    private readonly Dictionary<(StandardFontType type, PdfFontAttributes attributes), PdfFontBase> _fontCache = [];

    public ParagraphManager()
    {
        // Constructor vacío por ahora
    }

    /// <summary>
    /// Mide el tamaño que necesitará el párrafo.
    /// </summary>
    public PdfSize Measure(ParagraphBuilder builder, LayoutContext context)
    {
        Debug.WriteLine($"---> ParagraphManager.Measure START for text: '{builder.ConfiguredText?.Substring(0, 10)}...'"); // Log inicio
        Debug.WriteLine($"     Context Area: {context.AvailableArea}");
        var padding = builder.ConfiguredPadding;
        var availableWidth = context.AvailableArea.Width - padding.Left - padding.Right;
        if (availableWidth < 0) availableWidth = 0;
        Debug.WriteLine($"     Available Width (after padding): {availableWidth}");

        double textHeight = 0;
        double finalLineHeight = builder.ConfiguredLineHeight * builder.ConfiguredFontSize;

        if (builder.ConfiguredText != null)
        {
            var font = GetFont(builder.ConfiguredFontFamily, builder.ConfiguredFontAttributes);
            var fontSize = builder.ConfiguredFontSize;
            finalLineHeight = builder.ConfiguredLineHeight * fontSize;

            Debug.WriteLine($"     Measuring simple text with Font: {font}, Size: {fontSize}, LineHeight: {finalLineHeight}");
            var lines = BreakTextIntoLines(builder.ConfiguredText, availableWidth, font, fontSize);
            Debug.WriteLine($"     BreakTextIntoLines returned {lines.Count} lines.");
            //foreach(var l in lines) Debug.WriteLine($"       Line: '{l}'"); // Descomentar si es necesario

            if (lines.Count == 1) textHeight = fontSize;
            else if (lines.Count > 1) textHeight = (lines.Count - 1) * finalLineHeight + fontSize;
            else textHeight = 0; // Sin lineas
            if (lines.Count == 0 && builder.ConfiguredText == "") textHeight = finalLineHeight; // Texto vacío = 1 línea
            Debug.WriteLine($"     Calculated textHeight: {textHeight}");
        }
        // ... (manejo formatted text placeholder) ...

        var totalHeight = textHeight + padding.Top + padding.Bottom;
        Debug.WriteLine($"     Total Height (with padding): {totalHeight}");

        // ... (cálculo de measuredWidth) ...
        double measuredWidth = 0;
        // ... (cálculo simplificado de measuredWidth como antes) ...
        if (builder.ConfiguredText != null)
        {
            var font = GetFont(builder.ConfiguredFontFamily, builder.ConfiguredFontAttributes);
            var fontSize = builder.ConfiguredFontSize;
            var lines = BreakTextIntoLines(builder.ConfiguredText, double.PositiveInfinity, font, fontSize);
            measuredWidth = lines.Count > 0 ? lines.Max(line => font.GetTextWidth(line, fontSize)) : 0;
        }
        measuredWidth += padding.Left + padding.Right;
        Debug.WriteLine($"     Measured Width (with padding): {measuredWidth}");


        // Aplicar restricciones
        var finalWidth = builder.ConfiguredWidth ?? Math.Min(measuredWidth, context.AvailableArea.Width);
        var finalHeight = builder.ConfiguredHeight ?? totalHeight;
        Debug.WriteLine($"     Final Width (pre-clamp): {finalWidth}, Final Height (pre-clamp): {finalHeight}");

        // Clamp final
        if (!builder.ConfiguredWidth.HasValue) finalWidth = Math.Min(finalWidth, context.AvailableArea.Width);
        if (!builder.ConfiguredHeight.HasValue) finalHeight = Math.Min(finalHeight, context.AvailableArea.Height);
        if (finalWidth < 0) finalWidth = 0;
        if (finalHeight < 0) finalHeight = 0;

        var finalSize = new PdfSize(finalWidth, finalHeight);
        Debug.WriteLine($"---> ParagraphManager.Measure END. Returning Size: {finalSize}"); // Log final
        return finalSize; // ASEGÚRATE QUE ES ESTE RETURN
    }

    /// <summary>
    /// Posiciona y dibuja el párrafo en el PDF.
    /// </summary>
    public void Arrange(ParagraphBuilder builder, LayoutContext context)
    {
        Debug.WriteLine($"---> ParagraphManager.Arrange START for text: '{builder.ConfiguredText?.Substring(0, 10)}...' in Rect {context.AvailableArea}");
        Debug.WriteLine($"context.AvailableArea: {context.AvailableArea}");
        var finalRect = context.AvailableArea; // Rectángulo PDF [LLx, LLy, Width, Height] asignado por el padre
        var padding = builder.ConfiguredPadding; // Usa Microsoft.Maui.Thickness
        var contentStream = context.ContentStream;

        // Calcular área de contenido efectiva DENTRO del finalRect, considerando padding
        var contentAreaWidth = finalRect.Width - padding.Left - padding.Right;
        var contentAreaHeight = finalRect.Height - padding.Top - padding.Bottom;
        if (contentAreaWidth < 0) contentAreaWidth = 0;
        if (contentAreaHeight < 0) contentAreaHeight = 0;

        // Origen inferior-izquierdo del área de contenido
        var contentAreaX = finalRect.X + padding.Left;
        var contentAreaY_Bottom = finalRect.Y + padding.Bottom;
        // Origen superior-izquierdo del área de contenido (para cálculos desde arriba)
        var contentAreaY_Top = contentAreaY_Bottom + contentAreaHeight;

        // Guardar estado gráfico actual
        contentStream.SaveGraphicsState();

        // Dibujar fondo (si existe) usando el finalRect completo
        if (builder.ConfiguredBackgroundColor.HasValue())
        {
            var bgColor = builder.ConfiguredBackgroundColor.Value();
            contentStream.SetFillColor(bgColor.Red, bgColor.Green, bgColor.Blue);
            contentStream.Rectangle(finalRect.X, finalRect.Y, finalRect.Width, finalRect.Height); // Usar coords PDF
            contentStream.Fill();
        }

        // Iniciar bloque de texto
        contentStream.BeginText();

        // --- Renderizado de Texto Simple ---
        if (builder.ConfiguredText is not null)
        {
            DrawSimpleTextInternal(builder, contentStream,
                                   contentAreaX, contentAreaY_Bottom, contentAreaY_Top,
                                   contentAreaWidth, contentAreaHeight);
        }
        // --- Renderizado de Texto Formateado (Placeholder) ---
        else if (builder.ConfiguredFormattedText is not null)
        {
            DrawFormattedTextInternal(builder, contentStream,
                                      contentAreaX, contentAreaY_Bottom, contentAreaY_Top,
                                      contentAreaWidth, contentAreaHeight);
        }

        // Finalizar bloque de texto
        contentStream.EndText();
        contentStream.RestoreGraphicsState();
    }

    // --- Método Interno para Texto Simple ---
    private void DrawSimpleTextInternal(ParagraphBuilder builder, PdfContentStream contentStream,
                                        double areaX, double areaYBottom, double areaYTop,
                                        double areaWidth, double areaHeight)
    {
        Debug.WriteLine($"areaX, areaYBottom, areaYTop, areaWidth, areaHeight: {areaX}, {areaYBottom}, {areaYTop}, {areaWidth}, {areaHeight}");
        var text = builder.ConfiguredText ?? string.Empty;
        var font = GetFont(builder.ConfiguredFontFamily, builder.ConfiguredFontAttributes);
        var fontSize = builder.ConfiguredFontSize;
        var lineHeight = builder.ConfiguredLineHeight * fontSize;
        var alignment = builder.ConfiguredHorizontalTextAlignment;
        var verticalAlignment = builder.ConfiguredVerticalOptions; // Usar VerticalOptions del builder

        // Aplicar estado inicial
        contentStream.SetFont(font, fontSize);
        if (builder.ConfiguredTextColor.HasValue())
        {
            var color = builder.ConfiguredTextColor.Value();
            contentStream.SetTextColor(color.Red, color.Green, color.Blue);
        }
        else
        {
            contentStream.SetTextColor(0, 0, 0); // Default negro
        }

        // Realizar Word Wrapping
        var lines = BreakTextIntoLines(text, areaWidth, font, fontSize);

        // Calcular altura total real del texto resultante
        double totalActualTextHeight = Math.Max(0, lines.Count * lineHeight - (lineHeight - fontSize)); // Altura total desde linea base primera a ultima (aprox)
                                                                                                        // Si solo hay una linea, la altura es fontSize, si hay N > 1 es (N-1)*lineHeight + fontSize
        if (lines.Count == 1) totalActualTextHeight = fontSize;
        else if (lines.Count > 1) totalActualTextHeight = (lines.Count - 1) * lineHeight + fontSize;
        else totalActualTextHeight = 0; // Sin lineas


        // Calcular Y inicial para la línea base de la *primera* línea (coords PDF)
        double currentPdfY;

        if (verticalAlignment == PdfVerticalAlignment.Center)
        {
            currentPdfY = areaYBottom + (areaHeight - totalActualTextHeight) / 2 + (totalActualTextHeight - fontSize);
        }
        else if (verticalAlignment == PdfVerticalAlignment.End)
        {
            currentPdfY = areaYBottom + totalActualTextHeight - fontSize;
        }
        else // Start or Fill (treat Fill as Start for now)
        {
            currentPdfY = areaYTop - fontSize;
        }
        // Asegurarse que Y no empiece por debajo del área
        currentPdfY = Math.Min(currentPdfY, areaYTop - fontSize);


        bool isFirstLine = true;
        foreach (var line in lines)
        {
            // Validar si aún caben líneas verticalmente
            // Comprobar contra la parte inferior del área de contenido
            if (currentPdfY < areaYBottom - (fontSize * 0.2)) // Pequeña tolerancia
            {
                Debug.WriteLine("Warning: Text overflow in ParagraphManager.Arrange. Stopping rendering.");
                break;
            }

            // Calcular X para la línea actual según alineación
            double currentPdfX = areaX;
            if (alignment == PdfTextAlignment.Center || alignment == PdfTextAlignment.End)
            {
                double lineWidth = font.GetTextWidth(line, fontSize);
                lineWidth = Math.Min(lineWidth, areaWidth); // No exceder el ancho
                if (alignment == PdfTextAlignment.Center)
                {
                    currentPdfX = areaX + (areaWidth - lineWidth) / 2;
                }
                else // End
                {
                    currentPdfX = areaX + areaWidth - lineWidth;
                }
            }

            // Posicionar y mostrar
            if (isFirstLine)
            {
                contentStream.MoveTextPosition(currentPdfX, currentPdfY);
                isFirstLine = false;
            }
            else
            {
                // TODO: Usar operador Td o T* para eficiencia
                contentStream.MoveTextPosition(currentPdfX, currentPdfY);
            }
            Debug.WriteLine($"line, currentPdfX, currentPdfY: {line}, {currentPdfX}, {currentPdfY}");
            Debug.WriteLine(">> Appending Tj <<");
            contentStream.ShowText(line, font); // <-- Pasar la fuente

            // Preparar Y para la siguiente línea
            currentPdfY -= lineHeight;
        }
    }

    // --- Método Interno para Texto Formateado (Placeholder) ---
    private void DrawFormattedTextInternal(ParagraphBuilder builder, PdfContentStream contentStream,
                                          double areaX, double areaYBottom, double areaYTop,
                                          double areaWidth, double areaHeight)
    {
        // TODO: Implementar renderizado complejo para spans, similar a DrawSimpleTextInternal
        // pero iterando spans, cambiando estado (font, size, color), y manejando wrapping
        // a través de los spans. Es significativamente más complejo.

        var defaultFont = GetFont(builder.ConfiguredFontFamily, builder.ConfiguredFontAttributes);
        var defaultFontSize = builder.ConfiguredFontSize;

        contentStream.SetFont(defaultFont, 10); // Usar tamaño más pequeño para el placeholder
        contentStream.SetTextColor(1, 0, 0); // Rojo para indicar no implementado
        contentStream.MoveTextPosition(areaX, areaYTop - 10); // Posicionar arriba
        contentStream.ShowText("[Formatted Text Rendering Not Implemented]", defaultFont);
        Debug.WriteLine("Warning: Arrange for FormattedText is not implemented yet.");
    }


    // --- Helper para Obtener Fuente (Simplificado) ---
    private PdfFontBase GetFont(string? fontFamily, PdfFontAttributes attributes)
    {
        // TODO: Mapeo real fontFamily -> Tipo de Fuente
        StandardFontType baseType = StandardFontType.Helvetica; // Default si fontFamily es null o no reconocido

        // Mapeo BÁSICO de ejemplo (ignora mayúsculas/minúsculas, necesita robustez)
        if (!string.IsNullOrEmpty(fontFamily))
        {
            if (fontFamily.Contains("Times", StringComparison.OrdinalIgnoreCase)) baseType = StandardFontType.TimesRoman;
            else if (fontFamily.Contains("Courier", StringComparison.OrdinalIgnoreCase)) baseType = StandardFontType.Courier;
            else if (fontFamily.Contains("Symbol", StringComparison.OrdinalIgnoreCase)) baseType = StandardFontType.Symbol;
            else if (fontFamily.Contains("Zapf", StringComparison.OrdinalIgnoreCase) || fontFamily.Contains("Dingbats", StringComparison.OrdinalIgnoreCase)) baseType = StandardFontType.ZapfDingbats;
            // Si no, se queda en Helvetica
        }


        bool isBold = attributes.HasFlag(PdfFontAttributes.Bold);
        bool isItalic = attributes.HasFlag(PdfFontAttributes.Italic); // Oblique para Helvetica/Courier

        StandardFontType finalType = baseType;

        // Aplicar atributos (simplificado, asume combinaciones existen para los 3 principales)
        switch (baseType)
        {
            case StandardFontType.Helvetica:
                if (isBold && isItalic) finalType = StandardFontType.HelveticaBoldOblique;
                else if (isBold) finalType = StandardFontType.HelveticaBold;
                else if (isItalic) finalType = StandardFontType.HelveticaOblique;
                break;
            case StandardFontType.TimesRoman:
                if (isBold && isItalic) finalType = StandardFontType.TimesBoldItalic;
                else if (isBold) finalType = StandardFontType.TimesBold;
                else if (isItalic) finalType = StandardFontType.TimesItalic;
                break;
            case StandardFontType.Courier:
                if (isBold && isItalic) finalType = StandardFontType.CourierBoldOblique;
                else if (isBold) finalType = StandardFontType.CourierBold;
                else if (isItalic) finalType = StandardFontType.CourierOblique;
                break;
            // Symbol y ZapfDingbats no tienen variantes Bold/Italic estándar
            case StandardFontType.Symbol:
            case StandardFontType.ZapfDingbats:
                finalType = baseType;
                break;
        }


        var cacheKey = (finalType, attributes); // Usar el tipo final y atributos para cache
        if (!_fontCache.TryGetValue(cacheKey, out var font))
        {
            // Crear nueva instancia si no está en caché
            // Si usamos solo estándar 14, podemos crear PdfStandardFont
            // Si soportamos otras, aquí iría la lógica para cargarlas/crearlas
            font = new PdfStandardFont(finalType);
            _fontCache[cacheKey] = font;
        }
        return font;
    }


    // --- Helper para Word Wrapping (Básico) ---
    private List<string> BreakTextIntoLines(string text, double maxWidth, PdfFontBase font, double fontSize)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
        {
            // Devuelve una lista con el texto original (o vacío) si no hay nada que romper o no hay ancho
            lines.Add(text ?? string.Empty);
            return lines;
        }

        var paragraphs = text.Replace("\r\n", "\n").Split('\n');
        bool firstParagraph = true;

        foreach (var paragraph in paragraphs)
        {
            if (!firstParagraph)
            {
                lines.Add(string.Empty); // Añadir línea vacía entre párrafos originales (causados por \n)
            }
            firstParagraph = false;

            if (string.IsNullOrWhiteSpace(paragraph))
            {
                // Si el párrafo original estaba vacío o solo espacios, ya añadimos la línea vacía arriba
                continue;
            }

            var words = paragraph.Split(new[] { ' ' }, StringSplitOptions.None); // Mantener espacios múltiples si los hubiera? Split(' ') es simple.
            var currentLine = new StringBuilder();
            double currentLineWidth = 0;
            double spaceWidth = font.GetTextWidth(" ", fontSize); // Precalcular ancho del espacio

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word)) // Manejar espacios múltiples resultando en words vacías
                {
                    if (currentLine.Length > 0) // Añadir espacio si no estamos al inicio de línea
                    {
                        currentLine.Append(' ');
                        currentLineWidth += spaceWidth;
                    }
                    continue;
                }

                double wordWidth = font.GetTextWidth(word, fontSize);

                // Caso 1: La línea está vacía
                if (currentLine.Length == 0)
                {
                    if (wordWidth <= maxWidth)
                    {
                        currentLine.Append(word);
                        currentLineWidth = wordWidth;
                    }
                    else
                    {
                        // Palabra más ancha que el máximo: Romperla (muy básico)
                        // TODO: Implementar ruptura de palabra inteligente
                        lines.Add(word); // Añadir la palabra tal cual (desbordará)
                        currentLineWidth = 0; // Resetear para la siguiente palabra/línea
                    }
                }
                // Caso 2: La línea no está vacía
                else
                {
                    double potentialWidth = currentLineWidth + spaceWidth + wordWidth;
                    if (potentialWidth <= maxWidth)
                    {
                        // Añadir espacio y palabra
                        currentLine.Append(' ');
                        currentLine.Append(word);
                        currentLineWidth = potentialWidth;
                    }
                    else
                    {
                        // No cabe, cerrar línea actual y empezar nueva con la palabra
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();

                        if (wordWidth <= maxWidth)
                        {
                            currentLine.Append(word);
                            currentLineWidth = wordWidth;
                        }
                        else
                        {
                            // Palabra más ancha que el máximo al empezar nueva línea
                            // TODO: Implementar ruptura de palabra inteligente
                            lines.Add(word); // Añadir la palabra tal cual (desbordará)
                            currentLineWidth = 0; // Resetear
                        }
                    }
                }
            } // Fin foreach word

            // Añadir la última línea del párrafo si tiene contenido
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
            }
        } // Fin foreach paragraph

        // Si el texto original era null o vacío, lines estará vacía, lo cual es correcto.
        // Si el texto era "", lines contendrá [""].
        // Si el texto era "\n", lines contendrá ["", ""].

        return lines;
    }
}