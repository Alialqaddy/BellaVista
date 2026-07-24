using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Seeds realistic demo content (no Lorem Ipsum) so the site is demo-able right after
/// the first `dotnet run`, without anyone having to click through the backoffice first.
/// Every translatable field is seeded in German, English and Arabic (see README on why
/// this uses parallel "xxxEn"/"xxxAr" properties instead of Umbraco's native culture
/// variance). Dish names stay Italian in all three languages, as on a real menu.
/// </summary>
public class ContentSeeder
{
    private readonly IContentService _contentService;
    private readonly MediaSeeder _media;
    private readonly MemberAndAccessSeeder _memberAndAccess;
    private readonly TemplateSeeder _templates;

    public ContentSeeder(IContentService contentService, MediaSeeder media, MemberAndAccessSeeder memberAndAccess, TemplateSeeder templates)
    {
        _contentService = contentService;
        _media = media;
        _memberAndAccess = memberAndAccess;
        _templates = templates;
    }

    private void Publish(IContent content) => _contentService.SaveAndPublish(content, userId: Constants.Security.SuperUserId);

    private static string Placeholder(string name) => $"placeholders/{name}.jpg";

    /// <summary>Sets a base (German) property plus its English/Arabic parallel properties.</summary>
    private static void SetT(IContent content, string alias, string de, string en, string ar)
    {
        content.SetValue(alias, de);
        content.SetValue(alias + "En", en);
        content.SetValue(alias + "Ar", ar);
    }

    public async Task SeedAllAsync()
    {
        if (_contentService.GetRootContent().Any()) return; // already seeded

        IContent home = CreateHome();
        IContent about = CreateAbout(home);
        IContent menu = CreateMenu(home);
        IContent gallery = CreateGallery(home);
        IContent news = CreateNews(home);
        IContent contact = CreateContact(home);
        IContent loyalGuests = CreateLoyalGuests(home);
        IContent login = CreateLogin(home);
        IContent accessDenied = CreateAccessDenied(home);

        await _memberAndAccess.SeedGroupAndTestMemberAsync();
        _memberAndAccess.ProtectNode(loyalGuests, login, accessDenied);
    }

    private IContent CreateHome()
    {
        IContent home = _contentService.Create("Bella Vista", -1, "home");
        SetT(home, "pageTitle", "Bella Vista - Ristorante Italiano", "Bella Vista - Italian Restaurant", "بيلا فيستا - مطعم إيطالي");
        SetT(home, "metaDescription",
            "Bella Vista ist ein familiengeführtes italienisches Restaurant mit frischer, hausgemachter Pasta, Pizza aus dem Holzofen und saisonalen Spezialitäten.",
            "Bella Vista is a family-run Italian restaurant serving fresh, homemade pasta, wood-fired pizza and seasonal specials.",
            "بيلا فيستا مطعم إيطالي عائلي يقدم المعكرونة الطازجة محلية الصنع والبيتزا من الفرن الخشبي وأطباق موسمية مميزة.");
        home.SetValue("metaKeywords", "italienisches restaurant, pizza, pasta, bella vista");
        SetT(home, "titleAppend", "Bella Vista Ristorante", "Bella Vista Ristorante", "بيلا فيستا ريستورانتي");

        string slider = BlockValueBuilder.BuildSlider(new[]
        {
            new BlockValueBuilder.SlideItemInput(
                "Benvenuti da Bella Vista", "Welcome to Bella Vista", "أهلاً بكم في بيلا فيستا",
                "Authentische italienische Küche mitten in der Stadt", "Authentic Italian cooking in the heart of town", "مطبخ إيطالي أصيل في قلب المدينة",
                _media.FromThemeImage(Placeholder("hero-welcome"))),
            new BlockValueBuilder.SlideItemInput(
                "Frisch, saisonal, hausgemacht", "Fresh, Seasonal, Homemade", "طازج وموسمي ومصنوع منزلياً",
                "Unsere Pasta wird jeden Morgen frisch gerollt", "Our pasta is rolled fresh every single morning", "يتم تحضير المعكرونة طازجة كل صباح",
                _media.FromThemeImage(Placeholder("hero-pasta"))),
            new BlockValueBuilder.SlideItemInput(
                "Ein Tisch mit Aussicht", "A Table With a View", "طاولة بإطلالة رائعة",
                "Genießen Sie Ihr Essen auf unserer Terrasse", "Enjoy your meal out on our terrace", "استمتع بوجبتك على تراستنا",
                _media.FromThemeImage(Placeholder("hero-terrace"))),
        });
        home.SetValue("slider", slider);

        Publish(home);
        return home;
    }

