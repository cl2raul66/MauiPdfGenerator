
**Plan de Trabajo Organizado:**

**Fase 2: Implementación de Contenido Básico (v0.2.0)**

**Objetivo:** Permitir al usuario añadir elementos básicos (párrafo, línea horizontal) a una página PDF, con configuración fluida de propiedades y una disposición vertical automática (layout implícito). Usar fuentes estándar de PDF.

**Líneas Guía Detalladas (Fase 2 - v0.2.0):**

1.  **Definir Modelos de Elementos (Públicos) (`MauiPdfGenerator.Fluent.Models.Elements` - v0.2.0):**
    *   Crear clase base abstracta `public abstract class PdfElement` con:
        *   Propiedad `Margin` (tipo `Thickness`, default `Zero`).
        *   Método `SetMargin(Thickness margin)` que devuelve `this`.
        *   Método `SetMargin(double uniform)` (sobercarga).
        *   Método `SetMargin(double horizontal, double vertical)` (sobercarga).
        *   Método `SetMargin(double left, double top, double right, double bottom)` (sobercarga).
    *   Crear `public class PdfParagraph : PdfElement`:
        *   Constantes para defaults: `DefaultFontFamily = "Helvetica"`, `DefaultFontSize = 12f`, `DefaultTextColor = Colors.Black`, `DefaultAlignment = TextAlignment.Start`, `DefaultFontAttributes = FontAttributes.None`.
        *   Propiedades (privadas con getter público o solo getter): `Text { get; }`, `CurrentFontFamily { get; private set; }`, `CurrentFontSize { get; private set; }`, `CurrentTextColor { get; private set; }`, `CurrentAlignment { get; private set; }`, `CurrentFontAttributes { get; private set; }`.
        *   Constructor: `public PdfParagraph(string text)`. Inicializar propiedades con defaults.
        *   Métodos Fluent (devuelven `this`): `FontFamily(string family)`, `FontSize(float size)`, `TextColor(Color color)`, `Alignment(TextAlignment alignment)`, `Bold()`, `Italic()`. *(Nota: `Bold`/`Italic` modificarán `CurrentFontAttributes`)*.
    *   Crear `public class PdfHorizontalLine : PdfElement`:
        *   Constantes para defaults: `DefaultThickness = 1f`, `DefaultColor = Colors.Black`.
        *   Propiedades: `CurrentThickness { get; private set; }`, `CurrentColor { get; private set; }`.
        *   Constructor: `public PdfHorizontalLine()`. Inicializar con defaults.
        *   Métodos Fluent: `Thickness(float value)`, `Color(Color color)`.
    *   *(Post-v0.2.0: `PdfImage`, `PdfSpacer` si es necesario)*.

2.  **Definir API Contenedor (Pública) (`MauiPdfGenerator.Fluent.Interfaces` - v0.2.0):**
    *   Crear `public interface IPageContentBuilder`:
        *   `PdfParagraph PdfParagraph(string text);`
        *   `PdfHorizontalLine PdfHorizontalLine();`
        *   *(Post-v0.2.0: `PdfImage`)*.
    *   Modificar `MauiPdfGenerator.Fluent.Interfaces.Pages.IPdfContentPage`:
        *   Asegurarse que tiene: `IPdfContentPage Content(Action<IPageContentBuilder> contentSetup);`
        *   Asegurarse que tiene: `IPdfContentPage Spacing(float value);` (Default razonable, ej. 5f).
        *   Asegurarse que tiene: `IPdfContentPage DefaultFontFamily(string familyName);`
        *   Asegurarse que tiene: `IPdfContentPage DefaultFontSize(float size);`
        *   Asegurarse que tiene: `IPdfContentPage DefaultTextColor(Color color);`

