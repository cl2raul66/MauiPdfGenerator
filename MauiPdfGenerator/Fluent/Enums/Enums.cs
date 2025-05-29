namespace MauiPdfGenerator.Fluent.Enums;

public enum PageSizeType {
    A4,         
    A5,         
    A3,         
    Letter,     
    Legal,      
    Executive,  
    B5,         
    Tabloid,    
    Envelope_10,
    Envelope_DL
}

public enum PageOrientationType
{
    Portrait,
    Landscape
}

public enum DefaultMarginType
{
    Normal, 
    Narrow, 
    Moderate, 
    Wide
}

public enum FontDestinationType
{
    OnlyUI,
    OnlyPDF,
    Both
}
