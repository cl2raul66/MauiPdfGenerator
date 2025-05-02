using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using System.Text;

namespace MauiPdfGenerator.Fluent.Builders;
internal class PdfMetaDataBuilder : IPdfMetaData
{
    public DateTime? GetCreationDate { get; private set; }
    public string? GetTitle { get; private set; }
    public string? GetAuthor { get; private set; }
    public string? GetCreator { get; private set; }
    public string? GetKeywords { get; private set; }
    public string? GetSubject { get; private set; }
    public string? GetProducer { get; private set; }

    private readonly Dictionary<string, string> _customProperties = [];
    public IReadOnlyDictionary<string, string> GetCustomProperties => _customProperties;

    public IPdfMetaData Author(string author)
    {
        GetAuthor = author;
        return this;
    }

    public IPdfMetaData CreationDate(DateTime creationDate)
    {
        GetCreationDate = creationDate; 
        return this;
    }

    public IPdfMetaData Creator(string creator)
    {
        GetCreator = creator;
        return this;
    }

    public IPdfMetaData CustomProperty(string name, string value)
    {
        if (!string.IsNullOrEmpty(name)) // Evitar claves nulas o vacías
        {
            _customProperties[name] = value ?? string.Empty; // Almacenar o actualizar
        }
        return this;
    }

    public IPdfMetaData Keywords(string keywords)
    {
        GetKeywords = keywords;
        return this;
    }

    public IPdfMetaData Producer(string producer)
    {
        GetProducer = producer;
        return this;
    }

    public IPdfMetaData Subject(string subject)
    {
        GetSubject = subject;
        return this;
    }

    public IPdfMetaData Title(string title)
    {
        GetTitle = title;
        return this;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(GetTitle)) sb.Append($"Title: {GetTitle}, ");
        if (!string.IsNullOrEmpty(GetAuthor)) sb.Append($"Author: {GetAuthor}, ");
        if (!string.IsNullOrEmpty(GetSubject)) sb.Append($"Subject: {GetSubject}, ");
        if (!string.IsNullOrEmpty(GetKeywords)) sb.Append($"Keywords: {GetKeywords}, ");
        if (_customProperties.Any()) sb.Append($"CustomProps: {_customProperties.Count}, ");
        if (sb.Length > 2) sb.Length -= 2; // Eliminar la última coma y espacio
        return sb.ToString();
    }
}
