using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Interface for building the content of a single page by adding views (layouts/elements).
/// </summary>
public interface IPdfPageBuilder
{
    // --- Configuration (Optional, similar to Document Config but page-specific) ---
    // IPdfPageBuilder Configure(Action<IPdfPageConfigurator> configAction); // Placeholder

    /// <summary>
    /// Defines the content (elements and layouts) for this page.
    /// </summary>
    /// <param name="childrenAction">Action to add content using the provided builder.</param>
    /// <returns>The page builder instance (potentially for future chaining if needed).</returns>
    IPdfPageBuilder Children(Action<IPdfContainerContentBuilder> childrenAction);

    // --- Page Context ---
    // int CurrentPageNumber { get; } // Potentially useful in Footer/Header scenarios
    // int TotalPages { get; } // Usually known only at the end
}
// Placeholder interface
// public interface IPdfPageConfigurator { /* Page specific margins, size override? */ }
