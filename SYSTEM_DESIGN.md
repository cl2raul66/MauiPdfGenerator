# Documentación Técnica de la Biblioteca MauiPdfGenerator

---

# Parte I: Visión General y Conceptual

**Bienvenido a la Documentación Técnica de `MauiPdfGenerator`**

Esta primera parte establece la **visión y los principios fundamentales** que guían la biblioteca. Antes de sumergirnos en la implementación técnica o en la referencia de la API, es crucial comprender el "*porqué*" detrás de su diseño. Aquí exploraremos la filosofía de ser una extensión natural de .NET MAUI, el modelo conceptual de layout y los principios arquitectónicos que garantizan una experiencia de desarrollo productiva e intuitiva. Esta sección es la base conceptual sobre la cual se construye todo lo demás.

## 1. Filosofía y Principios de Diseño

### 1.1. Principio Rector: Extensión Natural del Ecosistema .NET MAUI

Esta biblioteca está diseñada como una **extensión natural del ecosistema .NET MAUI**, no como una herramienta externa. Su propósito es permitir que los desarrolladores MAUI generen PDFs de forma nativa utilizando conceptos familiares de .NET MAUI. Para aquellos desarrolladores con experiencia en la plataforma, les resultará más cómodo el uso de esta biblioteca, ya que su confección es similar a cuando se crea una UI de .NET MAUI, pero orientada a la generación de documentos PDF.

Para lograr esta integración profunda, la biblioteca adopta directamente tipos del ecosistema MAUI, incluyendo:
- `Microsoft.Maui.Graphics.Color`
- `Microsoft.Maui.Thickness`
- `Microsoft.Maui.Controls.LayoutAlignment`
- `Microsoft.Maui.FontAttributes`
- `Microsoft.Maui.TextDecorations`
- `Microsoft.Maui.TextTransform`

El desarrollador no aprende una nueva API desde cero, simplemente aplica su conocimiento existente de MAUI a un nuevo lienzo: el documento PDF.

### 1.2. Resolución del Conflicto Conceptual: Padding vs Margins

Durante el diseño de la biblioteca surgió un conflicto fundamental entre la terminología PDF tradicional (que utiliza "margins" para el espaciado de página) y la filosofía .NET MAUI (donde `ContentPage` utiliza "Padding" para el espaciado interno).

**Decisión Arquitectónica**: Se adoptó `Padding` por las siguientes razones:

1.  **Coherencia con el Ecosistema MAUI**: La biblioteca se define como "una extensión natural del ecosistema .NET MAUI", por lo que debe mantener consistencia conceptual con los tipos de datos familiares.
2.  **Modelo Mental Unificado**: Los desarrolladores MAUI ya comprenden que `ContentPage.Padding` define el espacio interno entre el borde de la página y su contenido.
3.  **Eliminación de Redundancia**: Evita la confusión de tener tanto `Margins` como `Padding` en el mismo contexto, donde ambos conceptos serían funcionalmente idénticos.

### 1.3. Principio de Garantía de Completitud

La biblioteca se rige por el principio de **Garantía de Completitud**, que dicta que el desarrollador nunca debe estar obligado a especificar cada detalle para obtener un resultado funcional. El objetivo es permitir la creación de documentos con el mínimo código posible, confiando en que la biblioteca aplicará valores predeterminados sensibles y estéticamente agradables.

#### ¿Qué es la Garantía de Completitud?

Es la promesa arquitectónica de que cada elemento (documento, página, layout o vista) es completamente funcional desde el momento de su creación, sin necesidad de configuración adicional obligatoria. Un desarrollador puede crear un documento PDF completo con código mínimo:

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

1.  **Maximizar la Productividad**: Permite crear documentos funcionales con líneas mínimas de código.
2.  **Reducir la Curva de Aprendizaje**: El desarrollador puede comenzar a generar PDFs inmediatamente, sin estudiar todas las propiedades disponibles.
3.  **Minimizar Errores**: Elimina la posibilidad de crear elementos "vacíos" o "invisibles" por falta de configuración.
4.  **Facilitar la Iteración**: Permite al desarrollador centrarse en el contenido y la estructura, refinando el estilo progresivamente.

## 2. Jerarquía Conceptual: Pages → Layouts → Views

### 2.1. Analogía Jerárquica: MAUI → PDF

- **Pages** → `IPdfContentPage` (Páginas del documento PDF)
- **Layouts** → `IPdfVerticalStackLayout`, `IPdfHorizontalStackLayout`, `PdfGrid` (Estructuras de organización visual en PDF)
- **Views** → `IPdfParagraph`, `IPdfImage`, `IPdfHorizontalLine` (Elementos de contenido visual en PDF)

### 2.2. Pages

Los documentos PDF constan de una o varias páginas. Cada página define el lienzo sobre el cual se organiza el contenido. MauiPdfGenerator contiene los siguientes tipos de Pages:

| Interface | Descripción 
| - | - |
| IPdfContentPage | Define una página cuyo propósito es **organizar una lista de elementos de contenido (Layouts y/o Views) en una secuencia vertical continua**, gestionando automáticamente la paginación cuando el contenido excede el espacio disponible. Es el tipo de página más común en documentos PDF. |

> **NOTA:** Para el futuro, se agregarán otros tipos de páginas especializadas.

### 2.3. Layouts

Los `Layouts` en MauiPdfGenerator se usan para organizar `Views` y otros `Layouts` en estructuras jerárquicas. Son los componentes clave para construir diseños complejos y anidados. La API expone interfaces como `IPdfVerticalStackLayout` que permiten esta composición recursiva.

| Interface / Clase | Descripción |
| - | - |
| IPdfVerticalStackLayout | Coloca sus elementos hijos en una pila vertical. |
| IPdfHorizontalStackLayout | Coloca sus elementos hijos en una pila horizontal. |
| `PdfGrid` | Coloca sus elementos hijos en una cuadrícula de filas y columnas. |

### 2.4. Views

Las `Views` de MauiPdfGenerator son los componentes que renderizan contenido específico. Son los "átomos" visuales del documento y siempre actúan como nodos finales (hojas) en el árbol de composición. Se interactúa con ellas a través de interfaces como `IPdfParagraph` e `IPdfImage`.

| Interface | Descripción |
| - | - |
| IPdfImage | Renderiza una imagen que se puede cargar desde un fichero, URI o secuencia. |
| IPdfParagraph | Renderiza texto plano y enriquecido de una o varias líneas. |
| IPdfHorizontalLine | Renderiza una línea horizontal, usada comúnmente como separador. |

> **NOTA sobre Colores:** Todas las propiedades que aceptan un color (ej. `TextColor`, `BackgroundColor`) utilizan el tipo `Microsoft.Maui.Graphics.Color`. Esto permite a los desarrolladores usar las mismas constantes (`Colors.Blue`) y estructuras que ya utilizan en sus aplicaciones.

## 3. Modelo Conceptual de Layout

### 3.1. Sistema de Tres Pasadas (Concepto General)

El motor emula deliberadamente el ciclo de Medición y Disposición (Measure/Arrange) de .NET MAUI, adaptándolo a un contexto de generación de documentos asíncrono y separando explícitamente el renderizado.

*   **Fase 1: La Pasada de Medición (`MeasureAsync`)**
    *   **Responsabilidad:** Calcular el tamaño que cada `View` *desea* tener (`DesiredSize`) basándose en su contenido y las restricciones recibidas de su `Layout` padre.

*   **Fase 2: La Pasada de Disposición (`ArrangeAsync`)**
    *   **Responsabilidad:** Asignar una posición (`X`, `Y`) y un tamaño final (`Width`, `Height`) a cada `View` dentro del espacio asignado por su `Layout` padre.

*   **Fase 3: La Pasada de Renderizado (`RenderAsync`)**
    *   **Responsabilidad:** Dibujar cada `View` en su posición final usando las APIs específicas del motor de renderizado.

### 3.2. Paginación Automática (Concepto General)

Esta es una de las características más potentes del motor. El orquestador de layout no procesa el árbol una sola vez. Realiza un ciclo de `Measure`/`Arrange` para el contenido de una página. Si durante la medición detecta que el contenido excede el espacio vertical disponible, crea una nueva página y reinicia el ciclo de layout con el contenido restante.

### 3.3. Elementos Atómicos vs Divisibles

Una de las características más potentes del motor de layout es su capacidad de paginar contenido automáticamente. Para gestionar esto de manera predecible, la biblioteca clasifica cada elemento de contenido en una de dos categorías, definiendo cómo interactúa con los saltos de página:

*   **Elementos Atómicos:** `IPdfImage`, `IPdfHorizontalLine`, `IPdfVerticalStackLayout`, `IPdfHorizontalStackLayout`. Estos elementos son tratados como unidades indivisibles. Si un elemento atómico no cabe en el espacio restante de una página, el motor de layout lo moverá **completo** a la página siguiente. Nunca se dividirá o recortará a través de un salto de página. Esta regla garantiza la integridad visual de componentes como imágenes o grupos de elementos en un `StackLayout`.

*   **Elementos Divisibles:** `IPdfParagraph` y `PdfGrid`. Estos son los únicos elementos que la biblioteca puede dividir inteligentemente a través de un salto de página para crear un flujo de contenido continuo.
    *   **División de `IPdfParagraph`:** El `ParagraphRenderer` calcula cuántas líneas de texto caben en el espacio disponible. Si no caben todas, renderiza las que sí caben y pasa el texto sobrante al orquestador para que lo coloque en la página siguiente, conservando todo el formato original.
    *   **División de `PdfGrid`:** El `GridRenderer` mide sus filas secuencialmente. Si al añadir una fila se excede el alto de la página, la división ocurre **entre la fila anterior y la actual**. La fila que no cabe, junto con todas las siguientes, se mueven a la página siguiente. La división nunca ocurre a mitad de una celda.

