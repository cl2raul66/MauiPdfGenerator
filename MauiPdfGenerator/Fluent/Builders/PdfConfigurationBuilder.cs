using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfConfigurationBuilder : IPdfDocumentConfigurator
{
    public PageSizeType GetPageSize { get; private set; }
    public Thickness GetMargin { get; private set; }
    public PageOrientationType GetPageOrientation { get; private set; }
    public PdfFontRegistryBuilder FontRegistry { get; }

    public PdfMetaDataBuilder MetaDataBuilder { get; }

    public PdfConfigurationBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        this.FontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry)); 
        this.MetaDataBuilder = new PdfMetaDataBuilder(); 
        GetMargin = MarginCalculator.GetThickness(DefaultMarginType.Normal);
        GetPageSize = PageSizeType.A4;
        GetPageOrientation = PageOrientationType.Portrait;
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
        metaDataAction(this.MetaDataBuilder); 
        return this;
    } 

    public IPdfDocumentConfigurator ConfigureFontRegistry(Action<IPdfFontRegistry> fontRegistryConfiguration)
    {
        ArgumentNullException.ThrowIfNull(fontRegistryConfiguration);
        fontRegistryConfiguration(this.FontRegistry); 
        return this;
    }

    public override string ToString()
    {
        return $"PageSize: {GetPageSize}, Orientation: {GetPageOrientation}, Margin: {GetMargin}, Fonts: {FontRegistry}, Meta: {MetaDataBuilder}";
    }
}
