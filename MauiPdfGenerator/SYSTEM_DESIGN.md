# Documentación Técnica de la Biblioteca MauiPdfGenerator

# Parte I: Visión General y Conceptual

## 1. Principio Rector

Esta biblioteca está diseñada como una **extensión natural del ecosistema .NET MAUI**, no como una herramienta externa. Su propósito es permitir que los desarrolladores MAUI generen PDFs de forma nativa utilizando conceptos familiares de .NET MAUI. Para aquellos desarrolladores con experiencia en la plataforma, les resultará más cómodo el uso de esta biblioteca ya que su confección es similar a cuando se crea una UI de .NET MAUI, pero orientada a la generación de documentos PDF.

### 1.1. Estructura Visual General

La interfaz visual de un documento PDF se construye con elementos que mapean conceptualmente a los controles de MAUI:

**MAUI → PDF (Analogía Visual)**
- **Pages** → Páginas del documento PDF
- **Layouts** → Estructuras de organización visual en PDF
- **Views** → Elementos de contenido visual en PDF

#### Páginas

Los documentos PDF constan de una o varias páginas. Cada página contiene al menos un diseño. MauiPdfGenerator contiene las siguientes páginas:

|Página|Descripción|
|---|---|
|PdfContentPage|Muestra un diseño principal y es el tipo de página más común en documentos PDF.|

> **NOTA:** Para el futuro, se agregarán otros tipos de páginas especializadas.

#### Diseños

Los diseños en MauiPdfGenerator se usan para organizar elementos visuales en estructuras jerárquicas dentro del documento PDF. Cada diseño normalmente contiene varios elementos hijos y otros diseños anidados. Las clases de diseño contienen lógica para establecer la posición y el tamaño de los elementos secundarios en el documento PDF.

MauiPdfGenerator contiene los siguientes diseños:

|Diseño|Descripción|
|---|---|
|PdfGrid|Coloca sus elementos secundarios en una cuadrícula de filas y columnas dentro del PDF.|
|PdfHorizontalStackLayout|Coloca los elementos secundarios en una pila horizontal en el documento PDF.|
|PdfVerticalStackLayout|Coloca los elementos secundarios en una pila vertical en el documento PDF.|

#### Elementos Visuales

Los elementos visuales de MauiPdfGenerator son componentes que renderizan contenido específico en el documento PDF, como párrafos, imágenes y figuras geométricas.

MauiPdfGenerator contiene los siguientes elementos visuales:

|Elemento|Descripción|
|---|---|
|PdfImage|Renderiza una imagen en el PDF que se puede cargar desde un archivo local, un URI o una secuencia.|
|PdfParagraph|Renderiza texto plano y enriquecido de una o varias líneas en el documento PDF.|
|PdfHorizontalLine|Renderiza una línea horizontal en el PDF de un punto inicial a un punto final.|

### 1.2. Consideraciones de Diseño
- **Layouts** definen la estructura espacial del contenido en el PDF
- **Views** proporcionan el contenido visual específico renderizado en el documento
- **Pages** contienen la composición completa de una página del PDF
- La interactividad se omite intencionalmente, ya que los archivos PDF generados por MauiPdfGenerator se enfocan en documentos estáticos

### 1.3. Aplicación Práctica

Al diseñar un PDF basado en controles MAUI:

1. **Planifica las páginas** → Define el número y tipo de páginas del documento PDF
2. **Diseña los layouts** → Estructura la organización visual del contenido en cada página
3. **Selecciona los elementos** → Elige los componentes visuales apropiados para el contenido
4. **Compón la jerarquía** → Organiza la estructura visual completa del documento

Esta analogía permite trasladar conceptos de diseño de aplicaciones MAUI a la composición de documentos PDF, manteniendo la lógica estructural pero adaptándola a un medio de documento estático.

## 2. Visión General Arquitectónica: Las Tres Capas

La arquitectura de la biblioteca está diseñada en torno a una clara **Separación de Capas (SoC)**, cada una con una responsabilidad única y bien definida. Esta separación permite evolución independiente, intercambiabilidad de componentes y mantenimiento del código a largo plazo.

