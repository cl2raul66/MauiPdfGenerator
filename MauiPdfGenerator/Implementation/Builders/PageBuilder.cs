using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Core.Objects;
using MauiPdfGenerator.Implementation.Layout.Engine;
using MauiPdfGenerator.Implementation.Layout.Models;
using System.Diagnostics;

namespace MauiPdfGenerator.Implementation.Builders;

internal class PageBuilder : IPdfPageBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfPage _pdfPage;
    private readonly DocumentBuilder _documentBuilder;
    private readonly PdfResources _pageResources;
    private readonly LayoutEngine _layoutEngine;

    // Almacena el ÚNICO builder raíz definido por Content()
    private object? _rootLayoutBuilder; // Podría ser VSLBuilder, HSLBuilder, GridBuilder, etc.

    private PdfRectangle _mediaBox;
    private Thickness _margins; // Almacena los márgenes de la página

    public PageBuilder(PdfDocument pdfDocument, PdfPage pdfPage, DocumentBuilder documentBuilder)
    {
        _pdfDocument = pdfDocument;
        _pdfPage = pdfPage;
        _documentBuilder = documentBuilder;
        _pageResources = _pdfPage.PageResources;
        _mediaBox = _pdfPage.MediaBox;

        // Inicializar márgenes con los defaults del documento
        _margins = new Thickness(
            documentBuilder.DefaultPageMarginLeft,
            documentBuilder.DefaultPageMarginTop,
            documentBuilder.DefaultPageMarginRight,
            documentBuilder.DefaultPageMarginBottom
        );

        // Crear LayoutEngine (Considerar inyectarlo o hacerlo singleton?)
        _layoutEngine = new LayoutEngine();
    }

    // --- IPdfPageBuilder Implementation ---
    public IPdfPageBuilder Content(Action<IPdfContainerContentBuilder> contentAction)
    {
        ArgumentNullException.ThrowIfNull(contentAction);

        // --- Define el Contenedor Raíz Implícito (Ej: VerticalStackLayout) ---
        var rootVSLBuilder = new VerticalStackLayoutBuilder(_pdfDocument, _pageResources);
        _rootLayoutBuilder = rootVSLBuilder; // Almacena el builder raíz

        // Configurar el "Padding" efectivo del VSL raíz para que coincida con los márgenes de la página
        rootVSLBuilder.Padding(_margins.Left, _margins.Top, _margins.Right, _margins.Bottom);
        // Configurar otras propiedades del VSL raíz si es necesario (ej. Spacing default?)
        // rootVSLBuilder.Spacing(_documentBuilder.DefaultElementSpacing); // Opcional

        // Ejecuta la acción del usuario, añadiendo hijos AL CONTENEDOR RAÍZ
        var contentBuilder = new ContainerContentBuilder(_pdfDocument, _pageResources, rootVSLBuilder);
        contentAction(contentBuilder);
        // Los elementos añadidos ahora son hijos de rootVSLBuilder, no directamente de PageBuilder

        // --- AÑADIR ESTA LÍNEA ---
        var childrenBuilders = contentBuilder.GetAddedElements();
        if (_rootLayoutBuilder is VerticalStackLayoutBuilder vslRoot) // Asegurar que es el tipo correcto
        {
            vslRoot.AddChildren(childrenBuilders); // Asigna los hijos
        }
        // TODO: Manejar otros tipos de layout raíz si es necesario
        // --- FIN LÍNEA AÑADIDA ---

        return this;
    }

    // --- FinalizePage (Usa LayoutEngine con el Contenedor Raíz) ---
    internal void FinalizePage()
    {
        Debug.WriteLine("FinalizePage Started");
        if (_rootLayoutBuilder is null)
        {
            Debug.WriteLine("Warning: Page has no content defined via Content(). Skipping layout.");
            // Añadir stream vacío? O lanzar error? Añadir stream vacío es más seguro.
            var emptyStream = new PdfContentStream(_pdfDocument, _pageResources);
            emptyStream.Dispose(); // Genera bytes vacíos
            _pdfPage.Contents = _pdfDocument.GetReference(emptyStream);
            return;
        }

        // 1. Crear Content Stream
        var contentStream = new PdfContentStream(_pdfDocument, _pageResources);

        // 2. Área inicial es TODO el MediaBox. El layout raíz aplicará los márgenes como padding.
        var initialAvailableArea = _mediaBox; // Coords PDF [LLx, LLy, Width, Height]

        // 3. Crear Contexto Layout inicial
        var initialConstraints = LayoutConstraints.None;
        var initialContext = new LayoutContext(initialAvailableArea, initialConstraints, contentStream, _pageResources, 0);

        // 4. Iniciar Layout en el elemento Raíz
        // _layoutEngine.BeginLayout(initialContext); // Eliminado si _currentContext se eliminó

        Debug.WriteLine($"_rootLayoutBuilder.GetType().Name: {_rootLayoutBuilder.GetType().Name}");
        Debug.WriteLine($"initialContext.AvailableArea: {initialContext.AvailableArea}");

        // Medir el árbol completo desde la raíz
        var totalNeededSize = _layoutEngine.Measure(_rootLayoutBuilder, initialContext);

        Debug.WriteLine($"totalNeededSize: {totalNeededSize}");

        Debug.WriteLine("Calling Arrange on Root");

        // Posicionar el árbol completo desde la raíz dentro del área inicial
        _layoutEngine.Arrange(_rootLayoutBuilder, initialContext);

        // _layoutEngine.EndLayout(); // Eliminado si _currentContext se eliminó

        // 5. Finalizar Content Stream
        contentStream.Dispose();

        // 6. Añadir Content Stream indirecto a la página
        var contentStreamRef = _pdfDocument.GetReference(contentStream);
        _pdfPage.Contents = contentStreamRef;

        Debug.WriteLine("FinalizePage Finished");

        Debug.WriteLine($"Page finalized. Root: {_rootLayoutBuilder.GetType().Name}, Content stream {contentStreamRef}, Resources {_pdfPage[PdfName.Resources]}.");
    }
}
