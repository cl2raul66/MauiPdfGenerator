using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using Microsoft.Maui;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfConfigurationBuilder : IPdfDocumentConfigurator
{
    public PageSizeType GetPageSize { get; private set; }
    public Thickness GetMargin { get; private set; }
    public PageOrientationType GetPageOrientation { get; private set; }
    public PdfFontRegistryBuilder FontRegistry { get; } // Hacerlo readonly si no se reemplaza
    public PdfMetaDataBuilder MetaDataBuilder { get; } // Hacerlo readonly si no se reemplaza

    public PdfConfigurationBuilder()
    {
        // Establecer Defaults Razonables
        GetPageSize = PageSizeType.A4;
        GetPageOrientation = PageOrientationType.Portrait;
        GetMargin = new Thickness(72); // Margen de 1 pulgada (72 puntos) como default Normal
        FontRegistry = new PdfFontRegistryBuilder();
        MetaDataBuilder = new PdfMetaDataBuilder();

        // Opcional: Registrar una fuente absolutamente esencial por defecto si es necesario
        // FontRegistry.Font(MauiFontAliases.OpenSansRegular).Default();
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
        // Aquí mapearías DefaultMarginType a valores Thickness específicos
        GetMargin = defaultMarginType switch
        {
            DefaultMarginType.Normal => new Thickness(72), // 1 pulgada
            DefaultMarginType.Narrow => new Thickness(36), // 0.5 pulgadas
            DefaultMarginType.Moderate => new Thickness(54, 72), // 0.75" V, 1" H
            DefaultMarginType.Wide => new Thickness(144, 72), // 2" H, 1" V
            _ => new Thickness(72) // Default fallback, 1 pulgada
        };
        return this;
    }

    public IPdfDocumentConfigurator MetaData(Action<IPdfMetaData> metaData)
    {
        metaData(MetaDataBuilder);
        return this;
    }

    public IPdfDocumentConfigurator PageOrientation(PageOrientationType pageOrientationType)
    {
        GetPageOrientation = pageOrientationType;
        return this;
    }

    public IPdfDocumentConfigurator PageSize(PageSizeType sizeType)
    {
        GetPageSize = sizeType;
        return this;
    }

    public IPdfDocumentConfigurator PdfFontRegistry(Action<IPdfFontRegistry> fontRegistry)
    {
        fontRegistry(FontRegistry);
        return this;
    }

    public override string ToString()
    {
        return $"PageSize: {PageSize}, Orientation: {PageOrientation}, Margin: {GetMargin}, Fonts: {FontRegistry}, Meta: {MetaDataBuilder}";
    }
}