Esta distinción es una decisión de diseño fundamental que proporciona al desarrollador un control y una previsibilidad claros sobre el flujo de sus documentos.

---

# Parte II: Arquitectura y Diseño Técnico

**Del Concepto a la Implementación**

Habiendo establecido la filosofía y los conceptos rectores en la Parte I, esta sección profundiza en el "*cómo funciona*" la biblioteca internamente. Realizaremos un análisis detallado de la **arquitectura de capas**, el flujo de datos y las decisiones de diseño técnico que hacen posible la API pública. Exploraremos el motor de layout, el sistema de paginación, la gestión de recursos y la implementación de los principios de diseño. Esta parte está dirigida a quienes deseen comprender, contribuir o extender la funcionalidad de `MauiPdfGenerator`.

## 1. Arquitectura de Capas y Flujo de Datos

La arquitectura se basa en una clara **Separación de Capas (SoC)**.

### 1.1. Capa Fluent (API Pública)

*   **Propósito:** Única puerta de entrada para el desarrollador. Su misión es ofrecer una experiencia declarativa, legible y fácil de usar a través de un conjunto de **interfaces públicas**.
*   **Responsabilidades:**
    *   **API Guiada por Interfaces:** La superficie pública de la API está compuesta enteramente por interfaces (`IPdfDocument`, `IPdfContentPage`, `IPdfParagraph`, etc.). Esto desacopla el código del cliente de los detalles de implementación, garantizando la estabilidad de la API a largo plazo.
    *   **Fluidez Contextual:** Los métodos encadenables exponen solo opciones válidas a través del patrón Type-State.
    *   **Encapsulación de Complejidad:** Las clases `builder` internas implementan las interfaces y gestionan la construcción de los modelos de datos, ocultando completamente la implementación.
    *   **Garantía de Completitud:** Es responsable de aplicar valores predeterminados sensibles a todas las propiedades opcionales. Cuando el desarrollador no especifica un valor, la capa `Fluent` asegura que el DTO correspondiente se cree con un valor válido.

#### Convenciones de Nomenclatura

- **Interfaces (`IPdf...`):** Toda la superficie pública de la API se expone a través de interfaces con el prefijo `IPdf`. Esto refuerza el **Principio de Inversión de Dependencias**, permitiendo al consumidor depender de abstracciones, no de implementaciones. Ejemplos: `IPdfDocument`, `IPdfParagraph`, `IPdfGrid`.
  
- **Builders (`...Builder`):** Las clases de implementación que construyen los modelos de datos utilizan el sufijo `...Builder`. Este patrón encapsula la complejidad de la construcción y la lógica de la API fluida, dejando clara su responsabilidad. Ejemplos: `PdfDocumentBuilder`, `PdfParagraphBuilder`, `PdfGridBuilder`.

### 1.2. Capa Core (Motor de Layout y Renderizado)

Sigue el **Principio de Inversión de Dependencias**.

#### Subcapa `Core.Integration` (Abstracciones)
*   **Propósito:** Contiene la lógica de layout independiente del motor de renderizado.
*   **Responsabilidades:** Lógica de `MeasureAsync` y `ArrangeAsync`, orquestación de pasadas y algoritmos de contenedores.

#### Subcapa `Core.Implementation.Sk` (Implementación Concreta)
*   **Propósito:** Implementa el renderizado usando SkiaSharp. Es la **primera y única implementación concreta** para la v1.0, pero la arquitectura permite que sea intercambiable.
*   **Responsabilidades:** Lógica de `RenderAsync` y gestión de recursos de SkiaSharp.

#### Convenciones de Nomenclatura

- **Renderizadores (`...Renderer`):** Las clases responsables de implementar el ciclo de tres pasadas (Medición, Disposición, Renderizado) para un elemento específico utilizan el sufijo `...Renderer`. Esto define claramente el objeto como un "hacedor" en el sistema de renderizado, facilitando la depuración y la extensibilidad. Ejemplos: `TextRenderer`, `ImageRenderer`, `GridRenderer`.
 
- **Implementación de Motor (`Sk...`):** Las clases específicas de un motor de renderizado concreto (actualmente SkiaSharp) llevan un prefijo que identifica a dicho motor. Esto permite la coexistencia de múltiples implementaciones y una arquitectura intercambiable. Ejemplo: `SkComposer`.

### 1.3. Capa Common (Contratos Compartidos)

*   **Propósito:** Define el "lenguaje común" entre capas.
*   **Contenido Principal:**
    *   **Modelos de Datos (DTOs):** Contiene las clases de modelo de datos internas, identificables por el sufijo `...Data` (ej. `PdfParagraphData`), que actúan como el contrato inmutable entre la capa `Fluent` y la capa `Core`.
    *   **Tipos Compartidos:** Value Objects, Enumeraciones (`DefaultPagePaddingType`, `PageSizeType`).
    *   **Utilidades:** Lógica de negocio compartida y sin estado, como `PagePaddingTypeCalculator`.

#### Utilidades de Cálculo Compartidas

La capa `Common` incluye utilidades que implementan lógica de negocio compartida:

- **`PaddingCalculator`**: Convierte `DefaultPagePaddingType` a valores `Thickness` específicos, centralizando los estándares de padding definidos por la biblioteca.

#### Convenciones de Nomenclatura

- **Modelos de Datos (`...Data`):** Las clases o registros que actúan como DTOs puros entre capas utilizan el sufijo `...Data`. Esto los identifica inequívocamente como contratos de datos inmutables, desprovistos de lógica de negocio, que describen el estado de un elemento. Ejemplos: `PdfParagraphData`, `PdfPageData`, `PdfGridData`.

### 1.4. Flujo de Datos y Comunicación Entre Capas

1.  **`Fluent` -> `Common`:** La API de interfaces y `builders` de la capa `Fluent` mapea las llamadas del usuario a un árbol de DTOs puros en `Common`, aplicando valores predeterminados en el proceso.
2.  **`Common` -> `Core.Integration`:** El orquestador del `Core` recibe los DTOs y crea los `IElementRenderer` correspondientes.
3.  **`Core.Integration` -> `Core.Implementation`:** Durante el layout, la capa de integración invoca a la implementación concreta para realizar el dibujado final en el lienzo.

### 1.5. Nomenclatura Arquitectónica: Un Lenguaje Común

La biblioteca no solo adopta tipos de .NET MAUI, sino también su filosofía de claridad. Para lograrlo, se establece un **lenguaje ubicuo** a través de una estricta convención de nomenclatura. Esta no es una simple guía de estilo; es un principio de diseño que garantiza que la arquitectura sea auto-descriptiva.
- **Predictibilidad como Feature:** Un desarrollador debe poder predecir la responsabilidad de una clase solo por su nombre. Esto reduce la carga cognitiva y acelera el desarrollo.
- **Consistencia Transversal:** Los patrones de nomenclatura se aplican rigurosamente en todas las capas, desde la API pública (`Fluent`) hasta el motor interno (`Core`).
- **Documentación Viva:** La nomenclatura coherente hace que el propio código actúe como una forma de documentación, reflejando fielmente los conceptos definidos en este documento.

## 2. Implementación del Sistema de Layout

### 2.1. Principios y Reglas Fundamentales de Layout

El motor de layout de MauiPdfGenerator, aunque adaptado para la generación de documentos, se basa en los mismos principios fundamentales que los sistemas de UI modernos como .NET MAUI. Comprender estas reglas es clave para predecir y controlar el comportamiento de los elementos.

*   **El Principio de Autoridad Parental:** Esta es la regla más importante del sistema de layout. Un elemento hijo (como un `IPdfParagraph`) **no se posiciona a sí mismo**. Sus propiedades de alineación y tamaño (`HorizontalOptions`, `VerticalOptions`, `WidthRequest`) son **solicitudes** que le hace a su `Layout` padre. Es el `Layout` padre, durante su pasada de `ArrangeAsync`, quien lee estas solicitudes y tiene la autoridad final para calcular y asignar la posición y el tamaño del hijo. Esta separación de responsabilidades garantiza un flujo de layout determinista y predecible de arriba hacia abajo.

*   **El Principio de Propagación de Restricciones:** Un elemento **NUNCA** decide su propio tamaño en el vacío. Siempre opera dentro de las restricciones (un tamaño disponible) que le impone su `Layout` padre. La única excepción es la `Page` raíz, cuyo espacio inicial es el tamaño de la página menos su `Padding`. La naturaleza de estas restricciones cambia según el tipo de layout:
     * *Layouts Verticales** (`PdfContentPage`, `PdfVerticalStackLayout`, en adelante VSL): Imponen una restricción de **ancho finito** a sus hijos, pero les ofrecen una altura conceptualmente **infinita** para que se midan. Por eso, un `IPdfParagraph` dentro de un VSL aplicará saltos de línea (`WordWrap`) para ajustarse al ancho dado.
    *   **Layouts Horizontales** (`PdfHorizontalStackLayout`): Imponen una restricción de **altura finita**, pero ofrecen un ancho conceptualmente **infinito**. Por eso, el mismo `IPdfParagraph` intentará renderizarse en una sola línea.
      > **NOTA:** Para ilustrar con mayor claridad: un `IPdfParagraph` con un texto largo, si se coloca dentro de un `PdfVerticalStackLayout` (ancho finito, altura infinita), aplicará saltos de línea para ajustarse al ancho disponible. Sin embargo, ese mismo `IPdfParagraph` dentro de un `PdfHorizontalStackLayout` (ancho infinito, altura finita) intentará renderizarse en una sola línea larga, potencialmente desbordando el layout si no se gestiona adecuadamente.

*   **El Modelo de Caja (Box Model):** Idéntico al modelo de MAUI y fundamental para el posicionamiento:
    *   **`Margin` (Margen):** Espacio **externo** y transparente que empuja la caja lejos de sus vecinos. No forma parte del `BackgroundColor`.
    *   **`Padding` (Relleno):** Espacio **interno** que empuja el contenido lejos del borde del elemento. El `BackgroundColor` **sí** se dibuja en esta área.

