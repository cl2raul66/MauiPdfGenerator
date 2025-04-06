using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Common.Geometry; // For PdfRectangle
using System;
using System.Collections.Generic;
using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Core.Fonts;
using System.Globalization;
using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Builders;

/// <summary>
/// Internal implementation of the page builder interface.
/// Manages the definition of content for a single PDF page.
/// </summary>
internal class PageBuilder : IPdfPageBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfPage _pdfPage; // The Core PdfPage object this builder targets
    private readonly DocumentBuilder _documentBuilder; // Access to document defaults/settings
    private readonly PdfResources _pageResources; // Resources specific to this page

    // Store the root content defined by the user (e.g., the root layout builder)
    // Option 1: Store the action itself
    private Action<IPdfContainerContentBuilder>? _contentAction;
    // Option 2: Store the result of the action (e.g., list of root builders/elements)
    private readonly List<object> _rootElements = []; // Store builders or intermediate models

    // --- Page specific settings (Defaults from DocumentBuilder, can be overridden) ---
    private PdfRectangle _mediaBox;
    private float _marginLeft;
    private float _marginTop;
    private float _marginRight;
    private float _marginBottom;
    // TODO: Add properties for Page-specific Rotation, CropBox etc. if needed

    public PageBuilder(PdfDocument pdfDocument, PdfPage pdfPage, DocumentBuilder documentBuilder)
    {
        _pdfDocument = pdfDocument ?? throw new ArgumentNullException(nameof(pdfDocument));
        _pdfPage = pdfPage ?? throw new ArgumentNullException(nameof(pdfPage));
        _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));

        // Initialize with defaults from DocumentBuilder
        _mediaBox = _pdfPage.MediaBox; // Get the MediaBox assigned by DocumentBuilder
        _marginLeft = _documentBuilder.DefaultPageMarginLeft;
        _marginTop = _documentBuilder.DefaultPageMarginTop;
        _marginRight = _documentBuilder.DefaultPageMarginRight;
        _marginBottom = _documentBuilder.DefaultPageMarginBottom;

        // Assign the pre-created Resources dictionary from PdfPage
        _pageResources = (_pdfPage.Resources as PdfDictionary) as PdfResources ??
                         throw new InvalidOperationException("PdfPage is missing its Resources dictionary.");
        if (!(_pdfPage.Resources is PdfResources)) // Ensure it's our specific type if needed later
        {
            // This might indicate an issue if PdfPage constructor created a plain PdfDictionary
            Console.WriteLine("Warning: Page Resources dictionary is not of type PdfResources.");
        }

    }

    // --- IPdfPageBuilder Implementation ---

    /// <summary>
    /// Defines the primary content structure for this page.
    /// </summary>
    /// <param name="contentAction">Action to add content using the provided builder.</param>
    /// <returns>The page builder instance.</returns>
    public IPdfPageBuilder Content(Action<IPdfContainerContentBuilder> contentAction)
    {
        ArgumentNullException.ThrowIfNull(contentAction);

        // Option 1: Store the action to execute later during FinalizePage
        // _contentAction = contentAction;

        // Option 2: Execute the action now and store the resulting builders/elements
        var contentBuilder = new ContainerContentBuilder(_pdfDocument, _pageResources, this /* or null context? */);
        contentAction(contentBuilder);
        _rootElements.AddRange(contentBuilder.GetAddedElements()); // Need GetAddedElements method

        return this;
    }

    // TODO: Implement IPdfPageBuilder.Configure(Action<IPdfPageConfigurator> configAction) if needed


    // --- Internal Methods ---

    /// <summary>
    /// Called by DocumentBuilder after the user's page build action has been executed.
    /// This method generates the actual PDF content stream for the page.
    /// </summary>
    internal void FinalizePage()
    {
        // 1. Crear el Content Stream
        var contentStream = new PdfContentStream(_pdfDocument, _pageResources);

        // 2. Obtener el área de contenido (respetando márgenes)
        // PDF Y=0 está en la parte inferior, pero nuestros cálculos suelen ser desde arriba.
        // ContentArea: X, Y (bottom-left), Width, Height
        PdfRectangle contentArea = GetContentArea(); // Usa el helper interno
        double currentY = contentArea.Y + contentArea.Height; // Start Y position from the TOP of the content area

        // 3. Procesar elementos raíz (Layout Simplificado: Apilamiento Vertical)
        Console.WriteLine($"Finalizing Page: Processing {_rootElements.Count} root elements.");

        foreach (var elementBuilderObject in _rootElements)
        {
            // Determinar tamaño y dibujar el elemento en la posición actual
            // Esta lógica necesita refactorizarse en un "Layout Engine" o métodos "Measure/Arrange"

            if (elementBuilderObject is ParagraphBuilder paraBuilder)
            {
                // --- Dibujar Párrafo (Simplificado) ---
                Console.WriteLine($"  Drawing Paragraph: '{paraBuilder.ConfiguredText?.Substring(0, Math.Min(30, paraBuilder.ConfiguredText?.Length ?? 0)) ?? "Formatted"}...'");

                // A. Obtener la fuente (¡Muy Simplificado!)
                // TODO: Implementar gestión de fuentes real (selección, métricas)
                var font = GetFontOrDefault(paraBuilder.ConfiguredFontFamily);
                var fontSize = paraBuilder.ConfiguredFontSize;

                // B. Medir Texto (¡Placeholder - Necesita métricas reales!)
                // TODO: Calcular alto real basado en texto, fuente, tamaño y ancho disponible
                double estimatedHeight = fontSize * 1.2 * CountLines(paraBuilder.ConfiguredText, contentArea.Width); // Estimación MUY básica

                // C. Calcular Posición (Origen inferior izquierdo del texto)
                // Asumimos que el párrafo ocupa todo el ancho disponible por ahora
                double x = contentArea.X; // Alineado a la izquierda
                currentY -= estimatedHeight; // Mover Y hacia abajo (recordar que Y=0 es abajo)
                double y = currentY;

                // D. Validar si cabe en la página actual (simplificado)
                if (y < contentArea.Y)
                {
                    Console.WriteLine("  Warning: Content exceeds page height (Paragraph).");
                    // TODO: Implementar lógica de salto de página
                    break; // Detener procesamiento en esta página
                }


                // E. Escribir operadores PDF
                contentStream.SaveGraphicsState(); // q
                contentStream.BeginText(); // BT
                contentStream.SetFont(font, fontSize); // /F1 12 Tf
                                                       // TODO: Set Text Color
                                                       // TODO: Handle Text Alignment (Td / Matrix?)
                contentStream.MoveTextPosition(x, y + fontSize * 0.8); // Posición inicial aproximada de la línea base (Td) - necesita ajuste
                                                                       // TODO: Handle multiple lines, FormattedText, Spans, LineHeight
                contentStream.ShowText(paraBuilder.ConfiguredText ?? "Formatted Text Placeholder", font); // (Text) Tj
                contentStream.EndText(); // ET
                contentStream.RestoreGraphicsState(); // Q

                // F. Actualizar Y para el siguiente elemento (añadir espaciado)
                currentY -= _documentBuilder.DefaultElementSpacing; // Usar el espaciado default

            }
            else if (elementBuilderObject is ImageBuilder imgBuilder)
            {
                // --- Dibujar Imagen (Simplificado) ---
                Console.WriteLine($"  Drawing Image: Source type {imgBuilder.ConfiguredSourceType}");

                try
                {
                    // A. Asegurarse que el XObject está creado
                    var imageXObject = imgBuilder.GetOrCreatePdfImageXObject();

                    // B. Obtener el nombre del recurso
                    var imageName = _pageResources.GetResourceName(imageXObject);

                    // C. Determinar Dimensiones de Dibujo (Respetando Aspect y Width/Height explícito)
                    // TODO: Implementar lógica de cálculo de tamaño real (Measure)
                    double desiredWidth = imgBuilder.ConfiguredWidth ?? imageXObject.Width;
                    double desiredHeight = imgBuilder.ConfiguredHeight ?? imageXObject.Height;
                    PdfSize drawSize = CalculateDrawSize(
                        new PdfSize(imageXObject.Width, imageXObject.Height), // Tamaño intrínseco
                        new PdfSize(desiredWidth, desiredHeight), // Tamaño deseado/explícito
                        new PdfSize(contentArea.Width, contentArea.Height), // Tamaño disponible (simplificado)
                        imgBuilder.ConfiguredAspect
                    );


                    // D. Calcular Posición (Origen inferior izquierdo)
                    // TODO: Implementar Horizontal/VerticalOptions
                    double x = contentArea.X; // Alineado a la izquierda (simplificado)
                    currentY -= drawSize.Height; // Mover Y hacia abajo
                    double y = currentY;

                    // E. Validar si cabe
                    if (y < contentArea.Y)
                    {
                        Console.WriteLine("  Warning: Content exceeds page height (Image).");
                        // TODO: Implementar lógica de salto de página
                        break;
                    }

                    // F. Escribir operadores PDF (q, cm, Do, Q)
                    contentStream.SaveGraphicsState(); // q
                                                       // Establecer CTM (Current Transformation Matrix) para posicionar y escalar
                                                       // Matriz: [Width 0 0 Height X Y] cm
                    contentStream.AppendOperator($"{Fd(drawSize.Width)} 0 0 {Fd(drawSize.Height)} {Fd(x)} {Fd(y)} cm");
                    // Dibujar la imagen
                    contentStream.DrawXObject(imageXObject); // /Im1 Do
                    contentStream.RestoreGraphicsState(); // Q

                    // G. Actualizar Y
                    currentY -= _documentBuilder.DefaultElementSpacing;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error processing image: {ex.Message}");
                    // Continuar con el siguiente elemento? O detenerse?
                }
            }
            // --- Añadir lógica para otros builders (Grid, VSL, HSL) ---
            // else if (elementBuilderObject is GridBuilder gridBuilder) { /*...*/ }
            else
            {
                Console.WriteLine($"  Warning: Skipping unsupported element type: {elementBuilderObject.GetType().Name}");
            }
        }

        // 4. Finalizar Content Stream y asignarlo a la página
        contentStream.Dispose(); // Esto llama a GetContentBytes y actualiza UnfilteredData
                                 // Ya no necesitamos llamar a GetContentBytes explícitamente aquí si Dispose lo hace.

        // 5. Añadir el stream como objeto indirecto y asignar a /Contents
        var contentStreamIndirect = _pdfDocument.AddIndirectObject(contentStream);
        _pdfPage.Contents = contentStreamIndirect.Reference;

        // 6. Añadir Recursos como objeto indirecto y asignar a /Resources
        var resourcesIndirect = _pdfDocument.AddIndirectObject(_pageResources);
        _pdfPage.Add(PdfName.Resources, resourcesIndirect.Reference);

        Console.WriteLine($"Page finalized. Content stream {contentStreamIndirect.Reference}, Resources {resourcesIndirect.Reference}.");
    }

    // Internal properties to provide context to child builders if needed
    internal double GetAvailableWidth() => _mediaBox.Width - (double)_marginLeft - (double)_marginRight;
    internal double GetAvailableHeight() => _mediaBox.Height - (double)_marginTop - (double)_marginBottom;
    internal PdfRectangle GetContentArea() => new PdfRectangle(
             _mediaBox.X + (double)_marginLeft,
             _mediaBox.Y + (double)_marginBottom,
             GetAvailableWidth(),
             GetAvailableHeight()
         );

    private PdfFontBase GetFontOrDefault(string? fontFamilyName)
    {
        // TODO: Implementar caché/gestión real de fuentes.
        // Buscar por nombre, si no, devolver un default.
        // Por ahora, siempre devuelve Helvetica.
        if (!string.IsNullOrEmpty(fontFamilyName))
        {
            Console.WriteLine($"  Font requested: {fontFamilyName} (Using Helvetica default)");
        }
        // Deberíamos cachear la instancia de la fuente estándar
        return new PdfStandardFont(StandardFontType.Helvetica);
    }

    private int CountLines(string? text, double maxWidth)
    {
        // Estimación extremadamente básica, no considera ancho real de caracteres.
        if (string.IsNullOrEmpty(text)) return 1;
        // Asume un ancho promedio de caracter (muy impreciso!)
        double avgCharWidth = 7; // Completamente arbitrario para fuente 12pt
        double charsPerLine = Math.Max(1, maxWidth / avgCharWidth);
        return (int)Math.Ceiling(text.Length / charsPerLine);
    }

    private static PdfSize CalculateDrawSize(PdfSize intrinsicSize, PdfSize requestedSize, PdfSize availableSize, PdfAspect aspect)
    {
        // TODO: Implementar lógica de Aspect Ratio real
        // Esta es una versión muy básica que prioriza tamaño explícito sobre intrínseco
        // y no implementa Aspect aún correctamente.
        double finalW = requestedSize.Width > 0 ? requestedSize.Width : intrinsicSize.Width;
        double finalH = requestedSize.Height > 0 ? requestedSize.Height : intrinsicSize.Height;

        // Simplificación: Limitar al tamaño disponible (sin aspect ratio)
        finalW = Math.Min(finalW, availableSize.Width);
        finalH = Math.Min(finalH, availableSize.Height);

        if (finalW <= 0 || finalH <= 0) return PdfSize.Zero; // Evitar tamaño inválido

        // --- Lógica de Aspect (Simplificada/Placeholder) ---
        if (aspect != PdfAspect.Fill && intrinsicSize.Width > 0 && intrinsicSize.Height > 0) // Avoid division by zero
        {
            double intrinsicRatio = intrinsicSize.Width / intrinsicSize.Height;
            double calculatedRatio = finalW / finalH;

            if (aspect == PdfAspect.AspectFit)
            {
                if (calculatedRatio > intrinsicRatio) // Contenedor más ancho que la imagen -> Limitar por altura
                {
                    finalW = finalH * intrinsicRatio;
                }
                else // Contenedor más alto (o igual) que la imagen -> Limitar por anchura
                {
                    finalH = finalW / intrinsicRatio;
                }
            }
            else if (aspect == PdfAspect.AspectFill)
            {
                if (calculatedRatio < intrinsicRatio) // Contenedor más estrecho que la imagen -> Limitar por altura (para llenar ancho)
                {
                    finalW = finalH * intrinsicRatio;
                }
                else // Contenedor más ancho (o igual) que la imagen -> Limitar por anchura (para llenar alto)
                {
                    finalH = finalW / intrinsicRatio;
                }
            }
            // Aspect Fill/Fit pueden exceder el availableSize inicial, re-ajustar si es necesario?
            // Esta parte se complica rápido. Por ahora, la lógica anterior es un intento básico.
            finalW = Math.Min(finalW, availableSize.Width); // Re-clamp after aspect calc?
            finalH = Math.Min(finalH, availableSize.Height);
        }


        return new PdfSize(finalW, finalH);
    }

    // Helper para formato double en operadores PDF
    private static string FormatDouble(double value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }
    private static string Fd(double value) => FormatDouble(value);

}
