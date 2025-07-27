# Documentación Técnica de la Biblioteca MauiPdfGenerator

---

# Parte I: Visión General y Conceptual

## 1. Principio Rector

Esta biblioteca está diseñada como una **extensión natural del ecosistema .NET MAUI**, no como una herramienta externa. Su propósito es permitir que los desarrolladores MAUI generen PDFs de forma nativa utilizando conceptos y, fundamentalmente, **tipos de datos** que ya les son familiares.

Para lograr esta integración profunda, la biblioteca adopta directamente tipos del ecosistema MAUI, incluyendo:
- `Microsoft.Maui.Graphics.Color`
- `Microsoft.Maui.Thickness`
- `Microsoft.Maui.Controls.LayoutAlignment`
- `Microsoft.Maui.FontAttributes`
- `Microsoft.Maui.TextDecorations`
- `Microsoft.Maui.TextTransform`

El desarrollador no aprende una nueva API desde cero, simplemente aplica su conocimiento existente de MAUI a un nuevo lienzo: el documento PDF.

### 1.1. Estructura Jerárquica General

La interfaz de un documento PDF se construye con una jerarquía que mapea conceptualmente a la de .NET MAUI. La terminología correcta es la siguiente:

**MAUI → PDF (Analogía Jerárquica)**
- **Pages** → `Page` (Páginas del documento PDF)
- **Layouts** → `Layout` (Estructuras de organización visual en PDF)
- **Views** → `View` (Elementos de contenido visual en PDF)

#### Pages

Los documentos PDF constan de una o varias páginas. Cada página contiene al menos un `Layout`. MauiPdfGenerator contiene la siguiente `Page`:

|Page|Descripción|
|---|---|
|PdfContentPage|Muestra un `Layout` principal y es el tipo de página más común en documentos PDF.|

> **NOTA:** Para el futuro, se agregarán otros tipos de páginas especializadas.

#### Layouts

Los `Layouts` en MauiPdfGenerator se usan para organizar `Views` en estructuras jerárquicas. Cada `Layout` normalmente contiene varios hijos, que pueden ser otras `Views` o `Layouts` anidados.

|Layout|Descripción|
|---|---|
|PdfGrid|Coloca sus `Views` hijas en una cuadrícula de filas y columnas.|
|PdfHorizontalStackLayout|Coloca las `Views` hijas en una pila horizontal.|
|PdfVerticalStackLayout|Coloca las `Views` hijas en una pila vertical.|

#### Views

Las `Views` de MauiPdfGenerator son los componentes que renderizan contenido específico en el documento PDF, como párrafos, imágenes y figuras geométricas.

|View|Descripción|
|---|---|
|PdfImage|Renderiza una imagen que se puede cargar desde un archivo, URI o secuencia.|
|PdfParagraph|Renderiza texto plano y enriquecido de una o varias líneas.|
|PdfHorizontalLine|Renderiza una línea horizontal, usada comúnmente como separador.|

> **NOTA sobre Colores:** Todas las propiedades que aceptan un color (ej. `TextColor`, `BackgroundColor`) utilizan el tipo `Microsoft.Maui.Graphics.Color`. Esto permite a los desarrolladores usar las mismas constantes (`Colors.Blue`) y estructuras que ya utilizan en sus aplicaciones.

### 1.2. Consideraciones de Diseño
- Los `Layouts` definen la estructura espacial del contenido.
- Las `Views` proporcionan el contenido visual específico.
- Las `Pages` contienen la composición completa de una página del PDF.
- La interactividad se omite intencionalmente; el enfoque es en documentos estáticos.

### 1.3. Principio de Garantía de Completitud

La biblioteca se rige por el principio de **Garantía de Completitud**, que dicta que el desarrollador nunca debe estar obligado a especificar cada detalle para obtener un resultado funcional. El objetivo es permitir la creación de documentos con el mínimo código posible, confiando en que la biblioteca aplicará valores predeterminados sensibles y estéticamente agradables.

> **NOTA CRÍTICA DE DEPENDENCIA:** MauiPdfGenerator depende de `MauiPdfGenerator.SourceGenerators` para generar la clase estática `PdfFonts`. Después de configurar las fuentes en `MauiProgram.cs` mediante `PdfConfigureFonts()`, **se debe compilar el proyecto** para que el Source Generator genere automáticamente las propiedades estáticas de tipo `PdfFontIdentifier`, haciendo disponibles referencias como `PdfFonts.OpenSansRegular`.

#### ¿Qué es la Garantía de Completitud?

Es la promesa arquitectónica de que cada componente (documento, página, layout o vista) es completamente funcional desde el momento de su creación, sin necesidad de configuración adicional obligatoria. Un desarrollador puede crear un documento PDF completo con código mínimo:

```csharp
var doc = pdfDocFactory.CreateDocument();
await doc
    .ContentPage()
    .Content(c => c.Paragraph("Hola mundo"))
    .Build()
    .SaveAsync(targetFilePath);
```

En este ejemplo, la biblioteca automáticamente aplica valores predeterminados para todas las propiedades no especificadas del documento.

#### ¿Para qué sirve este principio?

1. **Maximizar la Productividad**: Permite crear documentos funcionales con líneas mínimas de código.
2. **Reducir la Curva de Aprendizaje**: El desarrollador puede comenzar a generar PDFs inmediatamente, sin estudiar todas las propiedades disponibles.
3. **Minimizar Errores**: Elimina la posibilidad de crear elementos "vacíos" o "invisibles" por falta de configuración.
4. **Facilitar la Iteración**: Permite al desarrollador centrarse en el contenido y la estructura, refinando el estilo progresivamente.

#### ¿Cómo y dónde se implementa?

La implementación de este principio ocurre exclusivamente en la **Capa `Fluent`** de la arquitectura. Esta capa actúa como un interceptor inteligente que:

1. **Detecta Propiedades No Especificadas**: Cuando el desarrollador crea un elemento sin definir ciertas propiedades (ej. `FontSize`, `TextColor`).
2. **Aplica Valores Predeterminados Inteligentes**: Consulta una jerarquía de valores predeterminados (locales del componente → globales del documento → valores fijos de la biblioteca).
3. **Garantiza Completitud Antes del Procesamiento**: Antes de pasar los datos al motor de renderizado, asegura que todos los DTOs estén completamente poblados.

La jerarquía de resolución de valores predeterminados sigue este orden de prioridad:
1. **Valor Explícito**: Si el desarrollador especificó un valor, se usa ese valor.
2. **Valor Global del Documento**: Si se configuró un valor predeterminado en `IPdfDocumentConfigurator`.
3. **Valor Fijo de la Biblioteca**: Como último recurso, se usa un valor codificado que garantiza funcionalidad.

#### Valores Predeterminados del Documento

Cuando se crea un documento sin configuración explícita, se aplican estos valores predeterminados globales:

- **`PageSize`**: `PageSizeType.Letter` - Formato estándar en América.
- **`PageOrientation`**: `PageOrientationType.Portrait` - Orientación vertical.
- **`Margins`**: `DefaultMarginType.Normal` - Márgenes equilibrados.
- **`MetaData`**: `null` - Sin metadatos (comportamiento provisional).
- **`FontRegistry`**: Registro automático de fuentes configuradas en `MauiProgram.cs`.

#### Sistema Automático de Registro de Fuentes

Las fuentes configuradas en `MauiProgram.cs` con `FontDestinationType.Both` o `FontDestinationType.OnlyPDF` se registran automáticamente en el documento con el método `Default()`, haciéndolas disponibles pero no embebidas. Para embeber fuentes específicas, se utiliza `ConfigureFontRegistry()`.

### 1.4. Aplicación Práctica

Al diseñar un PDF:

1.  **Planifica las `Pages`**: Define el número y tipo de páginas.
2.  **Diseña los `Layouts`**: Estructura la organización visual en cada página.
3.  **Selecciona las `Views`**: Elige los componentes visuales para el contenido.
4.  **Compón la jerarquía**: Organiza la estructura completa del documento.

---

# Parte II: Diseño y Arquitectura de la Solución

## 1. Arquitectura de Capas y Flujo de Datos

La arquitectura se basa en una clara **Separación de Capas (SoC)**.

### 1.1. Capa `Fluent` (API Pública)

*   **Propósito:** Única puerta de entrada para el desarrollador. Su misión es ofrecer una experiencia declarativa, legible y fácil de usar.
*   **Responsabilidades:**
    *   **API Guiada:** Utiliza el patrón Type-State para prevenir errores en tiempo de compilación.
    *   **Fluidez Contextual:** Métodos encadenables que exponen solo opciones válidas.
    *   **Encapsulación de Complejidad:** Oculta completamente la implementación interna.
    *   **Garantía de Completitud:** Es responsable de aplicar valores predeterminados sensibles a todas las propiedades opcionales. Cuando el desarrollador no especifica un valor (ej. `FontSize`), la capa `Fluent` asegura que el DTO correspondiente se cree con un valor predeterminado válido, evitando que el motor reciba datos incompletos.

### 1.2. Capa `Core` (Motor de Layout y Renderizado)

Sigue el **Principio de Inversión de Dependencias**.

#### Subcapa `Core.Integration` (Abstracciones)
*   **Propósito:** Contiene la lógica de layout independiente del motor de renderizado.
*   **Responsabilidades:** Lógica de `MeasureAsync` y `ArrangeAsync`, contratos de medición (`ILayoutMetrics`), orquestación de pasadas y algoritmos de contenedores.

#### Subcapa `Core.Implementation.Sk` (Implementación Concreta)
*   **Propósito:** Implementa el renderizado usando SkiaSharp. Es la **primera y única implementación concreta** para la v1.0, pero la arquitectura permite que sea intercambiable.
*   **Responsabilidades:** Lógica de `RenderAsync`, implementación de `ILayoutMetrics` y gestión de recursos de SkiaSharp.

### 1.3. Capa `Common` (Contratos Compartidos)

*   **Propósito:** Define el "lenguaje común" entre capas.
*   **Contenido Principal:** DTOs (`PdfDocumentData`, `PdfPageData`), Value Objects, Enumeraciones, e interfaces de comunicación como `ILayoutMetrics`.

### 1.4. Flujo de Datos y Comunicación Entre Capas

1.  **`Fluent` -> `Common`:** La API `Fluent` mapea sus objetos de construcción a un árbol de DTOs puros en `Common`, aplicando valores predeterminados.
2.  **`Common` -> `Core.Integration`:** El orquestador del `Core` recibe los DTOs y crea los `IElementRenderer` correspondientes.
3.  **`Core.Integration` <-> `Core.Implementation`:** Durante `MeasureAsync`, la lógica de layout abstracta consulta las métricas de la implementación concreta a través de `ILayoutMetrics`.
4.  **`Core.Integration` -> `Core.Implementation`:** El "plano de layout" final se pasa a la fase `RenderAsync` de la implementación para el dibujado final.

## 2. Diseño del Motor de Layout

El motor emula deliberadamente el ciclo de Medición y Disposición (Measure/Arrange) de .NET MAUI, adaptándolo a un contexto de generación de documentos asíncrono y separando explícitamente el renderizado.

### 2.1. El Sistema de Tres Pasadas (Measure/Arrange/Render)

*   **Fase 1: La Pasada de Medición (`MeasureAsync`)**
    *   **Responsabilidad:** Calcular el tamaño que cada `View` *desea* tener (`DesiredSize`) basándose en su contenido y las restricciones recibidas de su `Layout` padre.
    *   **Ubicación Arquitectónica:** Lógica principal en `Core.Integration` (capa de abstracciones).