### 2.1. Capa `Fluent` (API Pública)

*   **Propósito:** Única puerta de entrada para el desarrollador. Su misión es ofrecer una experiencia de desarrollo declarativa, legible y fácil de usar.
*   **Responsabilidades:**
    *   **API Guiada:** Utiliza el patrón Type-State para prevenir errores en tiempo de compilación.
    *   **Fluidez Contextual:** Métodos encadenables que exponen solo opciones válidas según el contexto.
    *   **Encapsulación de Complejidad:** Oculta completamente la implementación interna del motor.
*   **Narrativa de Transformación:** Cuando el desarrollador construye un documento, los objetos de la API Fluent (ej. `PdfParagraph`) actúan como *builders* que acumulan configuración. Al finalizar el proceso (ej. en `SaveAsync`), estos objetos se transforman en sus DTOs equivalentes de la capa `Common` (ej. `PdfParagraphData`). Esta transformación despoja a los objetos de su lógica de construcción, conservando solo la configuración pura para ser procesada por el motor.

### 2.2. Capa `Core` (Motor de Layout y Renderizado)

El motor se subdivide siguiendo el **Principio de Inversión de Dependencias**, separando abstracciones de implementaciones concretas:

#### Subcapa `Core.Integration` (Abstracciones)
*   **Propósito:** Contiene la lógica de layout independiente del motor de renderizado.
*   **Responsabilidades:**
    *   **Sistema de Layout:** Implementa las fases `MeasureAsync` y `ArrangeAsync`.
    *   **Contratos de Medición:** Define `ILayoutMetrics` para obtener métricas sin depender de implementaciones.
    *   **Orquestación:** Coordina la secuencia de pasadas de layout.
    *   **Lógica de Contenedores:** Algoritmos de distribución para `Grid`, `StackLayout`, etc.

#### Subcapa `Core.Implementation.Sk` (Implementación Concreta)
*   **Propósito:** Implementa el renderizado específico usando un motor concreto (SkiaSharp).
*   **Responsabilidades:**
    *   **Renderizado Final:** Implementa `RenderAsync` usando APIs de SkiaSharp.
    *   **Métricas Concretas:** Proporciona la implementación real de `ILayoutMetrics`.
    *   **Gestión de Recursos:** Maneja `SKTypeface`, `SKImage`, etc.
*   **Principio de Diseño:** Cada implementación es un "plugin" intercambiable que cumple los contratos definidos en `Core.Integration`.

### 2.3. Capa `Common` (Contratos Compartidos)

*   **Propósito:** Define el "lenguaje común" que permite la comunicación entre capas sin crear dependencias directas.
*   **Contenido Principal:**
    *   **Contratos de Datos (DTOs):** `PdfDocumentData`, `PdfPageData`, `PdfParagraphData`, etc. Transportan información estructurada y pura.
    *   **Value Objects:** Tipos inmutables (`PdfFontIdentifier`, `PdfGridLength`, `Color`) que encapsulan valores con semántica.
    *   **Enumeraciones:** Estados y opciones del sistema (`LayoutPassType`, `TextAlignment`).
    *   **Interfaces de Comunicación:** `ILayoutMetrics` para obtener métricas del motor.
    *   **Estructuras de Layout:** `LayoutRequest`, `LayoutInfo`, que definen el protocolo de comunicación del sistema de layout.
*   **Principio de Diseño:** Actúa como una "zona neutra" donde las capas pueden intercambiar información sin conocer detalles de implementación mutuos.

### 2.4. Flujo de Datos y Comunicación Entre Capas

1.  **`Fluent` -> `Common`:** El desarrollador usa la API `Fluent`. Al finalizar, la capa `Fluent` mapea sus objetos de construcción a un árbol de DTOs puros en la capa `Common`.
2.  **`Common` -> `Core.Integration`:** El orquestador del `Core` recibe el árbol de DTOs. El `ElementRendererFactory` lo recorre, instanciando el `IElementRenderer` apropiado para cada DTO.
3.  **`Core.Integration` <-> `Core.Implementation`:** Durante `MeasureAsync`, la lógica de layout abstracta consulta las métricas de la implementación concreta a través de la interfaz `ILayoutMetrics`.
4.  **`Core.Integration` -> `Core.Implementation`:** Una vez completadas las fases de `Measure` y `Arrange`, el "plano de layout" final se pasa a la fase `RenderAsync` de la implementación para el dibujado final.

