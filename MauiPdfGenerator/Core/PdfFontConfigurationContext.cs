using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Core;

internal class PdfFontConfigurationContext
{
    public PdfFontRegistryBuilder FontRegistryBuilder { get; }

    public PdfFontConfigurationContext(PdfFontRegistryBuilder fontRegistryBuilder)
    {
        FontRegistryBuilder = fontRegistryBuilder ?? throw new ArgumentNullException(nameof(fontRegistryBuilder));
    }
}