*   **Fase 2: La Pasada de Disposición (`ArrangeAsync`)**
    *   **Responsabilidad:** Asignar una posición (`X`, `Y`) y un tamaño final (`Width`, `Height`) a cada `View` dentro del espacio asignado por su `Layout` padre.
    *   **Ubicación Arquitectónica:** Lógica principal en `Core.Integration`.

*   **Fase 3: La Pasada de Renderizado (`RenderAsync`)**
    *   **Responsabilidad:** Dibujar cada `View` en su posición final usando las APIs específicas del motor de renderizado.
    *   **Ubicación Arquitectónica:** Lógica principal en `Core.Implementation.Sk` (capa de implementación).

### 2.2. Principios y Reglas Fundamentales de Layout

*   **El Principio de Propagación de Restricciones:** Heredado directamente de MAUI: una `View` **NUNCA** asume su tamaño. Siempre opera dentro de un espacio finito (`LayoutRequest`) definido por su `Layout` padre. La única excepción es la `Page` raíz, cuyo espacio inicial es el tamaño de la página menos sus márgenes.

*   **La Dualidad de la Medición:** Una `View` debe ser capaz de responder a dos tipos de "preguntas de medición":
    *   **Pregunta de Medición Restringida (`LayoutPassType.Constrained`):**
        *   **Intención:** "Tienes este **ancho finito**. Adáptate a él (aplicando saltos de línea si es necesario) y dime qué altura necesitas".
        *   **Quién la hace:** `Layouts` de naturaleza vertical como `PdfContentPage` y `PdfVerticalStackLayout`.
    *   **Pregunta de Medición Ideal (`LayoutPassType.Ideal`):**
        *   **Intención:** "Ignora las restricciones de ancho. Dime cuál es tu tamaño **natural** si pudieras usar todo el espacio que necesites (sin saltos de línea)".
        *   **Quién la hace:** `Layouts` de naturaleza horizontal como `PdfHorizontalStackLayout`.

*   **El Modelo de Caja (Box Model):** Idéntico al modelo de MAUI y fundamental para el posicionamiento:
    *   **`Margin` (Margen):** Espacio **externo** y transparente que empuja la caja lejos de sus vecinos. No forma parte del `BackgroundColor`. Es gestionado exclusivamente por el `Layout` padre durante la pasada de `ArrangeAsync`.
    *   **`Padding` (Relleno):** Espacio **interno** que empuja el contenido lejos del borde. El `BackgroundColor` **sí** se dibuja en esta área.
    *   **Implementación del Fondo (`BackgroundColor`):** Es fundamental entender que el `BackgroundColor` no es una propiedad del lienzo. Internamente, cuando una `View` tiene un `BackgroundColor` definido, el motor de renderizado primero dibuja una `View` de tipo `PdfRectangle` (una forma) en la posición y tamaño del elemento. Inmediatamente después, dibuja el contenido real de la `View` (texto, imagen, etc.) encima de ese rectángulo. El rectángulo de fondo abarca el área del `Contenido + Padding`, lo que explica visualmente por qué el `Padding` es un espacio interno afectado por el color de fondo, mientras que el `Margin` permanece como un espacio externo transparente.

*   **Contexto de Layout y Orquestación de Fases:** Un objeto `LayoutContext` se propaga recursivamente por el árbol para comunicar información del padre a los hijos. Un orquestador centralizado en `Core.Integration` es responsable de invocar la secuencia de pasadas (`MeasureAsync`, `ArrangeAsync`, `RenderAsync`) en el orden correcto para todo el árbol.

## 3. Diseño del Sistema de Paginación

Esta es una de las características más potentes y complejas del motor.

*   **Orquestación Iterativa:** El orquestador de layout no procesa el árbol una sola vez. Realiza un ciclo de `Measure`/`Arrange` para el contenido de una página. Si durante la medición detecta que el contenido excede el espacio vertical disponible, crea una nueva página y reinicia el ciclo de layout con el contenido restante.

*   **Elementos Atómicos vs. Divisibles:** La política de paginación depende del tipo de `View`:
    *   **Atómicos:** `PdfImage`, `PdfHorizontalLine`, `PdfVerticalStackLayout`, `PdfHorizontalStackLayout`. Si una `View` atómica no cabe en el espacio restante de una página, se mueve **completa** a la página siguiente. Nunca se divide.
    *   **Divisibles:** `PdfParagraph` y `PdfGrid`. Estos son los únicos elementos que la biblioteca puede dividir a través de un salto de página.
        *   **División de `PdfParagraph`:** El `TextRenderer` calcula cuántas líneas de texto caben en el espacio disponible. Si no caben todas, renderiza las que sí caben y pasa el texto sobrante al orquestador para que lo coloque en la página siguiente.
        *   **División de `PdfGrid`:** El `GridRenderer` mide sus filas secuencialmente. Si al añadir una fila se excede el alto de la página, la división ocurre **entre la fila anterior y la actual**. La fila que no cabe, junto con todas las siguientes, se mueven a la página siguiente. La división nunca ocurre a mitad de una celda.

## 4. Diseño del Sistema de Estilos y Recursos

*   **El Principio de Herencia de Propiedades:** La biblioteca implementa un sistema de herencia de dos niveles: **Local** y **Global**. Cuando se resuelve una propiedad de estilo (como la fuente o el color), el valor especificado directamente en la `View` (Local) siempre tiene prioridad. Si no existe, se utiliza el valor definido en la configuración global del documento (Global), establecida a través de `IPdfDocumentConfigurator`.

*   **Unidades y Medidas:** Para mantener la coherencia con .NET MAUI, la biblioteca utiliza **unidades independientes del dispositivo** para todas las propiedades de tamaño y espaciado (`WidthRequest`, `FontSize`, `Margin`, etc.). El desarrollador trabaja con las unidades que ya conoce. Internamente, la biblioteca convierte de forma automática y transparente estas unidades a **puntos (points)**, el estándar en PDF (72 puntos por pulgada).

