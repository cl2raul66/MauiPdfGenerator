using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Fluent.Builders;

internal class StackLayoutContentBuilder : IStackLayoutBuilder
{
    private readonly PdfLayoutElement _layout;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public StackLayoutContentBuilder(PdfLayoutElement layout, PdfFontRegistryBuilder fontRegistry)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _fontRegistry = fontRegistry ?? throw new ArgumentNullException(nameof(fontRegistry));
    }

    public PdfParagraph Paragraph(string text)
    {
        var p = new PdfParagraph(text, _fontRegistry);
        _layout.Add(p);
        return p;
    }

    public PdfHorizontalLine HorizontalLine()
    {
        var line = new PdfHorizontalLine();
        _layout.Add(line);
        return line;
    }

    public PdfImage PdfImage(Stream stream)
    {
        var img = new PdfImage(stream);
        _layout.Add(img);
        return img;
    }

    public PdfVerticalStackLayout VerticalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfVerticalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        _layout.Add(stack);
        return stack;
    }

    public PdfHorizontalStackLayout HorizontalStackLayout(Action<IStackLayoutBuilder> content)
    {
        var stack = new PdfHorizontalStackLayout(_fontRegistry);
        var builder = new StackLayoutContentBuilder(stack, _fontRegistry);
        content(builder);
        _layout.Add(stack);
        return stack;
    }
}
// --- END OF FILE StackLayoutContentBuilder.cs ---
