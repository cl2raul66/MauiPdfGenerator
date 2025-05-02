using MauiPdfGenerator.Fluent.Interfaces.Elements.FormattedText;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IParagraph : IPdfVisualElement
{
    IParagraph Text(string text);

    IParagraph TextTransform(TextTransform textTransform);

    IParagraph FormattedText(Action<FormattedString> formattedString);

    IParagraph FormattedText(Action<IPdfFormattedStringBuilder> formattedString);

    IParagraph FontFamily(string fontFamilyName);

    IParagraph FontSize(double fontSize);

    IParagraph TextColor(Color? textColor);

    IParagraph FontAttributes(FontAttributes fontAttributes);

    IParagraph HorizontalTextAlignment(TextAlignment textAlignment);

    IParagraph VerticalTextAlignment(TextAlignment textAlignment);

    IParagraph LineHeight(double lineHeight);    
}