## 5. Diseño del Sistema de Fuentes

La gestión de fuentes es un aspecto crítico. La arquitectura lo aborda con dos características clave:

### 5.1. Dependencia Arquitectónica Crítica

**MauiPdfGenerator depende obligatoriamente de `MauiPdfGenerator.SourceGenerators`** para el sistema de fuentes. Esta dependencia es fundamental para la funcionalidad de identificación de fuentes con seguridad de tipos.

### 5.2. Generador de Código para Seguridad de Tipos

La biblioteca incluye un **generador de código fuente** que inspecciona las llamadas a `AddFont()` en `MauiProgram.cs`. Por cada fuente registrada, crea automáticamente una clase estática `public static class PdfFonts` con propiedades estáticas de tipo `PdfFontIdentifier`. Este diseño arquitectónico elimina el uso de "magic strings" (ej. `"OpenSans-Regular"`) y lo reemplaza por un acceso seguro en tiempo de compilación (ej. `PdfFonts.OpenSans_Regular`), proporcionando IntelliSense y evitando errores de tipeo.

#### Proceso de Generación Automática

1. **Configuración:** El desarrollador configura fuentes en `MauiProgram.cs` usando `PdfConfigureFonts()`
2. **Compilación:** Al compilar el proyecto, el Source Generator analiza la configuración
3. **Generación:** Se crea automáticamente la clase `PdfFonts` con propiedades estáticas
4. **Disponibilidad:** Las propiedades están disponibles para uso con IntelliSense

#### Ejemplo de Código Generado

Para una configuración como:
```csharp
fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
fonts.AddFont("comic.ttf", "Comic");
```

El Source Generator produce:
```csharp
public static class PdfFonts
{
    public static global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier OpenSansRegular { get; } = 
        new global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier("OpenSansRegular");
    public static global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier Comic { get; } = 
        new global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier("Comic");
}
```

### 5.3. Registro y Configuración Avanzada

El generador se alimenta a través del método de extensión `PdfConfigureFonts()`. Este método permite especificar el propósito de cada fuente a través de la enumeración `FontDestinationType`.

| Valor de `FontDestinationType` | Descripción |
| :--- | :--- |
| `OnlyUI` | La fuente solo estará disponible para los controles de la UI de .NET MAUI. |
| `OnlyPDF` | La fuente solo estará disponible para la generación de documentos PDF. Ideal para fuentes pesadas o con licencias específicas. |
| `Both` | La fuente se registra tanto en la UI como en el motor de PDF. |

> **NOTA:** Si se omite, la biblioteca asume de forma predeterminada el valor `FontDestinationType.Both`, siguiendo una filosofía de "valores predeterminados inteligentes" para mantener el código de configuración limpio y conciso.

## 6. Implementación de Valores Predeterminados Inteligentes por Componente

En línea con el Principio de Garantía de Completitud establecido en la Parte I, cada componente de la biblioteca posee un conjunto cuidadosamente seleccionado de valores predeterminados que garantizan funcionalidad inmediata y resultados estéticamente agradables.

### 6.1. Documento PDF (`PdfDocument`)

#### Valores Predeterminados del Documento
- **`PageSize`**: `PageSizeType.Letter` - Formato estándar en América, ampliamente utilizado.
- **`PageOrientation`**: `PageOrientationType.Portrait` - Orientación vertical estándar para documentos.
- **`Margins`**: `DefaultMarginType.Normal` - Márgenes equilibrados que proporcionan espacio de lectura cómodo.
- **`FontFamily`**: `Helvetica` - Fuente sans-serif legible y ampliamente soportada en PDF.
- **`FontSize`**: `12pt` - Tamaño estándar para texto de documento.
- **`TextColor`**: `Colors.Black` - Color de texto tradicional para máxima legibilidad.
- **`MetaData`**: `null` - Sin metadatos por defecto (comportamiento provisional, se analizarán valores predeterminados en futuras versiones).

#### Sistema Automático de Registro de Fuentes

Las fuentes configuradas en `MauiProgram.cs` mediante `PdfConfigureFonts()` con `FontDestinationType.Both` o `FontDestinationType.OnlyPDF` se registran automáticamente en el documento utilizando el método `Default()`. Esto significa que:

1. **Registro Automático**: Estas fuentes están disponibles para uso inmediato en el documento.
2. **No Embebidas por Defecto**: Las fuentes registradas automáticamente no se embeben en el PDF.
3. **Configuración Adicional**: Para embeber fuentes específicas, se debe utilizar `ConfigureFontRegistry()` explícitamente.

#### Propósito de ConfigureFontRegistry

El método `ConfigureFontRegistry()` es específicamente para configurar el embebido de fuentes:

```csharp
.Configuration(cfg =>
{
    cfg.ConfigureFontRegistry(cfr =>
    {
        cfr.Font(PdfFonts.Comic).EmbeddedFont();
    });
})
```

#### Justificación Arquitectónica
Estos valores fueron seleccionados basándose en estándares de la industria editorial y garantizan que cualquier documento generado sea inmediatamente legible y profesional sin configuración adicional.

### 6.2. Páginas (`PdfContentPage`)

#### Valores Predeterminados de Página
- **`Padding`**: `new Thickness(0)` - Sin relleno interno por defecto, permitiendo que el contenido use todo el espacio disponible.
- **`BackgroundColor`**: `Colors.Transparent` - Fondo transparente que respeta el color del papel PDF.

#### Justificación Arquitectónica
Las páginas actúan como contenedores neutros. Un padding de cero maximiza el espacio utilizable, mientras que el fondo transparente evita interferir con el diseño del contenido.

### 6.3. Layouts

