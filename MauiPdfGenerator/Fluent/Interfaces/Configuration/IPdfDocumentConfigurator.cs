using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

public interface IPdfDocumentConfigurator
{
    IPdfDocumentConfigurator PageSize(PageSizeType pageSizeType);
    IPdfDocumentConfigurator PageOrientation(PageOrientationType pageOrientationType);
    IPdfDocumentConfigurator Margins(float uniformMargin);
    IPdfDocumentConfigurator Margins(float verticalMargin, float horizontalMargin);
    IPdfDocumentConfigurator Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin);
    IPdfDocumentConfigurator Margins(DefaultMarginType defaultMarginType);
    IPdfDocumentConfigurator MetaData(Action<IPdfMetaData> metaData);
    IPdfDocumentConfigurator ConfigureFontRegistry(Action<IPdfFontRegistry> fontRegistryConfiguration);
}
