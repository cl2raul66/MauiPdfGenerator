# Documentación Técnica de la Biblioteca MauiPdfGenerator

# Parte I

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
|PdfShape|Renderiza figuras geométricas planas en el PDF.|

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

Cada elemento PDF que deriva de `PdfView` (incluyendo vistas y layouts) tiene propiedades `PdfHorizontalOptions` y `PdfVerticalOptions` de tipo `LayoutAlignment`. Esta estructura encapsula la alineación preferida de un elemento, determinando su posición y tamaño dentro de su layout padre cuando este contiene espacio no utilizado en la página PDF.

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

> **Nota**: El valor predeterminado de las propiedades `PdfHorizontalOptions` y `PdfVerticalOptions` es `LayoutAlignment.Fill`.

### Comportamiento en PdfStackLayout

Un `PdfStackLayout` solo respeta los campos `Start`, `Center`, `End` y `Fill` de `LayoutAlignment` en las vistas secundarias que están en la dirección opuesta a la orientación `PdfStackLayout`. Por lo tanto, las vistas secundarias dentro del `PdfStackLayout` con orientación vertical establecen sus propiedades `PdfHorizontalOptions` en uno de los campos `Start`, `Center`, `End` o `Fill`. De forma similar, las vistas secundarias dentro de un objeto `PdfStackLayout` con orientación horizontal pueden establecer sus propiedades `PdfVerticalOptions` en uno de los campos `Start`, `Center`, `End` o `Fill`.

`PdfStackLayout` no respeta los campos `Start`, `Center`, `End` y `Fill` de `LayoutAlignment` en las vistas secundarias que están en la misma dirección que la orientación `PdfStackLayout`. Por lo tanto, un `PdfStackLayout` con orientación vertical omite los campos `Start`, `Center`, `End` o `Fill` si se establecen en las propiedades `PdfVerticalOptions` de las vistas secundarias. De forma similar, un `PdfStackLayout` con orientación horizontal omite los campos `Start`, `Center`, `End` o `Fill` si se establecen en las propiedades `PdfHorizontalOptions` de las vistas secundarias.

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

## 1.1. Unidades de medidas: De unidades MAUI a puntos PDF

Para mantener la coherencia y la familiaridad, la biblioteca `MauiPdfGenerator` utiliza el mismo sistema de **unidades independientes del dispositivo** que .NET MAUI para todas las propiedades que definen tamaño y espaciado (`WidthRequest`, `HeightRequest`, `FontSize`, `Margin`, `Padding`, `Spacing`, etc.).

Sin embargo, el estándar para documentos PDF es el **punto (point)**, donde 72 puntos equivalen a una pulgada.

La biblioteca se encarga de realizar la **conversión de forma automática y transparente** para el desarrollador. No necesitas hacer ningún cálculo. Simplemente trabaja con las unidades a las que ya estás acostumbrado en MAUI.

### Regla de Conversión Clave

El sistema de layout de la biblioteca asume una correspondencia directa donde las unidades de MAUI se escalan para encajar en el sistema de puntos del PDF. La relación estándar es:

*   **1 unidad MAUI ≈ 1.33 puntos PDF**

### Ejemplos Prácticos

*   Si defines un `PdfParagraph` con `FontSize="12"`, el texto en el documento PDF resultante tendrá un tamaño de aproximadamente **16 puntos**.
*   Si estableces un `PdfImage` con `WidthRequest="150"`, su ancho en el PDF será de **200 puntos**.
*   Un `Margin` con valor `10` se traducirá a **13.3 puntos** de separación en el documento PDF.

Este enfoque te permite diseñar la estructura de tu documento usando la misma lógica de dimensionamiento que usarías para una interfaz de usuario en .NET MAUI, garantizando que la biblioteca genere un resultado predecible y profesional.

> **Nota:** Esta conversión se aplica a todas las propiedades de dimensionamiento: `WidthRequest`, `HeightRequest`, `FontSize`, `Margin`, `Padding`, `Spacing`, valores de `Thickness`, etc.

> **Referencia técnica:** Esta conversión está basada en el estándar PDF de 72 puntos por pulgada (DPI), asegurando compatibilidad total con visores y herramientas PDF estándar.

## 1.2. PdfContentPage

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

## 1.3. Layouts

Un layout se utiliza para componer las vistas de la interfaz visual en una estructura visual. Las clases de layout en MauiPdfGenerator derivan de la clase `PdfLayout`. La elección del layout a utilizar depende de cómo se necesite organizar y posicionar los elementos hijos.