#### PdfVerticalStackLayout - Valores Predeterminados
- **`Spacing`**: `0` - Sin espaciado automático, permitiendo control granular del desarrollador.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa todo el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Fill` - Se adapta al contenido verticalmente.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### PdfHorizontalStackLayout - Valores Predeterminados
- **`Spacing`**: `0` - Consistente con el comportamiento vertical.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa todo el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Fill` - Se adapta al contenido verticalmente.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### PdfGrid - Valores Predeterminados
- **`RowSpacing`**: `0` - Sin espaciado entre filas por defecto.
- **`ColumnSpacing`**: `0` - Sin espaciado entre columnas por defecto.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa todo el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Fill` - Se adapta al contenido verticalmente.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### Justificación Arquitectónica para Layouts
Los layouts adoptaron una filosofía de "espaciado cero por defecto" para evitar espacios no deseados que puedan interferir con el diseño preciso. El desarrollador puede añadir espaciado explícitamente cuando sea necesario, manteniendo control total sobre la apariencia.

### 6.4. Views

#### PdfParagraph - Valores Predeterminados
- **`FontFamily`**: Hereda del documento (por defecto `Helvetica`)
- **`FontSize`**: Hereda del documento (por defecto `12pt`)
- **`FontAttributes`**: `FontAttributes.None` - Texto normal sin negrita ni cursiva.
- **`TextColor`**: Hereda del documento (por defecto `Colors.Black`)
- **`BackgroundColor`**: `Colors.Transparent` - Sin fondo por defecto.
- **`HorizontalTextAlignment`**: `TextAlignment.Start` - Alineación a la izquierda para idiomas LTR.
- **`VerticalTextAlignment`**: `TextAlignment.Start` - Alineación superior.
- **`LineBreakMode`**: `LineBreakMode.WordWrap` - Ajuste de línea inteligente por palabras.
- **`LineHeight`**: `1.0` - Altura de línea estándar.
- **`CharacterSpacing`**: `0` - Sin espaciado adicional entre caracteres.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.
- **`MaxLines`**: `int.MaxValue` - Sin límite de líneas.
- **`TextDecorations`**: `TextDecorations.None` - Sin subrayado ni tachado.
- **`TextTransform`**: `TextTransform.None` - Sin transformación de texto.

#### PdfImage - Valores Predeterminados
- **`Aspect`**: `Aspect.AspectFit` - Mantiene proporciones de la imagen original.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Fill` - Se adapta al contenido verticalmente.
- **`BackgroundColor`**: `Colors.Transparent` - Sin fondo por defecto.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### PdfHorizontalLine - Valores Predeterminados
- **`Color`**: `Colors.Black` - Color de línea estándar.
- **`Thickness`**: `1.0` - Grosor de línea estándar.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa todo el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Center` - Se centra verticalmente.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### Justificación Arquitectónica para Views
Los valores predeterminados de las Views priorizan la legibilidad y la funcionalidad inmediata. Las fuentes y colores heredan de la configuración global del documento para mantener consistencia visual, mientras que las propiedades de espaciado inician en cero para evitar layouts no deseados.

---

# Parte III: Guía de Implementación y Referencia de API

## 1. Guía de Inicio Rápido: Configuración y Primer Documento

### 1.1. Integración con .NET MAUI: El Punto de Entrada

La biblioteca se adhiere a los patrones modernos de .NET, integrándose de forma nativa en el ecosistema de la aplicación. La inicialización se realiza en el fichero `MauiProgram.cs` mediante **dos métodos de extensión obligatorios**: `UseMauiPdfGenerator()` y `PdfConfigureFonts()`. 

**`UseMauiPdfGenerator()`** registra en el contenedor de inyección de dependencias (DI) todos los servicios necesarios, principalmente la interfaz `IPdfDocumentFactory`. 

**`PdfConfigureFonts()`** es **obligatorio** para el registro de fuentes y permite que el Source Generator cree la clase estática `PdfFonts`. **Después de configurar las fuentes, se debe compilar el proyecto** para que `MauiPdfGenerator.SourceGenerators` genere automáticamente las propiedades estáticas de tipo `PdfFontIdentifier`.

Este enfoque garantiza que la creación de PDFs sea una capacidad intrínseca de la aplicación, accesible desde cualquier parte de la misma de una manera limpia, desacoplada y testeable.

#### Flujo de Desarrollo con Source Generators

1. **Configuración Inicial:** Agregar `UseMauiPdfGenerator()` y `PdfConfigureFonts()` en `MauiProgram.cs`
2. **Compilación Requerida:** Compilar el proyecto para generar la clase `PdfFonts`
3. **Disponibilidad:** Las propiedades como `PdfFonts.OpenSansRegular` están disponibles con IntelliSense
4. **Uso:** Utilizar las fuentes generadas en el código de creación de PDFs

> **NOTA CRÍTICA:** Sin la compilación posterior a `PdfConfigureFonts()`, la clase `PdfFonts` no estará disponible y el código que la reference producirá errores de compilación.

*Ejemplo de configuración en MauiProgram.cs*

```csharp
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Maui.Controls;
using MauiPdfGenerator;

namespace Test;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiPdfGenerator() // 1. Registrar la biblioteca
            .PdfConfigureFonts(fonts => // 2. Configurar fuentes (Obligatorio para Source Generator)
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("comic.ttf", "Comic");
            });

        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
```

### 1.2. La Fábrica de Documentos: Creación mediante DI

Tras la configuración, la creación de un documento se realiza solicitando la interfaz `IPdfDocumentFactory` al contenedor de DI. Esta fábrica expone el método `CreateDocument()`, que devuelve una instancia de `IPdfDocument`, el punto de partida para construir el PDF. 

### 1.3. Configuración Global del Documento

Cada instancia de `IPdfDocument` ofrece el método `Configuration(Action<IPdfDocumentConfigurator> documentConfigurator)` para definir las características globales del documento. **Esta configuración es completamente opcional** gracias al Principio de Garantía de Completitud.

#### Ejemplo de Uso Mínimo (Sin Configuración)

```csharp
var doc = pdfDocFactory.CreateDocument();
await doc
    .ContentPage()
    .Content(c => c.Paragraph("Hola mundo"))
    .Build()
    .SaveAsync(targetFilePath);
