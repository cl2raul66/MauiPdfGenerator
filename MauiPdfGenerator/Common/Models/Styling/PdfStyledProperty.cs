using MauiPdfGenerator.Common.Enums;

namespace MauiPdfGenerator.Common.Models.Styling;

internal class PdfStyledProperty<T>
{
    public T Value { get; private set; }
    public PdfPropertyPriority Priority { get; private set; }

    public PdfStyledProperty(T defaultValue)
    {
        Value = defaultValue;
        Priority = PdfPropertyPriority.Default;
    }

    public void Set(T newValue, PdfPropertyPriority newPriority)
    {
        if (newPriority >= Priority)
        {
            Value = newValue;
            Priority = newPriority;
        }
    }

    public static implicit operator T(PdfStyledProperty<T> property) => property.Value;
}