---

# Parte II: Arquitectura y Diseño Interno

## 1. El Sistema de Layout de Tres Pasadas (Measure/Arrange/Render)

El motor emula deliberadamente el **ciclo de Medición y Disposición (Measure/Arrange) de .NET MAUI**, adaptándolo a un contexto de generación de documentos asíncrono y separando explícitamente el renderizado.

### 1.1. Fase 1: La Pasada de Medición (`MeasureAsync`)
*   **Responsabilidad:** Calcular el tamaño que cada elemento *desea* tener (`DesiredSize`) basándose en su contenido y las restricciones recibidas.
*   **Equivalencia MAUI:** Es el análogo directo de la pasada `Measure` de MAUI.
*   **Ubicación Arquitectónica:** Esta lógica reside en `Core.Integration` (capa de abstracciones), ya que es independiente del motor de renderizado específico.
*   **Proceso:** El sistema recorre el árbol de elementos de arriba hacia abajo. Cada elemento recibe un `LayoutRequest` de su padre que comunica el espacio disponible y el tipo de medición requerida.
*   **Dependencia de Métricas:** Para elementos que requieren métricas específicas del motor (como texto), esta fase utiliza la interfaz `ILayoutMetrics` proporcionada por `Core.Implementation.Sk`, manteniendo la independencia arquitectónica.
*   **Resultado:** Un `LayoutInfo` que contiene el tamaño deseado del elemento incluyendo su contenido y padding, pero **excluyendo explícitamente el margin**.

### 1.2. Fase 2: La Pasada de Disposición (`ArrangeAsync`)
*   **Responsabilidad:** Asignar una posición y un tamaño final y concreto a cada elemento dentro del espacio disponible.
*   **Equivalencia MAUI:** Es el análogo directo de la pasada `Arrange` de MAUI.
*   **Ubicación Arquitectónica:** También reside en `Core.Integration`, siendo independiente del motor de renderizado.
*   **Proceso:** Conociendo los tamaños deseados, el sistema recorre el árbol nuevamente. Cada contenedor padre distribuye su espacio entre sus hijos y les asigna un rectángulo final (`ArrangedRect`).
*   **Gestión del Margin:** Durante esta fase, cada contenedor padre es responsable de considerar el `Margin` de sus hijos al calcular sus posiciones finales.
*   **Resultado:** Un "plano de layout" completo donde cada elemento conoce su posición y tamaño final. Al finalizar, **nada se ha dibujado todavía**.

### 1.3. Fase 3: La Pasada de Renderizado (`RenderAsync`)
*   **Responsabilidad:** Dibujar cada elemento en su posición final usando las APIs específicas del motor.
*   **Equivalencia MAUI:** Es análogo al ciclo de dibujado de la plataforma nativa (Android Canvas, iOS CoreGraphics).
*   **Ubicación Arquitectónica:** Esta es la única fase que reside en `Core.Implementation.Sk`, ya que depende completamente del motor concreto (SkiaSharp).
*   **Proceso:** Recorre el "plano de layout" y utiliza las APIs del motor (ej. `SKCanvas`) para dibujar los elementos en el PDF.
*   **Intercambiabilidad:** Esta fase es completamente intercambiable. Se podría implementar un `Core.Implementation.PdfSharp` sin afectar las fases de medición y disposición.

## 2. Principios y Reglas Fundamentales de Layout

### 2.1. El Principio de Propagación de Restricciones
**Heredado directamente de MAUI:**
*   **Universo Finito:** Un elemento **NUNCA** asume su tamaño. Siempre opera dentro de un espacio finito definido por su contenedor padre.
*   **Propagación Obligatoria:** Un elemento **SIEMPRE** recibe un `LayoutRequest` de su padre que especifica el espacio disponible y el tipo de medición.
*   **Elemento Raíz:** La única excepción es `ContentPage`, cuyo espacio disponible inicial es el área de contenido de la página (tamaño de página menos márgenes).

