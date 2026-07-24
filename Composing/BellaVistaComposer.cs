using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace BellaVista.Composing;

/// <summary>
/// Registers the seeders below as plain services and hooks the schema/content seeding
/// into Umbraco's startup pipeline (chapter 9 "Event-Handling": a Composer + a
/// notification handler, same mechanism used by <see cref="BellaVista.NotificationHandlers.DishAndNewsPublishedNotificationHandler"/>).
/// </summary>
public class BellaVistaComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.DataEditors().Add<SpiceLevelDataEditor>();

        builder.Services.AddTransient<DataTypeSeeder>();
        builder.Services.AddTransient<ContentTypeSeeder>();
        // Scoped (not transient): ContentSeeder needs the *same* TemplateSeeder instance
        // that BellaVistaStartupSeedingHandler used, so it can see the Login template it created.
        builder.Services.AddScoped<TemplateSeeder>();
        builder.Services.AddTransient<MediaSeeder>();
        builder.Services.AddTransient<MemberAndAccessSeeder>();
        builder.Services.AddTransient<LanguageAndDictionarySeeder>();
        builder.Services.AddTransient<ContentSeeder>();

        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, BellaVistaStartupSeedingHandler>();
    }
}

/// <summary>
/// Runs once when Umbraco has finished booting: creates every Document/Element/Data
/// Type, the Templates, the languages + dictionary items, the member group, and the
/// demo content. Every step first checks whether it already ran, so this is safe to
/// execute on every application start.
/// </summary>
public class BellaVistaStartupSeedingHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly DataTypeSeeder _dataTypes;
    private readonly ContentTypeSeeder _contentTypes;
    private readonly TemplateSeeder _templates;
    private readonly LanguageAndDictionarySeeder _languages;
    private readonly ContentSeeder _content;

    public BellaVistaStartupSeedingHandler(
        DataTypeSeeder dataTypes,
        ContentTypeSeeder contentTypes,
        TemplateSeeder templates,
        LanguageAndDictionarySeeder languages,
        ContentSeeder content)
    {
        _dataTypes = dataTypes;
        _contentTypes = contentTypes;
        _templates = templates;
        _languages = languages;
        _content = content;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        // 1. Data types that don't depend on element types yet.
        _dataTypes.CreateCategoryDropDown();
        _dataTypes.CreateSpiceLevelEditor();
        _dataTypes.CreateSingleImagePicker("Bella Vista - Single Image", SchemaKeys.SingleImagePicker);

        // 2. Element types used inside Nested Content / Block List / Block Grid.
        IContentType highlightable = _contentTypes.CreateHighlightable();
        var categoryDropDown = _dataTypes.CreateCategoryDropDown();
        _contentTypes.CreateSlideItem();
        _contentTypes.CreateGalleryImage(categoryDropDown);
        _contentTypes.CreateMenuSection();
        _contentTypes.CreateDish(highlightable, categoryDropDown);

        // 3. The container data types, now that the element types above exist.
        _dataTypes.CreateSliderNestedContent();
        _dataTypes.CreateGalleryBlockList();
        _dataTypes.CreateMenuBlockGrid();

        // 4. Page document types.
        IContentType seoComposition = _contentTypes.CreateSeoComposition();
        IContentType master = _contentTypes.CreateMaster();
        IContentType home = _contentTypes.CreateHome(master, seoComposition);
        IContentType contentPage = _contentTypes.CreateContentPage(master, seoComposition);
        IContentType menuPage = _contentTypes.CreateMenuPage(master, seoComposition);
        IContentType galleryPage = _contentTypes.CreateGalleryPage(master, seoComposition);
        IContentType newsPage = _contentTypes.CreateNewsPage(master, seoComposition);
        IContentType newsItem = _contentTypes.CreateNewsItem(master, seoComposition);
        _contentTypes.ConfigureAllowedChildren(home, contentPage, menuPage, galleryPage, newsPage, newsItem);

        // 5. Templates (reads the real Views/*.cshtml already on disk).
        _templates.SeedTemplatesAndAssign(master, home, contentPage, menuPage, galleryPage, newsPage, newsItem);

        // 6. Languages + dictionary items, then the demo content tree + member.
        _languages.SeedLanguages();
        _languages.SeedDictionaryItems();
        // One-time startup seeding: blocking on the async member-creation call here is fine,
        // there's no request/UI thread to deadlock since this runs during app boot.
        _content.SeedAllAsync().GetAwaiter().GetResult();
    }
}
