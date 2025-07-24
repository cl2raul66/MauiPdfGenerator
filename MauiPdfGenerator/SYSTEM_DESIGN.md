# Documentación Técnica de la Biblioteca MauiPdfGenerator

# Parte I: Guía del Desarrollador: Componentes y Uso

## 1. Introducción a MauiPdfGenerator

### 1.1. Principio Rector

Esta biblioteca está diseñada como una **extensión natural del ecosistema .NET MAUI**, no como una herramienta externa. Su propósito es permitir que los desarrolladores MAUI generen PDFs de forma nativa utilizando conceptos familiares de .NET MAUI. Para aquellos desarrolladores con experiencia en la plataforma, les resultará más cómodo el uso de esta biblioteca ya que su confección es similar a cuando se crea una UI de .NET MAUI, pero orientada a la generación de documentos PDF.

### 1.2. Analogía con MAUI

La transición conceptual se establece así:

- **Pages (MAUI)** → **Páginas PDF**: Cada página representa una unidad completa de contenido del documento.
- **Layouts (MAUI)** → **Estructuras organizacionales PDF**: Organizan el contenido dentro de cada página del documento.
- **Views (MAUI)** → **Elementos visuales PDF**: Componentes que renderizan contenido específico en el documento final.
- **Experiencia de Desarrollo (DX):** Al igual que el MAUI moderno, la biblioteca adopta patrones de desarrollo avanzados como una API fluida (Fluent API), inyección de dependencias y generadores de código fuente para ofrecer una experiencia de desarrollo segura, productiva y que minimiza errores comunes como los 'magic strings'.

Los conceptos de alineación y posicionamiento de MAUI se traducen directamente en la disposición visual del contenido PDF dentro de las estructuras organizacionales de cada página del documento.

### 1.3. Estructura Visual General

La interfaz visual de un documento PDF se construye con elementos que mapean conceptualmente a los controles de MAUI:

**MAUI → PDF (Analogía Visual)**
- **Pages** → Páginas del documento PDF
- **Layouts** → Estructuras de organización visual en PDF
- **Views** → Elementos de contenido visual en PDF

## 2. Guía de Inicio Rápido: Configuración y Primer Documento

### 2.1. Integración con .NET MAUI: El Punto de Entrada

La biblioteca se adhiere a los patrones modernos de .NET, integrándose de forma nativa en el ecosistema de la aplicación. La inicialización se realiza en el fichero `MauiProgram.cs` mediante el método de extensión `UseMauiPdfGenerator()`. Este acto registra en el contenedor de inyección de dependencias (DI) todos los servicios necesarios, principalmente la interfaz `IPdfDocumentFactory`. Este enfoque garantiza que la creación de PDFs sea una capacidad intrínseca de la aplicación, accesible desde cualquier parte de la misma de una manera limpia, desacoplada y testeable.

### 2.2. El Sistema de Fuentes Unificado

La gestión de fuentes es un aspecto crítico. La biblioteca aborda esto con dos características clave:

**1. Generador de Código para Seguridad de Tipos**

La biblioteca incluye un **generador de código fuente** que inspecciona las llamadas a `AddFont()` en `MauiProgram.cs`. Por cada fuente registrada, crea automáticamente una clase estática `public static class PdfFonts` con propiedades estáticas de tipo `PdfFontIdentifier`.

Esto elimina el uso de "magic strings" (ej. `"OpenSans-Regular"`) y lo reemplaza por un acceso seguro en tiempo de compilación (ej. `PdfFonts.OpenSans_Regular`), proporcionando IntelliSense y evitando errores de tipeo.

**2. Registro y Configuración Avanzada**

El generador se alimenta a través del método de extensión `PdfConfigureFonts()`. Este método permite especificar el propósito de cada fuente a través de la enumeración `FontDestinationType`.

| Valor de `FontDestinationType` | Descripción |
| :--- | :--- |
| `OnlyUI` | La fuente solo estará disponible para los controles de la UI de .NET MAUI. |
| `OnlyPDF` | La fuente solo estará disponible para la generación de documentos PDF. Ideal para fuentes pesadas o con licencias específicas. |
| `Both` | La fuente se registra tanto en la UI como en el motor de PDF. |

