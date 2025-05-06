
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

Entendido. Respeto tu decisión final sobre el diseño de la API y el enum. Procederemos con tu especificación exacta.

Aquí tienes el **Plan Definitivo y Completo para la Fase 3 (v1.0.0)**, incorporando tus requisitos finales para la API de imágenes:

---

**Plan Definitivo Fase 3 (v1.0.0): Texto Ajustable e Imágenes**

**Versión:** 1.0.0

**Objetivos Principales:**

1.  **Refactorización del Renderizado de Texto:**
    *   Extraer la lógica compleja de `RenderParagraph` (medición, ajuste, dibujo multilínea) a una nueva clase interna: `MauiPdfGenerator.Core.Implementation.Sk.TextRenderer`.
2.  **Implementar Ajuste de Línea con `LineBreakMode`:**
    *   Añadir la propiedad `internal LineBreakMode? CurrentLineBreakMode` a `PdfParagraph`.
    *   Añadir el método fluido `LineBreakMode(LineBreakMode mode)` a `PdfParagraph`.
    *   `TextRenderer` implementará la lógica para `WordWrap` (default), `CharacterWrap`, y `NoWrap`. *(Truncación postergada)*.
3.  **Añadir Elemento `PdfImage` con Sobrecarga + Enum (Tu Diseño):**
    *   **Enum (público):**
        ```csharp
        public enum PdfImageSourceType
        {
            IsMauiSource, // Indica que el string es un identificador de recurso MAUI
            IsFileSource, // Indica que el string es una ruta de archivo
            IsUriSource   // Indica que el string es una URI
        }
        ```
    *   **Clase `PdfImage : PdfElement`:**
        *   Propiedades `internal object SourceData { get; }` (almacena string, Stream o Uri).
        *   Propiedad `internal PdfImageSourceKind DeterminedSourceKind { get; }` (enum interno: File, Resource, Uri, Stream).
        *   Propiedades `internal double? RequestedWidth`, `internal double? RequestedHeight`.
        *   Propiedad `internal Aspect CurrentAspect = Aspect.AspectFit;`.
    *   **API Fluida para `PdfImage`:**
        *   Métodos `.WidthRequest(double)`, `.HeightRequest(double)`, `.Aspect(Aspect)`.
    *   **`IPageContentBuilder`:**
        *   `PdfImage PdfImage(Stream stream)`
        *   `PdfImage PdfImage(Uri uri)`
        *   `PdfImage PdfImage(string source, PdfImageSourceType? sourceType = null)`
            *   **Lógica de Determinación (al crear `PdfImage`):**
                *   Si `sourceType == PdfImageSourceType.IsMauiSource`: `DeterminedSourceKind = Resource`.
                *   Si `sourceType == PdfImageSourceType.IsFileSource`: `DeterminedSourceKind = File`.
                *   Si `sourceType == PdfImageSourceType.IsUriSource`: `DeterminedSourceKind = Uri`.
                *   Si `sourceType == null` (Default): **Inferencia: ¿Es URI? -> Uri; ¿File.Exists? -> File; else -> Resource.** (Se mantiene la inferencia para el caso por defecto).
                *   Si la sobrecarga es `Stream`: `DeterminedSourceKind = Stream`.
                *   Si la sobrecarga es `Uri`: `DeterminedSourceKind = Uri`.
    *   **Renderizado (`ImageRenderer` o en `SkPdfGenerationService`):**
        *   **Resolver Fuente:** Usar `DeterminedSourceKind`.
        *   **Cargar Datos:** Implementar carga para File, Resource (vía servicios MAUI), Uri (vía HttpClient), Stream.
        *   **Decodificar:** `SKBitmap.Decode`/`SKImage.FromEncodedData`.
        *   **Manejo de Errores:** Log + Dibujar Placeholder (rectángulo + texto "[Error Imagen]").
        *   **Cálculo Tamaño/Posición:** Según datos, requests y `Aspect`.
        *   **Dibujo:** `canvas.DrawImage`.

**Impacto y Dependencias:**

*   Requiere dependencia de `Microsoft.Maui.Controls` (para `LineBreakMode`, `Aspect`) y `Microsoft.Maui.Graphics`.
*   La carga de recursos y potencialmente URIs introduce dependencia del contexto de ejecución MAUI (`IServiceProvider`, `HttpClient`).
*   La complejidad principal reside en la implementación de `TextRenderer` (ajuste de línea) y el `ImageRenderer` (carga de datos desde diversas fuentes MAUI y manejo de errores).
