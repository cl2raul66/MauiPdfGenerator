using System.Collections.Generic;

namespace MauiPdfGenerator.Core.Interfaces;

public interface ITextLine
{
    IReadOnlyList<ITextFragment> Fragments { get; }
}