using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

public interface IPdfDocumentConfigurator
{
    IPdfDocumentConfigurator PageSize(PageSizeType pageSizeType);
    IPdfDocumentConfigurator PageOrientation(PageOrientationType pageOrientationType);
    IPdfDocumentConfigurator Padding(float uniformPadding);
    IPdfDocumentConfigurator Padding(float verticalPadding, float horizontalPadding);
    IPdfDocumentConfigurator Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding);
    IPdfDocumentConfigurator Padding(DefaultPagePaddingType defaultPaddingType);
    IPdfDocumentConfigurator MetaData(Action<IPdfMetaData> metaData);
    IPdfDocumentConfigurator ConfigureFontRegistry(Action<IPdfFontRegistry> fontRegistryConfiguration);
}
