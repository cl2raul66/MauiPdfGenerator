# MauiPdfGenerator

<div align="center">
  <a href="https://www.youtube.com/watch?v=oJRWiksYeuA">
    <img src="https://img.youtube.com/vi/oJRWiksYeuA/hqdefault.jpg" alt="Video" />
  </a>
</div>

## Create PDFs with the .NET MAUI API You Already Know

**MauiPdfGenerator** is a library for .NET MAUI that allows you to generate PDF documents using a fluent, declarative API designed to feel like a natural extension of the MAUI ecosystem. Stop learning complex, external PDF libraries and start creating documents with the same concepts, components, and development patterns you use every day.

Our philosophy is simple: if you know how to build a UI in .NET MAUI, you already know how to create a PDF with `MauiPdfGenerator`.

For a complete guide, examples, and advanced patterns, **[visit our official Wiki](https://github.com/cl2raul66/MauiPdfGenerator/wiki)**.

---

## Why MauiPdfGenerator? The Developer Experience Ecosystem

This is more than just a PDF library; it's a complete ecosystem designed to maximize your productivity and minimize frustration.

-   **`MauiPdfGenerator`**: The core engine, featuring a powerful layout system that emulates .NET MAUI's behavior for predictable results.
-   **`MauiPdfGenerator.SourceGenerators`**: A smart code generator that eliminates "magic strings" for fonts, providing compile-time safety and IntelliSense for all your registered fonts.
-   **`MauiPdfGenerator.Diagnostics`**: A first-class diagnostics library that transforms layout debugging from a guessing game into a science, providing clear, actionable feedback directly in your console and optionally as a visual overlay on the PDF itself. Now integrated into the Core library.

Together, they provide a seamless and robust solution for creating professional PDFs, allowing you to focus on your application's logic, not on fighting with a new API.

## Features That Respect Your Time

### Core Engine (`MauiPdfGenerator`)

-   **Fluent & Declarative API**: Build complex documents with readable, chainable methods.
-   **MAUI-Native Components**: Use familiar concepts like `Grid`, `VerticalStackLayout`, `Image`, and `Paragraph`.
-   **Intelligent Layout System**: The engine understands contextual defaults, `Margin`, `Padding`, and `LayoutOptions` just like MAUI.
-   **Automatic Pagination**: Divisible elements like `Paragraph` and `Grid` flow across pages automatically, while atomic elements like `Image` and `StackLayout` maintain their integrity.
-   **Rich Styling**: Customize fonts, colors, sizes, and text styles using the same `Microsoft.Maui` types you already use.
-   **Full Document Control**: Define metadata, page size, orientation, and global styles with ease.

### Source Generators (`MauiPdfGenerator.SourceGenerators`)

-   **Type-Safe Fonts**: Automatically generates a static `PdfFonts` class from your `MauiProgram.cs` configuration.
-   **Eliminate Typos**: `PdfFonts.OpenSansBold` will compile. `"OpenSans-Bold"` might not. The generator ensures correctness at compile time.
-   **Effortless Workflow**: Just configure your fonts once and let the generator do the rest.

### Diagnostics (`MauiPdfGenerator.Diagnostics`)

-   **Actionable Feedback**: Get automatic, detailed feedback on common issues directly in your debug console and application logs by default.
-   **Visual Debugging**: Optionally enable a powerful visual overlay that draws layout bounds, highlights overflow errors, and displays diagnostic codes directly on the generated PDF.
-   **Accelerate Development**: Dramatically speed up the process of debugging complex layouts by seeing exactly what went wrong and where.

## NuGet Packages

**MauiPdfGenerator**

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator)

**MauiPdfGenerator.SourceGenerators**

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators)

**MauiPdfGenerator.Diagnostics**

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.Diagnostics.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator.Diagnostics)

## Project Status

[![Build Status](https://github.com/cl2raul66/MauiSystemFontFamily/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/cl2raul66/MauiSystemFontFamily/actions/workflows/nuget-publish.yml)

The project is under active development. We are continuously adding new features and improving existing ones based on community feedback.

## Contributing

This project thrives on community involvement. Your contributions are essential to its success.

-   **Star the Repository**: If you find this project useful, giving it a star on GitHub is a powerful way to show your support and increase its visibility.
-   **Open Issues**: Found a bug or have an idea for a feature that would improve your workflow? Open an issue. We value detailed reports and thoughtful suggestions.
-   **Pull Requests**: We welcome code contributions. Whether it's a bug fix, a new feature, or improved documentation, feel free to fork the repository and submit a pull request.

## License

This project is licensed under the terms of the `LICENSE.txt` file.