    private IContent CreateAbout(IContent home)
    {
        IContent about = _contentService.Create("About Us", home, "contentPage");
        SetT(about, "pageTitle", "Über uns", "About Us", "من نحن");
        SetT(about, "metaDescription",
            "Die Geschichte hinter Bella Vista, unser Küchenteam und warum unsere Gäste immer wiederkommen.",
            "The story behind Bella Vista, our kitchen team and why guests keep coming back.",
            "قصة بيلا فيستا وفريق مطبخنا ولماذا يعود ضيوفنا دائماً.");
        about.SetValue("metaKeywords", "über uns, italienisches restaurant, köche");
        SetT(about, "heroHeading", "Über uns", "About Us", "من نحن");
        SetT(about, "bodyText",
            """
            <h3>Unsere Geschichte</h3>
            <p>Bella Vista hat 2014 eröffnet, gegründet von der Familie Moretti mit einer einfachen Idee:
            das Essen kochen, mit dem sie selbst aufgewachsen sind, und es mit der Nachbarschaft teilen. Was als
            kleine Trattoria mit sechs Tischen begann, ist zu einem vollständigen Restaurant gewachsen - doch die
            Küche arbeitet noch immer nach den Rezepten von Nonna Moretti.</p>
            <h3>Unser Team</h3>
            <p>Küchenchef Marco Moretti hat in Bologna gelernt, bevor er zurückkam, um die Küche zu leiten. An seiner
            Seite backt Patissière Elena Rossi jeden Tag frisch alles, was als Dessert serviert wird, und unser
            Service-Team ist fast von Anfang an dabei.</p>
            <h3>Warum uns wählen</h3>
            <ul>
                <li>Frische Pasta, täglich im Haus hergestellt</li>
                <li>Holzofen, importiert aus Neapel</li>
                <li>Saisonale Karte, die sich nach dem Markt richtet</li>
                <li>Weinkarte mit kleinen italienischen Weingütern</li>
            </ul>
            """,
            """
            <h3>Our Story</h3>
            <p>Bella Vista opened its doors in 2014, started by the Moretti family with one simple idea:
            cook the same food they grew up with at home, and share it with the neighbourhood. What began
            as a small trattoria with six tables has grown into a full restaurant, but the kitchen still
            runs on the same recipes passed down from Nonna Moretti.</p>
            <h3>Our Team</h3>
            <p>Head chef Marco Moretti trained in Bologna before returning home to run the kitchen. Alongside
            him, pastry chef Elena Rossi bakes everything served for dessert fresh each day, and our
            front-of-house team has been with us since almost the very first service.</p>
            <h3>Why Choose Us</h3>
            <ul>
                <li>Fresh pasta made in-house every morning</li>
                <li>Wood-fired oven imported from Naples</li>
                <li>Seasonal menu that changes with what's fresh at the market</li>
                <li>A wine list built around small Italian producers</li>
            </ul>
            """,
            """
            <h3>قصتنا</h3>
            <p>افتتحت بيلا فيستا أبوابها عام 2014 على يد عائلة موريتي، بفكرة بسيطة: طهي نفس الأطعمة التي
            نشأوا عليها في المنزل ومشاركتها مع الحي. ما بدأ كمطعم صغير بستة طاولات نما ليصبح مطعماً
            كاملاً، لكن المطبخ ما زال يعمل بنفس وصفات الجدة موريتي.</p>
            <h3>فريقنا</h3>
            <p>تدرب الشيف ماركو موريتي في بولونيا قبل أن يعود لإدارة المطبخ. وإلى جانبه، تُعِدّ شيفة
            الحلويات إيلينا روسي كل ما يُقدَّم كحلوى طازجاً كل يوم، وفريق الخدمة معنا منذ الأيام الأولى تقريباً.</p>
            <h3>لماذا تختارنا</h3>
            <ul>
                <li>معكرونة طازجة تُحضَّر يومياً في المطبخ</li>
                <li>فرن خشبي مستورد من نابولي</li>
                <li>قائمة موسمية تتغير حسب ما هو طازج في السوق</li>
                <li>قائمة نبيذ من مزارع إيطالية صغيرة</li>
            </ul>
            """);
        Publish(about);
        return about;
    }

