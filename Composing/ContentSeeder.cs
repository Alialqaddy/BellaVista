using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Seeds realistic demo content (no Lorem Ipsum) so the site is demo-able right after
/// the first `dotnet run`, without anyone having to click through the backoffice first.
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
        home.SetValue("metaDescription", "Bella Vista is a family-run Italian restaurant serving fresh, homemade pasta, wood-fired pizza and seasonal specials.");
        home.SetValue("metaKeywords", "italian restaurant, pizza, pasta, bella vista");
        home.SetValue("titleAppend", "Bella Vista Ristorante");

        string slider = BlockValueBuilder.BuildSlider(new[]
        {
            new BlockValueBuilder.SlideItemInput("Benvenuti da Bella Vista", "Authentic Italian cooking in the heart of town", _media.FromThemeImage("slider1.jpg")),
            new BlockValueBuilder.SlideItemInput("Fresh, Seasonal, Homemade", "Our pasta is rolled fresh every single morning", _media.FromThemeImage("slider2.jpg")),
            new BlockValueBuilder.SlideItemInput("A Table With a View", "Join us for dinner out on the terrace", _media.FromThemeImage("slider_pic1.jpg")),
        });
        home.SetValue("slider", slider);

        Publish(home);
        return home;
    }

    private IContent CreateAbout(IContent home)
    {
        IContent about = _contentService.Create("About Us", home, "contentPage");
        about.SetValue("pageTitle", "About Us");
        about.SetValue("metaDescription", "The story behind Bella Vista, our kitchen team and why guests keep coming back.");
        about.SetValue("metaKeywords", "about, italian restaurant, chefs");
        about.SetValue("heroHeading", "About Us");
        about.SetValue("bodyText", """
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
            """);
        Publish(about);
        return about;
    }

    private IContent CreateMenu(IContent home)
    {
        IContent menu = _contentService.Create("Menu", home, "menuPage");
        menu.SetValue("pageTitle", "Menu");
        menu.SetValue("metaDescription", "Starters, mains, desserts and drinks at Bella Vista - fresh pasta, wood-fired pizza and homemade tiramisu.");
        menu.SetValue("metaKeywords", "menu, pasta, pizza, dessert");
        menu.SetValue("heroHeading", "Our Menu");
        menu.SetValue("intro", "Everything below is cooked to order in our open kitchen. Let your server know about any allergies.");

        Guid s1 = _media.FromThemeImage("s1.jpg");
        Guid s2 = _media.FromThemeImage("s2.jpg");
        Guid s3 = _media.FromThemeImage("s3.jpg");
        Guid s4 = _media.FromThemeImage("s4.jpg");
        Guid s5 = _media.FromThemeImage("s5.jpg");
        Guid s6 = _media.FromThemeImage("s6.jpg");
        Guid s7 = _media.FromThemeImage("s7.jpg");
        Guid s8 = _media.FromThemeImage("s8.jpg");

        string dishes = BlockValueBuilder.BuildMenu(new[]
        {
            new BlockValueBuilder.MenuSectionInput("Starters", new[]
            {
                new BlockValueBuilder.DishInput("Bruschetta al Pomodoro", "Toasted sourdough, vine tomatoes, basil, garlic and olive oil.", "6,50 €", "Starter", s1, 0, false),
                new BlockValueBuilder.DishInput("Arancini", "Crisp risotto balls filled with mozzarella and slow-cooked ragù.", "7,90 €", "Starter", s2, 1, true),
                new BlockValueBuilder.DishInput("Carpaccio di Manzo", "Thin-sliced beef, rocket, parmesan shavings, lemon dressing.", "9,50 €", "Starter", s3, 0, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Mains", new[]
            {
                new BlockValueBuilder.DishInput("Tagliatelle al Tartufo", "Fresh egg tagliatelle, black truffle cream, parmesan.", "16,90 €", "Main", s4, 0, true),
                new BlockValueBuilder.DishInput("Pizza Diavola", "Wood-fired, San Marzano tomato, mozzarella, spicy salami, chilli oil.", "12,50 €", "Main", s5, 3, false),
                new BlockValueBuilder.DishInput("Osso Buco alla Milanese", "Braised veal shank, saffron risotto, gremolata.", "22,00 €", "Main", s6, 1, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Desserts", new[]
            {
                new BlockValueBuilder.DishInput("Tiramisù della Casa", "Espresso-soaked savoiardi, mascarpone cream, cocoa.", "6,90 €", "Dessert", s7, 0, true),
                new BlockValueBuilder.DishInput("Panna Cotta ai Frutti di Bosco", "Vanilla panna cotta, forest berry compote.", "6,50 €", "Dessert", s8, 0, false),
            }),
            new BlockValueBuilder.MenuSectionInput("Drinks", new[]
            {
                new BlockValueBuilder.DishInput("Chianti Classico (glass)", "Dry, medium-bodied red from Tuscany.", "6,00 €", "Drink", s1, 0, false),
                new BlockValueBuilder.DishInput("Aperol Spritz", "Aperol, prosecco, soda, orange slice.", "7,50 €", "Drink", s2, 0, false),
                new BlockValueBuilder.DishInput("Espresso", "Single shot, roasted for us by a local Saarbrücken roastery.", "2,50 €", "Drink", s3, 0, false),
            }),
        });
        menu.SetValue("dishes", dishes);

        Publish(menu);
        return menu;
    }

    private IContent CreateGallery(IContent home)
    {
        IContent gallery = _contentService.Create("Gallery", home, "galleryPage");
        gallery.SetValue("pageTitle", "Gallery");
        gallery.SetValue("metaDescription", "Photos from the Bella Vista dining room, kitchen and terrace.");
        gallery.SetValue("metaKeywords", "gallery, photos, restaurant");
        gallery.SetValue("heroHeading", "Gallery");

        string images = BlockValueBuilder.BuildGallery(new[]
        {
            new BlockValueBuilder.GalleryImageInput("Bruschetta, fresh off the pass", "Starter", _media.FromThemeImage("s1.jpg")),
            new BlockValueBuilder.GalleryImageInput("Our arancini, plated for two", "Starter", _media.FromThemeImage("s2.jpg")),
            new BlockValueBuilder.GalleryImageInput("Tagliatelle al tartufo", "Main", _media.FromThemeImage("s4.jpg")),
            new BlockValueBuilder.GalleryImageInput("Pizza going into the wood oven", "Main", _media.FromThemeImage("s5.jpg")),
            new BlockValueBuilder.GalleryImageInput("Tiramisù, made fresh every morning", "Dessert", _media.FromThemeImage("s7.jpg")),
            new BlockValueBuilder.GalleryImageInput("Panna cotta with berry compote", "Dessert", _media.FromThemeImage("s8.jpg")),
            new BlockValueBuilder.GalleryImageInput("Aperol spritz on the terrace", "Drink", _media.FromThemeImage("s3.jpg")),
            new BlockValueBuilder.GalleryImageInput("Our wine list", "Drink", _media.FromThemeImage("s6.jpg")),
        });
        gallery.SetValue("images", images);

        Publish(gallery);
        return gallery;
    }

    private IContent CreateNews(IContent home)
    {
        IContent news = _contentService.Create("News", home, "newsPage");
        news.SetValue("pageTitle", "News & Events");
        news.SetValue("metaDescription", "The latest news, events and promotions from Bella Vista.");
        news.SetValue("metaKeywords", "news, events, promotions");
        news.SetValue("heroHeading", "News & Events");
        news.SetValue("intro", "What's happening in and around the restaurant.");
        Publish(news);

        (string Title, string Teaser, string Body, string Image)[] items =
        {
            ("Winter Truffle Menu Is Here", "Fresh black truffle, flown in weekly, on four dishes until the end of February.",
             "<p>Every winter we bring in fresh black truffle for a short, four-dish menu - this year it lands on the tagliatelle, the risotto, a truffle arancino and a truffle-butter steak. Available while stock lasts, usually a table's best bet is to ask when you book.</p>", "blog-pic1.jpg"),
            ("Bella Vista Turns 10", "Ten years since the Moretti family opened the doors - thank you to everyone who has eaten with us.",
             "<p>Ten years ago this month, the Moretti family opened a six-table trattoria a few streets from here. We've grown since, but the kitchen is still run on the same recipes. Thank you to every guest who has come back again and again - you're the reason we're still here.</p>", "blog-pic2.jpg"),
            ("Live Music on the Terrace", "Every Friday evening this summer, local musicians join us for dinner service.",
             "<p>Starting this Friday, we're hosting local acoustic musicians out on the terrace every Friday evening through the summer. No cover charge, just good food and live music - reservations recommended as terrace tables go quickly.</p>", "blog_pic1.jpg"),
            ("New Wine List, Small Producers Only", "We've rebuilt our wine list around small, independent Italian producers.",
             "<p>Our new wine list drops the bigger commercial labels in favour of small, family-run vineyards across Tuscany, Piedmont and Sicily. Ask your server for this week's by-the-glass recommendations.</p>", "blog_pic2.jpg"),
            ("Reserve Your Table for New Year's Eve", "Our set five-course New Year's Eve menu is now open for booking.",
             "<p>We're running a set five-course menu again this New Year's Eve, with a wine pairing option. Seats are limited and this evening sells out every year, so early booking is recommended.</p>", "det_pic.jpg"),
        };

        foreach ((string title, string teaser, string body, string image) in items)
        {
            IContent item = _contentService.Create(title, news, "newsItem");
            item.SetValue("pageTitle", title);
            item.SetValue("metaDescription", teaser);
            item.SetValue("teaser", teaser);
            item.SetValue("bodyText", body);
            item.SetValue("thumbnail", BlockValueBuilder.MediaPickerJson(_media.FromThemeImage(image)));
            Publish(item);
        }

        return news;
    }

    private IContent CreateContact(IContent home)
    {
        IContent contact = _contentService.Create("Contact", home, "contentPage");
        contact.SetValue("pageTitle", "Contact & Reservations");
        contact.SetValue("metaDescription", "Find Bella Vista, get in touch or request a table reservation.");
        contact.SetValue("metaKeywords", "contact, reservation, address");
        contact.SetValue("heroHeading", "Contact & Reservations");
        contact.SetValue("showContactForm", true);
        contact.SetValue("bodyText", """
            <address>
                <p>Bella Vista Ristorante<br/>Hauptstraße 12, 66111 Saarbrücken</p>
                <p>Phone: (0681) 555 0142<br/>Email: <a href="mailto:info@bellavista.example">info@bellavista.example</a></p>
                <p>Open Tuesday-Sunday, 17:00-23:00. Closed Mondays.</p>
            </address>
            """);
        Publish(contact);
        return contact;
    }

    private IContent CreateLoyalGuests(IContent home)
    {
        IContent page = _contentService.Create("Loyal Guests", home, "contentPage");
        page.SetValue("pageTitle", "Loyal Guests");
        page.SetValue("metaDescription", "Special offers for Bella Vista's loyal guests.");
        page.SetValue("heroHeading", "Loyal Guests - Special Offers");
        page.SetValue("bodyText", """
            <p>Thank you for being a regular at Bella Vista. This month, loyal guests get:</p>
            <ul>
                <li>A complimentary glass of prosecco with any main course</li>
                <li>10% off take-away orders placed by phone</li>
                <li>First access to our New Year's Eve set-menu booking, a week before it opens to the public</li>
            </ul>
            <p>See you soon!</p>
            """);
        Publish(page);
        return page;
    }

    private IContent CreateLogin(IContent home)
    {
        IContent login = _contentService.Create("Login", home, "contentPage");
        login.SetValue("pageTitle", "Log in");
        login.SetValue("metaDescription", "Log in to your Bella Vista loyal guest account.");
        login.SetValue("heroHeading", "Log in");
        login.TemplateId = _templates.LoginTemplate.Id;
        Publish(login);
        return login;
    }

    private IContent CreateAccessDenied(IContent home)
    {
        IContent page = _contentService.Create("Members Only", home, "contentPage");
        page.SetValue("pageTitle", "Members Only");
        page.SetValue("metaDescription", "This page is only available to logged-in loyal guests.");
        page.SetValue("heroHeading", "Members Only");
        page.SetValue("bodyText", "<p>This page is only available to logged-in loyal guests. Please log in to continue.</p>");
        Publish(page);
        return page;
    }
}
