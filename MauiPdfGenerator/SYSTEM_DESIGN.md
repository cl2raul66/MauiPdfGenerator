# Documentación Técnica de la Biblioteca MauiPdfGenerator

## Tabla de Contenidos

**PARTE I: ARQUITECTURA DEL SISTEMA**
- [1. Principio Rector: Extensión Natural de .NET MAUI](#1-principio-rector-extensión-natural-de-net-maui)
- [2. Visión General: Las Tres Capas](#2-visión-general-las-tres-capas)
  - [2.1. Capa `Fluent` (API Pública)](#21-capa-fluent-api-pública)
  - [2.2. Capa `Core` (Motor de Layout y Renderizado)](#22-capa-core-motor-de-layout-y-renderizado)
  - [2.3. Capa `Common` (Contratos Compartidos)](#23-capa-common-contratos-compartidos)
- [3. Filosofía y Principios de Diseño](#3-filosofía-y-principios-de-diseño)
  - [3.1. Filosofía del Motor `Core`: Robustez y Corrección por Construcción](#31-filosofía-del-motor-core-robustez-y-corrección-por-construcción)
  - [3.2. Filosofía de la API `Fluent`: Usabilidad, Seguridad y Descubribilidad](#32-filosofía-de-la-api-fluent-usabilidad-seguridad-y-descubribilidad)
- [4. Catálogo de Patrones Arquitectónicos](#4-catálogo-de-patrones-arquitectónicos)
  - [4.1. Patrones Globales](#41-patrones-globales)
  - [4.2. Patrones de la Capa `Fluent`](#42-patrones-de-la-capa-fluent)
  - [4.3. Patrones de la Capa `Common`](#43-patrones-de-la-capa-common)
  - [4.4. Patrones de la Capa `Core`](#44-patrones-de-la-capa-core)

**PARTE II: ARQUITECTURA DEL SISTEMA DE LAYOUT**
- [5. El Sistema de Layout de Tres Pasadas (Measure/Arrange/Render)](#5-el-sistema-de-layout-de-tres-pasadas-measurearrangerender)
  - [5.1. Fase 1: La Pasada de Medición (`MeasureAsync`)](#51-fase-1-la-pasada-de-medición-measureasync)
  - [5.2. Fase 2: La Pasada de Disposición (`ArrangeAsync`)](#52-fase-2-la-pasada-de-disposición-arrangeasync)
  - [5.3. Fase 3: La Pasada de Renderizado (`RenderAsync`)](#53-fase-3-la-pasada-de-renderizado-renderasync)
- [6. Principios y Reglas Fundamentales de Layout](#6-principios-y-reglas-fundamentales-de-layout)
  - [6.1. El Principio de Propagación de Restricciones](#61-el-principio-de-propagación-de-restricciones)
  - [6.2. La Dualidad de la Medición](#62-la-dualidad-de-la-medición)
  - [6.3. El Modelo de Caja (Box Model)](#63-el-modelo-de-caja-box-model)
  - [6.4. Contexto de Layout y Orquestación de Fases](#64-contexto-de-layout-y-orquestación-de-fases)
- [7. Comportamiento Detallado de Componentes](#7-comportamiento-detallado-de-componentes)
  - [7.1. Contenedores de Layout](#71-contenedores-de-layout)
  - [7.2. Elementos Primitivos](#72-elementos-primitivos)

---

# PARTE I: ARQUITECTURA DEL SISTEMA

## 1. Principio Rector: Extensión Natural de .NET MAUI

Esta biblioteca está diseñada como una **extensión natural del ecosistema .NET MAUI**, no como una herramienta externa. Su propósito es permitir que los desarrolladores MAUI generen PDFs reutilizando directamente su conocimiento existente de XAML, layouts y patrones de UI.

**Filosofía de Reutilización Conceptual:** En lugar de inventar abstracciones propias, la biblioteca adapta y extiende los conceptos nativos de MAUI al contexto PDF. Los desarrolladores encuentran `VerticalStackLayout`, `HorizontalStackLayout`, `Grid`, `Margin`, `Padding` y otros elementos familiares, comportándose exactamente como esperan pero generando contenido PDF.

**Curva de Aprendizaje Mínima:** Un desarrollador que sabe crear interfaces en XAML puede generar PDFs inmediatamente, aplicando los mismos principios de composición, layout y styling que ya domina.

## 2. Visión General: Las Tres Capas

La arquitectura de la biblioteca está diseñada en torno a una clara **Separación de Capas (SoC)**, cada una con una responsabilidad única y bien definida.

### 2.1. Capa `Fluent` (API Pública)
Es la puerta de entrada para el desarrollador. Su única misión es ofrecer una experiencia de desarrollo declarativa, legible y fácil de usar. Está diseñada para ser lo más estable posible, protegiendo a los usuarios de cambios en la implementación interna.

### 2.2. Capa `Core` (Motor de Layout y Renderizado)
Se subdivide en dos responsabilidades complementarias, siguiendo el Principio de Inversión de Dependencias:
- **`Core.Integration` (Abstracciones)**: Contiene las interfaces y la lógica de medición/disposición (`MeasureAsync` & `ArrangeAsync`) independiente del motor de renderizado. Define los contratos que cualquier motor debe cumplir e incluye la interfaz `ILayoutMetrics` para obtener métricas de renderizado (ej. tamaño de texto) sin depender de implementaciones específicas.
- **`Core.Implementation.Sk` (Implementación Concreta)**: Implementa el renderizado específico (`RenderAsync`) usando SkiaSharp y proporciona las métricas de layout concretas a través de `ILayoutMetrics`. Es intercambiable sin afectar la lógica de layout.

### 2.3. Capa `Common` (Contratos Compartidos)
Actúa como el "pegamento" o el lenguaje común que todas las capas utilizan para comunicarse. Contiene las definiciones, interfaces y estructuras de datos compartidas.

## 3. Filosofía y Principios de Diseño

La biblioteca se rige por dos conjuntos de principios complementarios: uno para el motor interno (`Core`) enfocado en la robustez, y otro para la API pública (`Fluent`) enfocado en la experiencia del desarrollador (DX).

### 3.1. Filosofía del Motor `Core`: Robustez y Corrección por Construcción

El motor de renderizado es la base de la biblioteca, y su fiabilidad es crítica.

*   **Diseño Guiado por Casos de Uso:** Para funcionalidades complejas (especialmente en el sistema de layout), seguimos un proceso riguroso:
    1.  **Identificar Casos de Uso:** Listar todos los escenarios posibles (normales, límite, inválidos).
    2.  **Definir Comportamiento Esperado:** Para cada caso, definir el resultado correcto de forma precisa.
    3.  **Implementar Lógica Generalista:** Diseñar una única pieza de lógica que satisfaga todos los casos.
    4.  **Refactorizar con Confianza:** Una vez que la lógica es correcta, se puede mejorar con seguridad.
    Este principio nos mueve de escribir código que "parece funcionar" a diseñar lógica que es **correcta por construcción**.

*   **Pragmatismo sobre Pureza Dogmática:** Evitamos la sobre-ingeniería. No se introducen capas de abstracción o patrones complejos a menos que resuelvan un problema real y presente en el motor.

### 3.2. Filosofía de la API `Fluent`: Usabilidad, Seguridad y Descubribilidad

La API pública es la cara de la biblioteca. Su diseño se centra en hacer que la creación de PDFs sea una tarea intuitiva, segura y agradable.

*   **API Guiada (Guided API):** Utilizamos el sistema de tipos de C# para guiar al desarrollador y prevenir errores en tiempo de compilación, no en tiempo de ejecución.
    *   **Ejemplo Clave:** El patrón **Estado Guiado por Tipos (Type-State)**. El método `PdfGrid.Children()` no devuelve `PdfGrid`, sino `IGridAfterChildren`, una interfaz que no expone métodos para redefinir columnas o filas. Esto hace que sea *imposible* para el desarrollador modificar la estructura de la rejilla después de haber añadido contenido, eliminando una clase entera de posibles bugs.

*   **Fluidez Específica y Descubribilidad (Specific Fluidity & Discoverability):** El encadenamiento de métodos debe ser natural y relevante.
    *   **Ejemplo Clave:** Ocultación de métodos con `new`. Clases como `PdfParagraph` o `PdfHorizontalLine` re-implementan métodos como `.Margin()` para devolver su propio tipo (`PdfParagraph`) en lugar del tipo base (`PdfElement`). Esto mejora la experiencia de desarrollo (DX) al evitar `casts` innecesarios y asegurar que IntelliSense solo muestre los métodos aplicables al elemento actual (ej: `.FontSize()` en un párrafo, pero no en una línea horizontal).

*   **Coherencia con el Ecosistema MAUI:** La API debe comportarse como una extensión natural del framework, manteniendo las mismas convenciones de nomenclatura, patrones de configuración y comportamientos de layout que los desarrolladores ya conocen.

*   **Defaults Inteligentes y Contextuales (Smart & Contextual Defaults):** La API debe minimizar la verbosidad y tomar decisiones lógicas por el desarrollador cuando sea posible.
    *   **Ejemplo Clave:** La clase `PdfLayoutDefaultOptions` proporciona comportamientos por defecto que dependen del contexto. Un elemento añadido directamente a una página (`ContentPage`) se expandirá horizontalmente por defecto (`Fill`), mientras que el mismo elemento dentro de un `HorizontalStackLayout` adoptará su tamaño natural (`Start`). Esto libera al desarrollador de tener que especificar explícitamente las opciones de layout más comunes.

## 4. Catálogo de Patrones Arquitectónicos

### 4.1. Patrones Globales
*   **Inyección de Dependencias (DI):** Sigue el patrón estándar de .NET MAUI, integrándose directamente en `MauiProgram.cs` con métodos de extensión familiares como `UseMauiPdfGenerator()` y `PdfConfigureFonts()`.
*   **Objeto de Contexto (Context Object):** El `PdfGenerationContext` encapsula todo el estado relevante para una operación de renderizado, simplificando las firmas de los métodos.
*   **Fachada (Facade Pattern):** La interfaz `IPdfCoreGenerator` actúa como una fachada simple para el complejo motor de renderizado.

### 4.2. Patrones de la Capa `Fluent`
*   **Interfaz Fluida (Fluent Interface):** Patrón principal de la API que permite encadenar métodos de forma legible.
*   **Builder Pattern Progresivo:** Objetos como `PdfDocumentBuilder` y `PdfGrid` usan transiciones de tipo (ej: `PdfGrid` → `IGridWithStructure` → `IGridAfterChildren`), donde cada interfaz expone solo los métodos válidos para esa fase de construcción.
*   **Estado Guiado por Tipos (Type-State Pattern):** Usa el sistema de tipos para guiar al usuario y prevenir configuraciones inválidas en diferentes fases de construcción.

### 4.3. Patrones de la Capa `Common`
*   **DTO (Data Transfer Object):** Clases como `PdfDocumentData` y `PdfPageData` transportan datos de forma estructurada entre capas.
*   **Value Object:** `structs` inmutables como `PdfFontIdentifier` y `PdfGridLength` que se definen por sus valores.
*   **Contrato de Medición (`LayoutRequest`):** `struct` que encapsula la "pregunta" de un contenedor a un hijo, comunicando el espacio disponible y la intención de la medición.

### 4.4. Patrones de la Capa `Core`
*   **Estrategia (Strategy Pattern):** `IElementRenderer` es la interfaz de la estrategia. Cada implementación (`TextRenderer`, `ImageRenderer`) es una estrategia concreta para medir y dibujar un tipo de elemento.
*   **Factoría (Factory Pattern):** La clase `ElementRendererFactory` desacopla al motor de la creación de los `Renderer` concretos.

---

# PARTE II: ARQUITECTURA DEL SISTEMA DE LAYOUT

## 5. El Sistema de Layout de Tres Pasadas (Measure/Arrange/Render)

El motor emula deliberadamente el **ciclo de Medición y Disposición (Measure/Arrange) de .NET MAUI**, adaptándolo a un contexto de generación de documentos asíncrono y separando explícitamente el renderizado.

### 5.1. Fase 1: La Pasada de Medición (`MeasureAsync`)
*   **Responsabilidad:** Calcular el tamaño que cada elemento *desea* tener (`DesiredSize`).
*   **Equivalencia MAUI:** Es el análogo directo de la pasada `Measure` de MAUI.
*   **Proceso:** El sistema recorre el árbol de elementos de arriba hacia abajo. Cada elemento recibe restricciones de su padre y devuelve un `LayoutInfo` con su tamaño deseado.
*   **Adaptación Clave:** Es **asíncrono** para manejar operaciones de I/O (ej. leer un stream de imagen) sin bloquear. El resultado es un `LayoutInfo` que contiene el tamaño deseado del elemento incluyendo su contenido y padding, pero **excluyendo el margin** (que será gestionado por el padre en la fase de disposición).

### 5.2. Fase 2: La Pasada de Disposición (`ArrangeAsync`)
*   **Responsabilidad:** Asignar una posición y un tamaño final y concreto a cada elemento.
*   **Equivalencia MAUI:** Es el análogo directo de la pasada `Arrange` de MAUI.
*   **Proceso:** Conociendo los tamaños deseados, el sistema recorre el árbol de nuevo. Cada padre posiciona a sus hijos dentro de su propio espacio, asignándoles un rectángulo final. El resultado es un "plano de layout" completo.
*   **Gestión del Margin:** Durante esta fase, cada contenedor padre es responsable de considerar el `Margin` de sus hijos al calcular sus posiciones finales.
*   **Estado:** En este punto, todo está medido y posicionado, pero **nada se ha dibujado todavía**.

### 5.3. Fase 3: La Pasada de Renderizado (`RenderAsync`)
*   **Responsabilidad:** Dibujar cada elemento en su posición final.
*   **Equivalencia MAUI:** Es análogo al ciclo de dibujado de la plataforma nativa (Android, iOS).
*   **Proceso:** Esta es la única fase que depende del motor de renderizado concreto (`Core.Implementation.Sk`). Recorre el "plano de layout" generado en la fase anterior y utiliza las APIs específicas del motor (ej. `SKCanvas`) para dibujar los elementos en el PDF.

## 6. Principios y Reglas Fundamentales de Layout

### 6.1. El Principio de Propagación de Restricciones
**Heredado directamente de MAUI:**
*   **Universo Finito:** Un elemento **NUNCA** asume su tamaño. Siempre opera dentro de un espacio finito definido por su contenedor padre.
*   **Propagación Obligatoria:** Un elemento **SIEMPRE** recibe un "espacio disponible" de su padre para poder medirse.
*   **Elemento Raíz:** La única excepción es `ContentPage`, cuyo espacio disponible inicial es el tamaño de la página del PDF.

### 6.2. La Dualidad de la Medición
**Adaptación del sistema de restricciones de MAUI:**
Un elemento primitivo debe ser capaz de responder a dos tipos de "preguntas de medición", comunicadas a través del contrato `LayoutRequest`.

*   **Pregunta de Medición Restringida (`LayoutPassType.Constrained`):**
    *   **Intención:** "Adáptate a este **ancho finito** y dime qué altura necesitas (aplicando saltos de línea si es necesario)".
    *   **Quién la hace:** Contenedores de naturaleza vertical como `ContentPage` y `VerticalStackLayout`.

*   **Pregunta de Medición Ideal (`LayoutPassType.Ideal`):**
    *   **Intención:** "Usa el ancho máximo disponible sin aplicar wrapping. Dime cuál es tu tamaño **natural** en una sola línea".
    *   **Quién la hace:** Contenedores de naturaleza horizontal como `HorizontalStackLayout`.

### 6.3. El Modelo de Caja (Box Model)
**Idéntico al modelo de MAUI:**
*   **`Margin` (Margen):** Espacio **externo** y transparente que empuja la caja lejos de sus vecinos. No forma parte del `BackgroundColor`. Es gestionado exclusivamente por el contenedor padre durante la pasada de `ArrangeAsync`, quien lo considera al calcular las posiciones finales de sus hijos.
*   **`Padding` (Relleno):** Espacio **interno** que empuja el contenido lejos del borde. El `BackgroundColor` **sí** se dibuja en esta área.
*   **Huella de Elemento:** El tamaño devuelto por `MeasureAsync` (`LayoutInfo`) representa la huella del `Contenido + Padding`, **excluyendo el Margin**. Esta separación permite que el contenedor padre maneje el spacing entre elementos de forma independiente.

### 6.4. Contexto de Layout y Orquestación de Fases
*   **Propagación de Contexto:** El sistema utiliza un `LayoutContext` que se propaga recursivamente por el árbol de elementos. Este objeto transporta información del contenedor padre, permitiendo a los hijos determinar sus comportamientos por defecto apropiados (ej. `PdfLayoutDefaultOptions`).
*   **Orquestación de Fases:** Un orquestador central (o máquina de estados) es responsable de invocar la secuencia de pasadas en el orden correcto para todo el árbol de elementos.
*   **Flujo de Ejecución:** Las tres fases se ejecutan secuencialmente para todo el árbol: primero se completa `MeasureAsync` para todos los elementos, luego `ArrangeAsync` para todos, y finalmente `RenderAsync`. Esta separación garantiza que cada elemento tenga toda la información necesaria (tamaños y posiciones) antes de proceder al renderizado.

## 7. Comportamiento Detallado de Componentes

### 7.1. Contenedores de Layout

#### `PdfVerticalStackLayout`
**Emula `VerticalStackLayout` de MAUI:**
1.  **`MeasureAsync`:** Hace una **Pregunta de Medición Restringida** a cada hijo. Su altura deseada es la suma de las alturas de los hijos.
2.  **`ArrangeAsync`:** Posiciona a sus hijos uno debajo del otro, considerando sus `Margin` y alineándolos horizontalmente según sus `HorizontalOptions`.
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo en su rectángulo final asignado. Aplica `ClipRect` si el contenido se desborda.

#### `PdfHorizontalStackLayout`
**Emula `HorizontalStackLayout` de MAUI:**
1.  **`MeasureAsync`:** Hace una **Pregunta de Medición Ideal** a cada hijo. Su ancho deseado es la suma de los anchos de los hijos.
2.  **`ArrangeAsync`:** Posiciona a sus hijos uno al lado del otro, considerando sus `Margin` y alineándolos verticalmente según sus `VerticalOptions`.
3.  **`RenderAsync`:** Orquesta la llamada al `RenderAsync` de cada hijo, aplicando `ClipRect` si el contenido se desborda.

### 7.2. Elementos Primitivos

#### `TextRenderer` / `ImageRenderer` / `HorizontalLineRender`
**Comportamiento equivalente a los primitivos de MAUI:**
1.  **`MeasureAsync`:** Responde a la pregunta (`Constrained` o `Ideal`). Calcula el tamaño de su contenido, le suma su `Padding`, y devuelve un `LayoutInfo` con su huella de elemento (`Contenido + Padding`).
2.  **`ArrangeAsync`:** Acepta el rectángulo final que su padre le asigna. No tiene hijos que organizar, por lo que esta fase es trivial para ellos.
3.  **`RenderAsync`:** Recibe un `renderRect` con su tamaño y posición final (ya con el `Margin` descontado por el padre). Calcula su `contentBox` restando su propio `Padding` del `renderRect` y dibuja su fondo y contenido dentro de ese espacio usando las APIs del motor.