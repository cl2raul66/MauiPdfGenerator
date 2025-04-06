using MauiPdfGenerator.Core.Content;
using MauiPdfGenerator.Core.Structure;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using Microsoft.Maui.Graphics; // Para Color
using System;
using System.Collections.Generic;

namespace MauiPdfGenerator.Fluent.Builders;

/// <summary>
/// Internal implementation for building PDF Paragraphs.
/// Stores configuration set via the fluent API.
/// </summary>
internal class ParagraphBuilder : IPdfParagraphBuilder
{
    private readonly PdfDocument _pdfDocument;
    private readonly PdfResources _resources;

    // --- Configuration Storage ---

    // Content
    private string? _text;
    private FormattedTextBuilder? _formattedText; // Holds spans if FormattedText is used

    // Formatting Defaults (apply to whole paragraph or spans without override)
    private string? _fontFamily;
    private double _fontSize = 12; // Default font size
    private Color? _textColor;
    private PdfFontAttributes _fontAttributes = PdfFontAttributes.None;
    private PdfTextDecorations _textDecorations = PdfTextDecorations.None;
    private PdfTextAlignment _horizontalTextAlignment = PdfTextAlignment.Start;
    private PdfTextAlignment _verticalTextAlignment = PdfTextAlignment.Start; // Less common for paragraph itself
    private double _lineHeight = 1.2; // Default line height multiplier
    private PdfPadding _padding = PdfPadding.Zero;

    // View Properties (from IPdfViewBuilder)
    private double? _explicitWidth;
    private double? _explicitHeight;
    private PdfMargin _margin = PdfMargin.Zero;
    private PdfHorizontalAlignment _horizontalOptions = PdfHorizontalAlignment.Start; // Default alignment within parent space
    private PdfVerticalAlignment _verticalOptions = PdfVerticalAlignment.Start;     // Default alignment within parent space
    private Color? _backgroundColor;

    // Public properties for PageBuilder/Layout engine access (read-only view)
    public string? ConfiguredText => _text;
    public FormattedTextBuilder? ConfiguredFormattedText => _formattedText;
    public string? ConfiguredFontFamily => _fontFamily;
    public double ConfiguredFontSize => _fontSize;
    public Color? ConfiguredTextColor => _textColor;
    public PdfFontAttributes ConfiguredFontAttributes => _fontAttributes;
    public PdfTextDecorations ConfiguredTextDecorations => _textDecorations;
    public PdfTextAlignment ConfiguredHorizontalTextAlignment => _horizontalTextAlignment;
    public PdfTextAlignment ConfiguredVerticalTextAlignment => _verticalTextAlignment;
    public double ConfiguredLineHeight => _lineHeight;
    public PdfPadding ConfiguredPadding => _padding;
    public double? ConfiguredWidth => _explicitWidth;
    public double? ConfiguredHeight => _explicitHeight;
    public PdfMargin ConfiguredMargin => _margin;
    public PdfHorizontalAlignment ConfiguredHorizontalOptions => _horizontalOptions;
    public PdfVerticalAlignment ConfiguredVerticalOptions => _verticalOptions;
    public Color? ConfiguredBackgroundColor => _backgroundColor;


    public ParagraphBuilder(PdfDocument pdfDocument, PdfResources resources)
    {
        _pdfDocument = pdfDocument;
        _resources = resources;
        // Initialize defaults if different from field initializers
    }

    // --- IPdfParagraphBuilder Implementation ---

    public IPdfParagraphBuilder Text(string text)
    {
        _text = text;
        _formattedText = null; // Clear formatted text if simple text is set
        return this;
    }

    public IPdfParagraphBuilder FormattedText(Action<IPdfFormattedTextBuilder> formattedTextAction)
    {
        if (formattedTextAction == null) throw new ArgumentNullException(nameof(formattedTextAction));
        _formattedText = new FormattedTextBuilder();
        formattedTextAction(_formattedText);
        _text = null; // Clear simple text if formatted text is set
        return this;
    }

    public IPdfParagraphBuilder FontFamily(string fontFamily)
    {
        _fontFamily = fontFamily;
        // TODO: Later, register this font family with PdfResources if needed
        return this;
    }

    public IPdfParagraphBuilder FontSize(double size)
    {
        _fontSize = size > 0 ? size : 1; // Ensure positive font size
        return this;
    }

    public IPdfParagraphBuilder TextColor(Color color)
    {
        _textColor = color;
        return this;
    }

    public IPdfParagraphBuilder FontAttributes(PdfFontAttributes attributes)
    {
        _fontAttributes = attributes;
        return this;
    }

    public IPdfParagraphBuilder TextDecorations(PdfTextDecorations decorations)
    {
        _textDecorations = decorations;
        return this;
    }

    public IPdfParagraphBuilder HorizontalTextAlignment(PdfTextAlignment alignment)
    {
        _horizontalTextAlignment = alignment;
        return this;
    }

    public IPdfParagraphBuilder VerticalTextAlignment(PdfTextAlignment alignment)
    {
        _verticalTextAlignment = alignment;
        return this;
    }

    public IPdfParagraphBuilder LineHeight(double multiplier)
    {
        _lineHeight = multiplier > 0 ? multiplier : 1.0; // Ensure positive multiplier
        return this;
    }

    public IPdfParagraphBuilder Padding(double uniformPadding)
    {
        _padding = new PdfPadding(uniformPadding);
        return this;
    }

    public IPdfParagraphBuilder Padding(double horizontal, double vertical)
    {
        _padding = new PdfPadding(horizontal, vertical);
        return this;
    }