*   **El Contrato de Medición y Disposición:** El motor opera a través de un ciclo recursivo de dos fases principales antes del dibujado:
    1.  **Pasada de Medición (`MeasureAsync`)**: El padre pregunta al hijo: "dado estas restricciones, ¿qué tamaño te gustaría tener?". Esta fase se propaga de arriba hacia abajo para comunicar las restricciones, y los resultados (el tamaño deseado) se agregan de abajo hacia arriba.
    2.  **Pasada de Disposición (`ArrangeAsync`)**: El padre le dice al hijo: "basado en mi lógica, este es tu tamaño y posición final". Esta fase se propaga estrictamente de arriba hacia abajo, asignando a cada elemento su lugar definitivo en la página.

### 2.2. El Sistema de Tres Pasadas (Implementación Técnica)

*   **Fase 1: La Pasada de Medición (`MeasureAsync`)**
    *   **Ubicación Arquitectónica:** Lógica principal en `Core.Integration` (capa de abstracciones).

*   **Fase 2: La Pasada de Disposición (`ArrangeAsync`)**
    *   **Ubicación Arquitectónica:** Lógica principal en `Core.Integration`.

*   **Fase 3: La Pasada de Renderizado (`RenderAsync`)**
    *   **Ubicación Arquitectónica:** Lógica principal en `Core.Implementation.Sk` (capa de implementación).

### 2.3. Sistema de Paginación Automática (Implementación)

El mecanismo de paginación automática, especialmente para elementos divisibles, se basa en un contrato de datos claro gestionado por el orquestador de layout (`ContentPageOrquestrator`) y los renderizadores de elementos.

*   **Orquestación Iterativa:** El orquestador no procesa el árbol de elementos una sola vez. Realiza un ciclo de `Measure`/`Arrange` para el contenido de una página. Si durante la disposición de un elemento detecta que este excede el espacio vertical disponible, inicia el proceso de paginación.

*   **El Contrato `LayoutInfo`:** La comunicación entre el orquestador y los renderizadores se realiza a través de la estructura `LayoutInfo`. Esta estructura contiene no solo el tamaño y la posición final del elemento, sino también una propiedad crucial:
    ```csharp
    internal readonly record struct LayoutInfo(
        object Element,
        float Width,
        float Height,
        PdfRect? FinalRect = null,
        PdfElementData? RemainingElement = null // La clave de la paginación
    );
    ```

*   **El Patrón de "Elemento de Continuación"**:
    1.  Cuando el orquestador llama a `ArrangeAsync` en un elemento divisible (como un `IPdfParagraph`) con un espacio limitado, el renderizador del elemento es responsable de dividirse.
    2.  El renderizador calcula qué parte de su contenido cabe en el espacio proporcionado.
    3.  Crea una **nueva instancia de su propio modelo de datos** (e.g., un nuevo `PdfParagraphData`) que contiene solo el contenido sobrante, pero hereda todas las propiedades de estilo del original.
    4.  Devuelve un `LayoutInfo` donde `FinalRect` describe el espacio ocupado en la página actual, y `RemainingElement` contiene el nuevo "elemento de continuación".
    5.  El orquestador recibe este `LayoutInfo`, renderiza la parte actual, y coloca el `RemainingElement` al frente de la cola de procesamiento para la siguiente página.

Este patrón garantiza que el proceso de paginación sea sin estado, robusto y desacoplado, ya que cada ciclo de página opera sobre una nueva cola de DTOs inmutables.

## 3. Implementación de Principios de Diseño

### 3.1. Garantía de Completitud en Capa Fluent

La implementación de este principio ocurre exclusivamente en la **Capa `Fluent`** de la arquitectura. Esta capa actúa como un interceptor inteligente que:

1.  **Detecta Propiedades No Especificadas**: Cuando el desarrollador crea un elemento sin definir ciertas propiedades (ej. `FontSize`, `TextColor`).
2.  **Aplica Valores Predeterminados Inteligentes**: Consulta una jerarquía de valores predeterminados (locales del elemento → globales del documento → valores fijos de la biblioteca).
3.  **Garantiza Completitud Antes del Procesamiento**: Antes de pasar los datos al motor de renderizado, asegura que todos los DTOs estén completamente poblados.

### 3.2. Jerarquía de Resolución de Valores Predeterminados

La jerarquía de resolución de valores predeterminados sigue este orden de prioridad:
1.  **Valor Explícito**: Si el desarrollador especificó un valor, se usa ese valor.
2.  **Valor Global del Documento**: Si se configuró un valor predeterminado en `IPdfDocumentConfigurator`.
3.  **Valor Fijo de la Biblioteca**: Como último recurso, se usa un valor codificado que garantiza funcionalidad.

### 3.3. Sistema de Source Generators para Fuentes

> **NOTA CRÍTICA DE DEPENDENCIA:** MauiPdfGenerator depende obligatoriamente de `MauiPdfGenerator.SourceGenerators` para el sistema de fuentes. Esta dependencia es fundamental para la funcionalidad de identificación de fuentes con seguridad de tipos. Después de configurar las fuentes en `MauiProgram.cs` mediante `PdfConfigureFonts()`, **se debe compilar el proyecto** para que el Source Generator genere automáticamente las propiedades estáticas de tipo `PdfFontIdentifier`, haciendo disponibles referencias como `PdfFonts.OpenSansRegular`.

La biblioteca incluye un **generador de código fuente** que inspecciona las llamadas a `AddFont()` dentro del método de extensión `PdfConfigureFonts()` en `MauiProgram.cs`. Por cada fuente registrada, crea automáticamente una clase estática `public static class PdfFonts` con propiedades estáticas de tipo `PdfFontIdentifier`. Este diseño arquitectónico elimina el uso de "magic strings" (ej. `"OpenSans-Regular"`) y lo reemplaza por un acceso seguro en tiempo de compilación (ej. `PdfFonts.OpenSansRegular`), proporcionando IntelliSense y evitando errores de tipeo.

#### Gestión de Recursos de Fuentes con `FontDestinationType`

Para ofrecer un control granular sobre los recursos, el método `PdfConfigureFonts()` permite especificar el propósito del **conjunto de fuentes que se están registrando en esa llamada**. Esta es una decisión arquitectónica clave que permite al desarrollador optimizar la carga de memoria y el tamaño del paquete de la aplicación. El destino se especifica a través de la enumeración `FontDestinationType`.

| Valor de `FontDestinationType` | Descripción Arquitectónica |
| - | - |
| `OnlyUI` | Las fuentes registradas en este bloque solo estarán disponibles para la UI de .NET MAUI. El motor de PDF no las conocerá. |
| `OnlyPDF` | Las fuentes registradas en este bloque solo estarán disponibles para el motor de PDF. Es ideal para fuentes pesadas o con licencias específicas que no se necesitan en la UI. |
| `Both` | (Predeterminado) Las fuentes registradas en este bloque estarán disponibles tanto en la UI como en el motor de PDF. |

Si se omite el parámetro, la biblioteca asume de forma predeterminada el valor `FontDestinationType.Both`, siguiendo una filosofía de "valores predeterminados inteligentes" para mantener el código de configuración limpio y conciso.

#### Estándar de Nomenclatura de Aliases

Para garantizar una generación de código predecible y limpia, se recomienda seguir un estándar de nomenclatura para los alias de fuentes.

**Formato Recomendado:** `{FontFamily}{Weight}{Style}`

*   **FontFamily**: El nombre de la familia de la fuente, sin espacios y en formato PascalCase (ej. "OpenSans", "Roboto").
*   **Weight**: El peso de la fuente (ej. Regular, Light, Medium, SemiBold, Bold, ExtraBold).
*   **Style**: Opcional. Se omite para el estilo normal y se añade para variantes como `Italic` u `Oblique`.

**Ejemplos de Aliases Ideales:**
```csharp
fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
fonts.AddFont("Roboto-LightItalic.ttf", "RobotoLightItalic");
```
> Para ilustrar la importancia de este estándar, si se proporciona un alias no ideal como `"Open Sans-Regular"`, el generador lo transformará en una propiedad llamada `Open_Sans_Regular`. Siguiendo el estándar recomendado con el alias `"OpenSansRegular"`, el generador creará directamente `OpenSansRegular`, resultando en un código más limpio y predecible: `PdfFonts.OpenSansRegular`.

#### Proceso de Generación Automática

1.  **Configuración:** El desarrollador configura fuentes en `MauiProgram.cs` usando `PdfConfigureFonts()`.
2.  **Compilación:** Al compilar el proyecto, el Source Generator analiza la configuración.
3.  **Generación:** Se crea automáticamente la clase `PdfFonts` con propiedades estáticas en el fichero `PdfFonts.g.cs`.
4.  **Disponibilidad:** Las propiedades están disponibles para uso con IntelliSense en todo el proyecto.

#### Ejemplos de Código Generado

Para una configuración como:
```csharp
fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
fonts.AddFont("comic.ttf", "Comic");
```

El Source Generator produce:
```csharp
// <auto-generated/>
public static class PdfFonts
{
    public static global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier OpenSansRegular { get; } = 
        new global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier("OpenSansRegular");
    public static global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier Comic { get; } = 
        new global::MauiPdfGenerator.Fluent.Models.PdfFontIdentifier("Comic");
}
```

#### Implementación Interna del Generador (`FontAliasGenerator`)

