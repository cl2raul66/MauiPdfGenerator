# MauiPdfGenerator

Biblioteca para generación de PDFs en .NET MAUI.

## Estructura del Proyecto

```
📦 MauiPdfGenerator
├── 📄 LICENSE.txt
├── 📄 MauiPdfGenerator.sln
├── 📄 README.md
├── 📂 MauiPdfGenerator/
│   ├── 📄 MauiPdfGenerator.csproj
│   ├── 📄 PdfGenerator.cs
│   ├── 📂 Common/
│   │   └── 📂 Geometry/
│   │       ├── 📄 PdfPoint.cs
│   │       ├── 📄 PdfRectangle.cs
│   │       └── 📄 PdfSize.cs
│   ├── 📂 Core/
│   │   ├── 📂 Content/
│   │   │   ├── 📄 PdfContentStream.cs
│   │   │   ├── 📄 PdfGraphicsState.cs
│   │   │   └── 📄 PdfResources.cs
│   │   ├── 📂 Fonts/
│   │   │   ├── 📄 PdfFontBase.cs
│   │   │   ├── 📄 PdfStandardFont.cs
│   │   │   └── 📄 StandardFontType.cs
│   │   ├── 📂 Images/
│   │   │   ├── 📄 IPdfImageProcessor.cs
│   │   │   ├── 📄 PdfImageProcessorFactory.cs
│   │   │   ├── 📄 PdfImageXObject.cs
│   │   │   └── 📄 SkiaSharpImageProcessor.cs
│   │   ├── 📂 IO/
│   │   │   ├── 📄 PdfCrossReferenceTable.cs
│   │   │   └── 📄 PdfWriter.cs
│   │   ├── 📂 Objects/
│   │   │   ├── 📄 PdfArray.cs
│   │   │   ├── 📄 PdfBoolean.cs
│   │   │   ├── 📄 PdfDictionary.cs
│   │   │   ├── 📄 PdfIndirectObject.cs
│   │   │   ├── 📄 PdfName.cs
│   │   │   ├── 📄 PdfNull.cs
│   │   │   ├── 📄 PdfNumber.cs
│   │   │   ├── 📄 PdfObject.cs
│   │   │   ├── 📄 PdfReference.cs
│   │   │   ├── 📄 PdfStream.cs
│   │   │   └── 📄 PdfString.cs
│   │   └── 📂 Structure/
│   │       ├── 📄 PdfCatalog.cs
│   │       ├── 📄 PdfDocument.cs
│   │       ├── 📄 PdfInfo.cs
│   │       ├── 📄 PdfPage.cs
│   │       └── 📄 PdfPageTreeNode.cs
│   ├── 📂 Fluent/
│   │   ├── 📂 Builders/
│   │   │   ├── 📄 ContainerContentBuilder.cs
│   │   │   ├── 📄 DocumentBuilder.cs
│   │   │   ├── 📄 GridBuilder.cs
│   │   │   ├── 📄 GridBuilderHelpers.cs
│   │   │   ├── 📄 GridChildInfo.cs
│   │   │   ├── 📄 HorizontalStackLayoutBuilder.cs
│   │   │   ├── 📄 ImageBuilder.cs
│   │   │   ├── 📄 PageBuilder.cs
│   │   │   ├── 📄 ParagraphBuilder.cs
│   │   │   └── 📄 VerticalStackLayoutBuilder.cs
│   │   ├── 📂 Enums/
│   │   │   ├── 📄 PageSizeType.cs
│   │   │   ├── 📄 PdfAspect.cs
│   │   │   └── 📄 PdfFontAttributes.cs
│   │   ├── 📂 Extensions/
│   │   ├── 📂 Interfaces/
│   │   └── 📂 Models/
│   └── 📂 Platforms/
│       ├── 📂 Android/
│       ├── 📂 iOS/
│       ├── 📂 MacCatalyst/
│       ├── 📂 Tizen/
│       └── 📂 Windows/
└── 📂 Test/
    ├── 📄 App.xaml
    ├── 📄 App.xaml.cs
    ├── 📄 AppShell.xaml
    ├── 📄 AppShell.xaml.cs
    ├── 📄 MainPage.xaml
    ├── 📄 MainPage.xaml.cs
    ├── 📄 MauiProgram.cs
    ├── 📄 Test.csproj
    ├── 📂 Properties/
    │   └── 📄 launchSettings.json
    ├── 📂 Resources/
    │   ├── 📂 AppIcon/
    │   ├── 📂 Fonts/
    │   ├── 📂 Images/
    │   ├── 📂 Raw/
    │   ├── 📂 Splash/
    │   └── 📂 Styles/
    └── 📂 Platforms/
        ├── 📂 Android/
        ├── 📂 iOS/
        ├── 📂 MacCatalyst/
        ├── 📂 Tizen/
        └── 📂 Windows/
```

## Descripción de la Estructura

### Biblioteca Principal (MauiPdfGenerator/)
- **Common/**: Componentes comunes como geometría básica
- **Core/**: Núcleo de la generación de PDF
  - **Content/**: Manejo de contenido y flujo gráfico
  - **Fonts/**: Sistema de fuentes
  - **Images/**: Procesamiento de imágenes
  - **IO/**: Operaciones de entrada/salida
  - **Objects/**: Objetos básicos PDF
  - **Structure/**: Estructura del documento PDF
- **Fluent/**: API fluida para construcción de documentos
  - **Builders/**: Constructores para diferentes elementos
  - **Enums/**: Enumeraciones
  - **Extensions/**: Extensiones de métodos
  - **Interfaces/**: Interfaces del API
  - **Models/**: Modelos de datos

### Aplicación de Prueba (Test/)
- Aplicación MAUI de demostración
- Incluye recursos y configuraciones multiplataforma
- Estructura estándar de una aplicación MAUI