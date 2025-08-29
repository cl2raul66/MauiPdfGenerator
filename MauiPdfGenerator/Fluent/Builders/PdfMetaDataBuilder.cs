using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using System.Text;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfMetaDataBuilder : IPdfMetaData
{
    private DateTime? _creationDate;
    private string? _title;
    private string? _author;
    private string? _creator;
    private string? _keywords;
    private string? _subject;
    private string? _producer;

    public DateTime? GetCreationDate => _creationDate;
    public string GetTitle => _title ?? "New PDF";
    public string GetAuthor => _author ?? "MauiPdfGenerator";
    public string? GetCreator => _creator;
    public string? GetKeywords => _keywords;
    public string? GetSubject => _subject;
    public string? GetProducer => _producer;
    private readonly Dictionary<string, string> _customProperties = [];
    public IReadOnlyDictionary<string, string> GetCustomProperties => _customProperties;

    public IPdfMetaData Author(string author)
    {
        _author = author;
        return this;
    }

    public IPdfMetaData CreationDate(DateTime creationDate)
    {
        _creationDate = creationDate;
        return this;
    }

    public IPdfMetaData Creator(string creator)
    {
        _creator = creator;
        return this;
    }

    public IPdfMetaData CustomProperty(string name, string value)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _customProperties[name] = value ?? string.Empty;
        }
        return this;
    }

    public IPdfMetaData Keywords(string keywords)
    {
        _keywords = keywords;
        return this;
    }

    public IPdfMetaData Producer(string producer)
    {
        _producer = producer;
        return this;
    }

    public IPdfMetaData Subject(string subject)
    {
        _subject = subject;
        return this;
    }

    public IPdfMetaData Title(string title)
    {
        _title = title;
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
        if (sb.Length > 2) sb.Length -= 2;
        return sb.ToString();
    }
}