### 2.2. La Dualidad de la Medición
**Adaptación del sistema de restricciones de MAUI:**
Un elemento debe ser capaz de responder a dos tipos de "preguntas de medición", comunicadas a través del `LayoutRequest`.

*   **Pregunta de Medición Restringida (`LayoutPassType.Constrained`):**
    *   **Intención:** "Tienes este **ancho finito**. Adáptate a él (aplicando saltos de línea si es necesario) y dime qué altura necesitas".
    *   **Quién la hace:** Contenedores de naturaleza vertical como `ContentPage` y `VerticalStackLayout`.

*   **Pregunta de Medición Ideal (`LayoutPassType.Ideal`):**
    *   **Intención:** "Ignora las restricciones de ancho. Dime cuál es tu tamaño **natural** si pudieras usar tanto espacio como necesites (sin saltos de línea)".
    *   **Quién la hace:** Contenedores de naturaleza horizontal como `HorizontalStackLayout`.

### 2.3. El Modelo de Caja (Box Model)
**Idéntico al modelo de MAUI:**
*   **`Margin` (Margen):** Espacio **externo** y transparente que empuja la caja lejos de sus vecinos. No forma parte del `BackgroundColor`. Es gestionado exclusivamente por el contenedor padre durante la pasada de `ArrangeAsync`.
*   **`Padding` (Relleno):** Espacio **interno** que empuja el contenido lejos del borde. El `BackgroundColor` **sí** se dibuja en esta área.
*   **Huella de Elemento:** El tamaño devuelto por `MeasureAsync` (`LayoutInfo`) representa la huella del `Contenido + Padding`, **excluyendo el Margin**.

### 2.4. Contexto de Layout y Orquestación de Fases
*   **Propagación de Contexto:** El sistema utiliza un `LayoutContext` que se propaga recursivamente por el árbol. Este objeto transporta información del contenedor padre, permitiendo a los hijos determinar sus comportamientos por defecto.
*   **Orquestación Centralizada:** Un orquestador en `Core.Integration` es responsable de invocar la secuencia de pasadas en el orden correcto.
*   **Flujo de Ejecución Estricto:** Las tres fases se ejecutan secuencialmente para todo el árbol: primero se completa `MeasureAsync` para todos, luego `ArrangeAsync` para todos, y finalmente `RenderAsync`. Esto garantiza que cada fase tenga la información necesaria antes de proceder.

### 2.5. El Principio de Herencia de Propiedades

La biblioteca implementa un sistema de herencia de dos niveles: **Local** y **Global**. Cuando se resuelve una propiedad de estilo (como la fuente o el color), el valor especificado directamente en el elemento (Local) siempre tiene prioridad. Si no existe, se utiliza el valor definido en la configuración global del documento (Global), establecida a través de `IPdfDocumentConfigurator`. Este mecanismo sienta las bases para futuras mejoras, como la herencia a nivel de página o contenedor, y promueve un código más limpio y mantenible.

### 2.6. Unidades y Medidas

#### Sistema de Unidades y Conversión Automática

Para mantener la coherencia con .NET MAUI, la biblioteca utiliza **unidades independientes del dispositivo** para todas las propiedades de tamaño y espaciado (`WidthRequest`, `FontSize`, `Margin`, etc.). El desarrollador trabaja con las unidades que ya conoce.

Internamente, la biblioteca convierte de forma automática y transparente estas unidades a **puntos (points)**, el estándar en PDF (72 puntos por pulgada). La relación de conversión es aproximadamente **1 unidad MAUI ≈ 1.33 puntos PDF**. Este detalle de implementación asegura que el desarrollador no necesite realizar cálculos manuales, manteniendo la filosofía de ser una extensión natural de MAUI.

## 3. Comportamiento Detallado de Componentes de Renderizado

### 3.1. Contenedores de Layout

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
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo en su celda asignada.

### 3.2. Elementos Primitivos

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

