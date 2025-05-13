using Microsoft.Maui.Controls;
using System.Diagnostics;
using MauiPdfGenerator;
using static Microsoft.Maui.Graphics.Colors;
using static Microsoft.Maui.Controls.FontAttributes;
using static Microsoft.Maui.LineBreakMode;
using static Microsoft.Maui.TextAlignment;
using static MauiPdfGenerator.Fluent.Enums.PageSizeType;
using static MauiPdfGenerator.Fluent.Enums.DefaultMarginType;
using static MauiPdfGenerator.Generated.MauiFontAliases;
using static MauiPdfGenerator.Fluent.Enums.PageOrientationType;

namespace Test;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void GeneratePdf_Clicked(object sender, EventArgs e)
    {
        using var httpClient = new HttpClient();
        var uri2 = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31");
        System.Diagnostics.Debug.WriteLine($"Attempting to download image from: {uri2}");
        using Stream imageUriStream = await httpClient.GetStreamAsync(uri2);

        string targetFilePath = Path.Combine(FileSystem.CacheDirectory, "Sample.pdf");
        try
        {
            var doc = PdfGenerator.CreateDocument(); Image i = new(); 

            await doc
                .Configuration(cfg =>
                {
                    cfg.MetaData(data =>
                    {
                        Title = "MauiPdfGenerator sample";
                    });
                })
                .ContentPage()
                .DefaultFont(f => f.Family("Helvetica").Size(10))
                .Spacing(8f)
                .Content(c =>
                {
                    c.Paragraph("Text Wrapping Demonstration")
                        .FontSize(16f)
                        .FontFamily("Helvetica")
                        .FontAttributes(Bold)
                        .Alignment(End);
                    c.HorizontalLine();
                    c.Paragraph("Default (WordWrap): This is a relatively long sentence designed to test the default word wrapping behavior which should break lines at spaces.")
                        .FontFamily(OpenSansRegular);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                        .FontFamily("Courier")
                        .LineBreakMode(CharacterWrap);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(HeadTruncation);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(MiddleTruncation);
                    c.Paragraph("CharacterWrap: This_very_long_unbroken_string_will_demonstrate_CharacterWrap and This_very_long_unbroken_string_will_demonstrate_CharacterWrap, breakingmidword.")
                         .LineBreakMode(TailTruncation);
                    c.HorizontalLine();
                    c.PdfImage(imageUriStream)
                         .WidthRequest(64).HeightRequest(64)
                         .Aspect(Aspect.AspectFit);
                    c.HorizontalLine();
                    c.Paragraph("Lorem ipsum dolor sit amet est duo sed invidunt diam stet kasd imperdiet vulputate nam est duo. Sed lorem in est sadipscing vulputate lorem elit dolor odio. Justo sanctus imperdiet diam dolores. Erat et justo at kasd sed in gubergren accusam diam illum nonumy consetetur. Dolore erat stet magna nonumy volutpat ipsum voluptua gubergren tempor invidunt dignissim duis consetetur. Sed illum ut dolor sea vero amet ipsum vero praesent clita dolor. Sanctus dolor accusam vulputate facer no labore nulla diam nulla magna.\r\n\r\nIpsum clita aliquip elitr at no labore clita euismod tincidunt lorem amet invidunt sit. Et dolor ea et magna eros hendrerit diam ea delenit rebum stet gubergren. Tempor justo ipsum vel ea. Erat et nobis sed lorem autem et vero eos et autem. Odio vel rebum nostrud consequat et autem consectetuer feugiat. Tempor sed sanctus elitr vero cum placerat aliquip et ut et stet nonummy rebum eum eirmod. Esse quod at vero mazim et diam. Vero augue iriure tempor esse dolore ea sadipscing ullamcorper aliquyam. Ipsum takimata eos commodo accusam ipsum dolore. Consectetuer mazim lorem ipsum dolore sed. Kasd elitr justo et minim feugait illum gubergren lorem nonumy lorem dolor placerat eos. Ipsum at nisl sea nonumy doming rebum sit placerat iusto magna tempor amet sea et et te no vero. Et at magna dolores aliquyam ipsum nulla eos. Et vel et ipsum euismod invidunt nulla amet at eirmod molestie. Dolor sit cum elit amet esse ipsum aliquyam vero tation elitr duo voluptua ut erat sadipscing.\r\n\r\nTe magna diam. Et invidunt kasd vero hendrerit blandit diam lorem consectetuer et. In at augue iriure sanctus vel quis amet. Elit dolore hendrerit dolor zzril. Amet consequat nonumy dolor dolor est sed nostrud voluptua luptatum qui. Et adipiscing diam. Et facer lorem duo gubergren ut et no ipsum amet. Sit et invidunt erat gubergren sea ipsum lobortis eirmod at ipsum. Vel ipsum labore invidunt aliquyam molestie erat elitr dolor lorem amet diam invidunt laoreet stet erat nihil eirmod et.\r\n\r\nJusto consetetur molestie diam et feugait justo clita sed dignissim augue justo volutpat commodo. No nibh diam nibh exerci sea no. At gubergren no ullamcorper sit consectetuer kasd lorem gubergren et te dolor aliquam vel vel laoreet clita. Est praesent dolor option no amet accusam sadipscing laoreet duo. Tation sanctus hendrerit feugait dolor vero magna stet lorem eos qui dolores odio ex ipsum eos diam et. Dolor eum ea no ipsum sea at rebum ut accusam vel ipsum stet elitr ipsum et ipsum diam molestie. Et luptatum sit no elit lorem consequat veniam amet gubergren erat nobis. Quis et eos sadipscing ipsum dolor eirmod diam dolore aliquyam eos velit accusam takimata et aliquam dolore qui nobis. Eum diam dolore duo voluptua imperdiet praesent dolor erat dolor.\r\n\r\nUt elitr erat voluptua vel et lorem dolor et sed kasd invidunt. In quis dolor sed elitr delenit no sit lorem dolore accusam velit nonumy et stet. Magna lorem feugiat. Nulla dolor erat consequat in erat invidunt et stet erat qui. Nonumy kasd duis. Ut qui invidunt assum ea ea ut at duo ut est no erat vel duis erat praesent aliquyam tincidunt. Vel clita accusam stet sed sanctus consequat accusam clita elitr feugiat erat diam labore gubergren et. Diam feugait wisi duo. Et accusam sea autem sed sed rebum lorem sit lorem dolor gubergren. Exerci kasd eos eros no consequat stet dolore elitr. Sadipscing et sed vulputate sed dolore diam accusam clita lorem rebum eros ex est sit ea est euismod. Dolor sed gubergren erat iriure no magna iriure ea ea rebum accusam consequat nonummy et consequat et clita. Ipsum et justo sed takimata nam feugait autem. Volutpat sadipscing nibh et nisl et. Nonummy et at dolor tincidunt diam nostrud.\r\n\r\nErat wisi sit et luptatum invidunt at dolor nonummy nonumy duo dolore sit esse elitr tempor clita et vero. Accusam cum amet voluptua vel vero. Sit consetetur magna sit dolore et eos no tempor stet option labore consetetur duo in dolore. Voluptua ut diam sed et magna ad sit. Takimata ut at ut aliquam et sadipscing sanctus vero sea. Dolore invidunt nonumy nonumy dolor lorem eos. Magna feugiat est clita et et aliquam duis ipsum nonumy duo delenit. Amet diam ea tempor dolore dolor nostrud erat sed eirmod amet vel labore sanctus. Amet dolore ut stet at invidunt duis justo et mazim takimata ipsum sit et sed.\r\n\r\nSed amet dolores eos vero sea vel nobis aliquyam dolor diam vero kasd eros. Kasd rebum nobis et est sanctus sed ea eleifend diam ad quis sea erat exerci nonumy stet accusam est. Nisl nisl dolores accusam accumsan est aliquyam clita dolores aliquyam voluptua takimata. Odio iriure odio. Adipiscing rebum duo tempor dolores et doming eu clita justo labore soluta clita ut takimata nonumy dolor. Nonumy autem magna facilisi lorem no et magna volutpat amet. Augue consequat voluptua sanctus et ex takimata vulputate lorem duo sanctus sed eros erat et. Nonummy lorem aliquyam et accusam stet lorem ipsum consetetur gubergren nonumy ea euismod eros est magna no.\r\n\r\nVolutpat sea eum delenit justo sit dolore. Labore dolores nonumy ut amet dolores nulla ipsum et. Ad labore amet vel facilisi nonummy eos tation tation ea kasd stet. Dolore aliquam luptatum dolore sanctus aliquyam sed diam justo est. Lorem invidunt sanctus nonumy stet vel amet nonumy clita vulputate diam takimata rebum eirmod duo et. Nibh sea aliquam erat stet takimata takimata invidunt erat sanctus vero et dolor dolor augue. Vel euismod dignissim dolore duo dolore. Et et dolores labore et minim dolores elitr est stet ea aliquam sed invidunt veniam ut erat lorem lorem. Et hendrerit et dolore clita consequat consetetur aliquyam diam. Sed at nonumy amet lorem eleifend invidunt et ut in. Ea blandit no invidunt sed justo voluptua elitr diam euismod nibh et eu odio zzril sanctus et dolor nulla. Esse augue sed erat nonumy diam labore nam kasd wisi sed consequat aliquyam iusto diam. Sea feugait consequat nonumy dolor. Dolor nonumy illum quis nonummy dolore. Amet magna ex diam takimata vero ut elitr voluptua. Aliquam ut aliquyam erat nibh ea aliquyam amet kasd diam consetetur ipsum rebum sea ipsum et dolores.\r\n\r\nIpsum amet diam feugiat accusam dolor labore in aliquam eos amet voluptua imperdiet consequat rebum kasd. Stet exerci nisl ipsum dolor facilisis volutpat sit est dolor duo iriure nihil diam aliquyam. Accusam dolor ut amet et dolores eum doming diam diam diam euismod blandit kasd sit blandit magna lorem lorem. Et justo labore zzril takimata eirmod dolores labore amet gubergren et consequat. Et facilisis dolor at consequat ex quis at clita sadipscing kasd sit sed kasd exerci sit duis amet sadipscing. Kasd gubergren volutpat justo eirmod stet et dolores iriure kasd amet nam. Wisi dolor diam sadipscing diam delenit aliquam sed zzril iriure voluptua nonumy nulla ea accusam. Et dolor eos nihil ea lorem laoreet sanctus. Vero stet kasd duis lorem dolores ipsum et et elitr enim enim sea no. Vero diam dolor ut autem aliquyam exerci est. Exerci et no nonumy gubergren dolor magna sadipscing ut tincidunt illum invidunt. Stet eu amet diam molestie nonumy mazim at gubergren duis elitr voluptua aliquam eirmod dolor dolores et. Et takimata sanctus eirmod dolore euismod no delenit sed accusam accusam dolore ea gubergren stet.\r\n\r\nConsectetuer consetetur et augue justo stet aliquyam ea feugiat soluta dolor est sea. Nibh velit takimata dolore sed ut at dolore et dolor et. Consequat est consetetur ipsum et clita accusam feugait duo. Clita laoreet wisi amet diam tation lorem eos clita sadipscing. Labore duis gubergren hendrerit gubergren invidunt ex et in tincidunt ea feugait. Diam sea sanctus magna aliquam ut labore. Labore ut vero eu invidunt et sea dolore nulla sit in.");
                })
                .Build()
            .SaveAsync(targetFilePath);

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(targetFilePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error generando PDF: {ex.Message}", "OK");
        }
    }
}