### PdfStackLayouts

El `PdfStackLayout` organiza los elementos en una pila unidimensional, ya sea horizontal o verticalmente. En lugar de usar el `PdfStackLayout` genérico, es más eficiente utilizar directamente `PdfHorizontalStackLayout` o `PdfVerticalStackLayout`, ya que son implementaciones optimizadas.

### PdfVerticalStackLayout

El `PdfVerticalStackLayout` organiza sus vistas hijas en una única columna vertical. Es una alternativa de mayor rendimiento al `PdfStackLayout` con orientación vertical.

**Propiedades clave:**

| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Spacing` | `double` | Define el espacio entre cada vista hija. El valor predeterminado es 0. |

El posicionamiento de las vistas dentro de un `PdfVerticalStackLayout` no utiliza las propiedades adjuntas `LayoutAlignment` (`Start`, `Center`, `End`, `Fill`). Cualquier vista hija que las establezca no tendrá efecto en su posición.

### PdfHorizontalStackLayout

El `PdfHorizontalStackLayout` organiza sus vistas hijas en una única fila horizontal. Es una alternativa de mayor rendimiento al `PdfStackLayout` con orientación horizontal.

**Propiedades clave:**

| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `Spacing` | `double` | Define el espacio entre cada vista hija. El valor predeterminado es 0. |

Al igual que el `PdfVerticalStackLayout`, el `PdfHorizontalStackLayout` ignora las propiedades `LayoutAlignment` en sus vistas hijas.

### PdfGrid

El `PdfGrid` es un layout potente para mostrar vistas en filas y columnas. Su estructura se define con los objetos `RowDefinition` y `ColumnDefinition`, que determinan el tamaño de cada fila y columna.

**Posicionamiento de Vistas:**

Las vistas se colocan en celdas específicas del `PdfGrid` utilizando las propiedades adjuntas `PdfGrid.Row` y `PdfGrid.Column`.

**Expansión de Vistas:**

Para que una vista ocupe múltiples filas o columnas, se utilizan las propiedades adjuntas `PdfGrid.RowSpan` y `PdfGrid.ColumnSpan`.

**Definición de Tamaño:**

El tamaño de las filas y columnas se controla a través de la propiedad `Height` de `RowDefinition` y `Width` de `ColumnDefinition`, respectivamente. Se pueden usar tres tipos de unidades:

| Unidad | Descripción |
| :--- | :--- |
| `Auto` | La fila o columna se ajusta automáticamente al tamaño de su contenido. |
| Valor numérico explícito | Un valor fijo en unidades independientes del dispositivo (ej. `100`). |
| Proporcional (o "estrella") | Distribuye el espacio restante de forma proporcional. `*` equivale a `1*`. `2*` tomará el doble de espacio que `*`. |

**Propiedades clave:**

| Propiedad | Tipo de Dato | Descripción |
| :--- | :--- | :--- |
| `RowDefinitions` | `RowDefinitionCollection` | La colección de objetos `RowDefinition` que definen las filas. |
| `ColumnDefinitions` | `ColumnDefinitionCollection` | La colección de objetos `ColumnDefinition` que definen las columnas. |
| `RowSpacing` | `double` | El espacio vertical entre las filas del grid. |
| `ColumnSpacing` | `double` | El espacio horizontal entre las columnas del grid. |

## 1.4. PdfImage

La interfaz visual de `PdfImage` en MauiPdfGenerator muestra una imagen que se puede cargar desde un archivo local, un URI o una secuencia. El elemento `PdfImage` se utiliza para mostrar una imagen en un documento PDF. La fuente de la imagen siempre debe ser proporcionada como un `Stream`.

`PdfImage` define las siguientes propiedades:

- `Aspect`, de tipo `Aspect`, define el modo de escalado de la imagen.
- `Source`, de tipo `PdfImageSource`, especifica el origen de la imagen.

La clase `PdfImageSource` define los métodos siguientes que se pueden usar para cargar una imagen de orígenes diferentes:

- `FromFile` devuelve un `FileImageSource` que lee una imagen de un archivo local.
- `FromUri` devuelve un `UriImageSource` que descarga y lee una imagen de un URI especificado.
- `FromStream` devuelve un `StreamImageSource` que lee una imagen de un flujo que proporciona datos de imagen.

> **Importante:** Las imágenes se mostrarán en su resolución completa a menos que el tamaño del `PdfImage` esté restringido por su diseño, o que se especifique la propiedad `HeightRequest` o `WidthRequest` del `PdfImage`.

### Carga de una imagen local

Las imágenes se pueden agregar a tu proyecto de aplicación arrastrándolas a la carpeta Resources\Images, donde se establecerá automáticamente su acción de compilación en `MauiImage`. 

Para cumplir con las reglas de nomenclatura de recursos de Android, todos los nombres de archivo de imagen local deben estar en minúsculas, iniciar y terminar con un carácter de letra y contener solo caracteres alfanuméricos o caracteres de subrayado. Para obtener más información, consulte [Control de imagen en .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/image?view=net-maui-8.0).

> **Importante:** .NET MAUI convierte archivos SVG en archivos PNG. Por lo tanto, al agregar un archivo SVG al proyecto de aplicación MAUI de .NET, se debe hacer referencia desde XAML o C# con una extensión .png.

El método `PdfImageSource.FromFile` requiere un argumento string y devuelve un nuevo objeto `FileImageSource` que lee la imagen del archivo. También hay un operador de conversión implícito que permite especificar el nombre de archivo como argumento string a la propiedad `PdfImage.Source`.

### Carga de una imagen remota

Las imágenes remotas se pueden descargar y mostrar especificando un URI como valor de la propiedad `Source`. El método `PdfImageSource.FromUri` requiere un argumento `Uri` y devuelve un nuevo objeto `UriImageSource` que lee la imagen de la `Uri`.

#### Almacenamiento en caché de imágenes

El almacenamiento en caché de imágenes descargadas está habilitado de forma predeterminada, con imágenes almacenadas en caché durante 1 día. Este comportamiento se puede cambiar estableciendo las propiedades de la clase `UriImageSource`.

La clase `UriImageSource` define las siguientes propiedades:

- `Uri`, de tipo `Uri`, representa el URI de la imagen que se va a descargar para su visualización.
- `CacheValidity`, de tipo `TimeSpan`, especifica cuánto tiempo se almacenará la imagen localmente. El valor predeterminado de esta propiedad es 1 día.
- `CachingEnabled`, de tipo `bool`, define si está habilitado el almacenamiento en caché de imágenes. El valor predeterminado de esta propiedad es `true`.

Para establecer un período de caché específico, establezca la propiedad Source en un objeto `UriImageSource` que establece su propiedad `CacheValidity`.

### Carga de una imagen a partir de una secuencia

Las imágenes se pueden cargar desde secuencias con el método `PdfImageSource.FromStream`.

> **Importante:** El almacenamiento en caché de imágenes está deshabilitado en Android al cargar una imagen desde una secuencia con el método `PdfImageSource.FromStream`. Esto se debe a la falta de datos de los que crear una clave de caché razonable.

### Control del escalado de imágenes
La propiedad Aspect determina cómo se escalará la imagen para ajustarse al área de visualización y debe establecerse en uno de los miembros de la enumeración Aspect:

- `AspectFit`: los cuadros de letras de la imagen (si es necesario) para que toda la imagen se ajuste al área de visualización, con espacio en blanco agregado a la parte superior/inferior o a los lados en función de si la imagen es ancha o alta.
- `AspectFill`: recorta la imagen para que rellene el área de visualización mientras conserva la relación de aspecto.
- `Fill`: amplía la imagen para rellenar completamente y exactamente el área de visualización. Esto puede dar lugar a que la imagen se distorsiona.
- `Center`: centra la imagen en el área de visualización a la vez que conserva la relación de aspecto.

## 1.5. PdfParagraph

Un `PdfParagraph` de la interfaz visual de MauiPdfGenerator muestra texto de una sola línea y de varias líneas. El texto que se muestra puede estar coloreado, espaciado y puede tener decoraciones de texto.

La etiqueta define las siguientes propiedades:

- `CharacterSpacing`, de tipo `double`, establece el espaciado entre caracteres en el texto.
- `FontAttributes`, de tipo `FontAttributes`, determina el estilo del texto.
- `FontFamily`, de tipo `string`, define la familia de fuentes.
- `FontSize`, de tipo `double`, define el tamaño de la fuente.
- `FormattedText`, de tipo `FormattedString`, especifica la presentación del texto con varias opciones de presentación, como fuentes y colores.
- `HorizontalTextAlignment`, de tipo `TextAlignment`, define la alineación horizontal del texto mostrado.
- `LineBreakMode`, de tipo `LineBreakMode`, determina cómo se debe controlar el texto cuando no cabe en una línea.
- `LineHeight`, de tipo `double`, especifica el multiplicador que se aplicará a la altura de línea predeterminada al mostrar texto.
- `MaxLines`, de tipo `int`, indica el número máximo de líneas permitidas en la etiqueta.
- `Padding`, de tipo `Thickness`, determina el relleno de la etiqueta.
- `Text`, de tipo `string`, define el texto que se muestra como el contenido de la etiqueta.
- `TextColor`, de tipo `Color`, define el color del texto mostrado.
- `TextDecorations`, de tipo `TextDecorations`, especifica las decoraciones de texto (subrayado y tachado) que se pueden aplicar.
- `TextTransform`, de tipo `TextTransform`, especifica el uso de mayúsculas y minúsculas en el texto mostrado.
- `VerticalTextAlignment`, de tipo `TextAlignment`, define la alineación vertical del texto mostrado.

### Controlar el truncamiento y el ajuste de texto

El ajuste y el truncamiento de texto se pueden controlar estableciendo la propiedad `LineBreakMode` en un valor de la enumeración `LineBreakMode`: 

- `NoWrap` -> no ajusta el texto, mostrando solo la cantidad de texto que cabe en una línea. Este es el valor predeterminado de la propiedad.LineBreakMode
- `WordWrap` -> Ajusta el texto en el límite de la palabra.
- `CharacterWrap` -> Ajusta el texto en una nueva línea en el límite de un carácter.
- `HeadTruncation` -> trunca el encabezado del texto, mostrando el final.
- `MiddleTruncation` -> Muestra el principio y el final del texto, con el centro reemplazado por puntos suspensivos.
- `TailTruncation` -> muestra el principio del texto, truncando el final.

### Mostrar un número específico de líneas

El número de líneas que muestra un `PdfParagraph` se puede especificar estableciendo la propiedad `MaxLines` en un valor `int`:

- Cuando `MaxLinesLine` es -1, que es su valor predeterminado, Label respeta el valor de la propiedad `BreakMode` para mostrar solo una línea, posiblemente truncada, o todas las líneas con todo el texto.
- Cuando `MaxLines` es 0, el `PdfParagraph` no se muestra.
- Cuando `MaxLines` es 1, el resultado es idéntico a establecer la propiedad `LineBreakMode` en `NoWrap`, `HeadTruncation`, `MiddleTruncation`, o `TailTruncation`. Sin embargo, el `PdfParagraph` respetará el valor de la propiedad `LineBreakMode` con respecto a la colocación de puntos suspensivos, si corresponde.
- Cuando `MaxLines` es mayor que 1, el `PdfParagraph` se mostrará hasta el número especificado de líneas, respetando el valor de la propiedad `LineBreakMode` con respecto a la ubicación de una elipsis, si corresponde. Sin embargo, establecer la propiedad `MaxLines` en un valor mayor que 1 no tiene ningún efecto si la propiedad `LineBreakMode` se establece en `NoWrap`.

### Decorar texto

Las decoraciones de texto subrayado y tachado se pueden aplicar a los objetos `PdfParagraph` estableciendo la propiedad `TextDecorations` en uno o varios miembros de enumeración `TextDecorations`:

- `None`
- `Underline`
- `Strikethrough`

>  **Nota:** Las decoraciones de texto también se pueden aplicar a las instancias de `PdfSpan`. Para obtener más información sobre la clase `PdfSpan`.

### Transformar texto

`PdfParagraph` puede transformar las mayúsculas y minúsculas de su texto, almacenado en la propiedad `Text`, estableciendo la propiedad `TextTransform` en un valor de la enumeración `TextTransform`. Esta enumeración tiene estos cuatro valores:

- `None` indica que el texto no se transformará.
- `Default` indica que se utilizará el comportamiento predeterminado de la plataforma. Este es el valor predeterminado de la propiedad `TextTransform`.
- `Lowercase` indica que el texto se transformará a minúsculas.
- `Uppercase` indica que el texto se transformará a mayúsculas.

### Usar texto con formato

`PdfParagraph` expone una propiedad `FormattedText` que permite la presentación de texto con varias fuentes y colores en la misma vista. La propiedad `FormattedText` es de tipo `FormattedString`, que comprende una o más instancias de `PdfSpan`, establecidas a través de la propiedad `Spans`.

`PdfSpan` define las siguientes propiedades:

- `BackgroundColor`, de tipo `Color`, que representa el color del fondo del intervalo.
- `CharacterSpacing`, de tipo `double`, establece el espaciado entre caracteres en el texto mostrado.
- `FontAttributes`, de tipo `FontAttributes`, determina el estilo del texto.
- `FontFamily`, de tipo `string`, define la familia de fuentes.
- `FontSize`, de tipo `double`, define el tamaño de la fuente.
- `LineHeight`, de tipo `double`, especifica el multiplicador que se aplicará a la altura de línea predeterminada al mostrar texto.
- `Style`, de tipo `Style`, que es el estilo que se va a aplicar al intervalo.
- `Text`, de tipo `string`, define el texto que se muestra como el contenido del intervalo.
- `TextColor`, de tipo `Color`, define el color del texto mostrado.
- `TextDecorations`, de tipo `TextDecorations`, especifica las decoraciones de texto (subrayado y tachado) que se pueden aplicar.
- `TextTransform`, de tipo `TextTransform`, especifica el uso de mayúsculas y minúsculas en el texto mostrado.

## 1.6. PdfShape

Una forma de MauiPdfGenerator es un tipo de vista que permite dibujar una forma en la pagina. Los objetos `PdfShape` se pueden usar dentro de las clases de diseño y la mayoría de los elementos visuales, ya que la clase `PdfShape` deriva de la clase `PdfView`.

El `PdfShape` define las siguientes propiedades:

- Aspect, de tipo `Stretch`, describe cómo la forma llena su espacio asignado. El valor predeterminado de esta propiedad es `Stretch.None`.
- `Fill`, de tipo `Brush`, indica el pincel utilizado para pintar el interior de la forma.
- `Stroke`, de tipo `Brush`, indica el pincel utilizado para pintar el contorno de la forma.
- `StrokeDashArray`, de tipo `DoubleCollection`, que representa una colección de valores `double` que indican el patrón de guiones y espacios que se usan para delinear una forma.
- `StrokeDashOffset`, de tipo `double`, especifica la distancia dentro del patrón de guiones donde comienza un guión. El valor predeterminado de esta propiedad es 0,0.
- `StrokeDashPattern`, de tipo `float[]`, indica el patrón de guiones y espacios que se usan al dibujar el trazo de una forma.
- `StrokeLineCap`, de tipo `PenLineCap`, describe la forma al principio y al final de una línea o segmento. El valor predeterminado de esta propiedad es `PenLineCap.Flat`.
- `StrokeLineJoin`, de tipo `PenLineJoin`, especifica el tipo de combinación que se utiliza en los vértices de una forma. El valor predeterminado de esta propiedad es `PenLineJoin.Miter`.
- `StrokeMiterLimit`, de tipo `double`, especifica el límite en la relación entre la longitud del inglete y la mitad del `StrokeThickness` de una forma. El valor predeterminado de esta propiedad es 10.0.
- `StrokeThickness`, de tipo `double`, indica el ancho del contorno de la forma. El valor predeterminado de esta propiedad es 1.0.

### Formas de pintura

Los objetos de pincel se utilizan para pintar el trazo y el relleno de una forma. Si no especifica un objeto `Brush` para Stroke, o si establece `StrokeThickness` en 0, no se dibuja el borde alrededor de la forma.

>  **Importante:** Los objetos `Brush` utilizan un convertidor de tipos que habilita los valores de `Color` especificados para la propiedad `Stroke`.

### Formas elásticas

Los objetos `PdfShape` tienen una propiedad `Aspect` de tipo `Stretch`. Esta propiedad determina cómo se estira el contenido de un objeto `PdfShape` para rellenar el espacio de diseño del objeto. El espacio de diseño de un objeto es la cantidad de espacio que el sistema de diseño de MauiPdfGenerator asigna a `PdfShape`, ya sea por una configuración explícita de `WidthRequest` y `HeightRequest` o por su configuración `HorizontalOptions` y `VerticalOptions`.

La enumeración `Stretch` define los siguientes miembros:

- `None`, lo que indica que el contenido conserva su tamaño original. Este es el valor predeterminado de la propiedad `PdfShape.Aspect`.
- `Fill`, que indica que se ha cambiado el tamaño del contenido para rellenar las dimensiones de destino. La relación de aspecto no se conserva.
- `Uniform`, lo que indica que el contenido se redimensiona para ajustarse a las dimensiones de destino, al tiempo que se conserva la relación de aspecto.
- `UniformToFill`, indica que se cambia el tamaño del contenido para rellenar las dimensiones de destino, al tiempo que se conserva la relación de aspecto. Si la relación de aspecto del rectángulo de destino difiere del de origen, el contenido de origen se recorta para que se ajuste a las dimensiones de destino.

### Dibujar formas discontinuas

Los objetos `PdfShape` tienen una propiedad StrokeDashArray, de tipo `DoubleCollection`. Esta propiedad representa una colección de valores `double` que indican el patrón de guiones y espacios que se utilizan para delinear una forma. Una `DoubleCollection` es una `ObservableCollection` de valores `double`. Cada uno de los miembros de la colección `double` especifica la longitud de un guión o un espacio. El primer elemento de la colección, que se encuentra en el índice 0, especifica la longitud de un guión. El segundo elemento de la colección, que se encuentra en el índice 1, especifica la longitud de un espacio. Por lo tanto, los objetos con un valor de índice par especifican guiones, mientras que los objetos con un valor de índice impar especifican espacios.

Los objetos `PdfShape` también tienen una propiedad `StrokeDashOffset`, de tipo `double`, que especifica la distancia dentro del patrón de guiones donde comienza un guión. Si no se establece esta propiedad, la forma tendrá un contorno sólido.

Las formas discontinuas se pueden dibujar estableciendo las propiedades `StrokeDashArray` y `StrokeDashOffset`. La propiedad `StrokeDashArray` debe establecerse en uno o varios valores `double`, con cada par delimitado por una sola coma o uno o varios espacios. Por ejemplo, "0.5 1.0" y "0.5,1.0" son válidos.

### Finales de la línea de control

Una línea tiene tres partes: tapa de inicio, cuerpo de línea y tapa de extremo. Las mayúsculas inicial y final describen la forma al principio y al final de una línea o segmento.

Los objetos de forma tienen una propiedad `StrokeLineCap`, de tipo `PenLineCap`, que describe la forma al principio y al final de una línea o segmento. La enumeración `PenLineCap` define los siguientes miembros:

- `Flat`, que representa un límite que no se extiende más allá del último punto de la línea. Esto es comparable a ningún límite de línea y es el valor predeterminado de la propiedad `StrokeLineCap`.
- `Square`, que representa un rectángulo que tiene una altura igual al grosor de la línea y una longitud igual a la mitad del grosor de la línea.
- `Round`, que representa un semicírculo que tiene un diámetro igual al grosor de la línea.

>  **Importante:** La propiedad `StrokeLineCap` no tiene ningún efecto si se establece en una forma que no tiene puntos inicial ni final. Por ejemplo, esta propiedad no tiene ningún efecto si se establece en una elipse o un rectángulo.

### Uniones de línea de control

Los objetos `PdfShape` tienen una propiedad `StrokeLineJoin`, de tipo `PenLineJoin`, que especifica el tipo de combinación que se utiliza en los vértices de la forma. La enumeración `PenLineJoin` define los siguientes miembros:

- `Miter`, que representa vértices angulares regulares. Este es el valor predeterminado de la propiedad `StrokeLineJoin`.
- `Bevel`, que representa vértices biselados.
- `Round`, que representa vértices redondeados.

> **Nota:** Cuando la propiedad `StrokeLineJoin` se establece en `Miter`, la propiedad `StrokeMiterLimit` se puede establecer en `double` para limitar la longitud de inglete de las combinaciones de línea en la forma.

# 2. Visión General: Las Tres Capas

La arquitectura de la biblioteca está diseñada en torno a una clara **Separación de Capas (SoC)**, cada una con una responsabilidad única y bien definida. Esta separación permite evolución independiente, intercambiabilidad de componentes y mantenimiento del código a largo plazo.

## 2.1. Capa `Fluent` (API Pública)

*   **Propósito:** Única puerta de entrada para el desarrollador. Su misión es ofrecer una experiencia de desarrollo declarativa, legible y fácil de usar.
*   **Responsabilidades:**
    *   **API Guiada:** Utiliza el patrón Type-State para prevenir errores en tiempo de compilación.
    *   **Fluidez Contextual:** Métodos encadenables que exponen solo opciones válidas según el contexto.
    *   **Encapsulan de Complejidad:** Oculta completamente la implementación interna del motor.
*   **Narrativa de Transformación:** Cuando el desarrollador construye un documento, los objetos de la API Fluent (ej. `PdfParagraph`) actúan como *builders* que acumulan configuración. Al finalizar el proceso (ej. en `SaveAsync`), estos objetos se transforman en sus DTOs equivalentes de la capa `Common` (ej. `PdfParagraphData`). Esta transformación despoja a los objetos de su lógica de construcción, conservando solo la configuración pura para ser procesada por el motor.
*   **Principio de Exposición:** Esta capa es de alcance externo para la los desarrolladores que usan la biblioteca.

## 2.2. Capa `Core` (Motor de Layout y Renderizado)

El motor se subdivide siguiendo el **Principio de Inversión de Dependencias**, separando abstracciones de implementaciones concretas:

### Subcapa `Core.Integration` (Abstracciones)
*   **Propósito:** Contiene la lógica de layout independiente del motor de renderizado.
*   **Responsabilidades:**
    *   **Sistema de Layout:** Implementa las fases `MeasureAsync` y `ArrangeAsync`.
    *   **Contratos de Medición:** Define `ILayoutMetrics` para obtener métricas sin depender de implementaciones.
    *   **Orquestación:** Coordina la secuencia de pasadas de layout.
    *   **Lógica de Contenedores:** Algoritmos de distribución para `Grid`, `StackLayout`, etc.
*   **Principio de Exposición:** Esta capa es de alcance interno para la biblioteca.

### Subcapa `Core.Implementation.Sk` (Implementación Concreta)
*   **Propósito:** Implementa el renderizado específico usando un motor concreto (actualmente se usa SkiaSharp, de ahí el directorio Sk).
*   **Responsabilidades:**
    *   **Renderizado Final:** Implementa `RenderAsync` usando APIs de SkiaSharp.
    *   **Métricas Concretas:** Proporciona la implementación real de `ILayoutMetrics`.
    *   **Gestión de Recursos:** Maneja `SKTypeface`, `SKImage`, etc.
*   **Principio de Diseño:** Cada implementación es un "plugin" intercambiable que cumple los contratos definidos en `Core.Integration`.
*   **Principio de Exposición:** Esta capa es de alcance interno para la biblioteca.

### 2.3. Capa `Common` (Contratos Compartidos)

*   **Propósito:** Define el "lenguaje común" que permite la comunicación entre capas sin crear dependencias directas.
*   **Contenido Principal:**
    *   **Contratos de Datos (DTOs):** `PdfDocumentData`, `PdfPageData`, `PdfParagraphData`, etc. Transportan información estructurada y pura.
    *   **Value Objects:** Tipos inmutables (`PdfFontIdentifier`, `PdfGridLength`, `Color`) que encapsulan valores con semántica.
    *   **Enumeraciones:** Estados y opciones del sistema (`LayoutPassType`, `TextAlignment`).
    *   **Interfaces de Comunicación:** `ILayoutMetrics` para obtener métricas del motor.
    *   **Estructuras de Layout:** `LayoutRequest`, `LayoutInfo`, que definen el protocolo de comunicación del sistema de layout.
*   **Principio de Diseño:** Actúa como una "zona neutra" donde las capas pueden intercambiar información sin conocer detalles de implementación mutuos.
*   **Principio de Exposición:** Esta capa es de alcance interno para la biblioteca.

### 2.4. Flujo de datos y comunicación entre capas

1.  **`Fluent` -> `Common`:** El desarrollador usa la API `Fluent`. Al finalizar, la capa `Fluent` mapea sus objetos de construcción a un árbol de DTOs puros en la capa `Common`.
2.  **`Common` -> `Core.Integration`:** El orquestador del `Core` recibe el árbol de DTOs. El `ElementRendererFactory` lo recorre, instanciando el `IElementRenderer` apropiado para cada DTO.
3.  **`Core.Integration` <-> `Core.Implementation`:** Durante `MeasureAsync`, la lógica de layout abstracta consulta las métricas de la implementación concreta a través de la interfaz `ILayoutMetrics`.
4.  **`Core.Integration` -> `Core.Implementation`:** Una vez completadas las fases de `Measure` y `Arrange`, el "plano de layout" final se pasa a la fase `RenderAsync` de la implementación para el dibujado final.

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