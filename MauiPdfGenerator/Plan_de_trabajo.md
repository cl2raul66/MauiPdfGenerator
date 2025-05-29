
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
                *   Si `sourceType is null` (Default): **Inferencia: ¿Es URI? -> Uri; ¿File.Exists? -> File; else -> Resource.** (Se mantiene la inferencia para el caso por defecto).
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


---

**Plan Definitivo Fase 4 (v1.0.0): Paginación Automática Primitiva**

**Versión:** 1.0.0

**Objetivos Principales:**

1.  **Implementar Paginación Automática para `PdfParagraph`:**
    *   Los párrafos que excedan el espacio vertical disponible en una página se dividirán por líneas, continuando en la(s) página(s) siguiente(s).
    *   La continuación de un párrafo en una nueva página no aplicará su margen superior original.
2.  **Implementar Paginación para `PdfImage`:**
    *   Si una imagen no cabe en el espacio vertical restante de la página actual, se moverá completa a la página siguiente.
    *   Si la imagen se mueve completa a una nueva página, se aplicará su margen superior original.
    *   Si una imagen es demasiado grande para caber incluso en una página nueva completa (considerando los márgenes de la página y los márgenes de la propia imagen), se renderizará un placeholder con el mensaje "[Imagen Demasiado Grande]" en el espacio disponible de la página actual donde se intentó colocar, y la imagen original se descartará (no se reintentará).
3.  **Consistencia de Configuración de Página:**
    *   Las nuevas páginas generadas debido a un desbordamiento heredarán la configuración (tamaño, orientación, márgenes, color de fondo, fuentes y espaciado predeterminados de la página) de la *definición de página original del usuario* que contenía el elemento que causó el desbordamiento.

**Estructuras de Datos y Modificaciones Clave:**

1.  **`RenderOutput` (Estructura de Datos):**
    *   Se definirá una `internal record struct RenderOutput` para que los métodos de renderizado de elementos devuelvan:
        *   `float HeightDrawnThisCall`: Altura real dibujada en la página actual.
        *   `PdfElement? RemainingElement`: El contenido restante del elemento si se dividió (e.g., un nuevo `PdfParagraph` con las líneas restantes), o `null` si el elemento se completó.
        *   `bool RequiresNewPage`: `true` si el elemento no cupo y necesita una nueva página para sí mismo (e.g., una imagen completa) o para su continuación.

2.  **`TextRenderer.Render`:**
    *   **Firma Modificada:** `internal RenderOutput Render(SKCanvas canvas, PdfParagraph paragraph, PdfPageData pageDefinition, SKRect currentPageContentRect, float currentYOnPage)`
    *   **Lógica:**
        *   Calculará el espacio vertical disponible en `currentPageContentRect` a partir de `currentYOnPage`.
        *   Dividirá el texto en líneas.
        *   Renderizará tantas líneas como quepan en el espacio disponible.
        *   Si quedan líneas, creará un nuevo `PdfParagraph` con las líneas restantes. Este `RemainingParagraph` NO debe tener márgenes propios (o márgenes cero), ya que es una continuación directa. Los estilos de fuente, alineación, etc., se heredarán.
        *   Devolverá un `RenderOutput` con la altura dibujada, el `RemainingParagraph` (si existe), y `RequiresNewPage` será `true` si hay un `RemainingParagraph`.

