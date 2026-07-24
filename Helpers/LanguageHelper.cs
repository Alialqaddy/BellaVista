using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BellaVista.Helpers;

/// <summary>
/// Resolves the current site language (DE/EN/AR) from Umbraco's ambient VariationContext -
/// which the domain-based routing set up in ContentSeeder.SeedDomains (chapter 7, slide 72:
/// path-suffix domains /de/, /en/, /ar/) already populates per request. This keeps the four
/// Block/NestedContent fields that still use parallel "xxxEn"/"xxxAr" properties (see
/// ContentTypeSeeder.AddTranslatable) in sync with the same culture Umbraco resolves natively
/// for every other property on the page.
/// </summary>
public static class LanguageHelper
{
    public const string DefaultLangCode = "de";

    public static string ResolveLangCode(IVariationContextAccessor variationContextAccessor)
    {
        string? isoCode = variationContextAccessor.VariationContext?.Culture;
        string lang = isoCode?.Split('-')[0] ?? DefaultLangCode;
        return lang is "de" or "en" or "ar" ? lang : DefaultLangCode;
    }

    public static string ToIsoCode(string langCode) => langCode switch
    {
        "en" => "en-US",
        "ar" => "ar-SA",
        _ => "de-DE",
    };

    public static CultureInfo ToCultureInfo(string langCode) => CultureInfo.GetCultureInfo(ToIsoCode(langCode));

    public static bool IsRtl(string langCode) => langCode == "ar";

    /// <summary>Picks the DE/EN/AR value for the current language, falling back to German if the EN/AR field is empty.</summary>
    public static string Pick(string langCode, string? de, string? en, string? ar) => langCode switch
    {
        "en" => string.IsNullOrEmpty(en) ? (de ?? "") : en,
        "ar" => string.IsNullOrEmpty(ar) ? (de ?? "") : ar,
        _ => de ?? "",
    };
}