    private IContent CreateMenu(IContent home)
    {
        IContent menu = _contentService.Create("Menu", home, "menuPage");
        SetT(menu, "pageTitle", "Speisekarte", "Menu", "قائمة الطعام");
        SetT(menu, "metaDescription",
            "Vorspeisen, Hauptgerichte, Desserts und Getränke bei Bella Vista - frische Pasta, Pizza aus dem Holzofen und hausgemachtes Tiramisù.",
            "Starters, mains, desserts and drinks at Bella Vista - fresh pasta, wood-fired pizza and homemade tiramisu.",
            "مقبلات وأطباق رئيسية وحلويات ومشروبات في بيلا فيستا - معكرونة طازجة وبيتزا من الفرن الخشبي وتيراميسو منزلي.");
        menu.SetValue("metaKeywords", "speisekarte, pasta, pizza, dessert");
        SetT(menu, "heroHeading", "Unsere Speisekarte", "Our Menu", "قائمة طعامنا");
        SetT(menu, "intro",
            "Alles unten wird auf Bestellung in unserer offenen Küche zubereitet. Bitte informieren Sie Ihren Kellner über Allergien.",
            "Everything below is cooked to order in our open kitchen. Let your server know about any allergies.",
            "كل ما يلي يُحضَّر عند الطلب في مطبخنا المفتوح. يرجى إبلاغ النادل بأي حساسية غذائية.");

        Guid starterImg = _media.FromThemeImage(Placeholder("menu-starter"));
        Guid mainImg = _media.FromThemeImage(Placeholder("menu-main"));
        Guid dessertImg = _media.FromThemeImage(Placeholder("menu-dessert"));
        Guid drinkImg = _media.FromThemeImage(Placeholder("menu-drink"));

        string dishes = BlockValueBuilder.BuildMenu(new[]
        {
            new BlockValueBuilder.MenuSectionInput("Vorspeisen", "Starters", "المقبلات", new[]
            {
                new BlockValueBuilder.DishInput("Bruschetta al Pomodoro",
                    "Getoastetes Sauerteigbrot, Rispentomaten, Basilikum, Knoblauch und Olivenöl.",
                    "Toasted sourdough, vine tomatoes, basil, garlic and olive oil.",
                    "خبز عجين مخمر محمّص، طماطم عنقودية، ريحان، ثوم وزيت زيتون.",
                    "6,50 €", "Starter", starterImg, 0, false),
                new BlockValueBuilder.DishInput("Arancini",
                    "Knusprige Risottobällchen, gefüllt mit Mozzarella und langsam geschmortem Ragù.",
                    "Crisp risotto balls filled with mozzarella and slow-cooked ragù.",
                    "كرات ريزوتو مقرمشة محشوة بالموزاريلا ورागو مطهو ببطء.",
                    "7,90 €", "Starter", starterImg, 1, true),
                new BlockValueBuilder.DishInput("Carpaccio di Manzo",
                    "Hauchdünnes Rinderfilet, Rucola, Parmesanspäne, Zitronendressing.",
                    "Thin-sliced beef, rocket, parmesan shavings, lemon dressing.",
                    "شرائح لحم بقري رقيقة جداً، جرجير، رقائق بارميزان، صلصة الليمون.",
                    "9,50 €", "Starter", starterImg, 0, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Hauptgerichte", "Mains", "الأطباق الرئيسية", new[]
            {
                new BlockValueBuilder.DishInput("Tagliatelle al Tartufo",
                    "Frische Eier-Tagliatelle, schwarze Trüffelcreme, Parmesan.",
                    "Fresh egg tagliatelle, black truffle cream, parmesan.",
                    "تالياتيلي بيض طازج، كريمة الكمأة السوداء، بارميزان.",
                    "16,90 €", "Main", mainImg, 0, true),
                new BlockValueBuilder.DishInput("Pizza Diavola",
                    "Aus dem Holzofen, San-Marzano-Tomaten, Mozzarella, scharfe Salami, Chiliöl.",
                    "Wood-fired, San Marzano tomato, mozzarella, spicy salami, chilli oil.",
                    "من الفرن الخشبي، طماطم سان مارزانو، موزاريلا، سلامي حار، زيت الفلفل الحار.",
                    "12,50 €", "Main", mainImg, 3, false),
                new BlockValueBuilder.DishInput("Osso Buco alla Milanese",
                    "Geschmorte Kalbshaxe, Safranrisotto, Gremolata.",
                    "Braised veal shank, saffron risotto, gremolata.",
                    "ساق عجل مطهوة ببطء، ريزوتو الزعفران، غريمولاتا.",
                    "22,00 €", "Main", mainImg, 1, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Desserts", "Desserts", "الحلويات", new[]
            {
                new BlockValueBuilder.DishInput("Tiramisù della Casa",
                    "Espresso-getränkte Savoiardi, Mascarponecreme, Kakao.",
                    "Espresso-soaked savoiardi, mascarpone cream, cocoa.",
                    "بسكويت سافوياردي منقوع بالإسبريسو، كريمة الماسكاربوني، كاكاو.",
                    "6,90 €", "Dessert", dessertImg, 0, true),
                new BlockValueBuilder.DishInput("Panna Cotta ai Frutti di Bosco",
                    "Vanille-Panna-Cotta, Waldbeerkompott.",
                    "Vanilla panna cotta, forest berry compote.",
                    "بانا كوتا الفانيليا، كومبوت التوت البري.",
                    "6,50 €", "Dessert", dessertImg, 0, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Getränke", "Drinks", "المشروبات", new[]
            {
                new BlockValueBuilder.DishInput("Chianti Classico (Glas)",
                    "Trockener, mittelkräftiger Rotwein aus der Toskana.",
                    "Dry, medium-bodied red wine from Tuscany.",
                    "نبيذ أحمر جاف متوسط القوام من توسكانا.",
                    "6,00 €", "Drink", drinkImg, 0, false),
                new BlockValueBuilder.DishInput("Aperol Spritz",
                    "Aperol, Prosecco, Soda, Orangenscheibe.",
                    "Aperol, prosecco, soda, orange slice.",
                    "أبيرول، بروسيكو، صودا، شريحة برتقال.",
                    "7,50 €", "Drink", drinkImg, 0, false),
                new BlockValueBuilder.DishInput("Espresso",
                    "Ein Shot, geröstet für uns von einer Saarbrücker Rösterei.",
                    "A single shot, roasted for us by a local Saarbrücken roastery.",
                    "شوت واحد، محمّص لنا من محمصة محلية في زاربروكن.",
                    "2,50 €", "Drink", drinkImg, 0, false),
            }),
        });
        menu.SetValue("dishes", dishes);

        Publish(menu);
        return menu;
    }

    private IContent CreateGallery(IContent home)
    {
        IContent gallery = _contentService.Create("Gallery", home, "galleryPage");
        SetT(gallery, "pageTitle", "Galerie", "Gallery", "معرض الصور");
        SetT(gallery, "metaDescription",
            "Fotos aus dem Gastraum, der Küche und der Terrasse von Bella Vista.",
            "Photos from the Bella Vista dining room, kitchen and terrace.",
            "صور من صالة الطعام والمطبخ والتراس في بيلا فيستا.");
        gallery.SetValue("metaKeywords", "galerie, fotos, restaurant");
        SetT(gallery, "heroHeading", "Galerie", "Gallery", "معرض الصور");

        Guid starterImg = _media.FromThemeImage(Placeholder("gallery-starter"));
        Guid starterImg2 = _media.FromThemeImage(Placeholder("gallery-starter-2"));
        Guid mainImg = _media.FromThemeImage(Placeholder("gallery-main"));
        Guid mainImg2 = _media.FromThemeImage(Placeholder("gallery-main-2"));
        Guid dessertImg = _media.FromThemeImage(Placeholder("gallery-dessert"));
        Guid dessertImg2 = _media.FromThemeImage(Placeholder("gallery-dessert-2"));
        Guid drinkImg = _media.FromThemeImage(Placeholder("gallery-drink"));
        Guid drinkImg2 = _media.FromThemeImage(Placeholder("gallery-drink-2"));

        string images = BlockValueBuilder.BuildGallery(new[]
        {
            new BlockValueBuilder.GalleryImageInput("Bruschetta, frisch aus der Küche", "Bruschetta, fresh from the kitchen", "بروسكيتا طازجة من المطبخ", "Starter", starterImg),
            new BlockValueBuilder.GalleryImageInput("Unsere Arancini, für zwei angerichtet", "Our arancini, plated for two", "أرانشيني، مقدمة لشخصين", "Starter", starterImg2),
            new BlockValueBuilder.GalleryImageInput("Tagliatelle al Tartufo", "Tagliatelle al Tartufo", "تالياتيلي بالكمأة", "Main", mainImg),
            new BlockValueBuilder.GalleryImageInput("Pizza auf dem Weg in den Holzofen", "Pizza going into the wood oven", "بيتزا في طريقها إلى الفرن الخشبي", "Main", mainImg2),
            new BlockValueBuilder.GalleryImageInput("Tiramisù, jeden Morgen frisch gemacht", "Tiramisù, made fresh every morning", "تيراميسو، يُحضَّر طازجاً كل صباح", "Dessert", dessertImg),
            new BlockValueBuilder.GalleryImageInput("Panna Cotta mit Beerenkompott", "Panna cotta with berry compote", "بانا كوتا مع كومبوت التوت", "Dessert", dessertImg2),
            new BlockValueBuilder.GalleryImageInput("Aperol Spritz auf der Terrasse", "Aperol spritz on the terrace", "أبيرول سبريتز على التراس", "Drink", drinkImg),
            new BlockValueBuilder.GalleryImageInput("Unsere Weinkarte", "Our wine list", "قائمة النبيذ لدينا", "Drink", drinkImg2),
        });
        gallery.SetValue("images", images);

        Publish(gallery);
        return gallery;
    }

    private IContent CreateNews(IContent home)
    {
        IContent news = _contentService.Create("News", home, "newsPage");
        SetT(news, "pageTitle", "Neuigkeiten & Veranstaltungen", "News & Events", "الأخبار والفعاليات");
        SetT(news, "metaDescription",
            "Die neuesten Nachrichten, Veranstaltungen und Aktionen von Bella Vista.",
            "The latest news, events and promotions from Bella Vista.",
            "أحدث الأخبار والفعاليات والعروض من بيلا فيستا.");
        news.SetValue("metaKeywords", "neuigkeiten, veranstaltungen, aktionen");
        SetT(news, "heroHeading", "Neuigkeiten & Veranstaltungen", "News & Events", "الأخبار والفعاليات");
        SetT(news, "intro", "Was sich im Restaurant und drumherum tut.", "What's happening in and around the restaurant.", "ما الذي يحدث في المطعم وحوله.");
        Publish(news);

        (string TitleDe, string TitleEn, string TitleAr, string TeaserDe, string TeaserEn, string TeaserAr, string BodyDe, string BodyEn, string BodyAr, string Image)[] items =
        {
            ("Winter-Trüffelkarte ist da", "Winter Truffle Menu Is Here", "قائمة الكمأة الشتوية وصلت",
             "Frische schwarze Trüffel, wöchentlich eingeflogen, auf vier Gerichten bis Ende Februar.",
             "Fresh black truffle, flown in weekly, on four dishes until the end of February.",
             "كمأة سوداء طازجة، تُشحن أسبوعياً، في أربعة أطباق حتى نهاية فبراير.",
             "<p>Jeden Winter bringen wir frische schwarze Trüffel für eine kurze Vier-Gänge-Karte - dieses Jahr auf der Tagliatelle, dem Risotto, einem Trüffel-Arancino und einem Steak mit Trüffelbutter. Solange der Vorrat reicht - am besten bei der Reservierung nachfragen.</p>",
             "<p>Every winter we bring in fresh black truffle for a short, four-dish menu - this year it lands on the tagliatelle, the risotto, a truffle arancino and a truffle-butter steak. Available while stock lasts, usually a table's best bet is to ask when you book.</p>",
             "<p>كل شتاء نحضر كمأة سوداء طازجة لقائمة قصيرة من أربعة أطباق - هذا العام على التالياتيلي والريزوتو وأرانشيني الكمأة وستيك بزبدة الكمأة. متوفر حتى نفاد الكمية، ويُفضَّل السؤال عند الحجز.</p>",
             "news-truffle"),
            ("Bella Vista wird 10", "Bella Vista Turns 10", "بيلا فيستا يحتفل بعامه العاشر",
             "Zehn Jahre, seit die Familie Moretti die Türen geöffnet hat - danke an alle, die bei uns gegessen haben.",
             "Ten years since the Moretti family opened the doors - thank you to everyone who has eaten with us.",
             "عشر سنوات منذ أن فتحت عائلة موريتي أبوابها - شكراً لكل من تناول الطعام معنا.",
             "<p>Vor zehn Jahren hat die Familie Moretti eine Trattoria mit sechs Tischen ein paar Straßen von hier eröffnet. Seitdem sind wir gewachsen, aber die Küche arbeitet noch immer nach denselben Rezepten. Danke an jeden Gast, der immer wieder zu uns zurückgekommen ist - Sie sind der Grund, warum es uns noch gibt.</p>",
             "<p>Ten years ago this month, the Moretti family opened a six-table trattoria a few streets from here. We've grown since, but the kitchen is still run on the same recipes. Thank you to every guest who has come back again and again - you're the reason we're still here.</p>",
             "<p>قبل عشر سنوات، افتتحت عائلة موريتي مطعماً صغيراً بستة طاولات على بعد شارعين من هنا. لقد نمونا منذ ذلك الحين، لكن المطبخ ما زال يعمل بنفس الوصفات. شكراً لكل ضيف عاد إلينا مراراً - أنتم السبب في استمرارنا.</p>",
             "news-anniversary"),
            ("Live-Musik auf der Terrasse", "Live Music on the Terrace", "موسيقى حية على التراس",
             "Jeden Freitagabend diesen Sommer spielen lokale Musiker bei uns zum Abendessen.",
             "Every Friday evening this summer, local musicians join us for dinner service.",
             "كل مساء جمعة هذا الصيف، ينضم إلينا موسيقيون محليون خلال العشاء.",
             "<p>Ab diesem Freitag laden wir jeden Freitagabend über den Sommer lokale Akustik-Musiker auf unsere Terrasse ein. Kein Eintritt, nur gutes Essen und Live-Musik - Reservierung empfohlen, da die Terrassentische schnell weg sind.</p>",
             "<p>Starting this Friday, we're hosting local acoustic musicians out on the terrace every Friday evening through the summer. No cover charge, just good food and live music - reservations recommended as terrace tables go quickly.</p>",
             "<p>بدءاً من هذه الجمعة، نستضيف موسيقيين أكوستيك محليين على التراس كل مساء جمعة طوال الصيف. بدون رسوم دخول، فقط طعام جيد وموسيقى حية - يُنصح بالحجز لأن طاولات التراس تنفد بسرعة.</p>",
             "news-music"),
            ("Neue Weinkarte, nur kleine Winzer", "New Wine List, Small Producers Only", "قائمة نبيذ جديدة، منتجون صغار فقط",
             "Wir haben unsere Weinkarte komplett um kleine, unabhängige italienische Winzer herum aufgebaut.",
             "We've rebuilt our wine list around small, independent Italian producers.",
             "أعدنا بناء قائمة النبيذ حول منتجين إيطاليين صغار ومستقلين.",
             "<p>Unsere neue Weinkarte verzichtet auf die großen Handelsmarken zugunsten kleiner, familiengeführter Weingüter in der Toskana, im Piemont und auf Sizilien. Fragen Sie Ihren Kellner nach den Glas-Empfehlungen der Woche.</p>",
             "<p>Our new wine list drops the bigger commercial labels in favour of small, family-run vineyards across Tuscany, Piedmont and Sicily. Ask your server for this week's by-the-glass recommendations.</p>",
             "<p>تتخلى قائمة النبيذ الجديدة عن العلامات التجارية الكبيرة لصالح مزارع عائلية صغيرة في توسكانا وبيدمونت وصقلية. اسأل النادل عن توصيات الأسبوع بالكأس.</p>",
             "news-wine"),
            ("Reservieren Sie Ihren Tisch für Silvester", "Reserve Your Table for New Year's Eve", "احجز طاولتك لليلة رأس السنة",
             "Unser Fünf-Gänge-Silvestermenü kann ab sofort gebucht werden.",
             "Our set five-course New Year's Eve menu is now open for booking.",
             "قائمة رأس السنة المكونة من خمسة أطباق متاحة الآن للحجز.",
             "<p>Auch dieses Silvester bieten wir wieder ein Fünf-Gänge-Menü mit optionaler Weinbegleitung an. Die Plätze sind begrenzt und der Abend ist jedes Jahr ausgebucht - frühzeitige Reservierung wird empfohlen.</p>",
             "<p>We're running a set five-course menu again this New Year's Eve, with a wine pairing option. Seats are limited and this evening sells out every year, so early booking is recommended.</p>",
             "<p>نقدم مرة أخرى قائمة من خمسة أطباق لليلة رأس السنة، مع خيار مرافقة النبيذ. الأماكن محدودة وتُحجز بالكامل كل عام، لذا يُنصح بالحجز المبكر.</p>",
             "news-nye"),
        };

        foreach (var item in items)
        {
            IContent contentItem = _contentService.Create(item.TitleDe, news, "newsItem");
            SetT(contentItem, "pageTitle", item.TitleDe, item.TitleEn, item.TitleAr);
            SetT(contentItem, "metaDescription", item.TeaserDe, item.TeaserEn, item.TeaserAr);
            SetT(contentItem, "teaser", item.TeaserDe, item.TeaserEn, item.TeaserAr);
            SetT(contentItem, "bodyText", item.BodyDe, item.BodyEn, item.BodyAr);
            contentItem.SetValue("thumbnail", BlockValueBuilder.MediaPickerJson(_media.FromThemeImage(Placeholder(item.Image))));
            Publish(contentItem);
        }

        return news;
    }

    private IContent CreateContact(IContent home)
    {
        IContent contact = _contentService.Create("Contact", home, "contentPage");
        SetT(contact, "pageTitle", "Kontakt & Reservierung", "Contact & Reservations", "اتصل بنا واحجز");
        SetT(contact, "metaDescription",
            "Finden Sie Bella Vista, nehmen Sie Kontakt auf oder reservieren Sie einen Tisch.",
            "Find Bella Vista, get in touch or request a table reservation.",
            "اعثر على بيلا فيستا، تواصل معنا أو اطلب حجز طاولة.");
        contact.SetValue("metaKeywords", "kontakt, reservierung, adresse");
        SetT(contact, "heroHeading", "Kontakt & Reservierung", "Contact & Reservations", "اتصل بنا واحجز");
        contact.SetValue("showContactForm", true);
        SetT(contact, "bodyText",
            """
            <address>
                <p>Bella Vista Ristorante<br/>Hauptstraße 12, 66111 Saarbrücken</p>
                <p>Telefon: (0681) 555 0142<br/>E-Mail: <a href="mailto:info@bellavista.example">info@bellavista.example</a></p>
                <p>Geöffnet Dienstag-Sonntag, 17:00-23:00 Uhr. Montags geschlossen.</p>
            </address>
            """,
            """
            <address>
                <p>Bella Vista Ristorante<br/>Hauptstraße 12, 66111 Saarbrücken, Germany</p>
                <p>Phone: (0681) 555 0142<br/>Email: <a href="mailto:info@bellavista.example">info@bellavista.example</a></p>
                <p>Open Tuesday-Sunday, 5pm-11pm. Closed Mondays.</p>
            </address>
            """,
            """
            <address>
                <p>بيلا فيستا ريستورانتي<br/>Hauptstraße 12، 66111 زاربروكن، ألمانيا</p>
                <p>الهاتف: (0681) 555 0142<br/>البريد الإلكتروني: <a href="mailto:info@bellavista.example">info@bellavista.example</a></p>
                <p>مفتوح من الثلاثاء إلى الأحد، 17:00-23:00. مغلق أيام الاثنين.</p>
            </address>
            """);
        Publish(contact);
        return contact;
    }

    private IContent CreateLoyalGuests(IContent home)
    {
        IContent page = _contentService.Create("Loyal Guests", home, "contentPage");
        SetT(page, "pageTitle", "Stammgäste", "Loyal Guests", "الضيوف المميزون");
        SetT(page, "metaDescription", "Sonderangebote für die Stammgäste von Bella Vista.", "Special offers for Bella Vista's loyal guests.", "عروض خاصة لضيوف بيلا فيستا المميزين.");
        SetT(page, "heroHeading", "Stammgäste - Sonderangebote", "Loyal Guests - Special Offers", "الضيوف المميزون - عروض خاصة");
        SetT(page, "bodyText",
            """
            <p>Danke, dass Sie regelmäßig bei Bella Vista zu Gast sind. Diesen Monat erhalten Stammgäste:</p>
            <ul>
                <li>Ein Glas Prosecco gratis zu jedem Hauptgericht</li>
                <li>10% Rabatt auf telefonisch bestellte Take-away-Gerichte</li>
                <li>Bevorzugten Zugang zur Silvester-Menü-Buchung, eine Woche vor der öffentlichen Freigabe</li>
            </ul>
            <p>Wir freuen uns auf Sie!</p>
            """,
            """
            <p>Thank you for being a regular at Bella Vista. This month, loyal guests get:</p>
            <ul>
                <li>A complimentary glass of prosecco with any main course</li>
                <li>10% off take-away orders placed by phone</li>
                <li>First access to our New Year's Eve set-menu booking, a week before it opens to the public</li>
            </ul>
            <p>See you soon!</p>
            """,
            """
            <p>شكراً لكونك ضيفاً دائماً لدى بيلا فيستا. هذا الشهر، يحصل الضيوف المميزون على:</p>
            <ul>
                <li>كأس بروسيكو مجاني مع أي طبق رئيسي</li>
                <li>خصم 10% على طلبات التيك أواي عبر الهاتف</li>
                <li>أولوية حجز قائمة رأس السنة، قبل أسبوع من فتحها للجمهور</li>
            </ul>
            <p>نراكم قريباً!</p>
            """);
        Publish(page);
        return page;
    }

    private IContent CreateLogin(IContent home)
    {
        IContent login = _contentService.Create("Login", home, "contentPage");
        SetT(login, "pageTitle", "Anmelden", "Log in", "تسجيل الدخول");
        SetT(login, "metaDescription", "Melden Sie sich bei Ihrem Bella Vista Stammgast-Konto an.", "Log in to your Bella Vista loyal guest account.", "سجّل الدخول إلى حساب الضيف المميز في بيلا فيستا.");
        SetT(login, "heroHeading", "Anmelden", "Log in", "تسجيل الدخول");
        login.TemplateId = _templates.LoginTemplate.Id;
        Publish(login);
        return login;
    }

    private IContent CreateAccessDenied(IContent home)
    {
        IContent page = _contentService.Create("Members Only", home, "contentPage");
        SetT(page, "pageTitle", "Nur für Mitglieder", "Members Only", "للأعضاء فقط");
        SetT(page, "metaDescription", "Diese Seite ist nur für angemeldete Stammgäste verfügbar.", "This page is only available to logged-in loyal guests.", "هذه الصفحة متاحة فقط للضيوف المميزين المسجلين.");
        SetT(page, "heroHeading", "Nur für Mitglieder", "Members Only", "للأعضاء فقط");
        SetT(page, "bodyText",
            "<p>Diese Seite ist nur für angemeldete Stammgäste verfügbar. Bitte melden Sie sich an, um fortzufahren.</p>",
            "<p>This page is only available to logged-in loyal guests. Please log in to continue.</p>",
            "<p>هذه الصفحة متاحة فقط للضيوف المميزين المسجلين. يرجى تسجيل الدخول للمتابعة.</p>");
        Publish(page);
        return page;
    }
}
