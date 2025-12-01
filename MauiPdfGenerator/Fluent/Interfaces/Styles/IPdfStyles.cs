using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Styles;

public interface IPdfParagraphStyle : IPdfParagraph<IPdfParagraphStyle> { }

public interface IPdfImageStyle : IPdfImage<IPdfImageStyle> { }

public interface IPdfHorizontalLineStyle : IPdfHorizontalLine<IPdfHorizontalLineStyle> { }

public interface IPdfVerticalStackLayoutStyle : IPdfVerticalStackLayout<IPdfVerticalStackLayoutStyle>
{
    IPdfVerticalStackLayoutStyle Spacing(float value);
}

public interface IPdfHorizontalStackLayoutStyle : IPdfHorizontalStackLayout<IPdfHorizontalStackLayoutStyle>
{
    IPdfHorizontalStackLayoutStyle Spacing(float value);
}

public interface IPdfGridStyle : IPdfGrid<IPdfGridStyle>
{
    IPdfGridStyle RowSpacing(double value);
    IPdfGridStyle ColumnSpacing(double value);
}
