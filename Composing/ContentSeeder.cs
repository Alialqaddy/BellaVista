using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Seeds realistic demo content (no Lorem Ipsum) so the site is demo-able right after
/// the first `dotnet run`, without anyone having to click through the backoffice first.
/// German is the site's default/registered language (chapter 7), so all seeded content is
/// written in German - dish names are kept in Italian, as they would be on a real Italian
/// menu. Content is not tripled into English/Arabic; see README for that scope decision.
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

    private static string Placeholder(string name) => $"placeholders/{name}.svg";

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
        home.SetValue("pageTitle", "Bella Vista - Ristorante Italiano");
        home.SetValue("metaDescription", "Bella Vista ist ein familiengeführtes italienisches Restaurant mit frischer, hausgemachter Pasta, Pizza aus dem Holzofen und saisonalen Spezialitäten.");
        home.SetValue("metaKeywords", "italienisches restaurant, pizza, pasta, bella vista");
        home.SetValue("titleAppend", "Bella Vista Ristorante");

        string slider = BlockValueBuilder.BuildSlider(new[]
        {
            new BlockValueBuilder.SlideItemInput("Benvenuti da Bella Vista", "Authentische italienische Küche mitten in der Stadt", _media.FromThemeImage(Placeholder("hero-welcome"))),
            new BlockValueBuilder.SlideItemInput("Frisch, saisonal, hausgemacht", "Unsere Pasta wird jeden Morgen frisch gerollt", _media.FromThemeImage(Placeholder("hero-pasta"))),
            new BlockValueBuilder.SlideItemInput("Ein Tisch mit Aussicht", "Genießen Sie Ihr Essen auf unserer Terrasse", _media.FromThemeImage(Placeholder("hero-terrace"))),
        });
        home.SetValue("slider", slider);

        Publish(home);
        return home;
    }

    private IContent CreateAbout(IContent home)
    {
        IContent about = _contentService.Create("About Us", home, "contentPage");
        about.SetValue("pageTitle", "Über uns");
        about.SetValue("metaDescription", "Die Geschichte hinter Bella Vista, unser Küchenteam und warum unsere Gäste immer wiederkommen.");
        about.SetValue("metaKeywords", "über uns, italienisches restaurant, köche");
        about.SetValue("heroHeading", "Über uns");
        about.SetValue("bodyText", """
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
            """);
        Publish(about);
        return about;
    }

    private IContent CreateMenu(IContent home)
    {
        IContent menu = _contentService.Create("Menu", home, "menuPage");
        menu.SetValue("pageTitle", "Speisekarte");
        menu.SetValue("metaDescription", "Vorspeisen, Hauptgerichte, Desserts und Getränke bei Bella Vista - frische Pasta, Pizza aus dem Holzofen und hausgemachtes Tiramisù.");
        menu.SetValue("metaKeywords", "speisekarte, pasta, pizza, dessert");
        menu.SetValue("heroHeading", "Unsere Speisekarte");
        menu.SetValue("intro", "Alles unten wird auf Bestellung in unserer offenen Küche zubereitet. Bitte informieren Sie Ihren Kellner über Allergien.");

        Guid starterImg = _media.FromThemeImage(Placeholder("food-starter"));
        Guid mainImg = _media.FromThemeImage(Placeholder("food-main"));
        Guid dessertImg = _media.FromThemeImage(Placeholder("food-dessert"));
        Guid drinkImg = _media.FromThemeImage(Placeholder("food-drink"));

        string dishes = BlockValueBuilder.BuildMenu(new[]
        {
            new BlockValueBuilder.MenuSectionInput("Vorspeisen", new[]
            {
                new BlockValueBuilder.DishInput("Bruschetta al Pomodoro", "Getoastetes Sauerteigbrot, Rispentomaten, Basilikum, Knoblauch und Olivenöl.", "6,50 €", "Starter", starterImg, 0, false),
                new BlockValueBuilder.DishInput("Arancini", "Knusprige Risottobällchen, gefüllt mit Mozzarella und langsam geschmortem Ragù.", "7,90 €", "Starter", starterImg, 1, true),
                new BlockValueBuilder.DishInput("Carpaccio di Manzo", "Hauchdünnes Rinderfilet, Rucola, Parmesanspäne, Zitronendressing.", "9,50 €", "Starter", starterImg, 0, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Hauptgerichte", new[]
            {
                new BlockValueBuilder.DishInput("Tagliatelle al Tartufo", "Frische Eier-Tagliatelle, schwarze Trüffelcreme, Parmesan.", "16,90 €", "Main", mainImg, 0, true),
                new BlockValueBuilder.DishInput("Pizza Diavola", "Aus dem Holzofen, San-Marzano-Tomaten, Mozzarella, scharfe Salami, Chiliöl.", "12,50 €", "Main", mainImg, 3, false),
                new BlockValueBuilder.DishInput("Osso Buco alla Milanese", "Geschmorte Kalbshaxe, Safranrisotto, Gremolata.", "22,00 €", "Main", mainImg, 1, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Desserts", new[]
            {
                new BlockValueBuilder.DishInput("Tiramisù della Casa", "Espresso-getränkte Savoiardi, Mascarponecreme, Kakao.", "6,90 €", "Dessert", dessertImg, 0, true),
                new BlockValueBuilder.DishInput("Panna Cotta ai Frutti di Bosco", "Vanille-Panna-Cotta, Waldbeerkompott.", "6,50 €", "Dessert", dessertImg, 0, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Getränke", new[]
            {
                new BlockValueBuilder.DishInput("Chianti Classico (Glas)", "Trockener, mittelkräftiger Rotwein aus der Toskana.", "6,00 €", "Drink", drinkImg, 0, false),
                new BlockValueBuilder.DishInput("Aperol Spritz", "Aperol, Prosecco, Soda, Orangenscheibe.", "7,50 €", "Drink", drinkImg, 0, false),
                new BlockValueBuilder.DishInput("Espresso", "Ein Shot, geröstet für uns von einer Saarbrücker Rösterei.", "2,50 €", "Drink", drinkImg, 0, false),
            }),
        });
        menu.SetValue("dishes", dishes);

        Publish(menu);
        return menu;
    }

    private IContent CreateGallery(IContent home)
    {
        IContent gallery = _contentService.Create("Gallery", home, "galleryPage");
        gallery.SetValue("pageTitle", "Galerie");
        gallery.SetValue("metaDescription", "Fotos aus dem Gastraum, der Küche und der Terrasse von Bella Vista.");
        gallery.SetValue("metaKeywords", "galerie, fotos, restaurant");
        gallery.SetValue("heroHeading", "Galerie");

        Guid starterImg = _media.FromThemeImage(Placeholder("food-starter"));
        Guid mainImg = _media.FromThemeImage(Placeholder("food-main"));
        Guid dessertImg = _media.FromThemeImage(Placeholder("food-dessert"));
        Guid drinkImg = _media.FromThemeImage(Placeholder("food-drink"));

        string images = BlockValueBuilder.BuildGallery(new[]
        {
            new BlockValueBuilder.GalleryImageInput("Bruschetta, frisch aus der Küche", "Starter", starterImg),
            new BlockValueBuilder.GalleryImageInput("Unsere Arancini, für zwei angerichtet", "Starter", starterImg),
            new BlockValueBuilder.GalleryImageInput("Tagliatelle al Tartufo", "Main", mainImg),
            new BlockValueBuilder.GalleryImageInput("Pizza auf dem Weg in den Holzofen", "Main", mainImg),
            new BlockValueBuilder.GalleryImageInput("Tiramisù, jeden Morgen frisch gemacht", "Dessert", dessertImg),
            new BlockValueBuilder.GalleryImageInput("Panna Cotta mit Beerenkompott", "Dessert", dessertImg),
            new BlockValueBuilder.GalleryImageInput("Aperol Spritz auf der Terrasse", "Drink", drinkImg),
            new BlockValueBuilder.GalleryImageInput("Unsere Weinkarte", "Drink", drinkImg),
        });
        gallery.SetValue("images", images);

        Publish(gallery);
        return gallery;
    }

    private IContent CreateNews(IContent home)
    {
        IContent news = _contentService.Create("News", home, "newsPage");
        news.SetValue("pageTitle", "Neuigkeiten & Veranstaltungen");
        news.SetValue("metaDescription", "Die neuesten Nachrichten, Veranstaltungen und Aktionen von Bella Vista.");
        news.SetValue("metaKeywords", "neuigkeiten, veranstaltungen, aktionen");
        news.SetValue("heroHeading", "Neuigkeiten & Veranstaltungen");
        news.SetValue("intro", "Was sich im Restaurant und drumherum tut.");
        Publish(news);

        (string Title, string Teaser, string Body, string Image)[] items =
        {
            ("Winter-Trüffelkarte ist da", "Frische schwarze Trüffel, wöchentlich eingeflogen, auf vier Gerichten bis Ende Februar.",
             "<p>Jeden Winter bringen wir frische schwarze Trüffel für eine kurze Vier-Gänge-Karte - dieses Jahr auf der Tagliatelle, dem Risotto, einem Trüffel-Arancino und einem Steak mit Trüffelbutter. Solange der Vorrat reicht - am besten bei der Reservierung nachfragen.</p>", "news-truffle"),
            ("Bella Vista wird 10", "Zehn Jahre, seit die Familie Moretti die Türen geöffnet hat - danke an alle, die bei uns gegessen haben.",
             "<p>Vor zehn Jahren hat die Familie Moretti eine Trattoria mit sechs Tischen ein paar Straßen von hier eröffnet. Seitdem sind wir gewachsen, aber die Küche arbeitet noch immer nach denselben Rezepten. Danke an jeden Gast, der immer wieder zu uns zurückgekommen ist - Sie sind der Grund, warum es uns noch gibt.</p>", "news-anniversary"),
            ("Live-Musik auf der Terrasse", "Jeden Freitagabend diesen Sommer spielen lokale Musiker bei uns zum Abendessen.",
             "<p>Ab diesem Freitag laden wir jeden Freitagabend über den Sommer lokale Akustik-Musiker auf unsere Terrasse ein. Kein Eintritt, nur gutes Essen und Live-Musik - Reservierung empfohlen, da die Terrassentische schnell weg sind.</p>", "news-music"),
            ("Neue Weinkarte, nur kleine Winzer", "Wir haben unsere Weinkarte komplett um kleine, unabhängige italienische Winzer herum aufgebaut.",
             "<p>Unsere neue Weinkarte verzichtet auf die großen Handelsmarken zugunsten kleiner, familiengeführter Weingüter in der Toskana, im Piemont und auf Sizilien. Fragen Sie Ihren Kellner nach den Glas-Empfehlungen der Woche.</p>", "news-wine"),
            ("Reservieren Sie Ihren Tisch für Silvester", "Unser Fünf-Gänge-Silvestermenü kann ab sofort gebucht werden.",
             "<p>Auch dieses Silvester bieten wir wieder ein Fünf-Gänge-Menü mit optionaler Weinbegleitung an. Die Plätze sind begrenzt und der Abend ist jedes Jahr ausgebucht - frühzeitige Reservierung wird empfohlen.</p>", "news-nye"),
        };

        foreach ((string title, string teaser, string body, string image) in items)
        {
            IContent item = _contentService.Create(title, news, "newsItem");
            item.SetValue("pageTitle", title);
            item.SetValue("metaDescription", teaser);
            item.SetValue("teaser", teaser);
            item.SetValue("bodyText", body);
            item.SetValue("thumbnail", BlockValueBuilder.MediaPickerJson(_media.FromThemeImage(Placeholder(image))));
            Publish(item);
        }

        return news;
    }

    private IContent CreateContact(IContent home)
    {
        IContent contact = _contentService.Create("Contact", home, "contentPage");
        contact.SetValue("pageTitle", "Kontakt & Reservierung");
        contact.SetValue("metaDescription", "Finden Sie Bella Vista, nehmen Sie Kontakt auf oder reservieren Sie einen Tisch.");
        contact.SetValue("metaKeywords", "kontakt, reservierung, adresse");
        contact.SetValue("heroHeading", "Kontakt & Reservierung");
        contact.SetValue("showContactForm", true);
        contact.SetValue("bodyText", """
            <address>
                <p>Bella Vista Ristorante<br/>Hauptstraße 12, 66111 Saarbrücken</p>
                <p>Telefon: (0681) 555 0142<br/>E-Mail: <a href="mailto:info@bellavista.example">info@bellavista.example</a></p>
                <p>Geöffnet Dienstag-Sonntag, 17:00-23:00 Uhr. Montags geschlossen.</p>
            </address>
            """);
        Publish(contact);
        return contact;
    }

    private IContent CreateLoyalGuests(IContent home)
    {
        IContent page = _contentService.Create("Loyal Guests", home, "contentPage");
        page.SetValue("pageTitle", "Stammgäste");
        page.SetValue("metaDescription", "Sonderangebote für die Stammgäste von Bella Vista.");
        page.SetValue("heroHeading", "Stammgäste - Sonderangebote");
        page.SetValue("bodyText", """
            <p>Danke, dass Sie regelmäßig bei Bella Vista zu Gast sind. Diesen Monat erhalten Stammgäste:</p>
            <ul>
                <li>Ein Glas Prosecco gratis zu jedem Hauptgericht</li>
                <li>10% Rabatt auf telefonisch bestellte Take-away-Gerichte</li>
                <li>Bevorzugten Zugang zur Silvester-Menü-Buchung, eine Woche vor der öffentlichen Freigabe</li>
            </ul>
            <p>Wir freuen uns auf Sie!</p>
            """);
        Publish(page);
        return page;
    }

    private IContent CreateLogin(IContent home)
    {
        IContent login = _contentService.Create("Login", home, "contentPage");
        login.SetValue("pageTitle", "Anmelden");
        login.SetValue("metaDescription", "Melden Sie sich bei Ihrem Bella Vista Stammgast-Konto an.");
        login.SetValue("heroHeading", "Anmelden");
        login.TemplateId = _templates.LoginTemplate.Id;
        Publish(login);
        return login;
    }

    private IContent CreateAccessDenied(IContent home)
    {
        IContent page = _contentService.Create("Members Only", home, "contentPage");
        page.SetValue("pageTitle", "Nur für Mitglieder");
        page.SetValue("metaDescription", "Diese Seite ist nur für angemeldete Stammgäste verfügbar.");
        page.SetValue("heroHeading", "Nur für Mitglieder");
        page.SetValue("bodyText", "<p>Diese Seite ist nur für angemeldete Stammgäste verfügbar. Bitte melden Sie sich an, um fortzufahren.</p>");
        Publish(page);
        return page;
    }
}
