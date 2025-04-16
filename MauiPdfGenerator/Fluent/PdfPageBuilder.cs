// --- START OF FILE MauiPdfGenerator/Fluent/PdfPageBuilder.cs ---
using MauiPdfGenerator.Common.Elements;
using MauiPdfGenerator.Common.Primitives;
using MauiPdfGenerator.Fluent.Primitives;
using MauiPdfGenerator.Fluent.Styles;
using MauiPdfGenerator.Core.Utils;
using MauiPdfGenerator.Common; // Needed for UnitConverter

namespace MauiPdfGenerator.Fluent
{
    /// <summary>
    /// Provides a fluent API to configure and add elements to a single PDF page.
    /// </summary>
    public class PdfPageBuilder
    {
        private readonly PageModel _pageModel;
        private readonly DocumentModel _documentModel;
        private readonly UnitConverter _unitConverter; // For converting inputs to points

        internal PdfPageBuilder(PageModel pageModel, DocumentModel documentModel)
        {
            _pageModel = pageModel ?? throw new ArgumentNullException(nameof(pageModel));
            _documentModel = documentModel ?? throw new ArgumentNullException(nameof(documentModel));
            _unitConverter = new UnitConverter(_documentModel.Settings.Units); // Use document's default unit context
        }

        /// <summary> Sets the specific page size (in points) for this page using standard types. </summary>
        public PdfPageBuilder SetPageSize(PageSizeType pageSizeType, PageOrientationType orientation = PageOrientationType.Portrait)
        {
            _pageModel.Size = DocumentConfigurator.MapPageSizeType(pageSizeType, orientation); // Get size in points
            return this;
        }

        /// <summary> Sets the specific page size (in points) for this page using explicit dimensions. </summary>
        public PdfPageBuilder SetPageSize(Size pageSize, PageOrientationType orientation = PageOrientationType.Portrait)
        {
            float width = (float)pageSize.Width;
            float height = (float)pageSize.Height;
            if (orientation == PageOrientationType.Landscape && height > width) { (width, height) = (height, width); }
            else if (orientation == PageOrientationType.Portrait && width > height) { (width, height) = (height, width); }
            _pageModel.Size = new PdfSize(width, height); // Store size in points
            return this;
        }

        /// <summary> Sets specific margins for all sides of this page. Value is stored as provided, using the specified unit context for later interpretation by the Core. </summary>
        public PdfPageBuilder SetMargins(float margin, Unit unit)
        {
            // Core needs to know the unit used here, maybe store unit with margins?
            // For now, assume Core uses DocumentSettings.Units to interpret this value.
            _pageModel.Margins = new PdfMargins(margin);
            return this;
        }

        /// <summary> Sets specific margins for each side of this page. Values are stored as provided, using the specified unit context for later interpretation by the Core. </summary>
        public PdfPageBuilder SetMargins(float left, float top, float right, float bottom, Unit unit)
        {
            // Core needs to know the unit used here.
            _pageModel.Margins = new PdfMargins(left, top, right, bottom);
            return this;
        }

        /// <summary> Adds text. Coords/sizes are converted to points immediately based on the specified unit. Font size assumed points. </summary>
        public PdfPageBuilder AddText(
            string text,
            float x, float y, Unit unit = Unit.Points,
            string fontFamily = null, float? fontSize = null, /* Assumed points */
            FontAttributes? attributes = null, Color textColor = null,
            float? maxWidth = null, TextStyle textStyle = null)
        {
            if (string.IsNullOrEmpty(text)) return this;

            var mappedUnit = PdfDocumentBuilder.MapUnit(unit);
            var positionInPoints = new PdfPoint(_unitConverter.ToPoints(x, mappedUnit),
                                                 _unitConverter.ToPoints(y, mappedUnit));
            float? maxWidthInPoints = maxWidth.HasValue
                                    ? _unitConverter.ToPoints(maxWidth.Value, mappedUnit)
                                    : null;

            var finalFontFamily = textStyle?.FontFamily ?? fontFamily ?? _documentModel.Settings.DefaultFont.Name;
            var finalFontSize = textStyle?.FontSize ?? fontSize ?? _documentModel.Settings.DefaultFont.Size; // Assumed points
            var finalAttributes = textStyle?.Attributes ?? attributes ?? MapPdfFontStyleToFontAttributes(_documentModel.Settings.DefaultFont.Style);
            var finalTextColor = textStyle?.TextColor ?? textColor;

            var element = new TextElementModel
            {
                Text = text,
                Position = positionInPoints, // STORED IN POINTS
                Font = new PdfFont(finalFontFamily, finalFontSize, PdfDocumentBuilder.MapFontAttributes(finalAttributes)),
                Color = finalTextColor != null ? PdfDocumentBuilder.MapColor(finalTextColor) : GetDefaultTextColor(),
                MaxWidth = maxWidthInPoints, // STORED IN POINTS
                HorizontalAlignment = default, // TODO: Add alignment params/style option
                VerticalAlignment = default   // TODO: Add alignment params/style option
            };
            _pageModel.Elements.Add(element);
            return this;
        }

