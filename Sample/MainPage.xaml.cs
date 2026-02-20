using MauiPdfGenerator;
using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fonts;
using MauiPdfGenerator.Styles;

namespace Sample;

public partial class MainPage : ContentPage
{
    private readonly IPdfDocumentFactory pdfDocFactory;

    public MainPage(IPdfDocumentFactory pdfDocumentFactory)
    {
        InitializeComponent();
        pdfDocFactory = pdfDocumentFactory;
    }

    private async void GenerateParagraphShowcase_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Paragraphs.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - Paragraph Showcase")))
                .Resources(rd =>
                {
                    rd.Style<IPdfParagraph>("DefaultParagraph", s =>
                    {
                        s.FontSize(11);
                        s.TextColor(Colors.DarkSlateGray);
                    });
                    rd.Style<IPdfParagraph>("Title", s =>
                    {
                        s.FontSize(24);
                        s.FontAttributes(FontAttributes.Bold);
                        s.TextColor(Colors.DarkBlue);
                        s.HorizontalTextAlignment(TextAlignment.Center);
                    });
                    rd.Style<IPdfParagraph>("SectionHeader", s =>
                    {
                        s.FontSize(18);
                        s.FontAttributes(FontAttributes.Bold);
                        s.TextColor(Colors.DarkSlateGray);
                    });
                    rd.Style<IPdfParagraph>("Note", s =>
                    {
                        s.FontSize(10);
                        s.FontAttributes(FontAttributes.Italic);
                        s.TextColor(Colors.DimGray);
                    });
                    rd.Style<IPdfSpan>("NoteTitle", s =>
                    {
                        s.FontSize(10);
                        s.FontAttributes(FontAttributes.Bold);
                        s.TextTransform(TextTransform.Uppercase);
                        s.TextColor(Colors.Indigo);
                    });
                    rd.Style<IPdfSpan>("NoteMsg", s =>
                    {
                        s.FontSize(10);
                        s.FontAttributes(FontAttributes.Italic);
                        s.TextColor(Colors.Indigo);
                    });
                    rd.Style<IPdfSpan>("Highlight", s =>
                    {
                        s.BackgroundColor(Colors.Yellow);
                        s.TextColor(Colors.Black);
                    });
                    rd.Style<IPdfSpan>("Code", s =>
                    {
                        s.TextColor(Colors.DarkRed);
                        s.BackgroundColor(Colors.LightGray);
                    });
                    rd.Style<IPdfSpan>("Emphasis", s =>
                    {
                        s.FontAttributes(FontAttributes.Italic);
                        s.TextColor(Colors.Teal);
                    });
                })
                .ContentPage()
                .Resources(rd =>
                {
                    rd.Style<IPdfSpan>("NotaSpan", s =>
                    {
                        s.FontSize(10);
                        s.TextColor(Colors.Red);
                    });
                })
                .Content(c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("Paragraph Showcase").Style(PdfStyles.Title);

                        ch.Paragraph("This document demonstrates all paragraph features including fonts, alignment, spacing, and internationalization support.").Style(PdfStyles.Note);

                        ch.HorizontalLine();

                        ch.Paragraph("1. Font and Style Properties").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("These properties modify the text appearance itself.").Style(PdfStyles.Note);
                        ch.Paragraph("Font Family: Comic Semibold.").FontFamily(PdfFonts.Comic);
                        ch.Paragraph("Font Size: 16 points.").FontSize(16);
                        ch.Paragraph("Font Attributes: Bold.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Font Attributes: Italic.").FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("Font Attributes: Bold + Italic.").FontAttributes(FontAttributes.Bold | FontAttributes.Italic);
                        ch.Paragraph("Text Decorations: Underline.").TextDecorations(TextDecorations.Underline);
                        ch.Paragraph("Text Decorations: Strikethrough.").TextDecorations(TextDecorations.Strikethrough);
                        ch.Paragraph("Text Transform: UPPERCASE.").TextTransform(TextTransform.Uppercase);
                        ch.Paragraph("Text Transform: lowercase.").TextTransform(TextTransform.Lowercase);
                        ch.Paragraph("Text Color: Red.").TextColor(Colors.Red);
                        ch.HorizontalLine();

                        ch.Paragraph(sp =>
                        {
                            sp.Text("This ").TextTransform(TextTransform.Uppercase);
                            sp.Text("is with").FontAttributes(FontAttributes.Bold).TextColor(Colors.Red);
                            sp.Text(" Span");
                        })
                        .TextColor(Colors.Blue).TextDecorations(TextDecorations.Underline);
                        ch.Paragraph(sp =>
                        {
                            sp.Text("Nota: ").Style(PdfStyles.NoteTitle);
                            sp.Text("El mejor generador de ficheros PDF para .NET MAUI").Style(PdfStyles.NoteMsg);
                        });
                        ch.HorizontalLine();

                        ch.Paragraph("2. Text Alignment (HorizontalTextAlignment)").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("Horizontal alignment within the paragraph box. Uses 'Start' and 'End' for internationalization support (automatically adapts to LTR and RTL languages).").Style(PdfStyles.Note);
                        ch.Paragraph("Start aligned text (default behavior).").HorizontalTextAlignment(TextAlignment.Start).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("Center aligned text.").HorizontalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("End aligned text.").HorizontalTextAlignment(TextAlignment.End).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("Justified text fills both edges, ideal for long paragraphs. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                            .HorizontalTextAlignment(TextAlignment.Justify).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("Note: 'BackgroundColor' is used to visualize paragraph box boundaries.").Style(PdfStyles.Note).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        ch.Paragraph("3. Box Model and Positioning").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("These properties affect the paragraph box (size, spacing, position). Vertical alignment is visible only when box has explicit height.").Style(PdfStyles.Note);
                        ch.Paragraph("Padding: 10pt internal space.").Padding(10).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("Margin: 10pt external space.").Margin(10).BackgroundColor(Colors.LightBlue);
                        ch.Paragraph("WidthRequest 300pt, centered horizontally.").WidthRequest(300).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("HeightRequest 50pt, vertically centered text.").HeightRequest(50).VerticalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("Note: 'BackgroundColor' makes the box visible to understand how properties affect layout.").Style(PdfStyles.Note).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        ch.Paragraph("4. Line Break Mode").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("Controls how text wraps or truncates when exceeding container width.").Style(PdfStyles.Note);

                        var longText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
                        ch.Paragraph("WordWrap (default): Text wraps at word boundaries.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText);
                        ch.Paragraph("NoWrap: Text doesn't wrap and overflows.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.NoWrap);
                        ch.Paragraph("CharacterWrap: Text wraps at character boundaries.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.CharacterWrap);
                        ch.Paragraph("HeadTruncation: Truncates at the beginning.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.HeadTruncation);
                        ch.Paragraph("MiddleTruncation: Truncates in the middle.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.MiddleTruncation);
                        ch.Paragraph("TailTruncation: Truncates at the end.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longText).LineBreakMode(LineBreakMode.TailTruncation);
                        ch.HorizontalLine();

                        ch.Paragraph("5. Spacing Properties (Character, Word, Line)").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("These properties control text spacing within paragraphs.").Style(PdfStyles.Note);

                        ch.Paragraph("CharacterSpacing (Default: 0): Normal character spacing.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Custom CharacterSpacing(2): Increased space between each character for emphasis and readability.").CharacterSpacing(2).BackgroundColor(Colors.LightYellow);
                        ch.Paragraph("Custom CharacterSpacing(4): Wide character spacing for special effects.").CharacterSpacing(4).BackgroundColor(Colors.LightCoral);

                        ch.Paragraph("WordSpacing (Default: 0): Normal word spacing.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Custom WordSpacing(2): Increased space between words for better readability in headings.").WordSpacing(2).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("Custom WordSpacing(3): Very wide word spacing for decorative purposes.").WordSpacing(3).BackgroundColor(Colors.LightCyan);

                        ch.Paragraph("LineSpacing (Default: 1): Normal line height (100%).").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Custom LineSpacing(1.5): 150% line height for better readability. This paragraph demonstrates increased line spacing which makes text easier to read, especially for people with dyslexia or visual impairments.").LineSpacing(1.5f).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("Custom LineSpacing(2): Double-spaced text, commonly used in academic papers for annotations.").LineSpacing(2f).BackgroundColor(Colors.Lavender);

                        ch.Paragraph("Combined Spacing: CharacterSpacing(1.5), WordSpacing(2), LineSpacing(1.8) all applied together for a unique visual effect.").CharacterSpacing(1.5f).WordSpacing(2f).LineSpacing(1.8f).BackgroundColor(Colors.Honeydew);
                        ch.HorizontalLine();

                        ch.Paragraph("6. Advanced Span Usage").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("Multiple styled spans within a single paragraph.").Style(PdfStyles.Note);

                        ch.Paragraph(s =>
                        {
                            s.Text("This paragraph demonstrates ");
                            s.Text("multiple span styles").Style(PdfStyles.Highlight);
                            s.Text(" in the same ");
                            s.Text("paragraph").Style(PdfStyles.Code);
                            s.Text(". You can combine ");
                            s.Text("different").Style(PdfStyles.Emphasis);
                            s.Text(" styles like ");
                            s.Text("bold").FontAttributes(FontAttributes.Bold);
                            s.Text(", ");
                            s.Text("italic").FontAttributes(FontAttributes.Italic);
                            s.Text(", ");
                            s.Text("underlined").TextDecorations(TextDecorations.Underline);
                            s.Text(", and ");
                            s.Text("colored").TextColor(Colors.DarkGreen);
                            s.Text(" text seamlessly.");
                        });
                        ch.HorizontalLine();

                        ch.Paragraph("7. Final Composition").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("Combining multiple properties for specific results.").Style(PdfStyles.Note);
                        ch.Paragraph("Comic font, size 12, dark red text, centered in a 375pt box with yellow background and 15pt padding.")
                            .FontFamily(PdfFonts.Comic).FontSize(12).TextColor(Colors.DarkRed)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .WidthRequest(375)
                            .BackgroundColor(Colors.LightYellow)
                            .Padding(15);
                    });
                })
                .Build()
                .ContentPage()
                .Culture("ar-SA")
                .DefaultFont(f => f.Family(PdfFonts.NotoSansArabic))
                .Content(c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("عرض الفقرات").Style(PdfStyles.Title);

                        ch.Paragraph("توضح هذه الوثيقة جميع ميزات الفقرات بما في ذلك الخطوط والمحاذاة والتباعد ودعم التدويل.").Style(PdfStyles.Note);

                        ch.HorizontalLine();

                        ch.Paragraph("١. خصائص الخط والنمط").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("تعدل هذه الخصائص مظهر النص نفسه.").Style(PdfStyles.Note);
                        ch.Paragraph(sp =>
                        {
                            sp.Text("عائلة الخط: ");
                            sp.Text("Harmattan").Culture("en-US");
                            sp.Text(".");
                        }).FontFamily(PdfFonts.Harmattan);
                        ch.Paragraph("حجم الخط: ١٦ نقطة.").FontSize(16);
                        ch.Paragraph("سمات الخط: غامق.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("سمات الخط: مائل.").FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("سمات الخط: غامق + مائل.").FontAttributes(FontAttributes.Bold | FontAttributes.Italic);
                        ch.Paragraph("زخارف النص: تحته خط.").TextDecorations(TextDecorations.Underline);
                        ch.Paragraph("زخارف النص: يتوسطه خط.").TextDecorations(TextDecorations.Strikethrough);
                        ch.Paragraph("تحويل النص: أحرف كبيرة.").TextTransform(TextTransform.Uppercase);
                        ch.Paragraph("تحويل النص: أحرف صغيرة.").TextTransform(TextTransform.Lowercase);
                        ch.Paragraph("لون النص: أحمر.").TextColor(Colors.Red);
                        ch.HorizontalLine();

                        ch.Paragraph(sp =>
                        {
                            sp.Text("هذا ").TextTransform(TextTransform.Uppercase);
                            sp.Text("نص مع").FontAttributes(FontAttributes.Bold).TextColor(Colors.Red);
                            sp.Text(" نطاقات");
                        })
                        .TextColor(Colors.Blue).TextDecorations(TextDecorations.Underline);
                        ch.Paragraph(sp =>
                        {
                            sp.Text("ملاحظة: ").Style(PdfStyles.NoteTitle);
                            sp.Text("أفضل مولد ملفات ").Style(PdfStyles.NoteMsg);
                            sp.Text("PDF").Style(PdfStyles.NoteMsg).Culture("en-US");
                            sp.Text(" لـ ").Style(PdfStyles.NoteMsg);
                            sp.Text(".NET MAUI").Style(PdfStyles.NoteMsg).Culture("en-US");
                        });
                        ch.HorizontalLine();

                        ch.Paragraph("٢. محاذاة النص (المحاذاة الأفقية)").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("المحاذاة الأفقية داخل مربع الفقرة. يستخدم 'البداية' و'النهاية' لدعم التدويل (يتكيف تلقائياً مع اللغات من اليسار إلى اليمين ومن اليمين إلى اليسار).").Style(PdfStyles.Note);
                        ch.Paragraph("نص محاذى للبداية (السلوك الافتراضي).").HorizontalTextAlignment(TextAlignment.Start).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("نص محاذى للوسط.").HorizontalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("نص محاذى للنهاية.").HorizontalTextAlignment(TextAlignment.End).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("النص المبرر يملأ كلا الحافتين، مثالي للفقرات الطويلة. لوريم إيبسوم دولور سيت أميت، كونسيكتيتور أديبيسيسينغ إيليت. سيد دو إيوسمود تيمبور إنسيديدونت أوت لابوري إت دولوري ماغنا أليكوا.")
                            .HorizontalTextAlignment(TextAlignment.Justify).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("ملاحظة: يُستخدم 'لون الخلفية' لتصور حدود مربع الفقرة.").Style(PdfStyles.Note).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        ch.Paragraph("٣. نموذج الصندوق والموضع").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("تؤثر هذه الخصائص على مربع الفقرة (الحجم والتباعد والموضع). المحاذاة العمودية مرئية فقط عندما يكون للمربع ارتفاع صريح.").Style(PdfStyles.Note);
                        ch.Paragraph("الحشو: مساحة داخلية ١٠ نقاط.").Padding(10).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("الهامش: مساحة خارجية ١٠ نقاط.").Margin(10).BackgroundColor(Colors.LightBlue);
                        ch.Paragraph("طلب العرض ٣٠٠ نقطة، متمركز أفقياً.").WidthRequest(300).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("طلب الارتفاع ٥٠ نقطة، نص متمركز عمودياً.").HeightRequest(50).VerticalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("ملاحظة: 'لون الخلفية' يجعل المربع مرئياً لفهم كيف تؤثر الخصائص على التخطيط.").Style(PdfStyles.Note).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        ch.Paragraph("٤. وضع كسر السطر").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("يتحكم في كيفية التفاف النص أو اقتطاعه عند تجاوز عرض الحاوية.").Style(PdfStyles.Note);

                        var longTextAr = "لوريم إيبسوم دولور سيت أميت، كونسيكتيتور أديبيسيسينغ إيليت. سيد دو إيوسمود تيمبور إنسيديدونت أوت لابوري إت دولوري ماغنا أليكوا.";
                        ch.Paragraph("التفاف الكلمات (افتراضي): يلتف النص عند حدود الكلمات.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextAr);
                        ch.Paragraph("بدون التفاف: النص لا يلتف ويفيض.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextAr).LineBreakMode(LineBreakMode.NoWrap);
                        ch.Paragraph("التفاف الأحرف: يلتف النص عند حدود الأحرف.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextAr).LineBreakMode(LineBreakMode.CharacterWrap);
                        ch.Paragraph("اقتطاع البداية: يقتطع من البداية.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextAr).LineBreakMode(LineBreakMode.HeadTruncation);
                        ch.Paragraph("اقتطاع الوسط: يقتطع من الوسط.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextAr).LineBreakMode(LineBreakMode.MiddleTruncation);
                        ch.Paragraph("اقتطاع النهاية: يقتطع من النهاية.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextAr).LineBreakMode(LineBreakMode.TailTruncation);
                        ch.HorizontalLine();

                        ch.Paragraph("٥. خصائص التباعد (الحرف، الكلمة، السطر)").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("تتحكم هذه الخصائص في تباعد النص داخل الفقرات.").Style(PdfStyles.Note);

                        ch.Paragraph("تباعد الأحرف (افتراضي: ٠): تباعد الأحرف العادي.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("تباعد أحرف مخصص (٢): مساحة متزايدة بين كل حرف للتأكيد والقراءة.").CharacterSpacing(2).BackgroundColor(Colors.LightYellow);
                        ch.Paragraph("تباعد أحرف مخصص (٤): تباعد أحرف واسع لتأثيرات خاصة.").CharacterSpacing(4).BackgroundColor(Colors.LightCoral);

                        ch.Paragraph("تباعد الكلمات (افتراضي: ٠): تباعد الكلمات العادي.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("تباعد كلمات مخصص (٢): مساحة متزايدة بين الكلمات لقراءة أفضل في العناوين.").WordSpacing(2).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("تباعد كلمات مخصص (٣): تباعد كلمات واسع جداً لأغراض زخرفية.").WordSpacing(3).BackgroundColor(Colors.LightCyan);

                        ch.Paragraph("تباعد الأسطر (افتراضي: ١): ارتفاع السطر العادي (١٠٠٪).").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("تباعد أسطر مخصص (١.٥): ارتفاع سطر ١٥٠٪ لقراءة أفضل. توضح هذه الفقرة تباعد الأسطر المتزايد الذي يجعل النص أسهل في القراءة، خاصة للأشخاص الذين يعانون من عسر القراءة أو ضعف البصر.").LineSpacing(1.5f).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("تباعد أسطر مخصص (٢): نص بتباعد مزدوج، يُستخدم عادة في الأوراق الأكاديمية للتعليقات.").LineSpacing(2f).BackgroundColor(Colors.Lavender);

                        ch.Paragraph("تباعد مدمج: تباعد الأحرف (١.٥)، تباعد الكلمات (٢)، تباعد الأسطر (١.٨) جميعها مطبقة معاً لتأثير بصري فريد.").CharacterSpacing(1.5f).WordSpacing(2f).LineSpacing(1.8f).BackgroundColor(Colors.Honeydew);
                        ch.HorizontalLine();

                        ch.Paragraph("٦. الاستخدام المتقدم للنطاقات").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("نطاقات متعددة بأنماط مختلفة داخل فقرة واحدة.").Style(PdfStyles.Note);

                        ch.Paragraph(s =>
                        {
                            s.Text("توضح هذه الفقرة ");
                            s.Text("أنماط نطاقات متعددة").Style(PdfStyles.Highlight);
                            s.Text(" في نفس ");
                            s.Text("الفقرة").Style(PdfStyles.Code);
                            s.Text(". يمكنك الجمع بين ");
                            s.Text("مختلف").Style(PdfStyles.Emphasis);
                            s.Text(" الأنماط مثل ");
                            s.Text("غامق").FontAttributes(FontAttributes.Bold);
                            s.Text("، ");
                            s.Text("مائل").FontAttributes(FontAttributes.Italic);
                            s.Text("، ");
                            s.Text("مسطر").TextDecorations(TextDecorations.Underline);
                            s.Text("، و");
                            s.Text("ملون").TextColor(Colors.DarkGreen);
                            s.Text(" نص بسلاسة.");
                        });
                        ch.HorizontalLine();

                        ch.Paragraph("٧. التركيب النهائي").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("الجمع بين خصائص متعددة لنتائج محددة.").Style(PdfStyles.Note);
                        ch.Paragraph("خط كوميك، حجم ١٢، نص أحمر داكن، متمركز في مربع ٣٧٥ نقطة مع خلفية صفراء وحشو ١٥ نقطة.")
                            .FontFamily(PdfFonts.Harmattan).FontSize(12).TextColor(Colors.DarkRed)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .WidthRequest(375)
                            .BackgroundColor(Colors.LightYellow)
                            .Padding(15);
                    });
                })
                .Build()
                .ContentPage()
                .Culture("he-IL")
                .DefaultFont(f => f.Family(PdfFonts.NotoSansHebrew))
                .Content(c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("הצגת פסקאות").Style(PdfStyles.Title);

                        ch.Paragraph("מסמך זה מדגים את כל תכונות הפסקה כולל גופנים, יישור, ריווח ותמיכה בבינאום.").Style(PdfStyles.Note);

                        ch.HorizontalLine();

                        ch.Paragraph("א. מאפייני גופן וסגנון").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("מאפיינים אלה משנים את מראה הטקסט עצמו.").Style(PdfStyles.Note);
                        ch.Paragraph("משפחת גופן: קומיק סמיבולד.").FontFamily(PdfFonts.VarelaRound);
                        ch.Paragraph("גודל גופן: 16 נקודות.").FontSize(16);
                        ch.Paragraph("תכונות גופן: מודגש.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("תכונות גופן: נטוי.").FontAttributes(FontAttributes.Italic);
                        ch.Paragraph("תכונות גופן: מודגש + נטוי.").FontAttributes(FontAttributes.Bold | FontAttributes.Italic);
                        ch.Paragraph("קישוטי טקסט: קו תחתון.").TextDecorations(TextDecorations.Underline);
                        ch.Paragraph("קישוטי טקסט: קו חוצה.").TextDecorations(TextDecorations.Strikethrough);
                        ch.Paragraph("המרת טקסט: אותיות גדולות.").TextTransform(TextTransform.Uppercase);
                        ch.Paragraph("המרת טקסט: אותיות קטנות.").TextTransform(TextTransform.Lowercase);
                        ch.Paragraph("צבע טקסט: אדום.").TextColor(Colors.Red);
                        ch.HorizontalLine();

                        ch.Paragraph(sp =>
                        {
                            sp.Text("זה ").TextTransform(TextTransform.Uppercase);
                            sp.Text("טקסט עם").FontAttributes(FontAttributes.Bold).TextColor(Colors.Red);
                            sp.Text(" טווחים");
                        })
                        .TextColor(Colors.Blue).TextDecorations(TextDecorations.Underline);
                        ch.Paragraph(sp =>
                        {
                            sp.Text("הערה: ").Style(PdfStyles.NoteTitle);
                            sp.Text("מחולל קבצי ").Style(PdfStyles.NoteMsg);
                            sp.Text("PDF").Style(PdfStyles.NoteMsg).Culture("en-US");
                            sp.Text(" הטוב ביותר עבור ").Style(PdfStyles.NoteMsg);
                            sp.Text(".NET MAUI").Style(PdfStyles.NoteMsg).Culture("en-US");
                        });
                        ch.HorizontalLine();

                        ch.Paragraph("ב. יישור טקסט (יישור אופקי)").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("יישור אופקי בתוך תיבת הפסקה. משתמש ב'התחלה' ו'סוף' לתמיכה בבינאום (מסתגל אוטומטית לשפות LTR ו-RTL).").Style(PdfStyles.Note);
                        ch.Paragraph("טקסט מיושר להתחלה (התנהגות ברירת מחדל).").HorizontalTextAlignment(TextAlignment.Start).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("טקסט מיושר למרכז.").HorizontalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("טקסט מיושר לסוף.").HorizontalTextAlignment(TextAlignment.End).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("טקסט מיושר ממלא את שני הקצוות, אידיאלי לפסקאות ארוכות. לורם איפסום דולור סיט אמט, קונסקטטור אדיפיסינג אלית. סד דו איוסמוד טמפור אינסידידונט אוט לאבורה את דולורה מגנה אליקוא.")
                            .HorizontalTextAlignment(TextAlignment.Justify).BackgroundColor(Colors.LightGray);
                        ch.Paragraph("הערה: 'צבע רקע' משמש להמחשת גבולות תיבת הפסקה.").Style(PdfStyles.Note).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        ch.Paragraph("ג. מודל תיבה ומיקום").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("מאפיינים אלה משפיעים על תיבת הפסקה (גודל, ריווח, מיקום). יישור אנכי גלוי רק כאשר לתיבה יש גובה מפורש.").Style(PdfStyles.Note);
                        ch.Paragraph("ריפוד: 10 נקודות מרווח פנימי.").Padding(10).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("שוליים: 10 נקודות מרווח חיצוני.").Margin(10).BackgroundColor(Colors.LightBlue);
                        ch.Paragraph("בקשת רוחב 300 נקודות, ממורכז אופקית.").WidthRequest(300).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("בקשת גובה 50 נקודות, טקסט ממורכז אנכית.").HeightRequest(50).VerticalTextAlignment(TextAlignment.Center).BackgroundColor(Colors.LightSteelBlue);
                        ch.Paragraph("הערה: 'צבע רקע' הופך את התיבה לגלויה כדי להבין כיצד מאפיינים משפיעים על הפריסה.").Style(PdfStyles.Note).Padding(5).BackgroundColor(Colors.AliceBlue);
                        ch.HorizontalLine();

                        ch.Paragraph("ד. מצב שבירת שורה").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("שולט כיצד הטקסט נעטף או נחתך בעת חריגה מרוחב המיכל.").Style(PdfStyles.Note);

                        var longTextHe = "לורם إيبسום دولור סיט אميت, קונסקטטור אדיפיסינג אלית. סד דו איוסמוד טמפור إنסידידונט אוט לאבורה את דולורה מגנה אליקוא.";
                        ch.Paragraph("עטיפת מילים (ברירת מחדל): הטקסט נעטף בגבולות המילים.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextHe);
                        ch.Paragraph("ללא עטיפה: הטקסט לא נעטף ועולה על גדותיו.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextHe).LineBreakMode(LineBreakMode.NoWrap);
                        ch.Paragraph("עטיפת תווים: הטקסט נעטף בגבולות התווים.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextHe).LineBreakMode(LineBreakMode.CharacterWrap);
                        ch.Paragraph("קיטום בראש: נחתך בהתחלה.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextHe).LineBreakMode(LineBreakMode.HeadTruncation);
                        ch.Paragraph("קיטום באמצע: נחתך באמצע.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextHe).LineBreakMode(LineBreakMode.MiddleTruncation);
                        ch.Paragraph("קיטום בסוף: נחתך בסוף.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph(longTextHe).LineBreakMode(LineBreakMode.TailTruncation);
                        ch.HorizontalLine();

                        ch.Paragraph("ה. מאפייני ריווח (תו, מילה, שורה)").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("מאפיינים אלה שולטים בריווח הטקסט בתוך פסקאות.").Style(PdfStyles.Note);

                        ch.Paragraph("ריווח תווים (ברירת מחדל: 0): ריווח תווים רגיל.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("ריווח תווים מותאם אישית (2): מרווח מוגבר בין כל תו להדגשה וקריאות.").CharacterSpacing(2).BackgroundColor(Colors.LightYellow);
                        ch.Paragraph("ריווח תווים מותאם אישית (4): ריווח תווים רחב לאפקטים מיוחדים.").CharacterSpacing(4).BackgroundColor(Colors.LightCoral);

                        ch.Paragraph("ריווח מילים (ברירת מחדל: 0): ריווח מילים רגיל.").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("ריווח מילים מותאם אישית (2): מרווח מוגבר בין מילים לקריאות טובה יותר בכותרות.").WordSpacing(2).BackgroundColor(Colors.LightGreen);
                        ch.Paragraph("ריווח מילים מותאם אישית (3): ריווח מילים רחב מאוד למטרות דקורטיביות.").WordSpacing(3).BackgroundColor(Colors.LightCyan);

                        ch.Paragraph("ריווח שורות (ברירת מחדל: 1): גובה שורה רגיל (100%).").FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("ריווח שורות מותאם אישית (1.5): גובה שורה של 150% לקריאות טובה יותר. פסקה זו מדגימה ריווח שורות מוגבר שהופך את הטקסט לקל יותר לקריאה, במיוחד עבור אנשים עם דיסלקציה או לקויות ראייה.").LineSpacing(1.5f).BackgroundColor(Colors.LightPink);
                        ch.Paragraph("ריווח שורות מותאם אישית (2): טקסט ברווח כפול, נפוץ בעבודות אקדמיות להערות.").LineSpacing(2f).BackgroundColor(Colors.Lavender);

                        ch.Paragraph("ריווח משולב: ריווח תווים (1.5), ריווח מילים (2), ריווח שורות (1.8) כולם מיושמים יחד לאפקט ויזואלי ייחודי.").CharacterSpacing(1.5f).WordSpacing(2f).LineSpacing(1.8f).BackgroundColor(Colors.Honeydew);
                        ch.HorizontalLine();

                        ch.Paragraph("ו. שימוש מתקדם בטווחים").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("מספר טווחים מעוצבים בתוך פסקה אחת.").Style(PdfStyles.Note);

                        ch.Paragraph(s =>
                        {
                            s.Text("פסקה זו מדגימה ");
                            s.Text("סגנונות טווח מרובים").Style(PdfStyles.Highlight);
                            s.Text(" באותה ");
                            s.Text("פסקה").Style(PdfStyles.Code);
                            s.Text(". אתה יכול לשלב ");
                            s.Text("שונים").Style(PdfStyles.Emphasis);
                            s.Text(" סגנונות כמו ");
                            s.Text("מודגש").FontAttributes(FontAttributes.Bold);
                            s.Text("، ");
                            s.Text("נטוי").FontAttributes(FontAttributes.Italic);
                            s.Text("، ");
                            s.Text("קו תחתון").TextDecorations(TextDecorations.Underline);
                            s.Text("، ו");
                            s.Text("צבעוני").TextColor(Colors.DarkGreen);
                            s.Text(" טקסט בצורה חלקה.");
                        });
                        ch.HorizontalLine();

                        ch.Paragraph("ז. הרכב סופי").Style(PdfStyles.SectionHeader);
                        ch.Paragraph("שילוב מספר מאפיינים לתוצאות ספציפיות.").Style(PdfStyles.Note);
                        ch.Paragraph("גופן קומיק, גודל ١٢، טקסט אדום כהה، ממרכז ב תיבה של ٣٧٥ נקודות עם רקע צהוב וריפוד של ١٥ נקודות.")
                            .FontFamily(PdfFonts.VarelaRound).FontSize(12).TextColor(Colors.DarkRed)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .HorizontalOptions(LayoutAlignment.Center)
                            .WidthRequest(375)
                            .BackgroundColor(Colors.LightYellow)
                            .Padding(15);
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    private async void GenerateImageShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Images.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - Image Showcase")))
                .ContentPage()
                .Content(async c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        // --- TÍTULO ---
                        ch.Paragraph("Image Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Comportamiento por Defecto ---
                        ch.Paragraph("1. Comportamiento por Defecto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Al pasar solo el stream, la imagen ocupa todo el ancho disponible (Fill) y ajusta su altura proporcionalmente para mostrarse completa (AspectFit).").FontSize(10).FontAttributes(FontAttributes.Italic);
                        ch.Image(imageStream);
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Modos de Aspecto (Aspect) ---
                        ch.Paragraph("2. Modos de Aspecto (Aspect)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Controla cómo se dibuja la imagen dentro de su caja asignada. Para visualizarlo, forzamos una caja fija de 200x80 con fondo azul claro.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("AspectFit (Defecto): La imagen se escala para caber completa sin deformarse. Pueden sobrar espacios (letterboxing).").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("AspectFill: La imagen llena la caja manteniendo proporción, recortando lo que sobre (zoom).").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.AspectFill).BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("Fill: La imagen se estira para llenar la caja exactamente (se deforma).").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.Fill).BackgroundColor(Colors.LightBlue);

                        ch.Paragraph("Center: La imagen no se escala, se centra en la caja. Si es más grande, se recorta.").FontSize(10).FontAttributes(FontAttributes.Bold);
                        ch.Image(imageStream).WidthRequest(200).HeightRequest(80).Aspect(Aspect.Center).BackgroundColor(Colors.LightBlue);
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Dimensiones Automáticas ---
                        ch.Paragraph("3. Dimensiones Automáticas").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Si defines solo una dimensión, la otra se calcula automáticamente manteniendo la proporción.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("Solo WidthRequest(100): La altura se ajusta sola.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).BackgroundColor(Colors.LightGreen);

                        ch.Paragraph("Solo HeightRequest(40): El ancho se ajusta solo.").FontSize(10);
                        ch.Image(imageStream).HeightRequest(40).HorizontalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightGreen);
                        ch.HorizontalLine();

                        // --- SECCIÓN 4: Alineación Horizontal ---
                        ch.Paragraph("4. Alineación Horizontal").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Si la imagen (o su caja) es más pequeña que el ancho de la página, 'HorizontalOptions' define su posición.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.Paragraph("Start (Inicio)").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).HorizontalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightPink);

                        ch.Paragraph("Center (Centro)").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).HorizontalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightPink);

                        ch.Paragraph("End (Final)").FontSize(10);
                        ch.Image(imageStream).WidthRequest(100).HorizontalOptions(LayoutAlignment.End).BackgroundColor(Colors.LightPink);
                        ch.HorizontalLine();

                        // --- SECCIÓN 5: Modelo de Caja (Padding y Margin) ---
                        ch.Paragraph("5. Modelo de Caja").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Padding: Espacio entre el borde de la caja (fondo) y la imagen.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).Padding(10).BackgroundColor(Colors.Orange);

                        ch.Paragraph("Margin: Espacio fuera de la caja.").FontSize(10);
                        ch.Image(imageStream).WidthRequest(150).Margin(20, 0, 0, 0).BackgroundColor(Colors.Orange);
                        ch.HorizontalLine();

                        // --- SECCIÓN 6: Composición Final (Estilo Polaroid) ---
                        ch.Paragraph("6. Ejemplo Complejo: Estilo Polaroid").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Combinando Padding, BackgroundColor y Alineación para crear un marco.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(card =>
                        {
                            card.WidthRequest(200)
                                .HorizontalOptions(LayoutAlignment.Center)
                                .BackgroundColor(Colors.WhiteSmoke)
                                .Padding(10) // Marco blanco
                                .Spacing(5);

                            card.Children(cardContent =>
                            {
                                // Imagen cuadrada
                                cardContent.Image(imageStream)
                                    .HeightRequest(180)
                                    .BackgroundColor(Colors.LightGray);

                                // Texto al pie
                                cardContent.Paragraph("Microsoft Logo")
                                    .HorizontalTextAlignment(TextAlignment.Center)
                                    .FontFamily(PdfFonts.Comic)
                                    .FontSize(14)
                                    .TextColor(Colors.Black);

                                cardContent.Paragraph("Ejemplo de composición")
                                    .HorizontalTextAlignment(TextAlignment.Center)
                                    .FontSize(8)
                                    .TextColor(Colors.Gray);
                            });
                        });
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    private async void GenerateHorizontalLineShowcase_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Lines.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - HorizontalLine Showcase")))
            .ContentPage()
            .Content(c =>
            {
                c.Spacing(15).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("HorizontalLine Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                        .HorizontalTextAlignment(TextAlignment.Center);

                    // --- SECCIÓN 1: Básicos ---
                    ch.Paragraph("1. Línea Básica").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Por defecto ocupa todo el ancho (Fill), grosor 1pt, color negro.").FontSize(10).FontAttributes(FontAttributes.Italic);
                    ch.HorizontalLine();

                    ch.Paragraph("Con Padding vertical para separar del texto:").FontSize(10);
                    ch.HorizontalLine().Padding(0, 10);

                    // --- SECCIÓN 2: Estilizado ---
                    ch.Paragraph("2. Estilizado (Grosor y Color)").FontSize(18).FontAttributes(FontAttributes.Bold);

                    ch.Paragraph("Grosor 5pt, Color Rojo:").FontSize(10);
                    ch.HorizontalLine().Thickness(5).Color(Colors.Red);

                    ch.Paragraph("Grosor 0.5pt (Línea fina), Color Gris:").FontSize(10);
                    ch.HorizontalLine().Thickness(0.5f).Color(Colors.Gray);

                    ch.Paragraph("Nota: Útil para separadores sutiles en listas.").FontSize(9).FontAttributes(FontAttributes.Italic);
                    ch.HorizontalLine();

                    // --- SECCIÓN 3: Control de Ancho y Alineación ---
                    ch.Paragraph("3. Ancho y Alineación").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Las líneas no tienen que ocupar todo el ancho. Pueden usarse como adornos.").FontSize(10);

                    ch.Paragraph("Ancho 100pt, Alineada al Inicio (Start):").FontSize(10);
                    ch.HorizontalLine().WidthRequest(100).Color(Colors.Blue).Thickness(2);

                    ch.Paragraph("Ancho 50%, Alineada al Centro (Center):").FontSize(10);
                    // Simulamos 50% de una página A4 (~500pt de contenido) -> 250pt
                    ch.HorizontalLine().WidthRequest(250).HorizontalOptions(LayoutAlignment.Center).Color(Colors.Blue).Thickness(2);

                    ch.Paragraph("Ancho 100pt, Alineada al Final (End):").FontSize(10);
                    ch.HorizontalLine().WidthRequest(100).HorizontalOptions(LayoutAlignment.End).Color(Colors.Blue).Thickness(2);
                    ch.HorizontalLine();

                    // --- SECCIÓN 4: Uso en Contexto ---
                    ch.Paragraph("4. Ejemplo en Contexto (Título con Adornos)").FontSize(18).FontAttributes(FontAttributes.Bold);

                    ch.HorizontalStackLayout(hsl =>
                    {
                        hsl.HorizontalOptions(LayoutAlignment.Center).Spacing(10);
                        hsl.Children(h =>
                        {
                            h.HorizontalLine().WidthRequest(50).Thickness(2).Color(Colors.DarkGoldenrod).VerticalOptions(LayoutAlignment.Center);
                            h.Paragraph("CAPÍTULO 1").FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkGoldenrod);
                            h.HorizontalLine().WidthRequest(50).Thickness(2).Color(Colors.DarkGoldenrod).VerticalOptions(LayoutAlignment.Center);
                        });
                    });
                });
            })
            .Build()
            .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateVerticalStackLayoutShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-VSL.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(m => m.Title("VerticalStackLayout Showcase")))
                .ContentPage()
                .Content(async c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("VerticalStackLayout (VSL)").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Mezcla de Vistas (Texto + Línea + Imagen) ---
                        ch.Paragraph("1. Mezcla de Vistas").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("El VSL maneja elementos de distinta naturaleza. Aquí vemos Texto, Línea e Imagen apilados.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.BackgroundColor(Colors.WhiteSmoke).Padding(15).Spacing(10);
                            vsl.Children(items =>
                            {
                                items.Paragraph("Encabezado del Reporte").FontAttributes(FontAttributes.Bold).HorizontalTextAlignment(TextAlignment.Center);
                                items.HorizontalLine().Color(Colors.DarkGray).Thickness(2);
                                items.Image(imageStream).HeightRequest(60).Aspect(Aspect.AspectFit).BackgroundColor(Colors.LightGray);
                                items.Paragraph("Pie de foto descriptivo.").FontSize(9).TextColor(Colors.Gray).HorizontalTextAlignment(TextAlignment.Center);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Alineación Transversal (Cross-Axis) con Imágenes ---
                        ch.Paragraph("2. Alineación de Imágenes").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Las imágenes suelen ser más estrechas que el contenedor. 'HorizontalOptions' es vital aquí.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(vsl =>
                        {
                            vsl.BackgroundColor(Colors.AliceBlue).Padding(10).Spacing(5);
                            vsl.Children(items =>
                            {
                                items.Paragraph("Start (Izquierda):").HorizontalOptions(LayoutAlignment.Start);
                                items.Image(imageStream).WidthRequest(50).HorizontalOptions(LayoutAlignment.Start);

                                items.Paragraph("Center (Centro):").HorizontalOptions(LayoutAlignment.Center);
                                items.Image(imageStream).WidthRequest(50).HorizontalOptions(LayoutAlignment.Center);

                                items.Paragraph("End (Derecha):").HorizontalOptions(LayoutAlignment.End);
                                items.Image(imageStream).WidthRequest(50).HorizontalOptions(LayoutAlignment.End);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Caso Real (Tarjeta de Perfil) ---
                        ch.Paragraph("3. Caso Real: Tarjeta de Perfil").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Combinando Padding, Bordes (simulados con líneas) y Alineación.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.VerticalStackLayout(card =>
                        {
                            card.WidthRequest(250).HorizontalOptions(LayoutAlignment.Center)
                                .BackgroundColor(Colors.White).Padding(0); // Sin padding externo para simular borde

                            // Borde Superior
                            card.Children(content =>
                            {
                                content.HorizontalLine().Color(Colors.DarkBlue).Thickness(5);

                                content.VerticalStackLayout(inner =>
                                {
                                    inner.Padding(15).Spacing(10);
                                    inner.Children(c =>
                                    {
                                        // Avatar Centrado
                                        c.Image(imageStream).WidthRequest(80).HeightRequest(80).HorizontalOptions(LayoutAlignment.Center);

                                        // Nombre y Cargo
                                        c.Paragraph("Jane Doe").FontSize(16).FontAttributes(FontAttributes.Bold).HorizontalTextAlignment(TextAlignment.Center);
                                        c.Paragraph("Senior Architect").TextColor(Colors.Gray).HorizontalTextAlignment(TextAlignment.Center);

                                        // Separador interno
                                        c.HorizontalLine().Color(Colors.LightGray).Padding(20, 5);

                                        // Detalles
                                        c.Paragraph("ID: 8493-22").FontSize(10).HorizontalTextAlignment(TextAlignment.Center);
                                    });
                                });

                                // Borde Inferior
                                content.HorizontalLine().Color(Colors.DarkBlue).Thickness(2);
                            });
                        });
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateHorizontalStackLayoutShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-HSL.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(m => m.Title("HorizontalStackLayout Showcase")))
                .ContentPage()
                .Content(async c =>
                {
                    c.Spacing(15).Padding(20);
                    c.Children(ch =>
                    {
                        ch.Paragraph("HorizontalStackLayout (HSL)").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                            .HorizontalTextAlignment(TextAlignment.Center);

                        // --- SECCIÓN 1: Patrón Icono + Texto ---
                        ch.Paragraph("1. Patrón Icono + Texto").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("El uso más común del HSL. Alineamos verticalmente al centro para que el texto cuadre con el icono.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(10).Padding(10).BackgroundColor(Colors.WhiteSmoke);
                            hsl.Children(items =>
                            {
                                // Icono
                                items.Image(imageStream).WidthRequest(24).HeightRequest(24).VerticalOptions(LayoutAlignment.Center);
                                // Texto
                                items.Paragraph("Configuración del Sistema").FontSize(14).VerticalTextAlignment(TextAlignment.Center);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 2: Separadores Verticales (Simulados) ---
                        ch.Paragraph("2. Separadores Verticales").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Usamos HorizontalLine con un ancho pequeño para simular guiones o separadores entre elementos.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(5).HorizontalOptions(LayoutAlignment.Center);
                            hsl.Children(items =>
                            {
                                items.Paragraph("Inicio").TextColor(Colors.Blue);
                                items.HorizontalLine().WidthRequest(10).Color(Colors.Black).VerticalOptions(LayoutAlignment.Center); // Guión
                                items.Paragraph("Productos").TextColor(Colors.Blue);
                                items.HorizontalLine().WidthRequest(10).Color(Colors.Black).VerticalOptions(LayoutAlignment.Center); // Guión
                                items.Paragraph("Detalle").FontAttributes(FontAttributes.Bold);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 3: Alineación Vertical Mixta ---
                        ch.Paragraph("3. Alineación Vertical Mixta").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Elementos de distinta altura (Imagen alta vs Texto bajo) alineados de formas diferentes.").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.BackgroundColor(Colors.AliceBlue).HeightRequest(80).Padding(5);
                            hsl.Children(items =>
                            {
                                items.Image(imageStream).WidthRequest(60).HeightRequest(60).VerticalOptions(LayoutAlignment.Start).BackgroundColor(Colors.LightPink);
                                items.Paragraph("Top").VerticalOptions(LayoutAlignment.Start);

                                items.Image(imageStream).WidthRequest(60).HeightRequest(60).VerticalOptions(LayoutAlignment.Center).BackgroundColor(Colors.LightGreen);
                                items.Paragraph("Center").VerticalOptions(LayoutAlignment.Center);

                                items.Image(imageStream).WidthRequest(60).HeightRequest(60).VerticalOptions(LayoutAlignment.End).BackgroundColor(Colors.LightBlue);
                                items.Paragraph("Bottom").VerticalOptions(LayoutAlignment.End);
                            });
                        });
                        ch.HorizontalLine();

                        // --- SECCIÓN 4: Galería de Imágenes (Overflow) ---
                        ch.Paragraph("4. Galería de Imágenes (Overflow)").FontSize(18).FontAttributes(FontAttributes.Bold);
                        ch.Paragraph("Varias imágenes en fila. Si exceden el ancho, se recortan (clipping).").FontSize(10).FontAttributes(FontAttributes.Italic);

                        ch.HorizontalStackLayout(hsl =>
                        {
                            hsl.Spacing(2);
                            hsl.Children(items =>
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    items.Image(imageStream).WidthRequest(100).HeightRequest(80).Aspect(Aspect.AspectFill);
                                }
                            });
                        });
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateGridShowcase_Clicked(object sender, EventArgs e)
    {
        var imageStream = await GetSampleImageStream();
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample-Grid.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc.Configuration(cfg => cfg.MetaData(data => data.Title("MauiPdfGenerator - Grid Showcase")))
            .ContentPage()
            .Content(async c =>
            {
                c.Spacing(15).Padding(20);
                c.Children(ch =>
                {
                    ch.Paragraph("Grid Showcase").FontSize(24).FontAttributes(FontAttributes.Bold).TextColor(Colors.DarkBlue)
                        .HorizontalTextAlignment(TextAlignment.Center);

                    // --- SECCIÓN 1: Media Object (Imagen + Texto) ---
                    ch.Paragraph("1. Patrón Media Object").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Columna Auto para la imagen, Star para el texto. Típico en listas de noticias.").FontSize(10).FontAttributes(FontAttributes.Italic);

                    ch.Grid(g =>
                    {
                        g.ColumnSpacing(10).Padding(10).BackgroundColor(Colors.WhiteSmoke);
                        g.ColumnDefinitions(cd => { cd.GridLength(PdfGridLength.Auto); cd.GridLength(PdfGridLength.Star); });

                        g.Children(cells =>
                        {
                            // Imagen (Auto)
                            cells.Image(imageStream).WidthRequest(80).HeightRequest(60).Aspect(Aspect.AspectFill).VerticalOptions(LayoutAlignment.Start);

                            // Contenido (Star)
                            cells.VerticalStackLayout(vsl =>
                            {
                                vsl.Column(1);
                                vsl.Children(t =>
                                {
                                    t.Paragraph("Título de la Noticia").FontAttributes(FontAttributes.Bold);
                                    t.Paragraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor.").FontSize(10).TextColor(Colors.Gray);
                                });
                            });
                        });
                    });
                    ch.HorizontalLine();

                    // --- SECCIÓN 2: Alineación en Celdas (Imagen vs Texto) ---
                    ch.Paragraph("2. Alineación en Celdas").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Centrando una imagen pequeña en una celda grande.").FontSize(10).FontAttributes(FontAttributes.Italic);

                    ch.Grid(g =>
                    {
                        g.BackgroundColor(Colors.AliceBlue).HeightRequest(100);
                        g.ColumnDefinitions(cd => { cd.GridLength(PdfGridUnitType.Star); cd.GridLength(PdfGridUnitType.Star); });

                        g.Children(cells =>
                        {
                            // Celda 1: Texto
                            cells.Paragraph("Texto Centrado").HorizontalTextAlignment(TextAlignment.Center).VerticalTextAlignment(TextAlignment.Center)
                                .Column(0).BackgroundColor(Colors.LightBlue);

                            // Celda 2: Imagen
                            cells.Image(imageStream).WidthRequest(40).HeightRequest(40)
                                .HorizontalOptions(LayoutAlignment.Center).VerticalOptions(LayoutAlignment.Center)
                                .Column(1).BackgroundColor(Colors.LightGreen);
                        });
                    });
                    ch.HorizontalLine();

                    // --- SECCIÓN 3: Tabla Compleja (Factura) ---
                    ch.Paragraph("3. Ejemplo Real: Detalle de Factura").FontSize(18).FontAttributes(FontAttributes.Bold);
                    ch.Paragraph("Mezcla de Texto, Líneas (bordes) e Imágenes (miniaturas de producto).").FontSize(10).FontAttributes(FontAttributes.Italic);

                    ch.Grid(table =>
                    {
                        table.RowSpacing(5);
                        table.ColumnDefinitions(cd =>
                        {
                            cd.GridLength(PdfGridLength.FromAbsolute(40));              // Miniatura
                            cd.GridLength(PdfGridLength.Star); // Descripción
                            cd.GridLength(PdfGridLength.FromAbsolute(60));              // Total
                        });

                        // Encabezados
                        table.Children(c =>
                        {
                            c.Paragraph("Img").FontAttributes(FontAttributes.Bold).Column(0);
                            c.Paragraph("Producto").FontAttributes(FontAttributes.Bold).Column(1);
                            c.Paragraph("Total").FontAttributes(FontAttributes.Bold).HorizontalTextAlignment(TextAlignment.End).Column(2);

                            // Línea separadora debajo de encabezados (Span 3 columnas)
                            c.HorizontalLine().Color(Colors.Black).Thickness(1).Row(1).Column(0).ColumnSpan(3);
                        });

                        // Fila de Datos 1
                        table.Children(c =>
                        {
                            c.Image(imageStream).WidthRequest(30).HeightRequest(30).Aspect(Aspect.AspectFill).Row(2).Column(0);
                            c.Paragraph("Laptop Gamer X500\nIntel i9, 32GB RAM").FontSize(10).Row(2).Column(1).VerticalTextAlignment(TextAlignment.Center);
                            c.Paragraph("$1200.00").HorizontalTextAlignment(TextAlignment.End).VerticalTextAlignment(TextAlignment.Center).Row(2).Column(2);
                        });

                        // Fila de Datos 2
                        table.Children(c =>
                        {
                            c.Image(imageStream).WidthRequest(30).HeightRequest(30).Aspect(Aspect.AspectFill).Row(3).Column(0);
                            c.Paragraph("Mouse Inalámbrico\nErgonómico").FontSize(10).Row(3).Column(1).VerticalTextAlignment(TextAlignment.Center);
                            c.Paragraph("$25.50").HorizontalTextAlignment(TextAlignment.End).VerticalTextAlignment(TextAlignment.Center).Row(3).Column(2);
                        });

                        // Línea final
                        table.Children(c => c.HorizontalLine().Color(Colors.Black).Thickness(1).Row(4).Column(0).ColumnSpan(3));
                    });
                });
            })
            .Build()
            .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async void GenerateSimpleReportPage_Clicked(object sender, EventArgs e)
    {
        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Simple-Report.pdf");
        try
        {
            var doc = pdfDocFactory.CreateDocument();
            await doc
                .Configuration(cfg => cfg.MetaData(data => data.Title("Reporte Simple")))
                .ReportPage()
                .Header(h =>
                {
                    h.Spacing(5);
                    h.Children(ch =>
                    {
                        ch.Paragraph("Titulo").HorizontalTextAlignment(TextAlignment.Center);
                    });
                })
                .Content(c =>
                {
                    c.Spacing(5);
                    c.Children(ch =>
                    {                        
                        ch.Paragraph("Este es el cuerpo o contenido del reporte simple.");
                    });
                })
                .Footer(f =>
                {
                    f.Spacing(5);
                    f.Children(ch =>
                    {
                        ch.Paragraph("by R and A Media Lab, Group")
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .FontAttributes(FontAttributes.Italic);
                    });
                })
                .Build()
                .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(targetFilePath) });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }

    async Task<Stream> GetSampleImageStream()
    {
        using var httpClient = new HttpClient();
        var uri = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
        var imageData = await httpClient.GetByteArrayAsync(uri);
        return new MemoryStream(imageData);
    }

    private void GenerateStylesShowcase_Clicked(object sender, EventArgs e)
    {

    }
}
