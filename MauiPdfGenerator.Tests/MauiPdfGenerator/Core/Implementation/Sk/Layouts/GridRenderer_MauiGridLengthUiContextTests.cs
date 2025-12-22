using System;
using System.Runtime.InteropServices;

// Aliases para evitar ambigüedades con MAUI
using MauiGridLength = Microsoft.Maui.GridLength;
using MauiColumnDefinition = Microsoft.Maui.Controls.ColumnDefinition;
using Xunit;

namespace MauiPdfGenerator.Tests.Core.Implementation.Sk.Layouts
{
    public class GridRenderer_MauiGridLengthUiContextTests
    {
        [Fact]
        public void ColumnDefinition_Instantiation_OffUiContext_Throws_COMException()
        {
            // Instanciar MAUI ColumnDefinition sin contexto de UI debería lanzar COMException
            var ex = Assert.Throws<COMException>(() => new MauiColumnDefinition(MauiGridLength.Auto));
            Assert.Contains("Class not registered", ex.Message);
        }
    }
}
