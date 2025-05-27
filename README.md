# MauiPdfGenerator

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.svg?style=flat-square)](https://nugett.org/packages//RandAMediaLabGroup.MauiPdfGenerator)
[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators)

## Introduction

MauiPdfGenerator is a powerful and flexible .NET MAUI library for generating PDF documents directly within your applications. It offers a fluent API for easy document construction, comprehensive content elements, and flexible styling options. This library is complemented by `MauiPdfGenerator.SourceGenerators`, a source generator that simplifies font management by providing compile-time safe font aliases for both standard PDF fonts and custom fonts registered in your MAUI application. Together, they provide a robust solution for creating, modifying, and managing PDFs with ease, enhancing your app's functionality and user experience.

## Features

This section outlines the key features of `MauiPdfGenerator` and its companion library, `MauiPdfGenerator.SourceGenerators`.

### MauiPdfGenerator

- **Fluent API for Document Construction:**
    - Effortlessly build PDF documents with an intuitive and easy-to-use fluent API (e.g., `CreateDocument().Page().Content().Build()`).
- **Comprehensive Document Structure:**
    - Create new PDF documents.
    - Add multiple pages to a document.
- **Versatile Content Elements:**
    - **Text Paragraphs:** Add text with customizable styling, including font family, size, color, bold, and italic attributes.
    - **Images:** Embed images into your PDF documents, with support for loading from streams.
    - **Horizontal Lines:** Include horizontal lines for visual separation and structure.
    - **Advanced Text Layout:** Benefit from automatic text wrapping and configurable text truncation options to manage overflow.
- **Flexible Styling Options:**
    - **Default Page Fonts:** Set default font styles (family, size) on a per-page basis for consistent typography.
    - **Element Spacing:** Control the spacing between different content elements for precise layout.
- **PDF Configuration:**
    - **Metadata:** Define standard PDF metadata such as title, author, subject, and keywords.
- **Efficient Output:**
    - **Asynchronous Saving:** Save your generated PDF documents asynchronously to prevent UI blocking.
- **Cross-Platform Compatibility:**
    - Specifically designed and built for .NET MAUI, ensuring seamless integration with your cross-platform applications.

### MauiPdfGenerator.SourceGenerators

- **Automatic Font Alias Generation:**
    - Automatically generates a `MauiFontAliases` static class containing constants for all your registered fonts.
- **Compile-Time Safe Font Usage:**
    - Eliminates runtime errors from misspelled font names by providing compile-time constants. Access fonts like `MauiFontAliases.OpenSansRegular` instead of error-prone strings.
- **Includes Standard PDF Fonts:**
    - Provides pre-defined aliases for the 14 standard PDF fonts (e.g., Helvetica, Times Roman, Courier) for immediate use.
- **Custom Font Discovery:**
    - Automatically discovers and includes aliases for custom fonts registered in your .NET MAUI application's `MauiProgram.cs` (e.g., via `builder.ConfigureFonts(fonts => fonts.AddFont("MyCustomFont.ttf", "MyFontAlias"))`).
- **Simplified Font Management:**
    - Greatly simplifies the process of using both standard and custom fonts within `MauiPdfGenerator`, making your PDF generation code cleaner and more robust.

## Installation

To use MauiPdfGenerator in your .NET MAUI project, you need to install two NuGet packages:

1.  **`RandAMediaLabGroup.MauiPdfGenerator`**: The main library for PDF generation.
2.  **`RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators`**: The companion source generator library that generates font aliases.

You can install these packages using the .NET CLI or the NuGet Package Manager in Visual Studio.

**Using .NET CLI:**

Open your terminal or command prompt, navigate to your project directory, and run the following commands:

```bash
dotnet add package RandAMediaLabGroup.MauiPdfGenerator
dotnet add package RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators
```

**Using NuGet Package Manager (Visual Studio):**

1.  Right-click on your project in the Solution Explorer and select "Manage NuGet Packages...".
2.  Go to the "Browse" tab.
3.  Search for `RandAMediaLabGroup.MauiPdfGenerator` and click "Install".
4.  Search for `RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators` and click "Install".

## Getting Started / Usage

This section provides a basic example to help you get started with creating your first PDF document using `MauiPdfGenerator`.

### Font Registration and the Role of the Source Generator

To use fonts in your PDF, especially custom fonts, you need to register them in your .NET MAUI application.

1.  **Add Custom Fonts:** Place your font files (e.g., `.ttf`) into your MAUI project, typically under the `Resources/Fonts/` directory. Ensure the build action for these font files is set to `MauiFont`.
2.  **Register Fonts in `MauiProgram.cs`:** In your `MauiProgram.cs` file, register your custom fonts within the `CreateMauiApp` method:

    ```csharp
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MyCustomFont.ttf", "MyFontAlias"); // Example custom font
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ... other configurations

        return builder.Build();
    }
    ```

Once you've installed the `RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators` package and registered your fonts, it will automatically:
- Discover these registered fonts (like `"MyFontAlias"`).
- Include aliases for the 14 standard PDF fonts (e.g., "Helvetica", "TimesRoman").
- Generate a static class `MauiPdfGenerator.Generated.MauiFontAliases` containing these font names as compile-time constants (e.g., `MauiFontAliases.MyFontAlias`, `MauiFontAliases.Helvetica`).

This significantly simplifies font management and helps prevent runtime errors caused by misspelled font names.

### Basic PDF Creation Example

Here's a simple C# example demonstrating how to create a PDF document with a line of text:

```csharp
using MauiPdfGenerator;
using MauiPdfGenerator.Fluent.Enums; // For enums like FontStyle
using MauiPdfGenerator.Generated;    // For MauiFontAliases
using static MauiPdfGenerator.Generated.MauiFontAliases; // Static import for direct access to font names

public class PdfService
{
    public async Task CreateSimplePdfAsync()
    {
        var filePath = Path.Combine(FileSystem.CacheDirectory, "HelloWorld.pdf");

        try
        {
            // 1. Create a new PDF document
            var document = PdfGenerator.CreateDocument();

            // 2. Add a page and set default font
            document.Page(page =>
            {
                // Set default font for this page.
                // Standard PDF fonts like Helvetica are available via MauiFontAliases
                // If you registered "MyFontAlias", you could use MauiFontAliases.MyFontAlias
                page.SetDefaultFont(family: Helvetica, size: 12);

                // 3. Add content to the page
                page.Content(content =>
                {
                    content.Paragraph("Hello, PDF World!");
                    content.Paragraph("This is a simple PDF generated with MauiPdfGenerator.",
                                      fontStyle: FontStyle.Italic, fontSize: 10);
                });
            });

            // 4. Save the document
            await document.SaveAsync(filePath);

            // 5. Optionally, open the generated PDF
            // Ensure you have appropriate permissions and error handling for production code
            Console.WriteLine($"PDF saved to: {filePath}");
            // await Launcher.OpenAsync(new OpenFileRequest("Open PDF", new ReadOnlyFile(filePath)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating PDF: {ex.Message}");
            // Handle exception (e.g., display an error message to the user)
        }
    }
}
```

### Expected Outcome

The code above will generate a PDF file named `HelloWorld.pdf` in your application's cache directory. The PDF will contain a single page with:
- The text "Hello, PDF World!" in the default page font (Helvetica, 12pt).
- Below that, the text "This is a simple PDF generated with MauiPdfGenerator." in italics and a slightly smaller font size (10pt).

This example demonstrates the basic fluent API for document creation, page and content management, font selection using the generated aliases, and saving the document.

## Project Status

[![Build Status](https://github.com/cl2raul66/MauiSystemFontFamily/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/cl2raul66/MauiSystemFontFamily/actions/workflows/nuget-publish.yml)

The project is currently under active development. We are continuously working on adding new features and improving existing ones.

## Contributing

We warmly welcome contributions to the MauiPdfGenerator project! Your help is essential for keeping it great. Contributions can come in many forms:

- **Star the repository:** If you find this project useful, please consider giving it a star on GitHub! It helps to raise awareness and acknowledge the effort.
- **Support the project:** If you or your organization benefits from this project, please consider supporting its development. (Further details on how to support the project will be provided if/when sponsorship options become available).
- **Opening Issues:** Have you found a bug or have a great idea for a new feature? We encourage you to open an issue on GitHub.
    - For **bug reports**, please provide a clear description of the issue and steps to reproduce it.
    - For **feature requests**, please describe the new feature and why it would be beneficial.
    - Please check if an issue template is available when creating a new issue, as this can help streamline the process.

We appreciate your interest in contributing to MauiPdfGenerator!

## License

This project is licensed under the terms of the `LICENSE.txt` file. Please see the file for more details.
