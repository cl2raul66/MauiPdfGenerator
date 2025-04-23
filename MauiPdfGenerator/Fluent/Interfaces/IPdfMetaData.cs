namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfMetaData
{
    IPdfMetaData Title(string title);

    IPdfMetaData Author(string author);

    IPdfMetaData Subject(string subject);

    IPdfMetaData Keywords(string keywords);

    IPdfMetaData Creator(string creator);

    IPdfMetaData Producer(string producer);

    IPdfMetaData CreationDate(DateTime creationDate);

    IPdfMetaData CustomProperty(string name, string value);
}
