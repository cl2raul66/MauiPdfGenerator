using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Core.IO;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Common.Geometry;
using System.Diagnostics;
using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Core.Fonts;

namespace MauiPdfGenerator.Implementation.Builders;

/// <summary>
/// Internal implementation of the main document builder interfaces.
/// Manages document configuration and page creation actions.
/// </summary>
internal partial class DocumentBuilder : IDocumentBuilder, IDocumentConfigurator
{
    private readonly PdfDocument _pdfDocument; // La instancia del documento Core

    // Configuraciones globales (defaults)
    private PageSizeType _defaultPageSizeType = PageSizeType.A4;
    private PdfRectangle _defaultMediaBox; // Calculado desde _defaultPageSizeType (usa double internamente)
    private float _defaultMarginLeft = 50f; // Usar float para coincidir con interfaz
    private float _defaultMarginTop = 50f;
    private float _defaultMarginRight = 50f;
    private float _defaultMarginBottom = 50f;
    private float _defaultSpacing = 10f; // Espaciado por defecto, tipo float

    private bool _continueIteration = true;

    /// <summary>
    /// Indica si se debe continuar con la iteración actual
    /// </summary>
    public bool ContinueIteration => _continueIteration;

    /// <summary>
    /// Pregunta al usuario si desea continuar con la iteración actual
    /// </summary>
    /// <returns>true si el usuario desea continuar, false en caso contrario</returns>
    public bool AskToContinue()
    {
        Console.Write("¿Desea continuar con la iteración? (S/N): ");
        var response = Console.ReadLine()?.Trim().ToUpper();
        _continueIteration = response == "S";
        return _continueIteration;
    }

    // Lista de acciones para construir cada página
    private readonly List<Action<IPdfPageBuilder>> _pageBuildActions = [];

    // TODO: Security settings storage
    // private PdfSecuritySettings _securitySettings = new PdfSecuritySettings();

    /// <summary>
    /// Initializes a new instance of the DocumentBuilder class.
    /// </summary>
    public DocumentBuilder()
    {
        _pdfDocument = new PdfDocument();
        // Establecer MediaBox inicial basado en A4 por defecto
        SetDefaultMediaBoxFromPageSize(_defaultPageSizeType);
        _pdfDocument.DefaultMediaBox = _defaultMediaBox; // Actualizar el default en el doc Core

        // Crear y configurar metadatos básicos
        var info = new PdfInfo
        {
            Producer = "MauiPdfGenerator Library v1.1",
            CreationDate = DateTimeOffset.Now
        };
        _pdfDocument.SetInfo(info);
    }

    // --- IDocumentBuilder Implementation ---