    public IPdfParagraphBuilder Padding(double left, double top, double right, double bottom)
    {
        _padding = new PdfPadding(left, top, right, bottom);
        return this;
    }

    // --- IPdfViewBuilder Implementation ---

    public IPdfParagraphBuilder Width(double width)
    {
        _explicitWidth = width >= 0 ? width : (double?)null;
        return this;
    }

    public IPdfParagraphBuilder Height(double height)
    {
        _explicitHeight = height >= 0 ? height : (double?)null;
        return this;
    }

    public IPdfParagraphBuilder Margin(double uniformMargin)
    {
        _margin = new PdfMargin(uniformMargin);
        return this;
    }

    public IPdfParagraphBuilder Margin(double horizontal, double vertical)
    {
        _margin = new PdfMargin(horizontal, vertical);
        return this;
    }

    public IPdfParagraphBuilder Margin(double left, double top, double right, double bottom)
    {
        _margin = new PdfMargin(left, top, right, bottom);
        return this;
    }

    public IPdfParagraphBuilder HorizontalOptions(PdfHorizontalAlignment alignment)
    {
        _horizontalOptions = alignment;
        return this;
    }

    public IPdfParagraphBuilder VerticalOptions(PdfVerticalAlignment alignment)
    {
        _verticalOptions = alignment;
        return this;
    }

    public IPdfParagraphBuilder BackgroundColor(Color color)
    {
        _backgroundColor = color;
        return this;
    }
}

// --- Helper Structs (Consider moving to Common/Models if used elsewhere) ---

/// <summary>
/// Represents padding values (internal helper).
/// </summary>
internal readonly struct PdfPadding
{
    public static readonly PdfPadding Zero = new(0);
    public double Left { get; }
    public double Top { get; }
    public double Right { get; }
    public double Bottom { get; }

    public PdfPadding(double uniform) : this(uniform, uniform) { }
    public PdfPadding(double horizontal, double vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public PdfPadding(double left, double top, double right, double bottom)
    {
        Left = left >= 0 ? left : 0;
        Top = top >= 0 ? top : 0;
        Right = right >= 0 ? right : 0;
        Bottom = bottom >= 0 ? bottom : 0;
    }
}

/// <summary>
/// Represents margin values (internal helper).
/// </summary>
internal readonly struct PdfMargin
{
    public static readonly PdfMargin Zero = new(0);
    public double Left { get; }
    public double Top { get; }
    public double Right { get; }
    public double Bottom { get; }

    public PdfMargin(double uniform) : this(uniform, uniform) { }
    public PdfMargin(double horizontal, double vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public PdfMargin(double left, double top, double right, double bottom)
    {
        // Margins can theoretically be negative, although less common for layout defaults
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}


// --- Implementations for Formatted Text / Spans ---

/// <summary>
/// Internal implementation for building formatted text with spans.
/// </summary>
internal class FormattedTextBuilder : IPdfFormattedTextBuilder
{
    internal List<SpanBuilder> Spans { get; } = new List<SpanBuilder>();

    public IPdfFormattedTextBuilder AddSpan(Action<IPdfSpanBuilder> spanAction)
    {
        if (spanAction == null) throw new ArgumentNullException(nameof(spanAction));
        var spanBuilder = new SpanBuilder();
        spanAction(spanBuilder);
        // Only add span if it actually contains text
        if (!string.IsNullOrEmpty(spanBuilder.ConfiguredText))
        {
            Spans.Add(spanBuilder);
        }
        return this;
    }
}

/// <summary>
/// Internal implementation for building individual styled text spans.
/// </summary>
internal class SpanBuilder : IPdfSpanBuilder
{
    // Store configuration for this specific span
    public string? ConfiguredText { get; private set; }
    public Color? ConfiguredTextColor { get; private set; }
    public string? ConfiguredFontFamily { get; private set; }
    public double? ConfiguredFontSize { get; private set; } // Nullable: Inherits from Paragraph if null
    public PdfFontAttributes? ConfiguredFontAttributes { get; private set; } // Nullable: Inherits if null? Or combines? Let's assume override/combine later.
    public PdfTextDecorations? ConfiguredTextDecorations { get; private set; } // Nullable: Inherits if null? Or combines?
    public Color? ConfiguredBackgroundColor { get; private set; }
    public double? ConfiguredLineHeight { get; private set; } // Nullable: Inherits

    public IPdfSpanBuilder Text(string text)
    {
        ConfiguredText = text;
        return this;
    }

    public IPdfSpanBuilder TextColor(Color color)
    {
        ConfiguredTextColor = color;
        return this;
    }

    public IPdfSpanBuilder FontFamily(string fontFamily)
    {
        ConfiguredFontFamily = fontFamily;
        return this;
    }

    public IPdfSpanBuilder FontSize(double size)
    {
        ConfiguredFontSize = size > 0 ? size : (double?)null;
        return this;
    }

    public IPdfSpanBuilder FontAttributes(PdfFontAttributes attributes)
    {
        ConfiguredFontAttributes = attributes;
        return this;
    }

    public IPdfSpanBuilder TextDecorations(PdfTextDecorations decorations)
    {
        ConfiguredTextDecorations = decorations;
        return this;
    }

    public IPdfSpanBuilder BackgroundColor(Color color)
    {
        ConfiguredBackgroundColor = color;
        return this;
    }

    public IPdfSpanBuilder LineHeight(double multiplier)
    {
        ConfiguredLineHeight = multiplier > 0 ? multiplier : (double?)null;
        return this;
    }
}

// Fin namespace
