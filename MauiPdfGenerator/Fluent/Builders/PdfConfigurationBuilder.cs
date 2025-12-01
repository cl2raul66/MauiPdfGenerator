using MauiPdfGenerator.Common.Models.Styling;
﻿using MauiPdfGenerator.Common.Utils;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfConfigurationBuilder : IPdfDocumentConfigurator
{
    public PageSizeType GetPageSize { get; private set; }
    public Thickness GetPadding { get; private set; }
    public PageOrientationType GetPageOrientation { get; private set; }
    public PdfFontRegistryBuilder FontRegistry { get; }
    public PdfMetaDataBuilder MetaDataBuilder { get; }
    public PdfResourceDictionary ResourceDictionary { get; }

    public PdfConfigurationBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        this.FontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
        this.MetaDataBuilder = new PdfMetaDataBuilder();
        this.ResourceDictionary = new PdfResourceDictionary();
        GetPadding = PdfPagePaddingTypeCalculator.GetThickness(DefaultPagePaddingType.Normal);
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

    public IPdfDocumentConfigurator Padding(float uniformPadding)
    {
        GetPadding = new Thickness(uniformPadding);
        return this;
    }

    public IPdfDocumentConfigurator Padding(float verticalPadding, float horizontalPadding)
    {
        GetPadding = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }

    public IPdfDocumentConfigurator Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding)
    {
        GetPadding = new Thickness(leftPadding, topPadding, rightPadding, bottomPadding);
        return this;
    }

    public IPdfDocumentConfigurator Padding(DefaultPagePaddingType defaultPaddingType)
    {
        GetPadding = PdfPagePaddingTypeCalculator.GetThickness(defaultPaddingType);
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
        return $"PageSize: {GetPageSize}, Orientation: {GetPageOrientation}, Padding: {GetPadding}, Fonts: {FontRegistry}, Meta: {MetaDataBuilder}";
    }
}
