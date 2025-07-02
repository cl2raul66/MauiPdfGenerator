
### **Análisis Arquitectónico**

#### **1. Las Fronteras: `Fluent`, `Core` y `Common`**

Tu definición de las fronteras es perfecta:

*   **`Fluent` (Capa de Interfaz de Usuario de la API):** Es la puerta de entrada. Su única misión es ofrecer una experiencia de desarrollo declarativa y legible. Está diseñada para ser lo más estable posible.
*   **`Core` (Capa de Implementación/Motor):** Es el motor de bajo nivel. Su responsabilidad es tomar una descripción abstracta de un documento y convertirla en un fichero PDF real. Esta capa está diseñada para ser **intercambiable**. Hoy usa SkiaSharp, pero la arquitectura permitiría crear un `Core.PdfSharp` o `Core.QuestPDF` en el futuro sin tocar `Fluent`.
*   **`Common` (Capa de Contrato Compartido):** Este es el "pegamento" o el lenguaje común que `Fluent` y `Core` utilizan para comunicarse. Contiene las definiciones y estructuras de datos que ambas capas entienden.

#### **2. El Uso Directo de los Modelos `Fluent` por el `Core`**

Aquí has tocado el punto clave de la decisión de diseño actual:

> "sucede que los modelos de fluent son usados por core directamente sin ponerlos en common es porque el core no necesita unas propiedades extras o especificas que requieran de crear un mapeo para uso interno"

**Correcto.** En la etapa actual del proyecto (MVP), los modelos definidos en `Fluent.Models` (como `PdfParagraph`, `PdfGrid`) son suficientemente ricos para que el `Core` pueda trabajar con ellos directamente. Crear una capa de DTOs (Data Transfer Objects) en `Common` y un proceso de mapeo entre `Fluent.Models` y `Common.DTOs` habría introducido lo que se conoce como **"sobre-ingeniería" (over-engineering)** en esta fase.

La arquitectura actual es pragmática: evita una capa de abstracción que, por ahora, no aportaría ningún valor y solo añadiría complejidad.

#### **3. Estrategias para la Evolución Futura del Modelo**

Tu visión sobre cómo evolucionar es, de nuevo, la correcta. Si en el futuro el `Core` necesita información que no tiene sentido exponer en la API `Fluent`, tenemos varias opciones, tal y como has delineado:

*   **Opción 1: Mapeo a Modelos del Core (DTOs)**
    *   **Cuándo usarlo:** Si el `Core` necesita una representación de los elementos muy diferente a la del `Fluent`. Por ejemplo, si un futuro motor de renderizado requiere que los datos estén en una estructura aplanada o con referencias numéricas en lugar de objetos.
    *   **Implementación:**
        1.  Crearíamos un nuevo conjunto de modelos en `Core.Models` (ej. `CoreParagraph`, `CoreImage`).
        2.  En el `PdfDocumentBuilder` o en el `SkComposer`, justo antes de empezar la generación, se ejecutaría un proceso de mapeo que convierte el árbol de `Fluent.Models.PdfElement` a un árbol de `Core.Models.CoreElement`.
    *   **Ventaja:** Desacoplamiento máximo. `Fluent` y `Core` podrían evolucionar sus modelos internos de forma completamente independiente.
    *   **Desventaja:** Mayor complejidad y sobrecarga de procesamiento por el mapeo.

*   **Opción 2: Herencia y Especialización**
    *   **Cuándo usarlo:** Si el `Core` solo necesita añadir unos pocos datos o comportamientos específicos sin cambiar la estructura fundamental del modelo.
    *   **Implementación:**
        1.  El modelo base (`PdfElement`) se movería a `Common`.
        2.  `Fluent` usaría una implementación (ej. `FluentParagraph` que hereda de `Common.PdfParagraph`).
        3.  El `Core` podría trabajar con la base `Common.PdfParagraph` o incluso crear su propia especialización (`CoreParagraph`) si fuera necesario.
    *   **Ventaja:** Reutilización de código y una evolución más gradual.
    *   **Desventaja:** Puede crear jerarquías de clases complejas si no se gestiona con cuidado.

*   **Opción 3: Métodos de Extensión o Factorías (El Enfoque Actual)**
    *   **Cuándo usarlo:** Cuando la diferencia es mínima y se puede gestionar con lógica simple. Es una forma de "decorar" el modelo existente con la funcionalidad que necesita el `Core`.
    *   **Implementación:** Es lo que estamos haciendo. El `Core` recibe el modelo `Fluent` y utiliza sus propios `Renderers` (que actúan como factorías de lógica) para interpretar el modelo y generar el resultado.
    *   **Ventaja:** La más simple y directa para el estado actual del proyecto.
    *   **Desventaja:** Si las necesidades del `Core` divergen mucho, esta opción puede llevar a un `Core` con demasiada lógica de "interpretación", lo que indicaría que es hora de pasar a la Opción 1 o 2.
