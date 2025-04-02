using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Interface for building a Grid Layout.
/// Arranges child elements in rows and columns.
/// </summary>
public interface IGridBuilder : /*ILayoutContainerBuilder,*/ ILayoutConfigurator<IGridBuilder> // Nota: Comentamos ILayoutContainerBuilder temporalmente
{
    // --- Definición de Estructura ---

    /// <summary>
    /// Defines the columns of the grid using MAUI-like string definitions.
    /// Examples: "*", "Auto", "100", "2*".
    /// Resets any previous column definitions.
    /// </summary>
    /// <param name="widths">An array of strings defining column widths.</param>
    /// <returns>The builder instance for chaining.</returns>
    IGridBuilder ColumnDefinitions(params string[] widths);

    /// <summary>
    /// Adds a single column definition to the existing ones.
    /// </summary>
    /// <param name="width">The string definition for the column width.</param>
    /// <returns>The builder instance for chaining.</returns>
    IGridBuilder AddColumnDefinition(string width);

    /// <summary>
    /// Defines the rows of the grid using MAUI-like string definitions.
    /// Examples: "*", "Auto", "50", "3*".
    /// Resets any previous row definitions.
    /// </summary>
    /// <param name="heights">An array of strings defining row heights.</param>
    /// <returns>The builder instance for chaining.</returns>
    IGridBuilder RowDefinitions(params string[] heights);


    /// <summary>
    /// Adds a single row definition to the existing ones.
    /// </summary>
    /// <param name="height">The string definition for the row height.</param>
    /// <returns>The builder instance for chaining.</returns>
    IGridBuilder AddRowDefinition(string height);


    /// <summary>
    /// Sets the spacing between columns in the grid.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IGridBuilder ColumnSpacing(float value);

    /// <summary>
    /// Sets the spacing between rows in the grid.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The builder instance for chaining.</returns>
    IGridBuilder RowSpacing(float value);

    // --- Añadir Elementos (Específico para Grid) ---
    // Necesitamos una forma de especificar la celda.
    // Opción 1: Métodos Add... específicos con parámetros de celda.
    // Opción 2: Un método intermedio .Cell(row, col)...AddParagraph(...)
    // Opción 3: Propiedades adjuntas en los elementos .GridRow(0).GridColumn(1)

    // Adoptamos Opción 1 por ahora para definir la interfaz del GridBuilder:
    // (Estos métodos 'new' ocultan los de una posible ILayoutContainerBuilder base si la heredamos directamente)

    IGridBuilder AddGrid(Action<IGridBuilder> gridAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1);
    IGridBuilder AddVerticalStackLayout(Action<IVerticalStackLayoutBuilder> vslAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1);
    IGridBuilder AddHorizontalStackLayout(Action<IHorizontalStackLayoutBuilder> hslAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1);
    IGridBuilder AddParagraph(Action<IParagraphBuilder> paragraphAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1);
    IGridBuilder AddImage(Action<IImageBuilder> imageAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1);
    IGridBuilder AddTable(Action<ITableBuilder> tableAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1); // Placeholder
    IGridBuilder AddBulletList(Action<IBulletListBuilder> listAction, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1); // Placeholder

    // Nota: Hemos comentado la herencia de ILayoutContainerBuilder porque los métodos Add...
    // en Grid necesitan los parámetros de celda. Si queremos reutilizar ILayoutContainerBuilder,
    // necesitaríamos una estrategia diferente (como la Opción 3 mencionada antes,
    // configurando la celda en el propio elemento). Por simplicidad *de la interfaz IGridBuilder*,
    // definimos los métodos aquí directamente.
}