3.  **Implementar Builders (Internos) (`MauiPdfGenerator.Fluent.Builders` - v0.2.0):**
    *   Crear `internal class PageContentBuilder : IPageContentBuilder`:
        *   Tendrá `private List<PdfElement> children = new();`.
        *   Implementará los métodos de la interfaz creando instancias de `PdfParagraph`/`PdfHorizontalLine`, añadiéndolas a `children`, y devolviéndolas.
        *   Tendrá `internal IReadOnlyList<PdfElement> GetChildren() => children;`.
    *   Modificar `internal class PdfContentPageBuilder`:
        *   Almacenará `private float pageSpacing = 5f;` (o el default elegido).
        *   Almacenará `private string pageDefaultFontFamily = PdfParagraph.DefaultFontFamily;` etc. para los defaults de página.
        *   Almacenará `private List<PdfElement> pageElements = new();`.
        *   Implementará `IPdfContentPage.Spacing(float value)` actualizando `pageSpacing`.
        *   Implementará `IPdfContentPage.DefaultFontFamily(string familyName)` etc.
        *   Implementará `IPdfContentPage.Content(Action<IPageContentBuilder> contentSetup)`:
            *   Creará `var builder = new PageContentBuilder();`.
            *   Llamará `contentSetup(builder);`.
            *   Asignará `this.pageElements = builder.GetChildren().ToList();` (o `AddRange`).
            *   Devolverá `this`.
        *   Modificar `IPdfContentPageBuilder` (la interfaz interna) para exponer métodos `GetElements()` y `GetPageDefaults()` (Spacing, FontFamily, FontSize, TextColor).

4.  **Actualizar Modelo Core (`MauiPdfGenerator.Core.Models` - v0.2.0):**
    *   Modificar `internal record PdfPageData`:
        *   Asegurarse que contiene `IReadOnlyList<PdfElement> Elements`. (Donde `PdfElement` es el tipo público de Fluent).
        *   Asegurarse que contiene `float PageDefaultSpacing`.
        *   Asegurarse que contiene `string PageDefaultFontFamily`.
        *   Asegurarse que contiene `float PageDefaultFontSize`.
        *   Asegurarse que contiene `Color PageDefaultTextColor`.

5.  **Implementar Renderizado Core (`MauiPdfGenerator.Core.Implementation.Sk` - v0.2.0):**
    *   Modificar `SkPdfGenerationService.GenerateAsync`:
        *   Dentro del bucle `foreach (var pageData in documentData.Pages)`:
            *   Obtener `elements = pageData.Elements` y los `pageData.PageDefault*`.
            *   Inicializar `currentY = contentRect.Top;`.
            *   Iterar `foreach (var element in elements)`:
                *   `currentY += (float)element.Margin.Top;`
                *   `switch (element)`:
                    *   `case PdfParagraph p:`:
                        *   Determinar `fontFamily`, `fontSize`, `textColor`, `attributes`, `alignment` efectivos (elemento ?? página ?? default del elemento).
                        *   Crear `SKTypeface` (usando `SKTypeface.FromFamilyName` con mapeo de atributos).
                        *   Crear `SKPaint` (con Typeface, Size, Color, Align).
                        *   **Medición y Ajuste de Línea (Simplificado v0.2.0):**
                            *   Usar `SKPaint.MeasureText(p.Text, ref bounds)` para obtener altura de una sola línea. Si necesitas ajuste, es más complejo: calcula cuántos caracteres caben (`SKPaint.BreakText`), dibuja esa línea, avanza Y, repite con el resto del texto. *Empezar sin ajuste de línea o con uno muy básico.*
                            *   `measuredHeight = bounds.Height;` (o la altura total si hay ajuste).
                        *   Dibujar texto(s) con `canvas.DrawText(lineText, contentRect.Left + (float)element.Margin.Left, currentY - bounds.Top, paint);` (Ajustar X por `Align` y `Margin.Left`, ajustar Y por `bounds.Top` ya que `DrawText` usa la línea base).
                        *   `currentY += measuredHeight;`
                    *   `case PdfHorizontalLine l:`:
                        *   Determinar `thickness`, `color` efectivos.
                        *   Crear `SKPaint` (Style = Stroke, StrokeWidth = thickness, Color = color).
                        *   Dibujar línea `canvas.DrawLine(contentRect.Left + (float)element.Margin.Left, currentY + thickness / 2f, contentRect.Right - (float)element.Margin.Right, currentY + thickness / 2f, paint);`
                        *   `currentY += thickness;`
                *   `currentY += (float)element.Margin.Bottom + pageData.PageDefaultSpacing;`
                *   Comprobar desbordamiento (`if (currentY > contentRect.Bottom) { break; // O manejar }`).