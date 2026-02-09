using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Builders.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Utils;

internal class StyleResolver
{
    private readonly PdfResourceDictionary _documentResources;
    private readonly IDiagnosticSink _diagnosticSink;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public StyleResolver(PdfResourceDictionary documentResources, IDiagnosticSink diagnosticSink, PdfFontRegistryBuilder fontRegistry)
    {
        _documentResources = documentResources;
        _diagnosticSink = diagnosticSink;
        _fontRegistry = fontRegistry;
    }

    public void ApplyStyles(List<PdfElementData> elements, PdfResourceDictionary? pageResources)
    {
        foreach (var element in elements)
        {
            var implicitKey = GetImplicitStyleKey(element);
            if (implicitKey.HasValue)
            {
                ApplyStyle(element, implicitKey.Value, pageResources, PdfPropertyPriority.ImplicitStyle, reportMissing: false);
            }

            if (element.StyleKey.HasValue)
            {
                ApplyStyle(element, element.StyleKey.Value, pageResources, PdfPropertyPriority.ExplicitStyle, reportMissing: true);
            }
        }
    }

    private void ApplyStyle(PdfElementData element, PdfStyleIdentifier key, PdfResourceDictionary? pageResources, PdfPropertyPriority priority, bool reportMissing)
    {
        Action<object>? setter = null;

        if (pageResources is not null)
        {
            setter = pageResources.GetCombinedSetter(key);
        }

        setter ??= _documentResources.GetCombinedSetter(key);

        if (setter is null)
        {
            if (reportMissing)
            {
                _diagnosticSink.Submit(new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    DiagnosticCodes.StyleKeyNotFound,
                    $"Style with key '{key.Key}' not found in Page or Document resources.",
                    null
                ));
            }
            return;
        }

        try
        {
            var tempBuilder = CreateTemporaryBuilderFor(element);
            if (tempBuilder is null) return;

            setter(tempBuilder);

            var styledSourceModel = tempBuilder.GetModel();

            MergeProperties(element, styledSourceModel, priority);
        }
        catch (Exception ex)
        {
            _diagnosticSink.Submit(new DiagnosticMessage(
                DiagnosticSeverity.Error,
                "STYLE-ERROR",
                $"Failed to apply style '{key.Key}' to element '{element.GetType().Name}'. Error: {ex.Message}",
                null
            ));
        }
    }

    private IBuildablePdfElement? CreateTemporaryBuilderFor(PdfElementData element)
    {
        return element switch
        {
            PdfParagraphData => new PdfParagraphBuilder(string.Empty, _fontRegistry),
            PdfImageData => new PdfImageBuilder(Stream.Null),
            PdfHorizontalLineData => new PdfHorizontalLineBuilder(),
            PdfVerticalStackLayoutData => new PdfVerticalStackLayoutBuilder(_fontRegistry),
            PdfHorizontalStackLayoutData => new PdfHorizontalStackLayoutBuilder(_fontRegistry),
            PdfGridData => new PdfGridBuilder(_fontRegistry),
            _ => null
        };
    }

    private PdfStyleIdentifier? GetImplicitStyleKey(PdfElementData element)
    {
        string? typeName = element switch
        {
            PdfParagraphData => typeof(IPdfParagraph).FullName,
            PdfImageData => typeof(IPdfImage).FullName,
            PdfHorizontalLineData => typeof(IPdfHorizontalLine).FullName,
            PdfVerticalStackLayoutData => typeof(IPdfVerticalStackLayout).FullName,
            PdfHorizontalStackLayoutData => typeof(IPdfHorizontalStackLayout).FullName,
            PdfGridData => typeof(IPdfGrid).FullName,
            _ => null
        };

        if (typeName is not null)
        {
            return new PdfStyleIdentifier(typeName);
        }
        return null;
    }

    private void MergeProperties(PdfElementData target, PdfElementData source, PdfPropertyPriority priority)
    {
        void Merge<T>(PdfStyledProperty<T> targetProp, PdfStyledProperty<T> sourceProp)
        {
            if (sourceProp.Priority > PdfPropertyPriority.Default)
            {
                targetProp.Set(sourceProp.Value, priority);
            }
        }

        Merge(target.BackgroundColorProp, source.BackgroundColorProp);
        Merge(target.WidthRequestProp, source.WidthRequestProp);
        Merge(target.HeightRequestProp, source.HeightRequestProp);
        Merge(target.MarginProp, source.MarginProp);
        Merge(target.PaddingProp, source.PaddingProp);
        Merge(target.HorizontalOptionsProp, source.HorizontalOptionsProp);
        Merge(target.VerticalOptionsProp, source.VerticalOptionsProp);

        if (target is PdfParagraphData targetP && source is PdfParagraphData sourceP)
        {
            Merge(targetP.FontFamilyProp, sourceP.FontFamilyProp);
            Merge(targetP.FontSizeProp, sourceP.FontSizeProp);
            Merge(targetP.TextColorProp, sourceP.TextColorProp);
            Merge(targetP.HorizontalTextAlignmentProp, sourceP.HorizontalTextAlignmentProp);
            Merge(targetP.VerticalTextAlignmentProp, sourceP.VerticalTextAlignmentProp);
            Merge(targetP.FontAttributesProp, sourceP.FontAttributesProp);
            Merge(targetP.LineBreakModeProp, sourceP.LineBreakModeProp);
            Merge(targetP.TextDecorationsProp, sourceP.TextDecorationsProp);
            Merge(targetP.TextTransformProp, sourceP.TextTransformProp);

            if (sourceP.FontFamilyProp.Priority > PdfPropertyPriority.Default)
            {
                targetP.ResolvedFontRegistration = sourceP.ResolvedFontRegistration;
            }
        }
        else if (target is PdfImageData targetImg && source is PdfImageData sourceImg)
        {
            Merge(targetImg.AspectProp, sourceImg.AspectProp);
        }
        else if (target is PdfHorizontalLineData targetLine && source is PdfHorizontalLineData sourceLine)
        {
            Merge(targetLine.ThicknessProp, sourceLine.ThicknessProp);
            Merge(targetLine.ColorProp, sourceLine.ColorProp);
        }
        else if (target is PdfLayoutElementData targetLayout && source is PdfLayoutElementData sourceLayout)
        {
            Merge(targetLayout.SpacingProp, sourceLayout.SpacingProp);

            if (target is PdfGridData targetGrid && source is PdfGridData sourceGrid)
            {
                Merge(targetGrid.RowSpacingProp, sourceGrid.RowSpacingProp);
                Merge(targetGrid.ColumnSpacingProp, sourceGrid.ColumnSpacingProp);
            }
        }
    }
}