Una vez registrada, se puede refinar el comportamiento de la fuente a través de la interfaz `IPdfFontRegistry`, accesible desde la configuración del documento.

| Método en `IFontRegistrationOptions` | Descripción |
| :--- | :--- |
| `.Default()` | Designa esta fuente como la predeterminada para todo el documento. |
| `.EmbeddedFont()` | Marca la fuente para ser incrustada en el fichero PDF, garantizando su correcta visualización. |

### 2.3. La Fábrica de Documentos: Creación mediante DI

Tras la configuración, la creación de un documento se realiza solicitando la interfaz `IPdfDocumentFactory` al contenedor de DI. Esta fábrica expone el método `CreateDocument()`, que devuelve una instancia de `IPdfDocument`, el punto de partida para construir el PDF. Este patrón desacopla el código del cliente de la implementación y facilita las pruebas.

### 2.4. Configuración Global del Documento

Cada instancia de `IPdfDocument` ofrece el método `Configuration(Action<IPdfDocumentConfigurator> documentConfigurator)` para definir las características globales del documento. La acción recibe una instancia de `IPdfDocumentConfigurator`.

| Método en `IPdfDocumentConfigurator` | Descripción |
| :--- | :--- |
| `.PageSize(PageSizeType size)` | Establece el tamaño de página por defecto para todo el documento. |
| `.PageOrientation(PageOrientationType orientation)` | Define la orientación por defecto (Vertical/Apaisada). |
| `.Margins(DefaultMarginType marginType)` | Aplica un conjunto de márgenes predefinidos (`Normal`, `Estrecho`, etc.). |
| `.Margins(float uniform)` | Aplica un margen uniforme a los cuatro lados. |
| `.MetaData(...)` | Accede al constructor de metadatos del PDF. |
| `.ConfigureFontRegistry(...)` | Accede a la configuración avanzada de fuentes (`IPdfFontRegistry`). |

### 2.5. Enriquecimiento con Metadatos

Dentro de la configuración global, el método `.MetaData(Action<IPdfMetaData> metaDataAction)` permite establecer los metadatos del PDF. La acción recibe una instancia de `IPdfMetaData`.

| Método en `IPdfMetaData` | Descripción |
| :--- | :--- |
| `.Title(string title)` | Define el título del documento. |
| `.Author(string author)` | Define el autor del documento. |
| `.Subject(string subject)` | Define el asunto. |
| `.Keywords(string keywords)` | Define las palabras clave. |
| `.CustomProperty(string name, string value)` | Añade un metadato personalizado. |

### 2.6. Construcción de Contenido de Página

Una vez configurado el documento, se añade una página con el método `.ContentPage()`, que devuelve un objeto `IPdfContentPage`. El paso final es llamar al método `.Content(Action<IPageContentBuilder> contentSetup)`. Este método pasa el control a un constructor de contenido (`IPageContentBuilder`) que es la caja de herramientas para añadir elementos.

| Método en `IPageContentBuilder` | Descripción |
| :--- | :--- |
| `.Paragraph(string text)` | Añade un elemento de texto. |
| `.PdfImage(Stream stream)` | Añade una imagen desde un `Stream`. |
| `.HorizontalLine()` | Añade un separador de línea horizontal. |
| `.VerticalStackLayout(...)` | Añade un layout de pila vertical y proporciona un constructor para su contenido. |
| `.HorizontalStackLayout(...)` | Añade un layout de pila horizontal y proporciona un constructor para su contenido. |
| `.PdfGrid()` | Añade un layout de rejilla configurable. |

## 3. Componentes Fundamentales

### 3.1. Páginas

#### PdfContentPage
La `PdfContentPage` es el tipo de página más simple y común en MauiPdfGenerator. Su propósito es mostrar una única vista, que suele ser un layout como `PdfGrid` o `PdfStackLayout`, el cual a su vez contiene otras vistas.

