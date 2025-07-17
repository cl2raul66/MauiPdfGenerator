# Documentación Técnica de la Biblioteca MauiPdfGenerator

# 1. Principio Rector

Esta biblioteca está diseñada como una **extensión natural del ecosistema .NET MAUI**, no como una herramienta externa. Su propósito es permitir que los desarrolladores MAUI generen PDFs de forma nativa utilizando conceptos familiares de .NET MAUI. Para aquellos desarrolladores con experiencia en la plataforma, les resultará más cómodo el uso de esta biblioteca ya que su confección es similar a cuando se crea una UI de .NET MAUI, pero orientada a la generación de documentos PDF.

## Estructura Visual General

La interfaz visual de un documento PDF se construye con elementos que mapean conceptualmente a los controles de MAUI:

**MAUI → PDF (Analogía Visual)**
- **Pages** → Páginas del documento PDF
- **Layouts** → Estructuras de organización visual en PDF
- **Views** → Elementos de contenido visual en PDF

### Páginas

Los documentos PDF constan de una o varias páginas. Cada página contiene al menos un diseño. MauiPdfGenerator contiene las siguientes páginas:

|Página|Descripción|
|---|---|
|PdfContentPage|Muestra un diseño principal y es el tipo de página más común en documentos PDF.|

> **NOTA:** Para el futuro, se agregarán otros tipos de páginas especializadas.

### Diseños

Los diseños en MauiPdfGenerator se usan para organizar elementos visuales en estructuras jerárquicas dentro del documento PDF. Cada diseño normalmente contiene varios elementos hijos y otros diseños anidados. Las clases de diseño contienen lógica para establecer la posición y el tamaño de los elementos secundarios en el documento PDF.

MauiPdfGenerator contiene los siguientes diseños:

|Diseño|Descripción|
|---|---|
|PdfGrid|Coloca sus elementos secundarios en una cuadrícula de filas y columnas dentro del PDF.|
|PdfHorizontalStackLayout|Coloca los elementos secundarios en una pila horizontal en el documento PDF.|
|PdfVerticalStackLayout|Coloca los elementos secundarios en una pila vertical en el documento PDF.|

### Elementos Visuales

Los elementos visuales de MauiPdfGenerator son componentes que renderizan contenido específico en el documento PDF, como párrafos, imágenes y figuras geométricas.

MauiPdfGenerator contiene los siguientes elementos visuales:

|Elemento|Descripción|
|---|---|
|PdfImage|Renderiza una imagen en el PDF que se puede cargar desde un archivo local, un URI o una secuencia.|
|PdfParagraph|Renderiza texto plano y enriquecido de una o varias líneas en el documento PDF.|
|PdfHorizontalLine|Renderiza una línea horizontal en el PDF de un punto inicial a un punto final.|

### Consideraciones de Diseño
- **Layouts** definen la estructura espacial del contenido en el PDF
- **Views** proporcionan el contenido visual específico renderizado en el documento
- **Pages** contienen la composición completa de una página del PDF
- La interactividad se omite intencionalmente, ya que los archivos PDF generados por MauiPdfGenerator se enfocan en documentos estáticos

### Aplicación Práctica

Al diseñar un PDF basado en controles MAUI:

1. **Planifica las páginas** → Define el número y tipo de páginas del documento PDF
2. **Diseña los layouts** → Estructura la organización visual del contenido en cada página
3. **Selecciona los elementos** → Elige los componentes visuales apropiados para el contenido
4. **Compón la jerarquía** → Organiza la estructura visual completa del documento

Esta analogía permite trasladar conceptos de diseño de aplicaciones MAUI a la composición de documentos PDF, manteniendo la lógica estructural pero adaptándola a un medio de documento estático.

## Alineación y Posicionamiento en PDF

### Conceptos Fundamentales

Cada elemento PDF que deriva de `PdfView` (incluyendo vistas y layouts) tiene propiedades `PdfHorizontalOptions` y `PdfHorizontalOptions` de tipo `LayoutAlignment`. Esta estructura encapsula la alineación preferida de un elemento, determinando su posición y tamaño dentro de su layout padre cuando este contiene espacio no utilizado en la página PDF.

### Alineación de Elementos en Layouts PDF

Los campos de alineación controlan cómo se posicionan los elementos dentro del documento PDF:

- **`Start`**
  - Alineación horizontal: Posiciona el elemento en el lado izquierdo del layout padre
  - Alineación vertical: Posiciona el elemento en la parte superior del layout padre