    /// <summary>
    /// Configures global document settings.
    /// </summary>
    /// <param name="configAction">Action to configure the document settings.</param>
    /// <returns>The document builder instance for chaining.</returns>
    public IDocumentBuilder Configure(Action<IDocumentConfigurator> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);
        // Como esta misma clase implementa IDocumentConfigurator, nos pasamos a nosotros mismos
        configAction(this);
        return this;
    }

    /// <summary>
    /// Adds an action to build a new page to the document.
    /// </summary>
    /// <param name="pageAction">Action to build the content of the page.</param>
    /// <returns>The document builder instance for chaining.</returns>
    public IDocumentBuilder PdfPage(Action<IPdfPageBuilder> pageAction) // Corregido nombre
    {
        ArgumentNullException.ThrowIfNull(pageAction);
        _pageBuildActions.Add(pageAction);
        return this;
    }

    /// <summary>
    /// Generates the PDF document by executing page build actions and writing to the specified file path.
    /// </summary>
    /// <param name="filePath">The full path where the PDF file will be saved.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

        // --- Proceso de Generación ---

        // 1. Actualizar Metadata (ModDate) justo antes de guardar
        _pdfDocument.Info.ModDate = DateTimeOffset.Now;

        // 2. Construir Páginas
        foreach (var pageAction in _pageBuildActions)
        {
            // TODO: Obtener MediaBox específico de la página si el usuario lo configura en pageAction
            // Por ahora, usamos el default global.
            var pageMediaBox = _defaultMediaBox;

            // Crear el objeto PdfPage (Core). El padre se asignará después.
            var pageTreeRoot = _pdfDocument.PageTreeRoot;
            var pageTreeRootRef = _pdfDocument.GetReference(pageTreeRoot);
            var pdfPage = new PdfPage(_pdfDocument, pageMediaBox, pageTreeRootRef);

            // Crear el PageBuilder (Fluent) para esta página Core (CLASE AÚN NO EXISTE)
            // Esta línea causará error hasta que PageBuilder.cs se cree.
            var pageBuilder = new PageBuilder(_pdfDocument, pdfPage, this); // Pasamos defaults/contexto

            // Ejecutar la acción del usuario para llenar la página usando el builder
            pageAction(pageBuilder);

            // Finalizar la página: PageBuilder debe haber creado el ContentStream,
            // asignado recursos, y asignado el stream a pdfPage.Contents
            pageBuilder.FinalizePage(); // Método necesario en PageBuilder

            // Añadir la página Core completada al documento (esto la añade al árbol y asigna Parent)
            _pdfDocument.AddPage(pdfPage);
        }

        // 3. Escribir el Documento PDF final
        // Se asume que el PdfDocument ahora contiene todos los objetos necesarios (Catalog, Info, Pages, Resources, Streams...)
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var writer = new PdfWriter(_pdfDocument);
            await writer.WriteAsync(fileStream);
        }
    }

    // --- IDocumentConfigurator Implementation ---

    /// <summary>
    /// Sets the default page size.
    /// </summary>
    /// <param name="size">The predefined page size.</param>
    /// <returns>The document configurator instance.</returns>
    public IDocumentConfigurator PageSize(PageSizeType size)
    {
        _defaultPageSizeType = size;
        SetDefaultMediaBoxFromPageSize(size);
        _pdfDocument.DefaultMediaBox = _defaultMediaBox; // Actualizar el default en el doc Core
        return this;
    }

    /// <summary>
    /// Sets the default spacing value.
    /// </summary>
    /// <param name="value">The spacing value.</param>
    /// <returns>The document configurator instance.</returns>
    public IDocumentConfigurator Spacing(float value) // Corregido tipo a float
    {
        if (value < 0) value = 0;
        _defaultSpacing = value;
        return this;
    }

    /// <summary>
    /// Sets the default uniform page margin.
    /// </summary>
    /// <param name="uniformMargin">The margin value.</param>
    /// <returns>The document configurator instance.</returns>
    public IDocumentConfigurator Margins(float uniformMargin) // Corregido tipo a float
    {
        if (uniformMargin < 0) uniformMargin = 0;
        _defaultMarginLeft = uniformMargin;
        _defaultMarginTop = uniformMargin;
        _defaultMarginRight = uniformMargin;
        _defaultMarginBottom = uniformMargin;
        return this;
    }
    // TODO: Implementar el overload Margins(float left, float top, float right, float bottom) si se añade a la interfaz

    /// <summary>
    /// Configures document metadata using an action.
    /// </summary>
    /// <param name="metadataAction">Action to configure metadata.</param>
    /// <returns>The document configurator instance.</returns>
    public IDocumentConfigurator Metadata(Action<IMetadataConfigurator> metadataAction)
    {
        if (metadataAction == null) throw new ArgumentNullException(nameof(metadataAction));
        // Usamos un adaptador simple que opera sobre _pdfDocument.Info
        var infoAdapter = new MetadataConfiguratorAdapter(_pdfDocument.Info);
        metadataAction(infoAdapter);
        return this;
    }

    /// <summary>
    /// Configures document security settings using an action. (Not Implemented Yet)
    /// </summary>
    /// <param name="securityAction">Action to configure security.</param>
    /// <returns>The document configurator instance.</returns>
    public IDocumentConfigurator SetSecurity(Action<ISecurityConfigurator> securityAction)
    {
        // TODO: Implementar ISecurityConfigurator y la lógica de seguridad en Core
        ArgumentNullException.ThrowIfNull(securityAction);
        Debug.WriteLine("Warning: DocumentBuilder.SetSecurity is not implemented yet."); // Placeholder
        // var securityAdapter = new SecurityConfiguratorAdapter(_securitySettings);
        // securityAction(securityAdapter);
        return this;
    }

    // --- IDisposable Implementation ---
    /// <summary>
    /// Disposes resources (if any). Currently no unmanaged resources directly held.
    /// </summary>
    public void Dispose()
    {
        // Implement IDisposable pattern if needed in the future for unmanaged resources
        GC.SuppressFinalize(this);
    }


    // --- Internal Helpers / Properties for PageBuilder ---

    /// <summary>
    /// Gets the default media box for pages.
    /// </summary>
    internal PdfRectangle DefaultPageMediaBox => _defaultMediaBox;
    /// <summary>
    /// Gets the default left margin for pages.
    /// </summary>
    internal float DefaultPageMarginLeft => _defaultMarginLeft;
    /// <summary>
    /// Gets the default top margin for pages.
    /// </summary>
    internal float DefaultPageMarginTop => _defaultMarginTop;
    /// <summary>
    /// Gets the default right margin for pages.
    /// </summary>
    internal float DefaultPageMarginRight => _defaultMarginRight;
    /// <summary>
    /// Gets the default bottom margin for pages.
    /// </summary>
    internal float DefaultPageMarginBottom => _defaultMarginBottom;
    /// <summary>
    /// Gets the default spacing between elements.
    /// </summary>
    internal float DefaultElementSpacing => _defaultSpacing;


    // --- Private Helpers ---

    /// <summary>
    /// Updates the internal default media box based on the selected page size type.
    /// </summary>
    /// <param name="size">The page size type.</param>
    private void SetDefaultMediaBoxFromPageSize(PageSizeType size)
    {
        _defaultMediaBox = GetStandardPageSize(size);
    }

    /// <summary>
    /// Gets the dimensions (as PdfRectangle starting at 0,0) for standard page sizes in PDF points.
    /// </summary>
    /// <param name="size">The standard page size enum value.</param>
    /// <returns>A PdfRectangle representing the page dimensions.</returns>
    private static PdfRectangle GetStandardPageSize(PageSizeType size)
    {
        // PDF units are points (1/72 inch)
        const double mmToPt = 72.0 / 25.4;
        const double inchToPt = 72.0;

        switch (size)
        {
            case PageSizeType.A4:
                // A4: 210 x 297 mm
                return new PdfRectangle(0, 0, Math.Round(210 * mmToPt), Math.Round(297 * mmToPt)); // Rounded values common
            case PageSizeType.Letter:
                // Letter: 8.5 x 11 inches
                return new PdfRectangle(0, 0, 8.5 * inchToPt, 11 * inchToPt); // 612 x 792 pt
            case PageSizeType.Legal:
                // Legal: 8.5 x 14 inches
                return new PdfRectangle(0, 0, 8.5 * inchToPt, 14 * inchToPt); // 612 x 1008 pt
                                                                              // Add other standard sizes (A3, A5, B5, Tabloid, etc.)
            default:
                // Default to A4 if unknown or not specified
                Debug.WriteLine($"Warning: Unknown PageSizeType '{size}'. Defaulting to A4.");
                return new PdfRectangle(0, 0, Math.Round(210 * mmToPt), Math.Round(297 * mmToPt));
        }
    }

    // --- MÉTODO DE PRUEBA PARA GENERACIÓN SIMPLE ---
    internal async Task GenerateSimpleTextPage(string textToDraw, string filePath)
    {
        Debug.WriteLine("GenerateSimpleTextPage Started");

        // 1. Asegurar Metadata
        if (_pdfDocument.Info == null) _pdfDocument.SetInfo(new PdfInfo());
        _pdfDocument.Info.ModDate = DateTimeOffset.Now;

        // 2. Crear la Página Core directamente
        var pageMediaBox = _pdfDocument.DefaultMediaBox; // Usar el A4 configurado
        var pageTreeRootRef = _pdfDocument.GetReference(_pdfDocument.PageTreeRoot);
        var pdfPage = new PdfPage(_pdfDocument, pageMediaBox, pageTreeRootRef);
        Debug.WriteLine($"PdfPage created. MediaBox: {pdfPage.MediaBox}"); // Verifica que MediaBox se lee bien

        // 3. Obtener Recursos y ContentStream
        var pageResources = pdfPage.PageResources; // Obtener de la página
        var contentStream = new PdfContentStream(_pdfDocument, pageResources);
        Debug.WriteLine("PdfContentStream created.");

        // --- DIBUJO DIRECTO Y SIMPLE ---
        try
        {
            // Obtener fuente (simple)
            var font = new PdfStandardFont(StandardFontType.Helvetica); // Fuente fija
            double fontSize = 12;
            var fontRef = pageResources.GetResourceName(font); // Asegurar que se añade a recursos
            Debug.WriteLine($"Font {fontRef} obtained/added to resources.");

            // Posición fija (Coordenadas PDF: origen abajo-izquierda)
            double x = 100;
            double y = 700;

            // Generar comandos PDF
            contentStream.BeginText(); Debug.WriteLine("  -> BT");
            contentStream.SetFont(font, fontSize); Debug.WriteLine($"  -> SetFont: {fontRef} {fontSize} Tf"); // Verifica la salida de SetFont
            contentStream.SetTextColor(0, 0, 0); Debug.WriteLine("  -> SetTextColor: 0 0 0 rg"); // Negro
            contentStream.MoveTextPosition(x, y); Debug.WriteLine($"  -> MoveTextPosition: {x} {y} Td");
            contentStream.ShowText(textToDraw, font); Debug.WriteLine($"  -> ShowText: '{textToDraw}'"); // Verifica el texto y la llamada
            contentStream.EndText(); Debug.WriteLine("  -> ET");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR during content stream generation: {ex.ToString()}");
        }
        // --- FIN DIBUJO DIRECTO ---

        // 4. Finalizar Stream y Asignar a Página
        contentStream.Dispose(); // Genera los bytes internos
        Debug.WriteLine($"ContentStream disposed. UnfilteredData Length: {contentStream.UnfilteredDataLength}"); // Verifica si se generaron bytes
        var contentStreamRef = _pdfDocument.GetReference(contentStream); // Añade stream como objeto indirecto
        pdfPage.Contents = contentStreamRef; // Asigna referencia a la página
        Debug.WriteLine($"ContentStream Ref {contentStreamRef} assigned to Page.");


        // Añadir la página al documento AHORA
        _pdfDocument.AddPage(pdfPage);
        Debug.WriteLine($"PdfPage added to document. Total objects before write: {_pdfDocument.GetIndirectObjects().Count()}");


        // 5. Escribir el Documento PDF final
        Debug.WriteLine("Writing PDF document...");
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var writer = new PdfWriter(_pdfDocument);
            await writer.WriteAsync(fileStream);
        }
        Debug.WriteLine("PDF document written.");
    }

    // --- Fin Método de Prueba ---


    // TODO: Implement SecurityConfiguratorAdapter class and PdfSecuritySettings class/logic

} // Fin clase DocumentBuilder

// Fin namespace MauiPdfGenerator.Fluent.Builders