### 3.3. Principios de Consistencia en Renderers
Todos los renderers deben seguir estos principios:
1.  **Separación de Responsabilidades:** La lógica de `Measure`/`Arrange` está separada del `Render`.
2.  **Gestión Uniforme de Padding:** Los elementos añaden su `Padding` durante la medición y lo restan durante el renderizado.
3.  **Gestión Delegada de Margin:** Los elementos nunca gestionan su propio `Margin`; es responsabilidad del contenedor padre.
4.  **Respuesta a Tipos de Medición:** Los elementos responden apropiadamente a `Constrained` e `Ideal`.
5.  **Clipping Consistente:** Los contenedores aplican clipping cuando el contenido se desborda.
6.  **Background Rendering:** El `BackgroundColor` cubre el área de `Contenido + Padding`, pero excluye el `Margin`.

---

# Parte III: Diseño de API y Guía de Uso

## 1. Guía de Inicio Rápido: Configuración y Primer Documento

### 1.1. Integración con .NET MAUI: El Punto de Entrada

La biblioteca se adhiere a los patrones modernos de .NET, integrándose de forma nativa en el ecosistema de la aplicación. La inicialización se realiza en el fichero `MauiProgram.cs` mediante el método de extensión `UseMauiPdfGenerator()`. Este acto registra en el contenedor de inyección de dependencias (DI) todos los servicios necesarios, principalmente la interfaz `IPdfDocumentFactory`. Este enfoque garantiza que la creación de PDFs sea una capacidad intrínseca de la aplicación, accesible desde cualquier parte de la misma de una manera limpia, desacoplada y testeable.

### 1.2. El Sistema de Fuentes Unificado

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

### 1.3. La Fábrica de Documentos: Creación mediante DI

Tras la configuración, la creación de un documento se realiza solicitando la interfaz `IPdfDocumentFactory` al contenedor de DI. Esta fábrica expone el método `CreateDocument()`, que devuelve una instancia de `IPdfDocument`, el punto de partida para construir el PDF. Este patrón desacopla el código del cliente de la implementación y facilita las pruebas.

### 1.4. Configuración Global del Documento

Cada instancia de `IPdfDocument` ofrece el método `Configuration(Action<IPdfDocumentConfigurator> documentConfigurator)` para definir las características globales del documento. La acción recibe una instancia de `IPdfDocumentConfigurator`.

| Método en `IPdfDocumentConfigurator` | Descripción |
| :--- | :--- |
| `.PageSize(PageSizeType size)` | Establece el tamaño de página por defecto para todo el documento. |
| `.PageOrientation(PageOrientationType orientation)` | Define la orientación por defecto (Vertical/Apaisada). |
| `.Margins(DefaultMarginType marginType)` | Aplica un conjunto de márgenes predefinidos (`Normal`, `Estrecho`, etc.). |
| `.Margins(float uniform)` | Aplica un margen uniforme a los cuatro lados. |
| `.MetaData(...)` | Accede al constructor de metadatos del PDF. |
| `.ConfigureFontRegistry(...)` | Accede a la configuración avanzada de fuentes (`IPdfFontRegistry`). |

### 1.5. Enriquecimiento con Metadatos

Dentro de la configuración global, el método `.MetaData(Action<IPdfMetaData> metaDataAction)` permite establecer los metadatos del PDF. La acción recibe una instancia de `IPdfMetaData`.

| Método en `IPdfMetaData` | Descripción |
| :--- | :--- |
| `.Title(string title)` | Define el título del documento. |
| `.Author(string author)` | Define el autor del documento. |
| `.Subject(string subject)` | Define el asunto. |
| `.Keywords(string keywords)` | Define las palabras clave. |
| `.CustomProperty(string name, string value)` | Añade un metadato personalizado. |

### 1.6. Construcción de Contenido de Página

Una vez configurado el documento, se añade una página con el método `.ContentPage()`, que devuelve un objeto `IPdfContentPage`. El paso final es llamar al método `.Content(Action<IPageContentBuilder> contentSetup)`. Este método pasa el control a un constructor de contenido (`IPageContentBuilder`) que es la caja de herramientas para añadir elementos.

