using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace BellaVista.Helpers;

/// <summary>
/// Resolves the current site language (DE/EN/AR) from the "?lang=" query string, falling
/// back to the "bv_lang" cookie, then to German. Every view calls this the same way so the
/// language switcher (Übung 7.1/7.2: plain text links + Umbraco.GetDictionaryValue) behaves
/// consistently, whether or not that view is rendered as part of Master's Layout.
/// </summary>
public static class LanguageHelper
{
    public const string DefaultLangCode = "de";

    /// <summary>Read-only: does not set the cookie. Master.cshtml is the only place that does that.</summary>
    public static string ResolveLangCode(HttpContext context)
    {
        string? queryLang = context.Request.Query["lang"].FirstOrDefault();
        string cookieLang = context.Request.Cookies["bv_lang"] ?? DefaultLangCode;
        string lang = queryLang ?? cookieLang;
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