```

#### Ejemplo de Uso con Configuración

```csharp
var doc = pdfDocFactory.CreateDocument();
await doc
    .Configuration(cfg =>
    {
        cfg.PageSize(PageSizeType.A4);
        cfg.PageOrientation(PageOrientationType.Landscape);
        cfg.Margins(DefaultMarginType.Wide);
    })
    .ContentPage()
    .Content(c => c.Paragraph("Hola mundo"))
    .Build()
    .SaveAsync(targetFilePath);
```

#### Métodos de Configuración Disponibles

| Método en `IPdfDocumentConfigurator` | Descripción |
| :--- | :--- |
| `.PageSize(PageSizeType size)` | Establece el tamaño de página por defecto para todo el documento. |
| `.PageOrientation(PageOrientationType orientation)` | Define la orientación por defecto (Vertical/Apaisada). |
| `.Margins(DefaultMarginType marginType)` | Aplica un conjunto de márgenes predefinidos (`Normal`, `Estrecho`, etc.). |
| `.Margins(float uniform)` | Aplica un margen uniforme a los cuatro lados. |
| `.MetaData(...)` | Accede al constructor de metadatos del PDF. |
| `.ConfigureFontRegistry(...)` | Accede a la configuración avanzada de fuentes (`IPdfFontRegistry`). |

#### Configuración de Registro de Fuentes

Una vez registrada una fuente en `MauiProgram.cs`, se puede refinar su comportamiento a través de la interfaz `IPdfFontRegistry`, accesible desde la configuración del documento.

| Método en `IFontRegistrationOptions` | Descripción |
| :--- | :--- |
| `.Default()` | Designa esta fuente como la predeterminada para todo el documento. |
| `.EmbeddedFont()` | Marca la fuente para ser incrustada en el fichero PDF, garantizando su correcta visualización. |

#### Propósito Específico de ConfigureFontRegistry

El método `ConfigureFontRegistry()` es específicamente para configurar el **embebido de fuentes**, no su disponibilidad (que es automática). Las fuentes configuradas en `MauiProgram.cs` con `FontDestinationType.Both` o `FontDestinationType.OnlyPDF` ya están disponibles automáticamente.

```csharp
.Configuration(cfg =>
{
    cfg.ConfigureFontRegistry(cfr =>
    {
        cfr.Font(PdfFonts.Comic).EmbeddedFont();
        cfr.Font(PdfFonts.OpenSansRegular).Default(); // Opcional, ya es automático
    });
})
```

### 1.4. Enriquecimiento con Metadatos

Dentro de la configuración global, el método `.MetaData(Action<IPdfMetaData> metaDataAction)` permite establecer los metadatos del PDF.

| Método en `IPdfMetaData` | Descripción |
| :--- | :--- |
| `.Title(string title)` | Define el título del documento. |
| `.Author(string author)` | Define el autor del documento. |
| `.Subject(string subject)` | Define el asunto. |
| `.Keywords(string keywords)` | Define las palabras clave. |
| `.CustomProperty(string name, string value)` | Añade un metadato personalizado. |

### 1.5. Construcción de Contenido de Página

Una vez configurado el documento, se añade una página con el método `.ContentPage()`, que devuelve un objeto `IPdfContentPage`. El paso final es llamar al método `.Content(Action<IPageContentBuilder> contentSetup)`. Este método pasa el control a un constructor de contenido (`IPageContentBuilder`) que es la caja de herramientas para añadir `Views` y `Layouts`.

| Método en `IPageContentBuilder` | Descripción |
| :--- | :--- |
| `.Paragraph(string text)` | Añade una `View` de texto. |
| `.PdfImage(Stream stream)` | Añade una `View` de imagen desde un `Stream`. |
| `.HorizontalLine()` | Añade una `View` de línea horizontal. |
| `.VerticalStackLayout(...)` | Añade un `Layout` de pila vertical y proporciona un constructor para su contenido. |
| `.HorizontalStackLayout(...)` | Añade un `Layout` de pila horizontal y proporciona un constructor para su contenido. |
| `.PdfGrid()` | Añade un `Layout` de rejilla configurable. |

## 2. Referencia de Componentes

### 2.1. Pages

#### PdfContentPage
La `PdfContentPage` es el tipo de `Page` más simple y común. Su propósito es mostrar un único `Layout` hijo, que a su vez contiene otras `Views`.

> **NOTA:** Los valores predeterminados se aplican automáticamente según el Principio de Garantía de Completitud: `Padding` inicia en cero, `BackgroundColor` es transparente, y las características del documento (tamaño Letter, orientación Portrait, márgenes Normal) se heredan automáticamente.

##### Creación y Uso Práctico
Se crea una instancia de página llamando al método `.ContentPage()` en un objeto `IPdfDocument`. Esto devuelve una interfaz `IPdfContentPage` que permite configurar propiedades específicas de la página (como `BackgroundColor` o `Spacing`) y definir su contenido.

##### Propiedades Principales
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Content` | `PdfLayout` | Define el `Layout` único que representa el contenido de la página. |
| `Padding` | `Microsoft.Maui.Thickness` | Define el espacio interior entre los bordes de la página y su contenido. |

### 2.2. Layouts (Contenedores)

Un `Layout` se utiliza para componer las `Views` en una estructura visual. Las clases de `Layout` en MauiPdfGenerator derivan de la clase `PdfLayout`.

#### PdfVerticalStackLayout
El `PdfVerticalStackLayout` organiza sus `Views` hijas en una única columna vertical.

> **NOTA:** Los valores predeterminados incluyen `Spacing` de 0, `Padding` y `Margin` en cero, y `HorizontalOptions` configurado en `Fill` para ocupar todo el ancho disponible.