| Método en `IPageContentBuilder` | Descripción |
| :--- | :--- |
| `.Paragraph(string text)` | Añade un elemento de texto. |
| `.PdfImage(Stream stream)` | Añade una imagen desde un `Stream`. |
| `.HorizontalLine()` | Añade un separador de línea horizontal. |
| `.VerticalStackLayout(...)` | Añade un layout de pila vertical y proporciona un constructor para su contenido. |
| `.HorizontalStackLayout(...)` | Añade un layout de pila horizontal y proporciona un constructor para su contenido. |
| `.PdfGrid()` | Añade un layout de rejilla configurable. |

## 2. Componentes Fundamentales

### 2.1. Páginas

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

### 2.2. Layouts (Contenedores)

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

### 2.3. Elementos Visuales

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

## 3. Sistema de Layout y Comportamiento

### 3.1. Alineación y Posicionamiento

#### Conceptos Fundamentales
Cada elemento PDF que deriva de `PdfView` (incluyendo vistas y layouts) tiene propiedades `PdfHorizontalOptions` y `PdfVerticalOptions` de tipo `LayoutAlignment`. Esta estructura encapsula la alineación preferida de un elemento, determinando su posición y tamaño dentro de su layout padre cuando este contiene espacio no utilizado en la página PDF.

#### Opciones de Alineación
Los campos de alineación controlan cómo se posicionan los elementos dentro del documento PDF:

- **`Start`**
  - Alineación horizontal: Posiciona el elemento en el lado izquierdo del layout padre.
  - Alineación vertical: Posiciona el elemento en la parte superior del layout padre.

- **`Center`**
  - Centra el elemento horizontal o verticalmente dentro del layout padre.

- **`End`**
  - Alineación horizontal: Posiciona el elemento en el lado derecho del layout padre.
  - Alineación vertical: Posiciona el elemento en la parte inferior del layout padre.

- **`Fill`**
  - Alineación horizontal: Asegura que el elemento llene el ancho disponible del layout padre.
  - Alineación vertical: Asegura que el elemento llene la altura disponible del layout padre.

> **Nota**: El valor predeterminado de las propiedades `PdfHorizontalOptions` y `PdfVerticalOptions` es `LayoutAlignment.Fill`.

#### Comportamiento en PdfStackLayout
Un `PdfStackLayout` solo respeta los campos `Start`, `Center`, `End` y `Fill` en elementos hijos que están en dirección opuesta a la orientación del stack:

- **PdfVerticalStackLayout**: Los elementos hijos pueden establecer sus propiedades `PdfHorizontalOptions`.
- **PdfHorizontalStackLayout**: Los elementos hijos pueden establecer sus propiedades `PdfVerticalOptions`.

> **Importante**: `LayoutAlignment.Fill` generalmente anula las solicitudes de tamaño especificadas usando las propiedades `HeightRequest` y `WidthRequest`.

### 3.2. Posicionamiento con Margin y Padding

#### Propiedades de Espaciado
Las propiedades `Margin` y `Padding` posicionan elementos PDF relativos a elementos adyacentes o hijos:

- **`Margin`**: Representa la distancia entre un elemento PDF y sus elementos adyacentes. Controla la posición de renderizado del elemento y de sus vecinos en el documento.
- **`Padding`**: Representa la distancia entre un elemento PDF y sus elementos hijos. Separa el elemento de su propio contenido interno.

#### Características del Espaciado en PDF
- Los valores de `Margin` son aditivos: dos elementos adyacentes con margen de 20 unidades tendrán una distancia de 40 unidades entre ellos en el PDF.
- Los valores de margin y padding se suman cuando ambos se aplican.
- Ambas propiedades son de tipo `Thickness`.

#### Estructura Thickness
Tres formas de crear una estructura `Thickness` para elementos PDF:

1. **Valor uniforme único**: Se aplica a los cuatro lados (izquierda, superior, derecha, inferior).
2. **Valores horizontal y vertical**: El valor horizontal se aplica simétricamente a izquierda y derecha, el vertical a superior e inferior.
3. **Cuatro valores distintos**: Se aplican específicamente a izquierda, superior, derecha e inferior.

> **Nota**: Los valores de `Thickness` pueden ser negativos, lo que típicamente recorta o superpone el contenido en el PDF.