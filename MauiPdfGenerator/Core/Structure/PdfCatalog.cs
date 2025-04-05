using MauiPdfGenerator.Core.Objects;

namespace MauiPdfGenerator.Core.Structure
{
    /// <summary>
    /// Represents the document catalog dictionary (/Catalog), the root of the document's object hierarchy.
    /// Section 7.7.2.
    /// </summary>
    internal class PdfCatalog : PdfDictionary
    {
        private readonly PdfDocument _document; // Reference back to the document for object management

        /// <summary>
        /// Gets or sets the reference to the root page tree node (/Pages). REQUIRED.
        /// </summary>
        public PdfReference PageTreeRoot
        {
            get => this[PdfName.Pages] as PdfReference ?? throw new InvalidOperationException("/Pages reference is missing or invalid in Catalog.");
            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value), "/Pages entry cannot be null.");
                Add(PdfName.Pages, value);
            }
        }

        // --- Optional entries ---

        /// <summary>
        /// Gets or sets the viewer preferences dictionary (/ViewerPreferences). Optional.
        /// </summary>
        public PdfDictionary? ViewerPreferences
        {
            get => this[PdfName.Get("ViewerPreferences")] as PdfDictionary;
            set => AddOrRemove(PdfName.Get("ViewerPreferences"), value);
        }

        /// <summary>
        /// Gets or sets the page layout hint (/PageLayout). Optional.
        /// Values: SinglePage, OneColumn, TwoColumnLeft, TwoColumnRight, TwoPageLeft, TwoPageRight
        /// </summary>
        public PdfName? PageLayout
        {
            get => this[PdfName.Get("PageLayout")] as PdfName;
            set => AddOrRemove(PdfName.Get("PageLayout"), value);
        }

        /// <summary>
        /// Gets or sets the page mode hint (/PageMode). Optional.
        /// Values: UseNone, UseOutlines, UseThumbs, FullScreen, UseOC, UseAttachments
        /// </summary>
        public PdfName? PageMode
        {
            get => this[PdfName.Get("PageMode")] as PdfName;
            set => AddOrRemove(PdfName.Get("PageMode"), value);
        }

        // Add other optional entries like /Outlines, /Metadata (for XMP), /OpenAction etc. as needed

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCatalog"/> class.
        /// </summary>
        /// <param name="document">The parent document.</param>
        /// <param name="pageTreeRootRef">Reference to the root page tree node (/Pages).</param>
        internal PdfCatalog(PdfDocument document, PdfReference pageTreeRootRef) : base()
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));

            Add(PdfName.Type, PdfName.Catalog); // Required /Type entry
            PageTreeRoot = pageTreeRootRef; // Required /Pages entry
        }

        // Helper to add/remove optional entries
        private void AddOrRemove(PdfName key, PdfObject? value)
        {
            if (value == null || value is PdfNull)
            {
                Remove(key);
            }
            else
            {
                Add(key, value);
            }
        }
    }
}