##### Creación y Uso Práctico
Se instancia a través del método `.VerticalStackLayout(Action<IStackLayoutBuilder> content)` en un constructor de contenido. El patrón de `Action` proporciona un nuevo constructor anidado (`IStackLayoutBuilder`) para definir los elementos hijos dentro del `Layout`.

##### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Spacing` | `double` | Define el espacio entre cada `View` hija. El valor predeterminado es 0. |

> **Comportamiento de Paginación:** Este `Layout` es **atómico**. Si no cabe en el espacio restante de la página actual, la biblioteca lo moverá completo a la siguiente página.

#### PdfHorizontalStackLayout
El `PdfHorizontalStackLayout` organiza sus `Views` hijas en una única fila horizontal.

> **NOTA:** Comparte los mismos valores predeterminados que `PdfVerticalStackLayout` para mantener consistencia en el comportamiento de los layouts.

##### Creación y Uso Práctico
Se instancia a través del método `.HorizontalStackLayout(Action<IStackLayoutBuilder> content)` en un constructor de contenido, siguiendo el mismo patrón que el `PdfVerticalStackLayout`.

##### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Spacing` | `double` | Define el espacio entre cada `View` hija. El valor predeterminado es 0. |

> **Comportamiento de Paginación:** Este `Layout` es **atómico**.

*Ejemplo de Uso de StackLayouts:*

```csharp
c.VerticalStackLayout(vsl => 
{
    vsl.Spacing(10);
    vsl.Paragraph("Elemento 1 en VerticalStack");
    
    vsl.HorizontalStackLayout(hsl =>
    {
        hsl.Spacing(8);
        hsl.BackgroundColor(Colors.LightGoldenrodYellow);
        hsl.Paragraph("Texto anidado 1");
        hsl.Paragraph("Texto anidado 2");
    });
});
```

#### PdfGrid
El `PdfGrid` es un `Layout` potente para mostrar `Views` en filas y columnas.

> **NOTA:** Los valores predeterminados incluyen `RowSpacing` y `ColumnSpacing` de 0, permitiendo diseños precisos sin espaciado no deseado.

##### Creación y Uso Práctico
Se instancia con el método `.PdfGrid()` en un constructor de contenido. La configuración de filas, columnas y la adición de hijos se realiza mediante una API fluida directamente sobre el objeto `PdfGrid` devuelto.

##### Posicionamiento y Expansión de Vistas:
Las `Views` se colocan en celdas específicas utilizando las propiedades adjuntas `.Row(int)` y `.Column(int)`. Para que una `View` ocupe múltiples filas o columnas, se utilizan `.RowSpan(int)` y `.ColumnSpan(int)`.

##### Definición de Tamaño:
El tamaño de las filas y columnas se controla a través de `RowDefinitions` y `ColumnDefinitions`, usando `GridLength` con valores `Auto`, un valor numérico explícito, o un valor proporcional (`Star`).

##### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `RowDefinitions` | `RowDefinitionCollection` | La colección de objetos `RowDefinition` que definen las filas. |
| `ColumnDefinitions` | `ColumnDefinitionCollection` | La colección de objetos `ColumnDefinition` que definen las columnas. |
| `RowSpacing` | `double` | El espacio vertical entre las filas del grid. |
| `ColumnSpacing` | `double` | El espacio horizontal entre las columnas del grid. |

> **Comportamiento de Paginación:** Este `Layout` es **divisible**. Si su contenido excede el espacio disponible en la página actual, la biblioteca lo dividirá automáticamente, continuando las filas restantes en la siguiente página. La división siempre ocurre entre filas.

*Ejemplo de Uso de PdfGrid:*

```csharp
c.PdfGrid()
    .BackgroundColor(Colors.LightSlateGray)
    .RowDefinitions(rd =>
    {
        rd.GridLength(42);
        rd.GridLength(GridUnitType.Auto);
        rd.GridLength(GridUnitType.Star);
    })
    .ColumnDefinitions(cd =>
    {
        cd.GridLength(GridUnitType.Star);
        cd.GridLength(100);
    })
    .Children(ch =>
    {
        ch.Paragraph("Celda (0,0)")
            .Row(0).Column(0);
        ch.Paragraph("Celda (0,1)")
            .Row(0).Column(1);
        ch.Paragraph("Esta celda ocupa dos columnas")
            .Row(1).Column(0).ColumnSpan(2)
            .HorizontalTextAlignment(TextAlignment.Center)
            .BackgroundColor(Colors.LightYellow);
    });
```

### 2.3. Views (Elementos Visuales)

#### PdfParagraph
Un `PdfParagraph` muestra texto de una sola línea y de varias líneas.

> **NOTA:** Los valores predeterminados garantizan texto legible inmediatamente: fuente Helvetica a 12pt, color negro, alineación a la izquierda y ajuste de línea por palabras. Las propiedades de fuente y color se heredan de la configuración global del documento.

> **Comportamiento de Paginación:** Esta `View` es **divisible**. Si su contenido excede el espacio disponible, será partido y continuará en la página siguiente.