##### Creación y Uso Práctico
Se crea una instancia de página llamando al método `.ContentPage()` en un objeto `IPdfDocument`. Esto devuelve una interfaz `IPdfContentPage` que permite configurar propiedades específicas de la página (como `BackgroundColor` o `Spacing`) y definir su contenido.

##### Estructura y Contenido
Una `PdfContentPage` está diseñada para contener un único elemento hijo. Para construir una interfaz visual compleja, este único elemento debe ser un layout que pueda organizar múltiples vistas hijas. El contenido de la página se asigna a la propiedad `PdfContent`.

##### Propiedades Principales
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `PdfContent` | `PdfView` | Define la vista única que representa el contenido de la página. |
| `Padding` | `Thickness` | Define el espacio interior entre los bordes de la página y su contenido. |

### 3.2. Layouts (Contenedores)

Un layout se utiliza para componer las vistas de la interfaz visual en una estructura visual. Las clases de layout en MauiPdfGenerator derivan de la clase `PdfLayout`.

#### PdfVerticalStackLayout
El `PdfVerticalStackLayout` organiza sus vistas hijas en una única columna vertical.

##### Creación y Uso Práctico
Se instancia a través del método `.VerticalStackLayout(Action<IStackLayoutBuilder> content)` en un constructor de contenido. El patrón de `Action` proporciona un nuevo constructor anidado (`IStackLayoutBuilder`) para definir los elementos hijos dentro del layout.

##### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Spacing` | `double` | Define el espacio entre cada vista hija. El valor predeterminado es 0. |

#### PdfHorizontalStackLayout
El `PdfHorizontalStackLayout` organiza sus vistas hijas en una única fila horizontal.

##### Creación y Uso Práctico
Se instancia a través del método `.HorizontalStackLayout(Action<IStackLayoutBuilder> content)` en un constructor de contenido, siguiendo el mismo patrón de constructor anidado que el `PdfVerticalStackLayout`.

##### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Spacing` | `double` | Define el espacio entre cada vista hija. El valor predeterminado es 0. |

#### PdfGrid
El `PdfGrid` es un layout potente para mostrar vistas en filas y columnas. Su estructura se define con los objetos `RowDefinition` y `ColumnDefinition`.

##### Creación y Uso Práctico
Se instancia con el método `.PdfGrid()` en un constructor de contenido. A diferencia de los StackLayouts, la configuración de filas, columnas y la adición de hijos se realiza directamente sobre el objeto `PdfGrid` devuelto.

##### Posicionamiento y Expansión de Vistas:
Las vistas se colocan en celdas específicas utilizando las propiedades adjuntas `PdfGrid.Row` y `PdfGrid.Column`. Para que una vista ocupe múltiples filas o columnas, se utilizan `PdfGrid.RowSpan` y `PdfGrid.ColumnSpan`.

##### Definición de Tamaño:
El tamaño de las filas y columnas se controla a través de la propiedad `Height` de `RowDefinition` y `Width` de `ColumnDefinition`. Se pueden usar tres tipos de unidades: `Auto`, un valor numérico explícito, o un valor proporcional (`*`).

##### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `RowDefinitions` | `RowDefinitionCollection` | La colección de objetos `RowDefinition` que definen las filas. |
| `ColumnDefinitions` | `ColumnDefinitionCollection` | La colección de objetos `ColumnDefinition` que definen las columnas. |
| `RowSpacing` | `double` | El espacio vertical entre las filas del grid. |
| `ColumnSpacing` | `double` | El espacio horizontal entre las columnas del grid. |

### 3.3. Elementos Visuales

#### PdfParagraph
Un `PdfParagraph` muestra texto de una sola línea y de varias líneas.

##### Creación y Uso Práctico
Se instancia llamando al método `.Paragraph(string text)` en un constructor de contenido que implemente `IPageContentBuilder`.