- **`Center`**
  - Centra el elemento horizontal o verticalmente dentro del layout padre

- **`End`**
  - Alineación horizontal: Posiciona el elemento en el lado derecho del layout padre
  - Alineación vertical: Posiciona el elemento en la parte inferior del layout padre

- **`Fill`**
  - Alineación horizontal: Asegura que el elemento llene el ancho disponible del layout padre
  - Alineación vertical: Asegura que el elemento llene la altura disponible del layout padre

> **Nota**: El valor predeterminado de las propiedades `PdfHorizontalOptions` y `PdfHorizontalOptions` es `LayoutAlignment.Fill`.

### Comportamiento en PdfStackLayout

Un `PdfStackLayout` solo respeta los campos `Start`, `Center`, `End` y `Fill` en elementos hijos que están en dirección opuesta a la orientación del stack:

- **PdfVerticalStackLayout**: Los elementos hijos pueden establecer sus propiedades `PdfHorizontalOptions`
- **PdfHorizontalStackLayout**: Los elementos hijos pueden establecer sus propiedades `PdfHorizontalOptions`

> **Importante**: `LayoutAlignment.Fill` generalmente anula las solicitudes de tamaño especificadas usando las propiedades `HeightRequest` y `WidthRequest`.

## Posicionamiento con Margin y Padding

### Propiedades de Espaciado

Las propiedades `Margin` y `Padding` posicionan elementos PDF relativos a elementos adyacentes o hijos:

- **`Margin`**: Representa la distancia entre un elemento PDF y sus elementos adyacentes. Controla la posición de renderizado del elemento y de sus vecinos en el documento.
- **`Padding`**: Representa la distancia entre un elemento PDF y sus elementos hijos. Separa el elemento de su propio contenido interno.

### Características del Espaciado en PDF

- Los valores de `Margin` son aditivos: dos elementos adyacentes con margen de 20 unidades tendrán una distancia de 40 unidades entre ellos en el PDF
- Los valores de margin y padding se suman cuando ambos se aplican
- Ambas propiedades son de tipo `Thickness`

### Estructura Thickness

Tres formas de crear una estructura `Thickness` para elementos PDF:

1. **Valor uniforme único**: Se aplica a los cuatro lados (izquierda, superior, derecha, inferior).
2. **Valores horizontal y vertical**: El valor horizontal se aplica simétricamente a izquierda y derecha, el vertical a superior e inferior.
3. **Cuatro valores distintos**: Se aplican específicamente a izquierda, superior, derecha e inferior.

> **Nota**: Los valores de `Thickness` pueden ser negativos, lo que típicamente recorta o superpone el contenido en el PDF.

## Analogía Final: MAUI → PDF

La transición conceptual se establece así:

- **Pages (MAUI)** → **Páginas PDF**: Cada página representa una unidad completa de contenido del documento
- **Layouts (MAUI)** → **Estructuras organizacionales PDF**: Organizan el contenido dentro de cada página del documento
- **Views (MAUI)** → **Elementos visuales PDF**: Componentes que renderizan contenido específico en el documento final

Los conceptos de alineación y posicionamiento de MAUI se traducen directamente en la disposición visual del contenido PDF dentro de las estructuras organizacionales de cada página del documento.

## 1.1. PdfContentPage

La `PdfContentPage` es el tipo de página más simple y común en MauiPdfGenerator. Su propósito es mostrar una única vista, que suele ser un layout como `PdfGrid` o `PdfStackLayout`, el cual a su vez contiene otras vistas.

### Estructura y Contenido

Una `PdfContentPage` está diseñada para contener un único elemento hijo. Para construir una interfaz visual compleja, este único elemento debe ser un layout que pueda organizar múltiples vistas hijas.

El contenido de la página se asigna a la propiedad `PdfContent`.

### Propiedades Principales

Además de las propiedades heredadas, `PdfContentPage` define las siguientes propiedades específicas:

| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `PdfContent` | `PdfView` | Define la vista única que representa el contenido de la página. |
| `Padding` | `Thickness` | Define el espacio interior entre los bordes de la página y su contenido. |

# 2. Visión General: Las Tres Capas

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

# PARTE II: ARQUITECTURA DEL SISTEMA DE LAYOUT

(El resto del documento permanece igual que en la versión anterior, ya que su estructura y contenido ya son sólidos y coherentes con esta nueva y detallada descripción de la Parte I)

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
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo en su celda asignada.

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