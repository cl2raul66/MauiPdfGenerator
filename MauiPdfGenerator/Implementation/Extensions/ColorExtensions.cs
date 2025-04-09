namespace MauiPdfGenerator.Implementation.Extensions;

/// <summary>
/// Extensiones para el manejo de colores en PDF.
/// </summary>
internal static class ColorExtensions
{
    /// <summary>
    /// Obtiene los componentes RGB de un color, si existe.
    /// </summary>
    public static (float r, float g, float b)? GetRgbComponents(this Color? color)
    {
        if (color is null)
            return null;

        return (color.Red, color.Green, color.Blue);
    }

    /// <summary>
    /// Indica si el color tiene valor.
    /// </summary>
    public static bool HasValue(this Color? color) => color != null;

    /// <summary>
    /// Obtiene el valor del color. Lanza excepción si es null.
    /// </summary>
    public static Color Value(this Color? color) => 
        color ?? throw new InvalidOperationException("El color es null");
}