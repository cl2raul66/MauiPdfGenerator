namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

public interface IParagraph : IPdfVisualElement
{
    IParagraph Text(string text);

    IParagraph FormattedText(Action<FormattedString> formattedString);

    IParagraph FontFamily(string fontFamily);

    IParagraph FontSize(double fontSize);

    IParagraph TextColor(Color? textColor);

    IParagraph FontAttributes(FontAttributes fontAttributes);

    IParagraph HorizontalTextAlignment(TextAlignment textAlignment);

    IParagraph VerticalTextAlignment(TextAlignment textAlignment);

    IParagraph LineHeight(double lineHeight);    
}