*   **Detección**: El método `IsPotentialAddFontCall()` localiza todas las invocaciones al método `AddFont()` dentro de la configuración.
*   **Validación**: Se asegura de que los argumentos proporcionados (nombre del fichero y alias) sean literales de `string` válidos para poder ser analizados en tiempo de compilación.
*   **Transformación**: Utiliza una rutina `CreateValidCSharpIdentifier()` para convertir el alias de la fuente en un nombre de propiedad C# válido. Esta rutina:
    *   Conserva caracteres alfanuméricos (`a-z`, `A-Z`, `0-9`) y guiones bajos (`_`).
    *   Convierte caracteres comunes como guiones (`-`), espacios (` `) y puntos (`.`) en guiones bajos (`_`).
    *   Elimina guiones bajos duplicados (`__` se convierte en `_`).
    *   Añade un prefijo de guion bajo si el identificador comienza con un número (ej. `123Font` se convierte en `_123Font`).
*   **Generación**: Construye el fichero `PdfFonts.g.cs` con la clase estática y sus propiedades.

#### Manejo de Errores y Casos Borde por el Generador

*   **Aliases Duplicados**: Si se registran múltiples fuentes con el mismo alias, el generador solo considerará la primera ocurrencia para evitar errores de compilación por propiedades duplicadas.
*   **Caracteres Especiales**: La conversión a un identificador C# válido es automática. Sin embargo, seguir el estándar de nomenclatura evita resultados inesperados (ej. `"Open Sans-Regular"` se convierte en `Open_Sans_Regular`).
*   **Palabras Reservadas de C#**: Si un alias resulta en una palabra reservada (ej. `class`), el generador le antepondrá el prefijo `@` para que sea un identificador válido (ej. `@class`).
*   **Diagnóstico y Debugging**: El generador puede emitir advertencias de compilación para aliases problemáticos, ayudando al desarrollador a corregir la configuración.

## 4. Gestión de Recursos y Performance

### 4.1. Ciclo de Vida de Recursos Nativos

La biblioteca MauiPdfGenerator gestiona los recursos nativos de SkiaSharp de forma segura para evitar fugas de memoria. El ciclo de vida de todos los objetos nativos está estrictamente acotado a la duración de la llamada al método `SaveAsync(...)`.

- **Gestión de Streams y Documentos:** Internamente, la clase `SkComposer` utiliza bloques `using` para todos los objetos que implementan `IDisposable`, como `SKFileWStream` (el stream de fichero) y `SKDocument` (el documento PDF en sí). Esto garantiza que, incluso si ocurre una excepción durante la generación, los manejadores de ficheros y la memoria nativa se liberan correctamente.

- **Concurrencia:** La `IPdfDocumentFactory` está registrada como un singleton, pero es segura para su uso en hilos concurrentes. Cada llamada a `CreateDocument()` produce una instancia completamente nueva e independiente de `PdfDocumentBuilder`. Esto significa que se pueden generar múltiples documentos PDF en paralelo en hilos separados sin riesgo de interferencia, ya que cada operación de `SaveAsync` gestionará su propio conjunto aislado de recursos nativos.

### 4.2. Integración con SkiaSharp

El motor de renderizado es desacoplable, actualmente estamos usando SkiaSharp, pero puede ser cualquier otro. Se puede generar múltiples PDFs concurrentemente desde la misma instancia de `IPdfDocumentFactory`.

### 4.3. Estrategia de Intercambiabilidad de Motores

La arquitectura del `Core` está diseñada explícitamente para ser agnóstica al motor de renderizado. Este desacoplamiento se logra aplicando patrones de diseño de software establecidos como **Strategy** y **Bridge**, que permiten variar la implementación de renderizado independientemente de la API pública con la que interactúa el cliente.

El sistema se basa en un conjunto de interfaces clave que actúan como un contrato entre la lógica de layout abstracta y la implementación de dibujado concreta:

- **`IPdfCoreGenerator`**: Es la interfaz principal que expone el método `GenerateAsync`. La capa `Fluent` solo conoce esta abstracción. La clase `SkComposer` es la implementación actual (la estrategia concreta) que utiliza SkiaSharp. Para cambiar de motor, se crearía una nueva implementación (ej. `QuestPdfComposer`) y se inyectaría en el `PdfDocumentBuilder`.

- **`IElementRenderer`**: Cada elemento visual (`IPdfParagraph`, `IPdfImage`, etc.) tiene un `Renderer` asociado que implementa esta interfaz. La interfaz define el ciclo de tres pasadas (`MeasureAsync`, `ArrangeAsync`, `RenderAsync`). La lógica de `Measure` y `Arrange` es mayormente independiente del motor, mientras que `RenderAsync` contiene el código específico de la tecnología de dibujado (ej. llamadas a `SKCanvas`).

Gracias a esta separación, portar la biblioteca a otro motor de renderizado no requeriría modificar la API pública (`Fluent`) ni la lógica de orquestación de layout, sino únicamente proveer una nueva implementación de `IPdfCoreGenerator` y un nuevo conjunto de `IElementRenderer` en un nuevo ensamblado (ej. `MauiPdfGenerator.Core.Implementation.QuestPdf`).

## 5. Diseño del Sistema de Estilos y Recursos

**El Principio de Herencia de Propiedades:** 
La biblioteca implementa un sistema de herencia de dos niveles: **Local** y **Global**. Cuando se resuelve una propiedad de estilo (como la fuente o el color), el valor especificado directamente en la `View` (Local) siempre tiene prioridad. Si no existe, se utiliza el valor definido en la configuración global del documento (Global), establecida a través de `IPdfDocumentConfigurator`.

**Unidades y Medidas:**
Para mantener la coherencia con .NET MAUI, la biblioteca utiliza **unidades independientes del dispositivo** para todas las propiedades de tamaño y espaciado (`WidthRequest`, `FontSize`, `Margin`, etc.). El desarrollador trabaja con las unidades que ya conoce. Internamente, la biblioteca convierte de forma automática y transparente estas unidades a **puntos (points)**, el estándar en PDF (72 puntos por pulgada).

> **Nota Arquitectónica sobre `PdfShape`:** Internamente, elementos como `HorizontalLine` derivan de un sistema de formas más genérico. Una aplicación visible de esto es la propiedad `BackgroundColor`. Cuando se establece un `BackgroundColor` en cualquier `View`, el motor de renderizado primero dibuja una forma `PdfRectangle` en la posición y tamaño del elemento, y luego dibuja el contenido real (texto, imagen, etc.) encima. Esto asegura un comportamiento consistente y predecible del modelo de caja.

## 6. Implementación de Valores Predeterminados Inteligentes por Elemento

Por diseño arquitectónico, cada elemento de la biblioteca posee un conjunto cuidadosamente seleccionado de valores predeterminados que garantizan funcionalidad inmediata y resultados estéticamente agradables.

### 6.1. Documento PDF (`PdfDocument`)

#### Valores Predeterminados del Documento
- **`PageSize`**: `PageSizeType.A4` - Formato estándar internacional más utilizado globalmente.
- **`PageOrientation`**: `PageOrientationType.Portrait` - Orientación vertical estándar para documentos.
- **`Padding`**: `DefaultPagePaddingType.Normal` - Padding equilibrado que proporciona espacio de lectura cómodo.
- **`FontFamily`**: La biblioteca no tiene una fuente predeterminada fija y codificada. Esta afirmación es clave: en lugar de imponer una fuente como *"Helvetica"*, utiliza un sistema de **resolución jerárquica en cascada** para determinar qué fuente usar. Esto garantiza que el comportamiento por defecto siempre se alinee con las fuentes que el propio desarrollador ha proporcionado. El orden de prioridad es el siguiente:
    1.  **Fuente Específica de la `View`**: El valor establecido directamente en el elemento (p. ej. `.FontFamily(PdfFonts.Roboto)`).
    2.  **Fuente Predeterminada de la Página**: El valor configurado a nivel de `IPdfContentPage` mediante `.DefaultFont(...)`.
    3.  **Fuente Predeterminada del Documento**: La fuente designada como `.Default()` en la configuración global del documento (`.ConfigureFontRegistry(...)`).
    4.  **Fallback Automático al Primer Registro:** Como primer respaldo, si no se especifica nada en los niveles anteriores, la biblioteca utilizará la **primera fuente registrada** en `MauiProgram.cs` mediante `PdfConfigureFonts()`. Este es el "valor predeterminado" final controlado por el desarrollador.
    5.  **Fallback del Motor de Renderizado:** En el caso extremo de que el desarrollador no registre **ninguna fuente**, la biblioteca delega la responsabilidad al motor de renderizado subyacente (SkiaSharp). El motor intentará resolver una fuente de sistema disponible, que en muchos entornos de generación de PDF puede ser una fuente base como Helvetica. Este es un comportamiento de último recurso y no una elección de la biblioteca.
    
    >**NOTA:** Este sistema garantiza que la biblioteca siempre priorice las elecciones del desarrollador, proporcionando un comportamiento predecible y eliminando la necesidad de configurar fuentes en cada elemento de texto.
- **`FontSize`**: `12pt` - Tamaño estándar para texto de documento.
- **`TextColor`**: `Colors.Black` - Color de texto tradicional para máxima legibilidad.
- **`MetaData`**: Se crea automáticamente un bloque de metadatos con valores predeterminados y fijos para garantizar la conformidad y la calidad. La siguiente tabla detalla cada propiedad, su valor, la capa arquitectónica responsable y la justificación de la decisión.

