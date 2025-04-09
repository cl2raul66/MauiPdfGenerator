using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator.Implementation.Builders;

internal partial class DocumentBuilder
{
    // --- Adaptador simple para IMetadataConfigurator usando PdfInfo ---
    /// <summary>
    /// Internal adapter to allow PdfInfo to be configured via IMetadataConfigurator.
    /// </summary>
    private class MetadataConfiguratorAdapter : IMetadataConfigurator
    {
        private readonly PdfInfo _info;
        public MetadataConfiguratorAdapter(PdfInfo info) { _info = info ?? throw new ArgumentNullException(nameof(info)); }

        public IMetadataConfigurator Title(string title) { _info.Title = title; return this; }
        public IMetadataConfigurator Author(string author) { _info.Author = author; return this; }
        public IMetadataConfigurator Subject(string subject) { _info.Subject = subject; return this; }
        public IMetadataConfigurator Keywords(string keywords) { _info.Keywords = keywords; return this; }
        // Add Creator, Producer if needed in public interface IMetadataConfigurator
    }


}
// Fin namespace MauiPdfGenerator.Fluent.Builders