##### Propiedades:
- `CharacterSpacing`: `double`, establece el espaciado entre caracteres.
- `FontAttributes`: `FontAttributes`, determina el estilo del texto (negrita, cursiva).
- `FontFamily`: `PdfFontIdentifier`, define la familia de fuentes. Para garantizar la seguridad de tipos, se debe utilizar la clase estática `PdfFonts` generada automáticamente (ej. `.FontFamily(PdfFonts.OpenSans_Regular)`).
- `FontSize`: `double`, define el tamaño de la fuente.
- `FormattedText`: `FormattedString`, permite texto con múltiples estilos usando `PdfSpan`.
- `HorizontalTextAlignment`: `TextAlignment`, define la alineación horizontal.
- `LineBreakMode`: `LineBreakMode`, determina el comportamiento de ajuste y truncamiento.
- `LineHeight`: `double`, especifica un multiplicador para la altura de línea.
- `MaxLines`: `int`, indica el número máximo de líneas.
- `Padding`: `Thickness`, determina el relleno interno.
- `Text`: `string`, define el contenido de texto.
- `TextColor`: `Color`, define el color del texto.
- `TextDecorations`: `TextDecorations`, aplica decoraciones como `Underline` y `Strikethrough`.
- `TextTransform`: `TextTransform`, especifica la transformación a mayúsculas o minúsculas.
- `VerticalTextAlignment`: `TextAlignment`, define la alineación vertical.

#### PdfImage
Muestra una imagen que se puede cargar desde un archivo local, un URI o una secuencia.

##### Creación y Uso Práctico
Se crea una instancia de `PdfImage` a través del método `.PdfImage(Stream stream)` en el `IPageContentBuilder`.

##### Propiedades:
- `Aspect`: `Aspect`, define el modo de escalado de la imagen (`AspectFit`, `AspectFill`, `Fill`, `Center`).
- `Source`: `PdfImageSource`, especifica el origen de la imagen.

La clase `PdfImageSource` proporciona métodos para cargar desde diferentes orígenes: `FromFile`, `FromUri` y `FromStream`. El almacenamiento en caché está habilitado por defecto para imágenes remotas.

#### PdfShape
Permite dibujar una forma en la página. `PdfShape` es una clase base.

##### Propiedades de Estilo:
- `Fill`: `Brush`, indica el pincel para pintar el interior de la forma.
- `Stroke`: `Brush`, indica el pincel para pintar el contorno.
- `StrokeThickness`: `double`, indica el ancho del contorno.
- `StrokeDashArray`: `DoubleCollection`, define un patrón de guiones y espacios.
- `StrokeLineCap`: `PenLineCap`, describe la forma al principio y al final de una línea.
- `StrokeLineJoin`: `PenLineJoin`, especifica el tipo de unión en los vértices.
- `Aspect`: `Stretch`, describe cómo la forma llena su espacio asignado.

## 4. Sistema de Layout y Comportamiento

### 4.1. Alineación y Posicionamiento

Cada elemento que deriva de `PdfView` tiene propiedades `PdfHorizontalOptions` y `PdfVerticalOptions` de tipo `LayoutAlignment`, que determinan su posición y tamaño dentro de su layout padre.

#### 4.1.1. Opciones de Alineación

- **`Start`**: Posiciona el elemento a la izquierda (horizontal) o en la parte superior (vertical) de su contenedor.
- **`Center`**: Centra el elemento horizontal o verticalmente.
- **`End`**: Posiciona el elemento a la derecha (horizontal) o en la parte inferior (vertical).
- **`Fill`**: Asegura que el elemento llene el ancho o la altura disponible de su contenedor.

#### 4.1.2. Comportamiento en StackLayouts

Un `PdfStackLayout` solo respeta las opciones de alineación en la dirección opuesta a su orientación. Por ejemplo, un `PdfVerticalStackLayout` solo aplicará las `PdfHorizontalOptions` (`Start`, `Center`, `End`, `Fill`) a sus hijos, ignorando las `PdfVerticalOptions`.

### 4.2. Margin y Padding

Las propiedades `Margin` y `Padding` (ambas de tipo `Thickness`) controlan el espaciado.

