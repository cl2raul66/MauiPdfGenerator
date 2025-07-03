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

### **Evolución y Mejoras Implementadas**

Basado en un análisis arquitectónico avanzado, se han implementado las siguientes mejoras estructurales para aumentar la robustez, mantenibilidad y profesionalidad de la biblioteca, sin caer en la sobre-ingeniería.

#### **1. Formalización del Contexto de Generación (`PdfGenerationContext`)**

*   **Problema Anterior:** Múltiples parámetros de estado (datos de la página, estado del layout, registro de fuentes) se pasaban individualmente a través de toda la pila de llamadas de renderizado, resultando en firmas de métodos largas y acopladas.
*   **Solución Implementada:** Se ha introducido la clase `PdfGenerationContext`. Este "Context Object" encapsula todo el estado relevante para la generación de una página. Ahora, los métodos de renderizado reciben una única referencia a este contexto.
*   **Beneficios:**
    *   **Código Limpio:** Las firmas de los métodos son más simples y estables.
    *   **Extensibilidad:** Añadir nuevo estado global (ej. configuración de caché) en el futuro solo requiere modificar la clase `PdfGenerationContext`, sin alterar las firmas de todos los renderers.
    *   **Cohesión:** Todo el estado relacionado con la generación está agrupado lógicamente.

#### **2. Integración de Logging Profesional (`Microsoft.Extensions.Logging`)**

*   **Problema Anterior:** El diagnóstico se realizaba mediante `Debug.WriteLine`, que es limitado, no configurable y solo útil en modo Debug.
*   **Solución Implementada:** Se ha integrado el sistema de logging estándar de .NET. Se inyecta un `ILogger` a través del `PdfGenerationContext`.
*   **Beneficios:**
    *   **Diagnóstico Profesional:** Permite el uso de niveles de log (Information, Warning, Error), lo que facilita filtrar y entender los problemas.
    *   **Configurable:** Los consumidores de la biblioteca pueden configurar dónde se escriben los logs (consola, ficheros, etc.).
    *   **Producción-Ready:** Es fundamental para diagnosticar problemas en entornos de producción.

#### **3. Adición de Validación Temprana ("Fail-Fast")**

*   **Problema Anterior:** Valores inválidos en la API fluida (ej. `Spacing(-10)`) no causaban un error hasta la fase de renderizado en el `Core`, dificultando la depuración.
*   **Solución Implementada:** Se han añadido validaciones (`ArgumentOutOfRangeException`) directamente en los métodos de la API `Fluent`.
*   **Beneficios:**
    *   **Mejor Experiencia de Desarrollador (DX):** El desarrollador que usa la biblioteca recibe feedback inmediato y claro si utiliza un valor incorrecto.
    *   **Robustez:** Se previene que estados inválidos se propaguen al motor de renderizado.

Estas mejoras estratégicas fortalecen la base arquitectónica de la biblioteca, preparándola para una evolución futura más estable y mantenible.
--- END OF FILE Arquitectura.md ---