| Propiedad | Valor Predeterminado / Fijo | Capa Responsable | Justificación Arquitectónica y Técnica |
| - | - | - | - |
| **`Title`** | "New PDF" | `Fluent` | **Garantía de Completitud.** Asegura que ningún documento sea anónimo, facilitando su identificación. El valor se define en la capa de API para ser independiente del motor de renderizado. |
| **`Author`** | "MauiPdfGenerator" | `Fluent` | **Identidad de la Herramienta.** Atribuye la autoría del documento (a nivel de herramienta) a la biblioteca, manteniendo la consistencia sin importar el motor. |
| **`Subject`** | `null` | `Fluent` | Se deja nulo ya que es un campo muy específico del contenido y un valor predeterminado genérico no aportaría valor. |
| **`Keywords`** | `null` | `Fluent` | Similar a `Subject`, se deja al desarrollador la responsabilidad de añadir palabras clave relevantes. |
| **`Producer`** | "MauiPdfGenerator (SkiaSharp)" | `Core.Implementation.Sk` | **Precisión Técnica (ISO 32000).** El `Producer` es el software que convierte los datos al formato PDF. Este valor es establecido por la implementación concreta del motor, reflejando con precisión la tecnología utilizada. |
| **`Modified`** | `DateTime.Now` | `Core.Implementation.Sk` | **Estándar de la Industria.** Campo no configurable. Refleja la fecha y hora de la generación del fichero, un comportamiento esperado en sistemas de ficheros. |
| **`RasterDpi`** | 300 | `Core.Implementation.Sk` | **Calidad de Impresión.** Campo no configurable en v1.0. Se fija a 300 DPI, el estándar para imágenes rasterizadas de alta calidad en documentos destinados a impresión. |
| **`EncodingQuality`** | 100 | `Core.Implementation.Sk` | **Máxima Calidad Visual.** Campo no configurable en v1.0. Se utiliza la máxima calidad (100%) para la compresión de imágenes (ej. JPEG), priorizando la fidelidad visual sobre el tamaño del fichero. |
| **`PdfA`** | `false` | `Core.Implementation.Sk` | **Compatibilidad General.** Campo no configurable en v1.0. Se establece en `false` por defecto, generando un PDF estándar. La conformidad con PDF/A (archivado a largo plazo) requiere restricciones adicionales que no son el objetivo principal en esta versión. |

### 6.2. Páginas (`PdfContentPage`)

#### Valores Predeterminados de Página
- **`BackgroundColor`**: Blanco. Por defecto, la página no especifica un color de fondo, resultando en el blanco estándar del medio PDF.

#### Justificación Arquitectónica
Las páginas actúan como contenedores neutros. Un fondo no especificado (blanco) es el comportamiento predeterminado y más versátil, evitando interferir con el diseño del contenido. Los valores de `Padding` y las propiedades de fuente se heredan de la configuración del documento, eliminando redundancia.

### 6.3. Layouts

A diferencia de las Views, los valores predeterminados de los Layouts, especialmente `HorizontalOptions` y `VerticalOptions`, no son fijos. Se rigen por un **principio de defaults contextuales**: el valor predeterminado de un elemento se determina por el tipo de `Layout` padre que lo contiene para promover un comportamiento intuitivo y reducir la verbosidad del código.

-   **En un contexto de ancho restringido (e.g., `PdfVerticalStackLayout`):** Los elementos hijos adoptan `HorizontalOptions.Fill` por defecto. Esto es intuitivo, ya que se espera que el contenido se expanda para llenar el ancho de la columna.
-   **En un contexto de ancho infinito (e.g., `PdfHorizontalStackLayout`):** Los elementos hijos adoptan `HorizontalOptions.Start` por defecto. `Fill` no tendría sentido en un espacio horizontalmente ilimitado.

Esta lógica se aplica a todos los elementos. Las siguientes secciones detallan los valores predeterminados específicos, teniendo en cuenta este principio.

#### Demostración Práctica del Comportamiento Contextual

Para ilustrar cómo funciona este principio, observemos el mismo `IPdfParagraph` dentro de dos `Layouts` diferentes sin especificar `HorizontalOptions`.

**Ejemplo 1: Párrafo dentro de un `PdfVerticalStackLayout`**

En un `VerticalStackLayout`, el contexto es de **ancho restringido**. Por lo tanto, el párrafo adopta `HorizontalOptions.Fill` por defecto para ocupar el espacio disponible.

```csharp
c.VerticalStackLayout(vsl =>
{
    // Este párrafo NO tiene .HorizontalOptions() definido.
    // Por defecto, se expandirá para llenar el ancho del VSL.
    vsl.Paragraph("Este texto se alinea en un layout vertical.")
       .BackgroundColor(Colors.LightYellow);
});
```

**Ejemplo 2: Párrafo dentro de un `PdfHorizontalStackLayout`**

En un `HorizontalStackLayout`, el contexto es de **ancho infinito**. `Fill` no es un default lógico, por lo que el párrafo adopta `HorizontalOptions.Start`, ajustándose al ancho de su contenido.

```csharp
c.HorizontalStackLayout(hsl =>
{
    // Este párrafo TAMPOCO tiene .HorizontalOptions() definido.
    // Por defecto, se ajustará al ancho de su texto y se alineará al inicio.
    hsl.Paragraph("Este texto se alinea en un layout horizontal.")
       .BackgroundColor(Colors.LightCyan);
});
```

Estos ejemplos demuestran cómo la biblioteca aplica valores predeterminados inteligentes para producir los resultados más intuitivos según el contexto del layout, reduciendo la necesidad de configuración explícita.

#### PdfVerticalStackLayout - Valores Predeterminados
- **`Spacing`**: `0` - Sin espaciado automático, permitiendo control granular del desarrollador.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa todo el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Start` - Se ajusta a la altura de su contenido.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### PdfHorizontalStackLayout - Valores Predeterminados
- **`Spacing`**: `0` - Consistente con el comportamiento vertical.
- **`HorizontalOptions`**: `LayoutAlignment.Start` - Se ajusta al ancho de su contenido.
- **`VerticalOptions`**: `LayoutAlignment.Fill` - Ocupa toda la altura disponible que le da su padre.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### PdfGrid - Valores Predeterminados
- **`RowSpacing`**: `0` - Sin espaciado entre filas por defecto.
- **`ColumnSpacing`**: `0` - Sin espaciado entre columnas por defecto.
- **`HorizontalOptions`**: `LayoutAlignment.Fill` - Ocupa todo el ancho disponible.
- **`VerticalOptions`**: `LayoutAlignment.Start` - Se ajusta a la altura de su contenido.
- **`Padding`**: `new Thickness(0)` - Sin relleno interno.
- **`Margin`**: `new Thickness(0)` - Sin margen externo.

#### Justificación Arquitectónica para Layouts
Los layouts adoptan una filosofía de "espaciado cero por defecto" para evitar espacios no deseados que puedan interferir con el diseño preciso. El desarrollador puede añadir espaciado explícitamente cuando sea necesario, manteniendo control total sobre la apariencia. Los `Options` contextuales minimizan la necesidad de configuración explícita para los casos de uso más comunes.

### 6.4. Views

#### IPdfParagraph - Valores Predeterminados
- **`FontFamily`**: Hereda del documento
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

#### IPdfImage - Valores Predeterminados
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

# Parte III: Guía de Uso y Referencia de API

**Poniendo la Biblioteca en Práctica**

Con una sólida comprensión de la filosofía (Parte I) y la arquitectura interna (Parte II), esta sección final se centra en el "*cómo se usa*". Actúa como una **guía práctica y una referencia exhaustiva de la API pública**. A través de ejemplos de código, desde la configuración inicial hasta la creación de documentos complejos, aprenderás a utilizar eficazmente cada componente. Esta es la sección principal para los desarrolladores que buscan integrar `MauiPdfGenerator` en sus aplicaciones y aprovechar todo su potencial.

## 1. Inicio Rápido: Tu Primer Documento

### 1.1. Integración con .NET MAUI: El Punto de Entrada

La biblioteca se adhiere a los patrones modernos de .NET, integrándose de forma nativa en el ecosistema de la aplicación. La inicialización se realiza en el fichero `MauiProgram.cs` mediante **dos métodos de extensión obligatorios**: `UseMauiPdfGenerator()` y `PdfConfigureFonts()`.

**`UseMauiPdfGenerator()`** registra en el contenedor de inyección de dependencias (DI) todos los servicios necesarios, principalmente la interfaz `IPdfDocumentFactory`.

**`PdfConfigureFonts()`** es **obligatorio** para el registro de fuentes y permite que el Source Generator cree la clase estática `PdfFonts`. **Después de configurar las fuentes, se debe compilar el proyecto** para que `MauiPdfGenerator.SourceGenerators` genere automáticamente las propiedades estáticas de tipo `PdfFontIdentifier`.

Este enfoque garantiza que la creación de PDFs sea una capacidad intrínseca de la aplicación, accesible desde cualquier parte de la misma de una manera limpia, desacoplada y testeable.

> **NOTA CRÍTICA DE CONFIGURACIÓN:** El orden de las llamadas de registro es fundamental. La llamada a `.UseMauiPdfGenerator()` **debe** realizarse siempre **antes** de la llamada a `.PdfConfigureFonts()`. `UseMauiPdfGenerator()` registra los servicios necesarios que `PdfConfigureFonts()` consume. Invertir el orden provocará una `InvalidOperationException` en tiempo de ejecución.

**Correcto:**
```csharp
builder
    .UseMauiPdfGenerator()
    .PdfConfigureFonts(...);
```

**Incorrecto:**
```csharp
builder
    .PdfConfigureFonts(...) // ¡ERROR!
    .UseMauiPdfGenerator();
