using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfConfigurationBuilder : IPdfDocumentConfigurator
{
    public PageSizeType GetPageSize { get; private set; } 
    public Thickness GetMargin { get; private set; }
    public PageOrientationType GetPageOrientation { get; private set; }
    public PdfFontRegistryBuilder FontRegistry { get; } = new();
    public PdfMetaDataBuilder MetaDataBuilder { get; } = new();

    // Ya no se necesitan propiedades mapeadas internas

    public PdfConfigurationBuilder()
    {        
        GetMargin = MarginCalculator.GetThickness(DefaultMarginType.Normal);
        GetPageSize = PageSizeType.A4;
        GetPageOrientation = PageOrientationType.Portrait;
        FontRegistry = new PdfFontRegistryBuilder();
        MetaDataBuilder = new PdfMetaDataBuilder();
    }
    
    public IPdfDocumentConfigurator PageSize(PageSizeType sizeType)
    {
        GetPageSize = sizeType;
        return this;
    }

    public IPdfDocumentConfigurator PageOrientation(PageOrientationType pageOrientationType)
    {
        GetPageOrientation = pageOrientationType;
        return this;
    }

    public IPdfDocumentConfigurator Margins(float uniformMargin)
    {
        GetMargin = new Thickness(uniformMargin);
        return this;
    }

    public IPdfDocumentConfigurator Margins(float verticalMargin, float horizontalMargin)
    {
        GetMargin = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }

    public IPdfDocumentConfigurator Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin)
    {
        GetMargin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public IPdfDocumentConfigurator Margins(DefaultMarginType defaultMarginType)
    {
        GetMargin = MarginCalculator.GetThickness(defaultMarginType);
        return this;
    }

    public IPdfDocumentConfigurator MetaData(Action<IPdfMetaData> metaDataAction)
    {
        ArgumentNullException.ThrowIfNull(metaDataAction);
        metaDataAction(MetaDataBuilder);
        return this;
    }

    public IPdfDocumentConfigurator PdfFontRegistry(Action<IPdfFontRegistry> fontRegistryAction)
    {
        ArgumentNullException.ThrowIfNull(fontRegistryAction);
        fontRegistryAction(FontRegistry);
        return this;
    }

    public override string ToString()
    {
        return $"PageSize: {GetPageSize}, Orientation: {GetPageOrientation}, Margin: {GetMargin}, Fonts: {FontRegistry}, Meta: {MetaDataBuilder}";
    }
}
