using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace BellaVista.Composing;

/// <summary>
/// Creates every Document Type and Element Type for Bella Vista in code, since we
/// don't have a backoffice UI available to click these together by hand. This mirrors
/// exactly what "Settings -> Document Types -> new Document Type" does, just in C#.
/// </summary>
public class ContentTypeSeeder
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IDataTypeService _dataTypeService;

    public ContentTypeSeeder(IContentTypeService contentTypeService, IShortStringHelper shortStringHelper, IDataTypeService dataTypeService)
    {
        _contentTypeService = contentTypeService;
        _shortStringHelper = shortStringHelper;
        _dataTypeService = dataTypeService;
    }

    private PropertyType Prop(string alias, string name, IDataType dataType) =>
        new(_shortStringHelper, dataType, alias) { Name = name };

    /// <summary>
    /// Adds a translatable field as three parallel properties (base = German, +En, +Ar),
    /// picked at render time via LanguageHelper.Pick - see README for why this is used
    /// instead of Umbraco's native culture variance (it would require per-culture domains
    /// and would change every page's URL segment, which the rest of the site depends on).
    /// </summary>
    private void AddTranslatable(IContentType ct, string alias, string name, IDataType dataType, string group)
    {
        ct.AddPropertyType(Prop(alias, name, dataType), group, group);
        ct.AddPropertyType(Prop(alias + "En", name + " (English)", dataType), group, group);
        ct.AddPropertyType(Prop(alias + "Ar", name + " (Arabic)", dataType), group, group);
    }

    private IContentType NewType(Guid key, string alias, string name, string icon, bool isElement)
    {
        var ct = new ContentType(_shortStringHelper, -1)
        {
            Key = key,
            Alias = alias,
            Name = name,
            Icon = icon,
            IsElement = isElement,
            AllowedAsRoot = false,
        };
        return ct;
    }

    private void Save(IContentType ct) => _contentTypeService.Save(ct, Constants.Security.SuperUserId);

    /// <summary>
    /// Element type composed into every page type, holding the three SEO fields
    /// (pageTitle / metaDescription / metaKeywords) asked for in the brief.
    /// </summary>
    public IContentType CreateSeoComposition()
    {
        IContentType? existing = _contentTypeService.Get("seoComposition");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var textarea = _dataTypeService.GetDataType("Textarea")!;

        var ct = NewType(SchemaKeys.SeoComposition, "seoComposition", "SEO Composition", "icon-search", isElement: false);
        AddTranslatable(ct, "pageTitle", "Page title", textstring, "SEO");
        AddTranslatable(ct, "metaDescription", "Meta description", textarea, "SEO");
        ct.AddPropertyType(Prop("metaKeywords", "Meta keywords", textstring), "SEO", "SEO");
        Save(ct);
        return ct;
    }

    /// <summary>
    /// The shared "Master" document type. It carries no properties of its own -
    /// the shared header/nav/footer markup lives in Views/Master.cshtml, which every
    /// page type below uses as its Layout. It exists mainly so the Master/Home/ContentPage
    /// hierarchy from the brief is visible in the backoffice document type tree too.
    /// </summary>
    public IContentType CreateMaster()
    {
        IContentType? existing = _contentTypeService.Get("master");
        if (existing != null) return existing;

        var ct = NewType(SchemaKeys.Master, "master", "Master", "icon-layout", isElement: false);
        Save(ct);
        return ct;
    }

    public IContentType CreateHome(IContentType master, IContentType seoComposition)
    {
        IContentType? existing = _contentTypeService.Get("home");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var slider = _dataTypeService.GetDataType("Bella Vista - Slider (Nested Content)")!;

        var ct = NewType(SchemaKeys.Home, "home", "Home", "icon-home", isElement: false);
        ct.AllowedAsRoot = true;
        ct.AddContentType(master);
        ct.AddContentType(seoComposition);
        AddTranslatable(ct, "titleAppend", "Title append (site name shown after every page title)", textstring, "Content");
        ct.AddPropertyType(Prop("slider", "Homepage slider", slider), "Content", "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateContentPage(IContentType master, IContentType seoComposition)
    {
        IContentType? existing = _contentTypeService.Get("contentPage");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var richtext = _dataTypeService.GetDataType("Richtext editor")!;
        var boolean = _dataTypeService.GetDataType("True/false")!;

        var ct = NewType(SchemaKeys.ContentPage, "contentPage", "Content Page", "icon-document", isElement: false);
        ct.AddContentType(master);
        ct.AddContentType(seoComposition);
        AddTranslatable(ct, "heroHeading", "Heading shown in the page banner", textstring, "Content");
        AddTranslatable(ct, "bodyText", "Body text", richtext, "Content");
        ct.AddPropertyType(Prop("showContactForm", "Show the reservation form + map (Contact page only)", boolean), "Content", "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateMenuPage(IContentType master, IContentType seoComposition)
    {
        IContentType? existing = _contentTypeService.Get("menuPage");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var textarea = _dataTypeService.GetDataType("Textarea")!;
        var blockGrid = _dataTypeService.GetDataType("Bella Vista - Menu (Block Grid)")!;

        var ct = NewType(SchemaKeys.MenuPage, "menuPage", "Menu Page", "icon-fork-knife", isElement: false);
        ct.AddContentType(master);
        ct.AddContentType(seoComposition);
        AddTranslatable(ct, "heroHeading", "Heading shown in the page banner", textstring, "Content");
        AddTranslatable(ct, "intro", "Short intro text", textarea, "Content");
        ct.AddPropertyType(Prop("dishes", "Menu (sections + dishes)", blockGrid), "Content", "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateGalleryPage(IContentType master, IContentType seoComposition)
    {
        IContentType? existing = _contentTypeService.Get("galleryPage");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var blockList = _dataTypeService.GetDataType("Bella Vista - Gallery (Block List)")!;

        var ct = NewType(SchemaKeys.GalleryPage, "galleryPage", "Gallery Page", "icon-picture", isElement: false);
        ct.AddContentType(master);
        ct.AddContentType(seoComposition);
        AddTranslatable(ct, "heroHeading", "Heading shown in the page banner", textstring, "Content");
        ct.AddPropertyType(Prop("images", "Gallery photos", blockList), "Content", "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateNewsPage(IContentType master, IContentType seoComposition)
    {
        IContentType? existing = _contentTypeService.Get("newsPage");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var textarea = _dataTypeService.GetDataType("Textarea")!;

        var ct = NewType(SchemaKeys.NewsPage, "newsPage", "News Page", "icon-newspaper", isElement: false);
        ct.AddContentType(master);
        ct.AddContentType(seoComposition);
        AddTranslatable(ct, "heroHeading", "Heading shown in the page banner", textstring, "Content");
        AddTranslatable(ct, "intro", "Short intro text", textarea, "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateNewsItem(IContentType master, IContentType seoComposition)
    {
        IContentType? existing = _contentTypeService.Get("newsItem");
        if (existing != null) return existing;

        var textarea = _dataTypeService.GetDataType("Textarea")!;
        var richtext = _dataTypeService.GetDataType("Richtext editor")!;
        var image = _dataTypeService.GetDataType("Bella Vista - Single Image")!;

        var ct = NewType(SchemaKeys.NewsItem, "newsItem", "News Item", "icon-news", isElement: false);
        ct.AddContentType(master);
        ct.AddContentType(seoComposition);
        AddTranslatable(ct, "teaser", "Teaser (shown in the news list)", textarea, "Content");
        AddTranslatable(ct, "bodyText", "Body text", richtext, "Content");
        ct.AddPropertyType(Prop("thumbnail", "Thumbnail image", image), "Content", "Content");
        Save(ct);
        return ct;
    }

    // -- Element types used inside NestedContent / Block List / Block Grid properties --

    public IContentType CreateSlideItem()
    {
        IContentType? existing = _contentTypeService.Get("slideItem");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var image = _dataTypeService.GetDataType("Bella Vista - Single Image")!;

        var ct = NewType(SchemaKeys.SlideItem, "slideItem", "Slide Item", "icon-picture", isElement: true);
        AddTranslatable(ct, "title", "Title", textstring, "Content");
        AddTranslatable(ct, "subTitle", "Subtitle", textstring, "Content");
        ct.AddPropertyType(Prop("bgImage", "Background image", image), "Content", "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateGalleryImage(IDataType categoryDropDown)
    {
        IContentType? existing = _contentTypeService.Get("galleryImage");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var image = _dataTypeService.GetDataType("Bella Vista - Single Image")!;

        var ct = NewType(SchemaKeys.GalleryImage, "galleryImage", "Gallery Image", "icon-picture", isElement: true);
        AddTranslatable(ct, "caption", "Caption", textstring, "Content");
        ct.AddPropertyType(Prop("category", "Category", categoryDropDown), "Content", "Content");
        ct.AddPropertyType(Prop("image", "Image", image), "Content", "Content");
        Save(ct);
        return ct;
    }

    /// <summary>
    /// Small composition-only element type (chapter 5.4 "Composition von Doc-/Elementtypes"):
    /// a single "is today's special" flag, composed into the Dish element type below.
    /// </summary>
    public IContentType CreateHighlightable()
    {
        IContentType? existing = _contentTypeService.Get("highlightable");
        if (existing != null) return existing;

        var boolean = _dataTypeService.GetDataType("True/false")!;
        var ct = NewType(SchemaKeys.Highlightable, "highlightable", "Highlightable", "icon-star", isElement: true);
        ct.AddPropertyType(Prop("isTodaysSpecial", "Feature as today's special", boolean), "Content", "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateMenuSection()
    {
        IContentType? existing = _contentTypeService.Get("menuSection");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var ct = NewType(SchemaKeys.MenuSection, "menuSection", "Menu Section", "icon-list", isElement: true);
        AddTranslatable(ct, "sectionTitle", "Section title (e.g. Starters)", textstring, "Content");
        Save(ct);
        return ct;
    }

    public IContentType CreateDish(IContentType highlightable, IDataType categoryDropDown)
    {
        IContentType? existing = _contentTypeService.Get("dish");
        if (existing != null) return existing;

        var textstring = _dataTypeService.GetDataType("Textstring")!;
        var textarea = _dataTypeService.GetDataType("Textarea")!;
        var image = _dataTypeService.GetDataType("Bella Vista - Single Image")!;
        var spiceLevel = _dataTypeService.GetDataType("Bella Vista - Spice Level")!;

        var ct = NewType(SchemaKeys.Dish, "dish", "Dish", "icon-restaurant", isElement: true);
        ct.AddContentType(highlightable);
        ct.AddPropertyType(Prop("dishName", "Dish name", textstring), "Content", "Content");
        AddTranslatable(ct, "description", "Description", textarea, "Content");
        ct.AddPropertyType(Prop("price", "Price (e.g. 12,50 €)", textstring), "Content", "Content");
        ct.AddPropertyType(Prop("category", "Category", categoryDropDown), "Content", "Content");
        ct.AddPropertyType(Prop("image", "Image", image), "Content", "Content");
        ct.AddPropertyType(Prop("spiceLevel", "Spice level", spiceLevel), "Content", "Content");
        Save(ct);
        return ct;
    }

    /// <summary>Wires up which document type is allowed to be created under which (the content tree shape).</summary>
    public void ConfigureAllowedChildren(IContentType home, IContentType contentPage, IContentType menuPage, IContentType galleryPage, IContentType newsPage, IContentType newsItem)
    {
        static ContentTypeSort SortOf(IContentType ct, int order) => new(new Lazy<int>(() => ct.Id), order, ct.Alias);

        home.AllowedContentTypes = new[]
        {
            SortOf(contentPage, 0),
            SortOf(menuPage, 1),
            SortOf(galleryPage, 2),
            SortOf(newsPage, 3),
        };
        Save(home);

        contentPage.AllowedContentTypes = new[]
        {
            SortOf(contentPage, 0),
        };
        Save(contentPage);

        newsPage.AllowedContentTypes = new[]
        {
            SortOf(newsItem, 0),
        };
        Save(newsPage);
    }
}