```

#### Funcionamiento Interno de `PdfConfigureFonts`

Es importante entender que el método `PdfConfigureFonts()` no es un sistema de fuentes paralelo, sino un **envoltorio inteligente (wrapper)** del método `ConfigureFonts()` estándar de .NET MAUI. Al utilizar nuestro método, no solo registras las fuentes para la UI (si así lo deseas), sino que también permites que la biblioteca intercepte estas definiciones para alimentar el motor de PDF y el generador de código `PdfFonts`. Por lo tanto, `PdfConfigureFonts` debe ser tu **único punto de configuración de fuentes** en el proyecto.

El método `PdfConfigureFonts` acepta un segundo parámetro opcional de tipo `FontDestinationType`, que controla dónde estará disponible el **conjunto de fuentes** registrado en esa llamada.

| Valor de `FontDestinationType` | Descripción |
| - | - |
| `OnlyUI` | Todas las fuentes registradas en este bloque solo estarán disponibles para la UI de .NET MAUI. El motor de PDF no las conocerá. |
| `OnlyPDF` | Todas las fuentes en este bloque solo estarán disponibles para la generación de PDF. |
| `Both` | (Valor predeterminado) Todas las fuentes en este bloque se registran en ambos sistemas. |

Si se omite el parámetro, la biblioteca asume `FontDestinationType.Both`. Si necesitas registrar diferentes conjuntos de fuentes para diferentes destinos, simplemente **encadena múltiples llamadas a `PdfConfigureFonts`**.

#### Flujo de Desarrollo con Source Generators

1.  **Configuración Inicial:** Agregar `UseMauiPdfGenerator()` y `PdfConfigureFonts()` en `MauiProgram.cs`
2.  **Compilación Requerida:** Compilar el proyecto para generar la clase `PdfFonts`
3.  **Disponibilidad:** Las propiedades como `PdfFonts.OpenSansRegular` están disponibles con IntelliSense
4.  **Uso:** Utilizar las fuentes generadas en el código de creación de PDFs

> **NOTA CRÍTICA:** Sin la compilación posterior a `PdfConfigureFonts()`, la clase `PdfFonts` no estará disponible y el código que la reference producirá errores de compilación.

*Ejemplo de configuración en MauiProgram.cs*

```csharp
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Maui.Controls;
using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Enums; // Necesario para FontDestinationType

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
            
            // 2. Configurar fuentes que estarán disponibles tanto en la UI como en el PDF
            .PdfConfigureFonts(fonts => 
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }) // Se usa el valor por defecto: FontDestinationType.Both

            // 3. Configurar fuentes que solo se usarán para generar PDFs
            .PdfConfigureFonts(fonts =>
            {
                fonts.AddFont("comic.ttf", "Comic");
            }, FontDestinationType.OnlyPDF);

        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
```

### 1.2. La Fábrica de Documentos: Creación mediante DI
Una vez configurada en `MauiProgram.cs`, la forma recomendada de usar la biblioteca es inyectar la interfaz `IPdfDocumentFactory` en tus clases (ViewModels, servicios, etc.) a través del constructor. Esto asegura un código desacoplado y fácil de probar.

```csharp
 public class MiViewModel
 {
     private readonly IPdfDocumentFactory _pdfDocFactory;

     public MiViewModel(IPdfDocumentFactory pdfDocFactory)
     {
         _pdfDocFactory = pdfDocFactory;
     }

     public async Task GenerarInformePdf(string targetFilePath)
     {
         // Ahora la variable _pdfDocFactory está disponible para su uso.
         var doc = _pdfDocFactory.CreateDocument();
         await doc
             .ContentPage()
             .Content(c => c.Paragraph("Hola mundo desde un ViewModel."))
             .Build()
             .SaveAsync(targetFilePath);
     }
}
```

Tras la configuración, la creación de un documento se realiza solicitando la interfaz `IPdfDocumentFactory` al contenedor de DI. Esta fábrica expone el método `CreateDocument()`, que devuelve una instancia de `IPdfDocument`, el punto de partida para construir el PDF.

#### Ejemplo de Uso Mínimo (Sin Configuración)

```csharp
var doc = pdfDocFactory.CreateDocument();
await doc
    .ContentPage()
    .Content(c => c.Paragraph("Hola mundo"))
    .Build()
    .SaveAsync(targetFilePath);
```

## 2. Configuración del Documento (Opcional)

### 2.1. Configuración Global del Documento

Cada instancia de `IPdfDocument` ofrece el método `Configuration(Action<IPdfDocumentConfigurator> documentConfigurator)` para definir las características globales del documento. **Esta configuración es completamente opcional** por diseño.

#### Ejemplo de Uso con Configuración

```csharp
var doc = pdfDocFactory.CreateDocument();
await doc
    .Configuration(cfg =>
    {
        cfg.PageSize(PageSizeType.A4);
        cfg.PageOrientation(PageOrientationType.Landscape);
        cfg.Padding(DefaultPagePaddingType.Wide);
    })
    .ContentPage()
    .Content(c => c.Paragraph("Hola mundo"))
    .Build()
    .SaveAsync(targetFilePath);
```

#### Métodos de Configuración Disponibles

| Método en `IPdfDocumentConfigurator` | Descripción |
| - | - |
| `.PageSize(PageSizeType size)` | Establece el tamaño de página por defecto para todo el documento. |
| `.PageOrientation(PageOrientationType orientation)` | Define la orientación por defecto (`Portrait`/`Landscape`). |
| `.Padding(DefaultPagePaddingType paddingType)` | Aplica un conjunto de padding predefinidos (`DefaultPagePaddingType`). |
| `.Padding(float uniformPadding)` | Aplica un padding uniforme a los cuatro lados con un único valor `float`. |
| `.Padding(float verticalPadding, float horizontalPadding)` | Aplica padding diferenciados vertical y horizontal. |
| `.Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding)` | Aplica padding específicos para cada lado. |
| `.MetaData(...)` | Accede al constructor de meta datos del PDF. |
| `.ConfigureFontRegistry(...)` | Accede a al registro avanzado de fuentes (`IPdfFontRegistry`). |

#### Tipos de Padding de Página Predeterminados

La enumeración `DefaultPagePaddingType` proporciona conjuntos de padding predefinidos basados en estándares editoriales:

| Tipo | Descripción | Valores (en puntos) |
| - | - | - |
| `Normal` | Padding estándar equilibrado | 72pt en todos los lados |
| `Narrow` | Padding reducido para maximizar espacio | 36pt en todos los lados |
| `Moderate` | Padding moderado con diferenciación vertical/horizontal | Horizontal: 72pt, Vertical: 54pt |
| `Wide` | Padding amplios para documentos formales | Horizontal: 144pt, Vertical: 72pt |

### 2.2. Gestión de Fuentes (Embebido y Fuentes Predeterminadas)

Una vez registrada una fuente en `MauiProgram.cs` y generada por el Source Generator, se puede refinar su comportamiento a través de la interfaz `IPdfFontRegistry`, accesible desde la configuración del documento.

| Método en `IFontRegistrationOptions` | Descripción |
| - | - |
| `.Default()` | Designa esta fuente como la predeterminada para todo el documento. |
| `.EmbeddedFont()` | Marca la fuente para ser incrustada en el fichero PDF, garantizando su correcta visualización en cualquier sistema. |

#### Propósito Específico de ConfigureFontRegistry

El método `ConfigureFontRegistry()` es específicamente para configurar el **embebido de fuentes** y para establecer una **fuente predeterminada** para el documento. La disponibilidad de la fuente ya es automática si se registró con `FontDestinationType.Both` o `FontDestinationType.OnlyPDF` en `MauiProgram.cs`.

En caso de no establecer una fuente predeterminada explícitamente con `.Default()`, la biblioteca tomará la primera fuente registrada en `PdfConfigureFonts()` como la fuente por defecto.

*Ejemplo de configuración de registro de fuentes:*
```csharp
.Configuration(cfg =>
{
    cfg.ConfigureFontRegistry(cfr =>
    {
        // Establece OpenSansSemibold como la fuente por defecto para todo el documento.
        cfr.Font(PdfFonts.OpenSansSemibold).Default();

        // Asegura que la fuente Comic sea incrustada en el fichero PDF.
        cfr.Font(PdfFonts.Comic).EmbeddedFont();
    });
})
```

> **NOTA:** Por lógica, solo se debe establecer una fuente predeterminada para el documento. Si el usuario especifica más de una llamada a `.Default()`, la biblioteca tomará la última que fue configurada, sobrescribiendo las anteriores.

### 2.3. Enriquecimiento con Metadatos

Por diseño, cada documento PDF se crea con un conjunto robusto de metadatos para garantizar la calidad y la trazabilidad. La biblioteca distingue claramente entre dos categorías:
*   **Metadatos de Contenido:** Gestionados por el desarrollador (ej. `Title`, `Author`, `Subject`) para describir el documento. La biblioteca proporciona valores predeterminados funcionales que se recomienda sobrescribir.
*   **Metadatos Técnicos:** Fijados por la biblioteca (ej. `Producer`, `Modified`) para garantizar la integridad y el cumplimiento del estándar PDF. Estos no son configurables.

El método `.MetaData(Action<IPdfMetaData> metaDataAction)` permite **sobrescribir o complementar** los valores predeterminados configurables.

**Metadatos Gestionados por la Biblioteca:**

La biblioteca aplica automáticamente los siguientes valores, algunos de los cuales son configurables y otros fijos para garantizar la calidad.

| Propiedad | Valor Predeterminado / Fijo | Configurable | Descripción |
| - | - | - | - |
| `Title` | "New PDF" | **Sí** | Título del documento. Se recomienda establecer un título descriptivo. |
| `Author` | "MauiPdfGenerator" | **Sí** | Identifica la herramienta de autoría. Puede ser sobrescrito por el nombre del autor del contenido. |
| `Subject` | `null` | **Sí** | Asunto del documento. |
| `Keywords` | `null` | **Sí** | Palabras clave separadas por comas. |
| `CustomProperties` | `empty` | **Sí** | Colección de propiedades personalizadas. |
| `Producer` | "MauiPdfGenerator (SkiaSharp)" | No | Identifica el software que convirtió el contenido a PDF. Fijado por el motor de renderizado. |
| `Modified` | Fecha y hora actual | No | Fecha de la última modificación, establecida automáticamente durante la generación. |

> **NOTA:** Se recomienda encarecidamente establecer explícitamente los metadatos como `Title`, `Author`, y `Subject` para mejorar la indexación, accesibilidad y profesionalismo del documento PDF. Los valores predeterminados son un respaldo funcional.

*Ejemplo de cómo sobrescribir los metadatos:*

```csharp
.Configuration(cfg =>
{
    cfg.MetaData(m =>
    {
        m.Title("Informe Anual 2025");
        m.Author("Mi Empresa, Inc.");
        m.Subject("Resultados Financieros");
    });
})
```

## 3. Referencia de Componentes y API

### 3.1. Construcción de Contenido de Página

Una vez configurado el documento, se procede a definir su contenido. El proceso sigue una secuencia lógica:

1.  Se añade una página usando el método `.ContentPage()`, que devuelve una interfaz `IPdfContentPage`.
2.  Se define el contenido de la página mediante el método `.Content(Action<IPageContentBuilder> contentSetup)`. Este método proporciona un constructor de contenido (`IPageContentBuilder`) que actúa como una caja de herramientas para construir el **árbol de elementos visuales** de la página.
3.  Finalmente, se llama al método `Build()`, que devuelve un `IPdfDocument` listo para ser guardado o procesado.

El constructor de contenido (`IPageContentBuilder`) expone métodos fluidos para crear todos los elementos visuales soportados. La capacidad de anidar llamadas a constructores de `Layout` permite la creación de jerarquías de contenido complejas de manera declarativa e intuitiva.

| Método en `IPageContentBuilder` | Descripción | Retorna |
| - | - | - |
| `.Paragraph(string text)` | Añade un elemento de texto (View). | `IPdfParagraph` |
| `.Image(Stream stream)` | Añade un elemento de imagen desde un `Stream` (View).  | `IPdfImage` |
| `.HorizontalLine()` | Añade un elemento de línea horizontal (View). | `IPdfHorizontalLine` |
| `.VerticalStackLayout(...)` | Añade un layout de pila vertical (Layout). | `IPdfVerticalStackLayout` | 
| `.HorizontalStackLayout(...)` | Añade un layout de pila horizontal (Layout). | `IPdfHorizontalStackLayout` |
| `.PdfGrid()` | Añade un layout de rejilla configurable (Layout). | `PdfGrid` |

### 3.2. Pages

#### PdfContentPage
La `IPdfContentPage` es la interfaz para el tipo de `Page` más común. Su propósito es mostrar contenido visual.

#### El Comportamiento de Layout de `PdfContentPage`

Por diseño arquitectónico, `PdfContentPage` no es un simple contenedor pasivo; **su responsabilidad principal es organizar una secuencia de elementos de contenido en un flujo vertical continuo.** Este comportamiento es inherente a su tipo y es fundamental para habilitar la paginación automática de manera eficiente. Los elementos hijos directos de la página, que pueden ser una mezcla de `Views` y `Layouts`, se apilan verticalmente.

Esta decisión de diseño cumple varios objetivos:
1.  **Rendimiento**: Evita la creación de un objeto de layout (`VerticalStackLayout`) adicional e innecesario en el árbol de datos para el caso de uso más común, manteniendo el procesamiento más ligero.
2.  **Paginación Automática**: El orquestador de layout del `Core` puede operar sobre una lista simple de elementos para calcular los saltos de página, un algoritmo mucho más directo que navegar un árbol de layouts anidados.
3.  **Claridad de API**: El desarrollador no necesita pensar en el layout raíz para un documento simple, cumpliendo con el "Principio de Garantía de Completitud".

En la práctica, esto se traduce en dos escenarios de uso:

**1. Caso de Uso Común (Flujo Vertical Implícito):**
Cuando se añaden múltiples elementos (sean `Views` o `Layouts`) directamente en el constructor `.Content()`, estos se incorporan a la lista de contenido de la página. El motor de renderizado los procesará y los apilará verticalmente uno tras otro.

*Ejemplo de flujo vertical implícito con elementos mixtos:*
```csharp
// La página se encarga de apilar verticalmente el párrafo, la línea y el layout.
.ContentPage()
    .Content(c => 
    {
        c.Paragraph("Título de la sección");
        c.HorizontalLine();
        c.HorizontalStackLayout(hsl => { /* ... contenido del stack ... */ });
    })