##### Propiedades:
- `CharacterSpacing`: `double`, establece el espaciado entre caracteres.
- `FontAttributes`: `Microsoft.Maui.Controls.FontAttributes`, determina el estilo del texto (negrita, cursiva).
- `FontFamily`: `PdfFontIdentifier`, define la familia de fuentes. Se debe utilizar la clase estática `PdfFonts` generada automáticamente.
- `FontSize`: `double`, define el tamaño de la fuente.
- `FormattedText`: `FormattedString`, permite texto con múltiples estilos usando `PdfSpan`.
- `HorizontalTextAlignment`: `Microsoft.Maui.TextAlignment`, define la alineación horizontal.
- `LineBreakMode`: `Microsoft.Maui.LineBreakMode`, determina el comportamiento de ajuste y truncamiento.
- `LineHeight`: `double`, especifica un multiplicador para la altura de línea.
- `MaxLines`: `int`, indica el número máximo de líneas.
- `Padding`: `Microsoft.Maui.Thickness`, determina el relleno interno.
- `Text`: `string`, define el contenido de texto.
- `TextColor`: `Microsoft.Maui.Graphics.Color`, define el color del texto.
- `TextDecorations`: `Microsoft.Maui.TextDecorations`, aplica decoraciones como `Underline` y `Strikethrough`.
- `TextTransform`: `Microsoft.Maui.TextTransform`, especifica la transformación a mayúsculas o minúsculas.
- `VerticalTextAlignment`: `Microsoft.Maui.TextAlignment`, define la alineación vertical.

*Ejemplo de Uso de PdfParagraph:*

```csharp
c.Paragraph("[P1] Texto simple con propiedades predeterminadas");

c.Paragraph("[P2] Texto azul, centrado y con un tamaño de fuente mayor.")
    .TextColor(Colors.Blue)
    .HorizontalTextAlignment(TextAlignment.Center)
    .FontSize(16);

c.Paragraph("[P3] Este párrafo tiene un fondo, padding y margin.")
    .Padding(10, 20)
    .Margin(0, 10)
    .BackgroundColor(Colors.LightCoral);

c.Paragraph("[P4] Estilo Completo: Subrayado, Negrita, Itálica y fuente Comic.")
    .FontFamily(PdfFonts.Comic)
    .FontAttributes(FontAttributes.Bold | FontAttributes.Italic)
    .TextDecorations(TextDecorations.Underline);
```

#### PdfImage
Muestra una imagen que se puede cargar desde un archivo local, un URI o una secuencia.

> **NOTA:** Los valores predeterminados incluyen `Aspect.AspectFit` para mantener las proporciones de la imagen, y opciones de layout configuradas en `Fill` para adaptarse al espacio disponible.

> **Comportamiento de Paginación:** Esta `View` es **atómica**.

##### Propiedades:
- `Aspect`: `Microsoft.Maui.Aspect`, define el modo de escalado de la imagen (`AspectFit`, `AspectFill`, `Fill`, `Center`).
- `Source`: `PdfImageSource`, especifica el origen de la imagen.

*Ejemplo de Uso de PdfImage:*

```csharp
// Asumiendo que 'imageData' es un byte[]
c.Paragraph("Imagen con AspectFit y fondo para ver el área");
c.PdfImage(new MemoryStream(imageData))
    .WidthRequest(150)
    .HeightRequest(75)
    .Aspect(Aspect.AspectFit)
    .HorizontalOptions(LayoutAlignment.Center)
    .BackgroundColor(Colors.LightBlue);
```

#### PdfHorizontalLine
Es una `View` pública cuyo propósito es dibujar una línea horizontal, comúnmente usada como separador visual. Internamente, es una implementación especializada de la clase base `PdfShape`.

> **NOTA:** Los valores predeterminados incluyen color negro y grosor de 1.0, con `HorizontalOptions` configurado en `Fill` para ocupar todo el ancho disponible.

> **Comportamiento de Paginación:** Esta `View` es **atómica**.

##### Propiedades:
- `Color`: `Microsoft.Maui.Graphics.Color`, el color de la línea.
- `Thickness`: `double`, el grosor de la línea.

*Ejemplo de Uso de PdfHorizontalLine:*

```csharp
c.Paragraph("Texto sobre la línea.");
c.HorizontalLine()
    .Color(Colors.Green)
    .Thickness(3)
    .Margin(0, 10); // Espacio vertical alrededor de la línea
c.Paragraph("Texto debajo de la línea.");
```

#### PdfShape
Es una clase base que permite dibujar formas en la página. Aunque los desarrolladores pueden derivar de ella para crear formas personalizadas, la biblioteca proporciona `Views` listas para usar, como `PdfHorizontalLine`, que se basan en este sistema. El `BackgroundColor` de todas las `Views` se implementa internamente dibujando un `PdfRectangle` (una `PdfShape`) debajo del contenido.

##### Propiedades de Estilo:
- `Fill`: `Brush`, indica el pincel para pintar el interior de la forma.
- `Stroke`: `Brush`, indica el pincel para pintar el contorno.
- `StrokeThickness`: `double`, indica el ancho del contorno.
- `StrokeDashArray`: `DoubleCollection`, define un patrón de guiones y espacios.
- `StrokeLineCap`: `PenLineCap`, describe la forma al principio y al final de una línea.
- `StrokeLineJoin`: `PenLineJoin`, especifica el tipo de unión en los vértices.
- `Aspect`: `Stretch`, describe cómo la forma llena su espacio asignado.

## 3. Sistema de Layout en la Práctica

### 3.1. Alineación y Posicionamiento

Cada `View` o `Layout` tiene propiedades `HorizontalOptions` y `VerticalOptions` de tipo `Microsoft.Maui.Controls.LayoutAlignment`. Esta estructura determina su posición y tamaño dentro de su `Layout` padre cuando este contiene espacio no utilizado.

#### Opciones de Alineación
- **`Start`**: Alinea a la izquierda o arriba.
- **`Center`**: Centra horizontal o verticalmente.
- **`End`**: Alinea a la derecha o abajo.
- **`Fill`**: Asegura que el elemento llene el espacio disponible.

> **Nota**: El valor predeterminado de `HorizontalOptions` y `VerticalOptions` es `LayoutAlignment.Fill`.

### 3.2. Posicionamiento con Margin y Padding

Las propiedades `Margin` y `Padding`, de tipo `Microsoft.Maui.Thickness`, controlan el espaciado.

- **`Margin`**: Distancia **externa** entre un elemento y sus vecinos.
- **`Padding`**: Distancia **interna** entre el borde de un elemento y su contenido.