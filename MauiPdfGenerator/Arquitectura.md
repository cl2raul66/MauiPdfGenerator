# Arquitectura de la Biblioteca MauiPdfGenerator

## 1. Visión General: Las Tres Capas

La arquitectura de la biblioteca está diseñada en torno a una clara **Separación de Capas (SoC)**, cada una con una responsabilidad única y bien definida. Este enfoque garantiza la mantenibilidad, extensibilidad y la capacidad de evolucionar cada parte del sistema de forma independiente.

*   **`Fluent` (Capa de Interfaz de Usuario de la API):** Es la puerta de entrada para el desarrollador. Su única misión es ofrecer una experiencia de desarrollo declarativa, legible y fácil de usar. Está diseñada para ser lo más estable posible, protegiendo a los usuarios de cambios en la implementación interna.

*   **`Core` (Capa de Implementación/Motor):** Es el motor de bajo nivel. Su responsabilidad es tomar la descripción abstracta de un documento (proporcionada por la capa `Fluent`) y convertirla en un fichero PDF real. Esta capa está diseñada para ser **intercambiable**. La implementación actual (`Core.SkiaSharp`) podría ser reemplazada en el futuro por otras (ej. `Core.PdfSharp`) sin afectar a la capa `Fluent`.

*   **`Common` (Capa de Contrato Compartido):** Actúa como el "pegamento" o el lenguaje común que `Fluent` y `Core` utilizan para comunicarse. Contiene las definiciones, interfaces y estructuras de datos que ambas capas entienden.

## 2. Filosofía de Diseño y Principios Arquitectónicos

Además de la estructura de capas, la biblioteca se guía por los siguientes principios de diseño.

### 2.1. Diseño Guiado por Casos de Uso y Pruebas (Use Case & Test-Driven Design)

Para combatir la fragilidad del código y el efecto "arreglar un bug crea otro", hemos adoptado una disciplina de diseño más rigurosa para las funcionalidades complejas. En lugar de escribir código que solo resuelve el problema inmediato, seguimos un proceso para asegurar que la lógica sea robusta y generalista por diseño.

**El Proceso:**

1.  **Identificar y Listar los Casos de Uso:** Antes de modificar una función crítica, se realiza una lista exhaustiva de todos los escenarios posibles (casos normales, casos límite, entradas inválidas).
2.  **Definir el Comportamiento Esperado:** Para cada caso de uso, se define claramente cuál debe ser el resultado correcto.
3.  **Implementar la Lógica Generalista:** Se diseña una única pieza de lógica que satisfaga todos los casos de uso identificados. Si la lógica se vuelve demasiado compleja, es una señal para reevaluar el enfoque.
4.  **Refactorizar con Confianza:** Una vez que la lógica es correcta, se puede refactorizar para mejorar su limpieza o rendimiento, con la seguridad de que no se ha roto ninguna funcionalidad conocida.

Este principio nos mueve de escribir código que "parece funcionar" a diseñar lógica que es **correcta por construcción**.

### 2.2. Pragmatismo sobre Pureza Dogmática

Evitamos la sobre-ingeniería. No se introducen capas de abstracción o patrones complejos a menos que resuelvan un problema real y presente. Por ejemplo, el `Core` actualmente consume los modelos de `Fluent` directamente porque son suficientemente ricos, evitando una capa de DTOs y mapeo innecesaria en esta etapa.

## 3. Patrones de Diseño y Arquitecturas Implementadas

A continuación se detalla un catálogo de los patrones clave utilizados en la biblioteca, organizados por su ámbito de aplicación.

### 3.1. Patrones Globales de la Biblioteca

*   **Inyección de Dependencias (Dependency Injection):** Es el pilar de la extensibilidad. Se utiliza en `MauiProgram.cs` para registrar y resolver servicios como `IPdfDocumentFactory` e `ILoggerFactory`, desacoplando los componentes.
*   **Objeto de Contexto (Context Object):** El `PdfGenerationContext` es un ejemplo clave. Encapsula todo el estado relevante para una operación de renderizado (datos de la página, estado del layout, logger, etc.), simplificando las firmas de los métodos y centralizando el estado.
*   **Patrón de Fachada (Facade Pattern):** La interfaz `IPdfCoreGenerator` actúa como una fachada para todo el motor de renderizado. La capa `Fluent` solo interactúa con esta interfaz simple, ocultando toda la complejidad del `Core` (renderers, layout, SkiaSharp).

### 3.2. Capa `Fluent` (API Pública)

*   **Fluent Interface:** Es el patrón principal de la API. Permite encadenar llamadas a métodos de forma legible y declarativa (ej. `.Paragraph(...).TextColor(...).FontSize(...)`).
*   **Builder Pattern (variante Fluent Configuration):** Los objetos como `PdfDocumentBuilder`, `PdfContentPageBuilder` y los propios elementos (`PdfGrid`, `PdfParagraph`) actúan como builders. En lugar de constructores con muchos parámetros, se configuran paso a paso a través de métodos fluentes.
*   **Finalización de Estado Guiada por Tipos (Type-State Pattern):** Una forma sutil pero potente de guiar al usuario. Por ejemplo, el método `PdfGrid.Children(...)` devuelve una interfaz `IGridAfterChildren` que no tiene los métodos de configuración de `RowDefinitions`, impidiendo que el usuario modifique la estructura de la rejilla después de haber añadido hijos.

### 3.3. Capa `Common` (Contratos Compartidos)

*   **Data Transfer Object (DTO):** Las clases como `PdfDocumentData` y `PdfPageData` son DTOs puros. Su única responsabilidad es transportar datos de forma estructurada entre la capa `Fluent` y la capa `Core`.
*   **Value Object:** Estructuras como `PdfFontIdentifier` y `PdfGridLength` son Value Objects. Son inmutables, se definen por sus valores y tienen semántica de igualdad sobreescrita, lo que las hace seguras y predecibles.

### 3.4. Capa `Core` (Motor de Renderizado)

*   **Two-Pass Layout (Measure/Render):** Es la arquitectura fundamental del motor de layout.
    1.  **Pasada de Medición (`MeasureAsync`):** El sistema recorre el árbol de elementos de arriba hacia abajo, y cada elemento calcula cuánto espacio necesita sin dibujar nada. Esto permite que el tamaño de los hijos influya en el de los padres.
    2.  **Pasada de Dibujo (`RenderAsync`):** Una vez que se conoce el tamaño y la posición de todo, el sistema recorre el árbol de nuevo y cada elemento se dibuja en el área que se le ha asignado.
*   **Patrón de Estrategia (Strategy Pattern):** `IElementRenderer` es la interfaz de la estrategia. Cada implementación (`TextRenderer`, `ImageRenderer`, etc.) es una estrategia concreta para medir y dibujar un tipo de elemento. El motor de layout utiliza la estrategia adecuada para cada elemento sin conocer los detalles de su implementación.
*   **Patrón de Factoría (Factory Pattern):** La clase `ElementRendererFactory` es una factoría simple. Desacopla al motor de layout de la creación de los objetos `Renderer` concretos. Para añadir soporte a un nuevo elemento, solo es necesario crear su `Renderer` y registrarlo en la factoría.