- **`Margin`**: Representa la distancia **externa** entre un elemento y sus vecinos. Es gestionado por el contenedor padre.
- **`Padding`**: Representa la distancia **interna** entre el borde de un elemento y su propio contenido.

### 4.3. El Principio de Herencia de Propiedades

La biblioteca implementa un sistema de herencia de dos niveles: **Local** y **Global**. Cuando se resuelve una propiedad de estilo (como la fuente o el color), el valor especificado directamente en el elemento (Local) siempre tiene prioridad. Si no existe, se utiliza el valor definido en la configuración global del documento (Global), establecida a través de `IPdfDocumentConfigurator`. Este mecanismo sienta las bases para futuras mejoras, como la herencia a nivel de página o contenedor, y promueve un código más limpio y mantenible.

## 5. Unidades y Medidas

### 5.1. Sistema de Unidades y Conversión Automática

Para mantener la coherencia con .NET MAUI, la biblioteca utiliza **unidades independientes del dispositivo** para todas las propiedades de tamaño y espaciado (`WidthRequest`, `FontSize`, `Margin`, etc.). El desarrollador trabaja con las unidades que ya conoce.

Internamente, la biblioteca convierte de forma automática y transparente estas unidades a **puntos (points)**, el estándar en PDF (72 puntos por pulgada). La relación de conversión es aproximadamente **1 unidad MAUI ≈ 1.33 puntos PDF**. Este detalle de implementación asegura que el desarrollador no necesite realizar cálculos manuales, manteniendo la filosofía de ser una extensión natural de MAUI.

---

# PARTE II: ARQUITECTURA DEL SISTEMA DE LAYOUT

## 5. El Sistema de Layout de Tres Pasadas (Measure/Arrange/Render)

El motor emula deliberadamente el **ciclo de Medición y Disposición (Measure/Arrange) de .NET MAUI**, adaptándolo a un contexto de generación de documentos asíncrono y separando explícitamente el renderizado.

### 5.1. Fase 1: La Pasada de Medición (`MeasureAsync`)

*   **Responsabilidad:** Calcular el tamaño que cada elemento *desea* tener (`DesiredSize`) basándose en su contenido y las restricciones recibidas.
*   **Equivalencia MAUI:** Es el análogo directo de la pasada `Measure` de MAUI.
*   **Ubicación Arquitectónica:** Esta lógica reside en `Core.Integration` (capa de abstracciones), ya que es independiente del motor de renderizado específico.
*   **Proceso:** El sistema recorre el árbol de elementos de arriba hacia abajo. Cada elemento recibe un `LayoutRequest` de su padre que comunica el espacio disponible y el tipo de medición requerida.
*   **Dependencia de Métricas:** Para elementos que requieren métricas específicas del motor (como texto), esta fase utiliza la interfaz `ILayoutMetrics` proporcionada por `Core.Implementation.Sk`, manteniendo la independencia arquitectónica.
*   **Resultado:** Un `LayoutInfo` que contiene el tamaño deseado del elemento incluyendo su contenido y padding, pero **excluyendo explícitamente el margin**.

### 5.2. Fase 2: La Pasada de Disposición (`ArrangeAsync`)

*   **Responsabilidad:** Asignar una posición y un tamaño final y concreto a cada elemento dentro del espacio disponible.
*   **Equivalencia MAUI:** Es el análogo directo de la pasada `Arrange` de MAUI.
*   **Ubicación Arquitectónica:** También reside en `Core.Integration`, siendo independiente del motor de renderizado.
*   **Proceso:** Conociendo los tamaños deseados, el sistema recorre el árbol nuevamente. Cada contenedor padre distribuye su espacio entre sus hijos y les asigna un rectángulo final (`ArrangedRect`).
*   **Gestión del Margin:** Durante esta fase, cada contenedor padre es responsable de considerar el `Margin` de sus hijos al calcular sus posiciones finales.
*   **Resultado:** Un "plano de layout" completo donde cada elemento conoce su posición y tamaño final. Al finalizar, **nada se ha dibujado todavía**.