3.  **`ImageRenderer.RenderAsync`:**
    *   **Firma Modificada:** `internal Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfImage image, PdfPageData pageDefinition, SKRect currentPageContentRect, float currentYOnPage)`
    *   **Lógica:**
        *   Calculará el espacio vertical disponible (`availableHeightForElementContent`).
        *   Intentará cargar y decodificar la imagen. Si falla, renderizará un placeholder `[Image Error]` y devolverá `RenderOutput` indicando que el elemento se completó (sin `RemainingElement`, `RequiresNewPage = false`).
        *   Si la imagen se carga, calculará su `targetRect`.
        *   **Caso 1 (Cabe en la página actual):** Si `targetRect.Height <= availableHeightForElementContent`, dibujará la imagen y devolverá `RenderOutput` con la altura de la imagen, `RemainingElement = null`, `RequiresNewPage = false`.
        *   **Caso 2 (No cabe en la página actual, pero sí en una nueva):**
            *   Calculará si la imagen cabría en una página nueva completa (usando `pageDefinition.Size`, `pageDefinition.Margins`, y los márgenes de la propia imagen).
            *   Si cabe en una nueva página: No dibujará nada en la página actual. Devolverá `RenderOutput` con `HeightDrawnThisCall = 0`, `RemainingElement = image` (la imagen original), `RequiresNewPage = true`.
        *   **Caso 3 (No cabe ni en una nueva página completa):** Renderizará un placeholder `[Imagen Demasiado Grande]` en el espacio disponible de la página actual. Devolverá `RenderOutput` con la altura del placeholder, `RemainingElement = null`, `RequiresNewPage = false`.

4.  **`SkPdfGenerationService.GenerateAsync`:**
    *   **Lógica de Paginación Principal:**
        *   El bucle `for` externo iterará sobre las *definiciones de página* del usuario (`PdfPageData`).
        *   Se introducirá un bucle `while` interno que procesará una cola (`Queue<PdfElement>`) de elementos para la definición de página actual. Esta cola se inicializará con los elementos de la `PdfPageData` actual.
        *   Se mantendrá un `PdfElement? currentProcessingElement` para el contenido restante de un elemento que se está dividiendo.
        *   **Gestión de Páginas Físicas en el PDF:**
            *   Se abrirá una nueva página física en el `SKDocument` (`pdfDoc.BeginPage()`) al inicio, o cuando el contenido desborde o un `RenderOutput` indique `RequiresNewPage = true`.
            *   `currentY` se reseteará al `Top` del `contentRect` de la nueva página física.
        *   **Procesamiento de Elementos:**
            *   Se tomará un elemento de `currentProcessingElement` (prioridad) o de la cola.
            *   Se determinará si es una continuación. Se aplicarán márgenes superiores según las reglas definidas (no para continuación de párrafo, sí para imagen movida completa).
            *   Se llamará al método `Render` o `RenderAsync` del elemento.
            *   Según el `RenderOutput`:
                *   Se actualizará `currentY` con `HeightDrawnThisCall`.
                *   Si hay `RemainingElement`, se almacenará en `currentProcessingElement` para la siguiente iteración del bucle `while` (que probablemente comenzará en una nueva página).
                *   Si `RequiresNewPage` es `true` (y no se dibujó nada, como una imagen que se mueve completa), se forzará una nueva página física y `currentProcessingElement` se establecerá con el elemento original.
                *   Se manejarán los desbordamientos después de aplicar márgenes inferiores y espaciado de página, forzando una nueva página física si es necesario.
        *   La página física actual se cerrará (`pdfDoc.EndPage()`) antes de crear una nueva o al finalizar todos los elementos de la definición de página del usuario.

**Impacto y Dependencias:**

*   La complejidad principal reside en la nueva lógica de bucle y gestión de estado en `SkPdfGenerationService`.
*   Los renderizadores de elementos (`TextRenderer`, `ImageRenderer`) deben adaptar sus firmas de retorno y su lógica interna para soportar la división/movimiento.
*   No se introducen nuevas dependencias externas.

**Pruebas Clave:**

*   Párrafo corto que cabe en una página.
*   Párrafo largo que se extiende por dos o más páginas.
*   Párrafo largo que comienza cerca del final de una página y se divide.
*   Imagen que cabe en el espacio restante.
*   Imagen que no cabe en el espacio restante pero sí en una nueva página.
*   Imagen que no cabe en el espacio restante Y es demasiado grande para una página nueva completa.
*   Secuencia de múltiples elementos que causan varios saltos de página.
*   Verificar que los márgenes y la configuración de página se apliquen correctamente en las páginas nuevas.

---
