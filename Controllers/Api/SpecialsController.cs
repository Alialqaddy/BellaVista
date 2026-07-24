using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using BellaVista.Helpers;

namespace BellaVista.Controllers.Api;

/// <summary>Mapper class - only the fields the frontend actually needs (chapter 10, "Mapper-Klassen").</summary>
public class SpecialDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SpiceLevel { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Chapter 10 "Die REST-API von Umbraco": an auto-routed Umbraco API controller
/// (/umbraco/api/specials/get) exposing today's specials (dishes flagged
/// "isTodaysSpecial" in the Menu page's Block Grid), consumed by wwwroot/js/specials.js
/// on the homepage. Supports the same startFrom paging pattern shown in the course's
/// News "load more" exercise.
/// </summary>
public class SpecialsController : UmbracoApiController
{
    private readonly IPublishedContentQuery _publishedContentQuery;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public SpecialsController(
        IPublishedContentQuery publishedContentQuery,
        IPublishedUrlProvider publishedUrlProvider,
        IVariationContextAccessor variationContextAccessor)
    {
        _publishedContentQuery = publishedContentQuery;
        _publishedUrlProvider = publishedUrlProvider;
        _variationContextAccessor = variationContextAccessor;
    }

    [HttpGet]
    public IActionResult GetSpecials(int startFrom = 0, int take = 3, string lang = LanguageHelper.DefaultLangCode)
    {
        List<SpecialDto> all = new();
        // This route isn't a culture-prefixed content URL, so there's no ambient
        // VariationContext the way a normal page request gets one from domain routing - the
        // calling page passes its already-resolved language explicitly instead (see
        // wwwroot/js/specials.js and the "data-lang" attribute in Views/Home.cshtml), and we
        // set the VariationContext from it ourselves so Menu/MenuPage (now culture-variant)
        // resolve as published and Model.Value(...) picks the right culture.
        lang = lang is "de" or "en" or "ar" ? lang : LanguageHelper.DefaultLangCode;
        _variationContextAccessor.VariationContext = new VariationContext(LanguageHelper.ToIsoCode(lang));

        // Descendants(alias) (rather than DescendantsOrSelf(alias)) - with domain-based culture
        // routing now in play, DescendantsOrSelf(alias) was mis-matching and returning Home
        // itself instead of filtering by content type, so this walks + filters explicitly.
        List<IPublishedContent> menuPages = _publishedContentQuery.ContentAtRoot()
            .SelectMany(root => root.Descendants())
            .Where(c => c.ContentType.Alias == "menuPage")
            .ToList();

        foreach (IPublishedContent menuPage in menuPages)
        {
            var dishes = menuPage.Value<BlockGridModel>("dishes");
            if (dishes == null) continue;

            foreach (BlockGridItem section in dishes)
            {
                foreach (BlockGridArea area in section.Areas)
                {
                    foreach (BlockGridItem dishItem in area)
                    {
                        IPublishedElement dish = dishItem.Content;
                        if (!dish.Value<bool>("isTodaysSpecial")) continue;

                        var image = dish.Value<MediaWithCrops>("image");
                        all.Add(new SpecialDto
                        {
                            Name = dish.Value<string>("dishName") ?? "",
                            Description = LanguageHelper.Pick(lang, dish.Value<string>("description"), dish.Value<string>("descriptionEn"), dish.Value<string>("descriptionAr")),
                            Price = dish.Value<string>("price") ?? "",
                            Category = dish.Value<string>("category") ?? "",
                            SpiceLevel = dish.Value<int>("spiceLevel"),
                            ImageUrl = image?.Content.Url(_publishedUrlProvider),
                        });
                    }
                }
            }
        }

        List<SpecialDto> page = all.Skip(startFrom).Take(take).ToList();
        return Ok(new { total = all.Count, startFrom, items = page });
    }
}
