using MauiPdfGenerator.Core.Objects;

namespace MauiPdfGenerator.Core.Structure;

/// <summary>
/// Represents a page tree node dictionary (/Pages). Section 7.7.3.
/// Contains references to child nodes (other page trees or page objects).
/// </summary>
internal class PdfPageTreeNode : PdfDictionary
{
    private readonly PdfDocument _document;
    private readonly List<PdfReference> _kids = [];

    /// <summary>
    /// Gets the list of references to children (either PdfPage or PdfPageTreeNode). REQUIRED.
    /// </summary>
    public IReadOnlyList<PdfReference> Kids => _kids;

    /// <summary>
    /// Gets the total count of leaf page objects in the subtree rooted at this node. REQUIRED.
    /// This property 'hides' the base PdfDictionary.Count.
    /// </summary>
    public new int Count { get; private set; } // **** Añadida palabra clave 'new' ****

    /// <summary>
    /// Gets or sets the reference to the parent node in the page tree. REQUIRED except for root.
    /// </summary>
    public PdfReference? Parent { get; internal set; } // Settable by the document structure builder

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPageTreeNode"/> class.
    /// </summary>
    /// <param name="document">The parent document.</param>
    internal PdfPageTreeNode(PdfDocument document) : base()
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        Add(PdfName.Type, PdfName.Pages); // Required /Type entry
        // Add an empty /Kids array initially, will be populated later
        Add(PdfName.Kids, new PdfArray());
        UpdateCount(0); // Initialize count using helper method
    }

    /// <summary>
    /// Adds a child reference (page or another node) to this node.
    /// Updates the /Kids array and recalculates the /Count.
    /// </summary>
    /// <param name="childRef">The reference to the child object.</param>
    /// <param name="childPageCount">The number of leaf pages under the added child.</param>
    internal void AddChild(PdfReference childRef, int childPageCount)
    {
        ArgumentNullException.ThrowIfNull(childRef);
        if (childPageCount < 0) throw new ArgumentOutOfRangeException(nameof(childPageCount), "Child page count cannot be negative.");

        _kids.Add(childRef);

        // Update the /Kids array in the dictionary
        if (this[PdfName.Kids] is PdfArray kidsArray)
        {
            kidsArray.Add(childRef);
        }
        else // Should not happen if constructed correctly
        {
            kidsArray = [.. _kids];
            Add(PdfName.Kids, kidsArray);
        }

        // Update count
        UpdateCount(this.Count + childPageCount);
    }

    // Called when building the structure to link parent references
    internal void SetParentReference()
    {
        if (Parent is not null)
        {
            Add(PdfName.Parent, Parent);
        }
        else // Should only be null for the root /Pages node
        {
            Remove(PdfName.Parent);
        }
    }

    // Helper method to update Count property and dictionary entry consistently
    private void UpdateCount(int newCount)
    {
        this.Count = newCount;
        Add(PdfName.Count, new PdfNumber(this.Count)); // Update /Count entry in the dictionary
    }
}