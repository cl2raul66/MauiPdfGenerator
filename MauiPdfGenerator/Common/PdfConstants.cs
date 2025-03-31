// Ignore Spelling: Dict Eof Flate

using System.Text;

namespace MauiPdfGenerator.Common;

/// <summary>
/// Defines common constants used in PDF generation according to ISO 32000-2.
/// Marked internal as they are details of the PDF structure implementation.
/// </summary>
internal static class PdfConstants
{
    public static readonly byte[] NewLine = [(byte)'\n'];
    public static readonly byte[] Space = [(byte)' '];
    public static readonly byte[] DictStart = [(byte)'<', (byte)'<'];
    public static readonly byte[] DictEnd = [(byte)'>', (byte)'>'];
    public static readonly byte[] ArrayStart = [(byte)'['];
    public static readonly byte[] ArrayEnd = [(byte)']'];
    public static readonly byte[] StreamKeyword = [(byte)'s', (byte)'t', (byte)'r', (byte)'e', (byte)'a', (byte)'m'];
    public static readonly byte[] EndStreamKeyword = [(byte)'e', (byte)'n', (byte)'d', (byte)'s', (byte)'t', (byte)'r', (byte)'e', (byte)'a', (byte)'m'];
    public static readonly byte[] ObjKeyword = [(byte)'o', (byte)'b', (byte)'j'];
    public static readonly byte[] EndObjKeyword = [(byte)'e', (byte)'n', (byte)'d', (byte)'o', (byte)'b', (byte)'j'];
    public static readonly byte[] RKeyword = [(byte)'R'];
    public static readonly byte[] XRefKeyword = [(byte)'x', (byte)'r', (byte)'e', (byte)'f'];
    public static readonly byte[] TrailerKeyword = [(byte)'t', (byte)'r', (byte)'a', (byte)'i', (byte)'l', (byte)'e', (byte)'r'];
    public static readonly byte[] StartXRefKeyword = [(byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'x', (byte)'r', (byte)'e', (byte)'f'];
    public static readonly byte[] EofKeyword = [(byte)'%', (byte)'%', (byte)'E', (byte)'O', (byte)'F'];
    public static readonly byte[] TrueKeyword = [(byte)'t', (byte)'r', (byte)'u', (byte)'e'];
    public static readonly byte[] FalseKeyword = [(byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e'];
    public static readonly byte[] NullKeyword = [(byte)'n', (byte)'u', (byte)'l', (byte)'l'];


    public static class Names
    {
        // Core Structure
        public const string Type = "/Type";
        public const string Catalog = "/Catalog";
        public const string Pages = "/Pages";
        public const string Page = "/Page";
        public const string Kids = "/Kids";
        public const string Count = "/Count";
        public const string Parent = "/Parent";
        public const string MediaBox = "/MediaBox";
        public const string Resources = "/Resources";
        public const string Contents = "/Contents";
        public const string Length = "/Length";
        public const string Filter = "/Filter";
        public const string Size = "/Size";
        public const string Root = "/Root";

        // Resources
        public const string Font = "/Font";
        public const string XObject = "/XObject";
        public const string Image = "/Image"; // Subtype of XObject
        public const string ProcSet = "/ProcSet"; // Often needed for compatibility

        // Fonts
        public const string Subtype = "/Subtype";
        public const string Type1 = "/Type1";
        public const string TrueType = "/TrueType";
        public const string BaseFont = "/BaseFont";
        public const string Encoding = "/Encoding";
        public const string Name = "/Name"; // Often used for resource names like /F1

        // Images
        public const string Width = "/Width";
        public const string Height = "/Height";
        public const string ColorSpace = "/ColorSpace";
        public const string BitsPerComponent = "/BitsPerComponent";
        public const string DCTDecode = "/DCTDecode"; // For JPEGs
        public const string FlateDecode = "/FlateDecode"; // For PNGs/general compression

        // Other common names
        public const string Version = "/Version";
        public const string CreationDate = "/CreationDate";
        public const string ModDate = "/ModDate";
        public const string Producer = "/Producer";
        public const string Creator = "/Creator";
        public const string Title = "/Title";
        public const string Author = "/Author";
        public const string Subject = "/Subject";
        public const string Keywords = "/Keywords";
        public const string Info = "/Info"; // Discouraged in PDF 2.0 for metadata
    }

    public static class ProcSets
    {
        // Common procsets required by some viewers
        public static readonly byte[] PDF = Encoding.ASCII.GetBytes("/PDF ");
        public static readonly byte[] Text = Encoding.ASCII.GetBytes("/Text ");
        public static readonly byte[] ImageB = Encoding.ASCII.GetBytes("/ImageB ");
        public static readonly byte[] ImageC = Encoding.ASCII.GetBytes("/ImageC ");
        public static readonly byte[] ImageI = Encoding.ASCII.GetBytes("/ImageI ");
    }
}
