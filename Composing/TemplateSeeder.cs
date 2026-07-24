using Microsoft.AspNetCore.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Creates the Template row for each Document Type. The actual Razor markup already
/// lives in Views/*.cshtml (hand-written, checked into git) - we just read that file's
/// current content from disk so the template row Umbraco creates matches it exactly,
/// instead of seeding some placeholder that would only get out of sync.
/// </summary>
public class TemplateSeeder
{
    private readonly IFileService _fileService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TemplateSeeder(IFileService fileService, IContentTypeService contentTypeService, IWebHostEnvironment webHostEnvironment)
    {
        _fileService = fileService;
        _contentTypeService = contentTypeService;
        _webHostEnvironment = webHostEnvironment;
    }

    private string ReadView(string alias)
    {
        var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Views", $"{alias}.cshtml");
        return System.IO.File.Exists(path)
            ? System.IO.File.ReadAllText(path)
            : $"@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{{ Layout = \"Master.cshtml\"; }}\n";
    }

    private ITemplate GetOrCreate(string alias, string name, ITemplate? master)
    {
        ITemplate? existing = _fileService.GetTemplate(alias);
        if (existing != null) return existing;

        return _fileService.CreateTemplateWithIdentity(name, alias, ReadView(alias), master, Constants.Security.SuperUserId);
    }

    /// <summary>The "Login" node (chapter 8) reuses the Content Page document type but gets its own template.</summary>
    public ITemplate LoginTemplate { get; private set; } = null!;

    public void SeedTemplatesAndAssign(
        IContentType master, IContentType home, IContentType contentPage,
        IContentType menuPage, IContentType galleryPage, IContentType newsPage, IContentType newsItem)
    {
        ITemplate masterTemplate = GetOrCreate("Master", "Master", null);

        void Assign(IContentType contentType, ITemplate template)
        {
            contentType.AllowedTemplates = new[] { template };
            contentType.SetDefaultTemplate(template);
            _contentTypeService.Save(contentType, Constants.Security.SuperUserId);
        }

        Assign(home, GetOrCreate("Home", "Home", masterTemplate));

        ITemplate contentPageTemplate = GetOrCreate("ContentPage", "Content Page", masterTemplate);
        LoginTemplate = GetOrCreate("Login", "Login", masterTemplate);
        contentPage.AllowedTemplates = new[] { contentPageTemplate, LoginTemplate };
        contentPage.SetDefaultTemplate(contentPageTemplate);
        _contentTypeService.Save(contentPage, Constants.Security.SuperUserId);

        Assign(menuPage, GetOrCreate("MenuPage", "Menu Page", masterTemplate));
        Assign(galleryPage, GetOrCreate("GalleryPage", "Gallery Page", masterTemplate));
        Assign(newsPage, GetOrCreate("NewsPage", "News Page", masterTemplate));
        Assign(newsItem, GetOrCreate("NewsItem", "News Item", masterTemplate));
    }
}
