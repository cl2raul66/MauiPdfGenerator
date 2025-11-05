# MauiPdfGenerator

<div align="center">
  <a href="https://www.youtube.com/watch?v=fyodOXgCJMY">
    <img src="https://img.youtube.com/vi/fyodOXgCJMY/hqdefault.jpg" alt="Video" />
  </a>
</div>

## Introduction

MauiPdfGenerator is a powerful and flexible library for .NET MAUI that allows you to generate PDF documents directly in your applications. It offers a fluent API for easy document construction, comprehensive content elements, and flexible styling options.

This ecosystem is designed not only for powerful PDF creation but also for a superior developer experience. It is composed of three complementary libraries:
-   **`MauiPdfGenerator`**: The core engine for PDF generation.
-   **`MauiPdfGenerator.SourceGenerators`**: A code generator that simplifies font management by providing compile-time safe aliases.
-   **`MauiPdfGenerator.Diagnostics`**: A dedicated diagnostics library to streamline debugging and provide clear, actionable feedback on layout and resource issues.

Together, they provide a robust solution for creating, modifying, and managing PDFs easily, enhancing your app's functionality and user experience.

## Features

### MauiPdfGenerator

-   **Easily create PDF documents:** Quickly and easily generate PDFs in your .NET MAUI applications.
-   **Flexible content:** Add pages, text, images, lines, and organize content in vertical or horizontal stacks for clear and attractive layouts.
-   **Visual customization:** Adjust text styles, fonts, sizes, and colors for each element.
-   **Document configuration:** Add metadata such as title or author and control the appearance of each page.
-   **Compatible and efficient:** Works on all platforms supported by .NET MAUI and allows you to save PDFs asynchronously to avoid blocking the app.

### MauiPdfGenerator.SourceGenerators

-   **Automatic font management:** Automatically detects and generates safe aliases for all fonts registered in your MAUI project.
-   **Standard and custom fonts:** Easily access both standard PDF fonts and any custom fonts you register.
-   **Avoid typos:** Use compile-time generated constants to prevent errors from misspelled font names.
-   **Simplifies font usage:** Makes working with fonts in PDF generation easier and safer.

### MauiPdfGenerator.Diagnostics

-   **Actionable Feedback:** Get automatic, detailed feedback on common issues directly in your debug console and application logs by default.
-   **Visual Debugging:** Optionally enable a powerful visual overlay that draws layout bounds, highlights overflow errors, and displays diagnostic codes directly on the generated PDF.
-   **Accelerate Development:** Dramatically speed up the process of debugging complex layouts by seeing exactly what went wrong and where.
-   **Configurable:** Easily enable or disable the visualizer in your `MauiProgram.cs` to keep your production PDFs clean.

This suite of libraries provides a complete ecosystem for generating and customizing PDFs in .NET MAUI, focusing on both power and developer productivity.

## NuGet Versions

**MauiPdfGenerator**

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator)

**MauiPdfGenerator.SourceGenerators**

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator.SourceGenerators)

**MauiPdfGenerator.Diagnostics**

[![NuGet Version](https://img.shields.io/nuget/v/RandAMediaLabGroup.MauiPdfGenerator.Diagnostics.svg?style=flat-square)](https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator.Diagnostics)

## Project Status

[![Build Status](https://github.com/cl2raul66/MauiSystemFontFamily/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/cl2raul66/MauiSystemFontFamily/actions/workflows/nuget-publish.yml)

The project is currently under active development. New features are continuously being added and existing ones improved.

## Contributing

We welcome contributions to the MauiPdfGenerator project! Your help is essential to keep it great and improve it. You can contribute in several ways:

-   **Star the repository:** If you find this project useful, consider giving it a star on GitHub. It helps raise awareness and acknowledge the effort!
-   **Support the project:** If you or your organization benefit from this project, consider supporting its development. (More details on how to support the project will be provided if sponsorship options are enabled).
-   **Open issues:** Have you found a bug or have an idea for a new feature? We encourage you to open an issue on GitHub.
    -   For **bug reports**, provide a clear description of the problem and steps to reproduce it.
    -   For **feature requests**, describe the new feature and why it would be useful.
    -   Please check if there is an issue template available when creating a new one, as it can help streamline the process.

We appreciate your interest in contributing to MauiPdfGenerator!

## License

This project is licensed under the terms of the `LICENSE.txt` file. See the file for more details.