### 5.3. Fase 3: La Pasada de Renderizado (`RenderAsync`)

*   **Responsabilidad:** Dibujar cada elemento en su posición final usando las APIs específicas del motor.
*   **Equivalencia MAUI:** Es análogo al ciclo de dibujado de la plataforma nativa (Android Canvas, iOS CoreGraphics).
*   **Ubicación Arquitectónica:** Esta es la única fase que reside en `Core.Implementation.Sk`, ya que depende completamente del motor concreto (SkiaSharp).
*   **Proceso:** Recorre el "plano de layout" y utiliza las APIs del motor (ej. `SKCanvas`) para dibujar los elementos en el PDF.
*   **Intercambiabilidad:** Esta fase es completamente intercambiable. Se podría implementar un `Core.Implementation.PdfSharp` sin afectar las fases de medición y disposición.

## 6. Principios y Reglas Fundamentales de Layout

### 6.1. El Principio de Propagación de Restricciones

**Heredado directamente de MAUI:**
*   **Universo Finito:** Un elemento **NUNCA** asume su tamaño. Siempre opera dentro de un espacio finito definido por su contenedor padre.
*   **Propagación Obligatoria:** Un elemento **SIEMPRE** recibe un `LayoutRequest` de su padre que especifica el espacio disponible y el tipo de medición.
*   **Elemento Raíz:** La única excepción es `ContentPage`, cuyo espacio disponible inicial es el área de contenido de la página (tamaño de página menos márgenes).

### 6.2. La Dualidad de la Medición

**Adaptación del sistema de restricciones de MAUI:**
Un elemento debe ser capaz de responder a dos tipos de "preguntas de medición", comunicadas a través del `LayoutRequest`.

*   **Pregunta de Medición Restringida (`LayoutPassType.Constrained`):**
    *   **Intención:** "Tienes este **ancho finito**. Adáptate a él (aplicando saltos de línea si es necesario) y dime qué altura necesitas".
    *   **Quién la hace:** Contenedores de naturaleza vertical como `ContentPage` y `VerticalStackLayout`.

*   **Pregunta de Medición Ideal (`LayoutPassType.Ideal`):**
    *   **Intención:** "Ignora las restricciones de ancho. Dime cuál es tu tamaño **natural** si pudieras usar tanto espacio como necesites (sin saltos de línea)".
    *   **Quién la hace:** Contenedores de naturaleza horizontal como `HorizontalStackLayout`.

### 6.3. El Modelo de Caja (Box Model)
**Idéntico al modelo de MAUI:**
*   **`Margin` (Margen):** Espacio **externo** y transparente que empuja la caja lejos de sus vecinos. No forma parte del `BackgroundColor`. Es gestionado exclusivamente por el contenedor padre durante la pasada de `ArrangeAsync`.
*   **`Padding` (Relleno):** Espacio **interno** que empuja el contenido lejos del borde. El `BackgroundColor` **sí** se dibuja en esta área.
*   **Huella de Elemento:** El tamaño devuelto por `MeasureAsync` (`LayoutInfo`) representa la huella del `Contenido + Padding`, **excluyendo el Margin**.

### 6.4. Contexto de Layout y Orquestación de Fases
*   **Propagación de Contexto:** El sistema utiliza un `LayoutContext` que se propaga recursivamente por el árbol. Este objeto transporta información del contenedor padre, permitiendo a los hijos determinar sus comportamientos por defecto.
*   **Orquestación Centralizada:** Un orquestador en `Core.Integration` es responsable de invocar la secuencia de pasadas en el orden correcto.
*   **Flujo de Ejecución Estricto:** Las tres fases se ejecutan secuencialmente para todo el árbol: primero se completa `MeasureAsync` para todos, luego `ArrangeAsync` para todos, y finalmente `RenderAsync`. Esto garantiza que cada fase tenga la información necesaria antes de proceder.

## 7. Comportamiento Detallado de Componentes

### 7.1. Contenedores de Layout

