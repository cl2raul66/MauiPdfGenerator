using MauiPdfGenerator.Fluent.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Fluent.Builders;

internal interface IPdfContentPageBuilder
{
    PageSizeType GetEffectivePageSize();

    Thickness GetEffectiveMargin();

    PageOrientationType GetEffectivePageOrientation();

    Color? GetEffectiveBackgroundColor();

    Action<IPageContentBuilder>? GetContentAction();
}