```

**2. Caso de Uso Avanzado (Layout Único como Raíz):**
Si necesitas un control total sobre la estructura, como un espaciado específico entre elementos, un layout horizontal o una rejilla como contenedor principal, debes definir **un único `Layout` como el elemento raíz** dentro del constructor `.Content()`. En este escenario, `PdfContentPage` delega la responsabilidad de organizar el contenido a ese único elemento hijo, que ocupará todo el espacio disponible de la página.

*Ejemplo de layout explícito como raíz:*
```csharp
// Para controlar el espaciado global, el desarrollador define un VerticalStackLayout explícito.
// La página ahora solo tiene un hijo, y es este layout el que gestiona los párrafos.
.ContentPage()
    .Content(c => c.VerticalStackLayout(vsl =>
    {
        vsl.Paragraph("Párrafo 1");
        vsl.Paragraph("Párrafo 2");
    }).Spacing(12)) // El espaciado se aplica al layout explícito.
```

Este enfoque mantiene la API simple para los casos comunes, pero ofrece total flexibilidad para los casos avanzados, manteniendo siempre un modelo mental claro y predecible.

#### Propiedades Principales
| Propiedad | Tipo de Dato | Descripción |
| - | - | - |
| Propiedades de `IPdfPage<TSelf>` | Varios | Incluye métodos para configurar `Padding`, `PageSize`, `PageOrientation`, `BackgroundColor` a nivel de página. Las propiedades de layout pertenecen a los layouts hijos. |

### 3.3. Layouts (Contenedores)

Un `Layout` se utiliza para componer `Views` en una estructura visual. Una característica fundamental de los `Layouts` en MauiPdfGenerator es su **capacidad de contener no solo `Views`, sino también otros `Layouts`**. Esto permite la creación de jerarquías de diseño profundamente anidadas y complejas.

Todos los layouts se rigen por los principios de **Propagación de Restricciones** y el **Modelo de Caja** detallados en la sección de *Arquitectura y Diseño Técnico*.

#### PdfVerticalStackLayout
El `PdfVerticalStackLayout` organiza sus elementos hijos (sean `Views` u otros `Layouts`) en una única columna vertical.

> **Comportamiento de Paginación:** Este `Layout` es **atómico**. Esto tiene una implicación importante: si el `VerticalStackLayout` y todo su contenido no caben en el espacio restante de una página, la **unidad completa** se moverá a la página siguiente. La biblioteca no dividirá el contenido de un `StackLayout` a través de un salto de página. Para contenido que debe fluir y dividirse a través de las páginas, los elementos deben ser hijos directos de `IPdfContentPage` o de un `PdfGrid`.

#### Creación y Uso Práctico
Se instancia a través del método `.VerticalStackLayout(Action<IStackLayoutBuilder> content)` en un constructor de contenido. El patrón de `Action` proporciona un nuevo constructor anidado (`IStackLayoutBuilder`) para definir los elementos hijos dentro del `Layout`.

#### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| - | - | - |
| `Spacing` | `double` | Define el espacio entre cada elemento hijo. El valor predeterminado es 0. |

#### PdfHorizontalStackLayout
El `PdfHorizontalStackLayout` organiza sus elementos hijos en una única fila horizontal.

> **Comportamiento de Paginación:** Este `Layout` es **atómico**, siguiendo la misma regla "todo o nada" que el `VerticalStackLayout`.

> **Característica de Depuración (Overflow):** El `HorizontalStackLayout` incluye un mecanismo avanzado de depuración. Si durante la disposición de sus hijos detecta que un elemento excede el ancho disponible, en lugar de simplemente recortarlo o esconderlo, le indicará al renderizador de ese hijo que dibuje un estado de error visual (por ejemplo, un recuadro rojo para una imagen). Este renderizado de error se realiza en la **posición y tamaño ideales** que el elemento habría ocupado si hubiera tenido espacio suficiente, proporcionando una retroalimentación visual invaluable para corregir problemas de layout.

#### Creación y Uso Práctico
Se instancia a través del método `.HorizontalStackLayout(Action<IStackLayoutBuilder> content)` en un constructor de contenido.

#### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| - | - | - |
| `Spacing` | `double` | Define el espacio entre cada elemento hijo. El valor predeterminado es 0. |

*Ejemplo de Uso de StackLayouts Anidados:*
```csharp
c.VerticalStackLayout(vsl => 
{
    vsl.Spacing(10);
    vsl.Paragraph("Elemento 1 en VerticalStack");
    
    // Layout anidado
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
El `PdfGrid` es un `Layout` potente para mostrar `Views` y otros `Layouts` en filas y columnas.

> **Comportamiento de Paginación:** Este `Layout` es **divisible**. Si su contenido excede el espacio disponible en la página actual, la biblioteca lo dividirá automáticamente, continuando las filas restantes en la siguiente página. La división siempre ocurre entre filas, nunca a mitad de una celda.

#### Creación y Uso Práctico
Se instancia con el método `.PdfGrid()` en un constructor de contenido. La configuración de filas, columnas y la adición de hijos se realiza mediante una API fluida directamente sobre el objeto `PdfGrid` devuelto.

#### Posicionamiento y Expansión de Vistas:
Los elementos hijos se colocan en celdas específicas utilizando las propiedades adjuntas `.Row(int)` y `.Column(int)`. Para que un elemento ocupe múltiples filas o columnas, se utilizan `.RowSpan(int)` y `.ColumnSpan(int)`.

#### Definición de Tamaño:
El tamaño de las filas y columnas se controla a través de `RowDefinitions` y `ColumnDefinitions`, usando `GridLength` con valores `Auto`, un valor numérico explícito, o un valor proporcional (`Star`).

#### Propiedades clave:
| Propiedad | Tipo de Dato | Descripción |
| - | - | - |
| `RowDefinitions` | `RowDefinitionCollection` | La colección de objetos `RowDefinition` que definen las filas. |
| `ColumnDefinitions` | `ColumnDefinitionCollection` | La colección de objetos `ColumnDefinition` que definen las columnas. |
| `RowSpacing` | `double` | El espacio vertical entre las filas del grid. |
| `ColumnSpacing` | `double` | El espacio horizontal entre las columnas del grid. |

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

### 3.4. Views (Elementos Visuales)

Las `Views` son los componentes que renderizan el contenido final en la página del PDF. Son los "átomos" visuales del documento y siempre actúan como nodos finales (hojas) en el árbol de composición.

#### IPdfParagraph
La interfaz `IPdfParagraph` representa un elemento que muestra texto de una sola línea y de varias líneas.

##### Valores Predeterminados:
- `HorizontalOptions` y `VerticalOptions` se establecen en `LayoutAlignment.Fill`. 
- Otras propiedades de estilo como `FontFamily` se heredan a través de la jerarquía de resolución en cascada.

##### Comportamiento de Paginación:
Esta `View` es **divisible**. Si un párrafo no cabe completamente en el espacio restante de una página, la biblioteca lo dividirá automáticamente. Las líneas que caben se renderizarán en la página actual, y el texto restante se transferirá a la página siguiente, conservando todo el estilo original.

##### Propiedades:
- `FontFamily`: `PdfFontIdentifier`, define la familia de fuentes. Se debe utilizar la clase estática `PdfFonts` generada automáticamente.
- `FontSize`: `float`, define el tamaño de la fuente.
- `TextColor`: `Microsoft.Maui.Graphics.Color`, define el color del texto.
- `FontAttributes`: `Microsoft.Maui.Controls.FontAttributes`, determina el estilo del texto (negrita, cursiva).
- `HorizontalTextAlignment`: `Microsoft.Maui.TextAlignment`, define la alineación horizontal del texto dentro de su contenedor.
- `VerticalTextAlignment`: `Microsoft.Maui.TextAlignment`, define la alineación vertical del texto (útil cuando el párrafo tiene un `HeightRequest` fijo).
- `LineBreakMode`: `Microsoft.Maui.LineBreakMode`, determina el comportamiento de ajuste y truncamiento (`WordWrap`, `CharacterWrap`, `TailTruncation`, etc.).
- `TextDecorations`: `Microsoft.Maui.TextDecorations`, aplica decoraciones como `Underline` y `Strikethrough`.
- `TextTransform`: `Microsoft.Maui.TextTransform`, especifica la transformación a mayúsculas o minúsculas.
- Además de las propiedades de `IPdfElement` (`Margin`, `Padding`, `BackgroundColor`, etc.).

*Ejemplo de Uso de IPdfParagraph:*
```csharp
c.Paragraph("Texto simple con propiedades predeterminadas");

c.Paragraph("Texto azul, centrado y con un tamaño de fuente mayor.")
    .TextColor(Colors.Blue)
    .HorizontalTextAlignment(TextAlignment.Center)
    .FontSize(16);

c.Paragraph("Estilo Completo: Subrayado, Negrita, Itálica y fuente Comic.")
    .FontFamily(PdfFonts.Comic)
    .FontAttributes(FontAttributes.Bold | FontAttributes.Italic)
    .TextDecorations(TextDecorations.Underline);
```

#### IPdfImage
La interfaz `IPdfImage` muestra una imagen que se puede cargar desde un `Stream`.

##### Valores Predeterminados:
- `HorizontalOptions` y `VerticalOptions` se establecen en `LayoutAlignment.Fill`. 
- `Aspect` se establece en `Aspect.AspectFit`.

##### Comportamiento de Paginación:
Esta `View` es **atómica**. Si la imagen no cabe en el espacio restante de la página, se moverá completa a la página siguiente. Nunca será recortada por un salto de página.

##### Característica de Depuración (Overflow):
Para facilitar el diseño, si una imagen se coloca en un contenedor con espacio horizontal limitado (como un `HorizontalStackLayout`) y su tamaño excede el espacio disponible, el renderizador dibujará un recuadro rojo con un mensaje de error en el PDF generado. Esto proporciona una retroalimentación visual inmediata sobre problemas de layout.

##### Propiedades:
- `Aspect`: `Microsoft.Maui.Aspect`, define el modo de escalado de la imagen (`AspectFit`, `AspectFill`, `Fill`).
- Además de las propiedades de `IPdfElement` (`Margin`, `Padding`, `BackgroundColor`, etc.).

*Ejemplo de Uso de IPdfImage:*
```csharp
// Asumiendo que 'imageData' es un byte[]
c.Image(new MemoryStream(imageData))
    .WidthRequest(150)
    .HeightRequest(75)
    .Aspect(Aspect.AspectFit)
    .HorizontalOptions(LayoutAlignment.Center)
    .BackgroundColor(Colors.LightBlue);
```

#### IPdfHorizontalLine
La interfaz `IPdfHorizontalLine` es una `View` cuyo propósito es dibujar una línea horizontal, comúnmente usada como separador visual.

##### Valores Predeterminados:
- `HorizontalOptions` se establece en `LayoutAlignment.Fill`. 
- `VerticalOptions` se establece en `LayoutAlignment.Center`.

##### Comportamiento de Paginación: 
Esta `View` es **atómica**.

##### Propiedades:
- `Color`: `Microsoft.Maui.Graphics.Color`, el color de la línea.
- `Thickness`: `double`, el grosor de la línea.
- Además de las propiedades de `IPdfElement` (`Margin`, `Padding`, `BackgroundColor`, etc.).

*Ejemplo de Uso de IPdfHorizontalLine:*
```csharp
c.Paragraph("Texto sobre la línea.");
c.HorizontalLine()
    .Color(Colors.Green)
    .Thickness(3)
    .Margin(0, 10);
c.Paragraph("Texto debajo de la línea.");
```

## 4. Guías Prácticas y Patrones de Uso (Cookbook)

Esta sección profundiza en los conceptos prácticos de alineación y espaciado para ayudarte a dominar el diseño de tus documentos.

### 4.1. Alineación y Posicionamiento: El Principio de Autoridad Parental

Como se detalla en la Parte II, un elemento no se posiciona a sí mismo. Sus propiedades `HorizontalOptions` y `VerticalOptions` son **solicitudes** a su `Layout` padre.

*   **`LayoutAlignment.Fill` (Predeterminado):** La solicitud es "ocupa todo el espacio que tu padre me asigne en este eje". Por eso, un `Paragraph` dentro de un `VerticalStackLayout` se estira horizontalmente por defecto.
*   **`LayoutAlignment.Start`, `Center`, `End`:** La solicitud es "mide mi tamaño natural y luego posicióname al inicio, centro o final del espacio que mi padre me asigne".

Este principio explica por qué la alineación de un elemento siempre es relativa a su contenedor directo.

### 4.2. La Diferencia Crucial: ...Options vs. ...TextAlignment

Es fundamental entender la diferencia entre estas dos familias de propiedades para alinear el contenido correctamente.

*   **`HorizontalOptions` / `VerticalOptions`:** Estas propiedades posicionan la **caja completa del elemento** (incluyendo su `Padding` y `BackgroundColor`) dentro del espacio que le asigna su `Layout` padre.
*   **`HorizontalTextAlignment` / `VerticalTextAlignment`:** Estas propiedades posicionan el **contenido del texto** dentro de la propia caja del `IPdfParagraph`, específicamente dentro de su área de `Padding`.

Imagina dos cajas anidadas:
1.  **Caja Exterior (El Elemento):** `HorizontalOptions` la mueve a la izquierda, centro o derecha del `Layout`.
2.  **Caja Interior (El Contenido):** `HorizontalTextAlignment` mueve el texto a la izquierda, centro o derecha *dentro* de la Caja Exterior.

*Ejemplo práctico:* Para tener un texto centrado en medio de la página, necesitarías:
```csharp
c.Paragraph("Texto Centrado")
    // No es necesario, porque el VSL padre ya ocupa todo el ancho.
    // .HorizontalOptions(LayoutAlignment.Fill) 
    
    // Esto centra el TEXTO DENTRO de la caja del párrafo.
    .HorizontalTextAlignment(TextAlignment.Center);
```

Para tener una caja de color de 200px de ancho centrada en la página, con el texto alineado a la derecha dentro de ella:
```csharp
c.Paragraph("Caja centrada, texto a la derecha")
    .WidthRequest(200)
    // Esto centra la CAJA COMPLETA en el layout padre.
    .HorizontalOptions(LayoutAlignment.Center) 
    // Esto alinea el TEXTO a la derecha DENTRO de la caja de 200px.
    .HorizontalTextAlignment(TextAlignment.End) 
    .BackgroundColor(Colors.LightYellow);
```

### 4.3. Espaciado: Margin vs. Padding vs. Spacing

Estos tres mecanismos controlan el espacio en el documento, pero cada uno tiene un propósito específico.

*   **`Margin`:** Es el espacio **exterior** de un elemento. Crea un "campo de fuerza" transparente a su alrededor que empuja a los elementos vecinos. Es la herramienta principal para crear espacio *entre* dos elementos específicos.

*   **`Padding`:** Es el espacio **interior** de un elemento, entre su borde y su contenido. El `BackgroundColor` de un elemento se extiende por su área de `Padding`. Es la herramienta para dar "aire" al contenido dentro de un borde o fondo.

*   **`Spacing`:** Es una propiedad de conveniencia que solo existe en los `Layouts` (`VerticalStackLayout`, `HorizontalStackLayout`, etc.). Internamente, el `Layout` aplica un `Margin` uniforme a sus hijos para crear un espaciado consistente *entre todos* ellos. Es la herramienta para un espaciado rítmico y regular dentro de un grupo de elementos.

| Propiedad | Dónde se aplica | Propósito | Afectado por `BackgroundColor` |
| - | - | - | - |
| `Margin` | En cualquier `View` o `Layout` | Espacio **entre** este elemento y sus vecinos. | No |
| `Padding` | En cualquier `View` o `Layout` | Espacio **dentro** de un elemento. | Sí |
| `Spacing` | Solo en `Layouts` | Espacio **entre todos los hijos** de un layout. | No (porque aplica `Margin`) |