#### `VerticalStackLayoutRenderer`
**Emula `VerticalStackLayout` de MAUI:**
1.  **`MeasureAsync`:** Hace una **Pregunta de Medición Restringida** a cada hijo. Su altura deseada es la suma de las alturas de los hijos más el `Spacing`. Su ancho deseado es el del hijo más ancho. Añade su propio `Padding`.
2.  **`ArrangeAsync`:** Posiciona a sus hijos uno debajo del otro, considerando sus `Margin` y alineándolos horizontalmente (`PdfHorizontalOptions`).
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo. Aplica `ClipRect` si el contenido se desborda.

#### `HorizontalStackLayoutRenderer`
**Emula `HorizontalStackLayout` de MAUI:**
1.  **`MeasureAsync`:** Hace una **Pregunta de Medición Ideal** a cada hijo. Su ancho deseado es la suma de los anchos de los hijos más el `Spacing`. Su altura deseada es la del hijo más alto. Añade su propio `Padding`.
2.  **`ArrangeAsync`:** Posiciona a sus hijos uno al lado del otro, considerando sus `Margin` y alineándolos verticalmente (`PdfHorizontalOptions`).
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo. Aplica `ClipRect` si el contenido se desborda.

#### `GridRenderer`
**Emula `Grid` de MAUI:**
1.  **`MeasureAsync`:** Resuelve las definiciones de filas/columnas (`Star`, `Auto`, Absoluto). Hace preguntas de medición específicas a cada hijo según su celda. Calcula el tamaño total de la rejilla.
2.  **`ArrangeAsync`:** Distribuye el espacio entre filas/columnas. Posiciona cada hijo en su celda, considerando `Margin` y alineación. Maneja `RowSpan` y `ColumnSpan`.
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo en su celada asignada.

### 7.2. Elementos Primitivos

#### `TextRenderer`
**Equivalente a `Label` de MAUI:**
1.  **`MeasureAsync`:** Usa `ILayoutMetrics` para calcular el tamaño del texto. Responde a `Constrained` (con saltos de línea) o `Ideal` (sin saltos). Añade `Padding`.
2.  **`ArrangeAsync`:** Acepta el rectángulo final asignado por su padre.
3.  **`RenderAsync`:** Dibuja el texto dentro del `renderRect` recibido, después de restar su propio `Padding` para obtener el `contentBox`.

#### `ImageRenderer`
**Equivalente a `Image` de MAUI:**
1.  **`MeasureAsync`:** Carga la imagen asíncronamente. Calcula el tamaño basándose en las dimensiones naturales y el `Aspect`. Añade `Padding`.
2.  **`ArrangeAsync`:** Acepta el rectángulo final y calcula la escala/posición de la imagen según el `Aspect` (`Fill`, `AspectFit`, `AspectFill`).
3.  **`RenderAsync`:** Dibuja la imagen escalada y posicionada dentro del `renderRect` recibido, después de restar su `Padding`.

#### `HorizontalLineRenderer`
1.  **`MeasureAsync`:** Responde a `Constrained` (ancho completo) o `Ideal` (ancho mínimo). Añade `Padding`.
2.  **`ArrangeAsync`:** Acepta el rectángulo final asignado.
3.  **`RenderAsync`:** Dibuja una línea dentro del `renderRect` recibido, después de restar su `Padding`.

### 7.3. Principios de Consistencia en Renderers
Todos los renderers deben seguir estos principios:
1.  **Separación de Responsabilidades:** La lógica de `Measure`/`Arrange` está separada del `Render`.
2.  **Gestión Uniforme de Padding:** Los elementos añaden su `Padding` durante la medición y lo restan durante el renderizado.
3.  **Gestión Delegada de Margin:** Los elementos nunca gestionan su propio `Margin`; es responsabilidad del contenedor padre.
4.  **Respuesta a Tipos de Medición:** Los elementos responden apropiadamente a `Constrained` e `Ideal`.
5.  **Clipping Consistente:** Los contenedores aplican clipping cuando el contenido se desborda.
6.  **Background Rendering:** El `BackgroundColor` cubre el área de `Contenido + Padding`, pero excluye el `Margin`.