        /// <summary> Adds an image. Rectangle coords/size are converted to points immediately based on the specified unit. </summary>
        public PdfPageBuilder AddImage(Stream imageStream, float x, float y, float width, float height, Unit unit = Unit.Points, ImageStyle imageStyle = null)
        {
            if (imageStream == null) throw new ArgumentNullException(nameof(imageStream));
            if (!imageStream.CanRead) throw new ArgumentException("Image stream must be readable.", nameof(imageStream));

            byte[] imageData;
            using (var memoryStream = new MemoryStream()) { imageStream.CopyTo(memoryStream); if (imageStream.CanSeek) imageStream.Position = 0; imageData = memoryStream.ToArray(); }
            if (imageData.Length == 0) return this;

            var mappedUnit = PdfDocumentBuilder.MapUnit(unit);
            var targetRectInPoints = new PdfRect(
                _unitConverter.ToPoints(x, mappedUnit),
                _unitConverter.ToPoints(y, mappedUnit),
                _unitConverter.ToPoints(width, mappedUnit),
                _unitConverter.ToPoints(height, mappedUnit)
            );

            var element = new ImageElementModel(imageData, targetRectInPoints); // STORED IN POINTS
            _pageModel.Elements.Add(element);
            return this;
        }

        /// <summary> Adds a line. Coords are converted to points immediately. Thickness assumed points. </summary>
        public PdfPageBuilder AddLine(float x1, float y1, float x2, float y2, Unit unit = Unit.Points, float thickness = 1f, Color color = null)
        {
            var mappedUnit = PdfDocumentBuilder.MapUnit(unit);
            var startPointInPoints = new PdfPoint(_unitConverter.ToPoints(x1, mappedUnit), _unitConverter.ToPoints(y1, mappedUnit));
            var endPointInPoints = new PdfPoint(_unitConverter.ToPoints(x2, mappedUnit), _unitConverter.ToPoints(y2, mappedUnit));

            var element = new LineElementModel
            {
                StartPoint = startPointInPoints, // STORED IN POINTS
                EndPoint = endPointInPoints,     // STORED IN POINTS
                Thickness = thickness,           // Assumed points
                Color = color != null ? PdfDocumentBuilder.MapColor(color) : PdfColor.Black
            };
            _pageModel.Elements.Add(element);
            return this;
        }

        /// <summary> Adds a rectangle. Coords/size are converted to points immediately. Stroke thickness assumed points. </summary>
        public PdfPageBuilder AddRectangle(float x, float y, float width, float height, Unit unit = Unit.Points, float strokeThickness = 1f, Color strokeColor = null, Color fillColor = null)
        {
            var mappedUnit = PdfDocumentBuilder.MapUnit(unit);
            var positionInPoints = new PdfPoint(_unitConverter.ToPoints(x, mappedUnit), _unitConverter.ToPoints(y, mappedUnit));
            var sizeInPoints = new PdfSize(_unitConverter.ToPoints(width, mappedUnit), _unitConverter.ToPoints(height, mappedUnit));

            var element = new RectangleElementModel
            {
                Position = positionInPoints, // STORED IN POINTS
                Size = sizeInPoints,         // STORED IN POINTS
                StrokeThickness = strokeThickness, // Assumed points
                StrokeColor = strokeColor != null ? PdfDocumentBuilder.MapColor(strokeColor) : PdfColor.Black,
                FillColor = fillColor != null ? PdfDocumentBuilder.MapColor(fillColor) : PdfColor.Transparent
            };
            _pageModel.Elements.Add(element);
            return this;
        }

        private PdfColor GetDefaultTextColor()
        {
            // Consider adding Color to PdfFont or DocumentSettings for a true default
            return PdfColor.Black; // Fallback
        }

        private static FontAttributes MapPdfFontStyleToFontAttributes(PdfFontStyle style)
        {
            FontAttributes attributes = FontAttributes.None;
            if ((style & PdfFontStyle.Bold) == PdfFontStyle.Bold) attributes |= FontAttributes.Bold;
            if ((style & PdfFontStyle.Italic) == PdfFontStyle.Italic) attributes |= FontAttributes.Italic;
            return attributes;
        }
    }
}
// --- END OF FILE MauiPdfGenerator/Fluent/PdfPageBuilder.cs ---
