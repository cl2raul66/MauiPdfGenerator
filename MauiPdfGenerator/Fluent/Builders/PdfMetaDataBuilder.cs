using MauiPdfGenerator.Fluent.Interfaces.Configuration;

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
    public string? GetCustomProperty { get; private set; }

    public IPdfMetaData Author(string author)
    {
        throw new NotImplementedException();
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
        GetCustomProperty = name + value;
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

    public override string ToString() => $"Title: {Title}, Author: {Author}";
}
