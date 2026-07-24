using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Registers German/English/Arabic as Umbraco languages (chapter 7 "Mehrsprachigkeit") and
/// seeds Dictionary Items for the UI chrome (nav, footer, buttons) used by the language
/// switcher in Views/Master.cshtml. Content itself (dishes, news, page text) is kept
/// invariant/single-language - see README for why that scope was chosen.
/// </summary>
public class LanguageAndDictionarySeeder
{
    private readonly ILocalizationService _localizationService;

    public const string German = "de-DE";
    public const string English = "en-US";
    public const string Arabic = "ar-SA";

    public LanguageAndDictionarySeeder(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public void SeedLanguages()
    {
        EnsureLanguage(German, "German (Germany)", isDefault: true);
        EnsureLanguage(English, "English (United States)", isDefault: false);
        EnsureLanguage(Arabic, "Arabic (Saudi Arabia)", isDefault: false);
    }

    private void EnsureLanguage(string isoCode, string cultureName, bool isDefault)
    {
        if (_localizationService.GetLanguageByIsoCode(isoCode) != null) return;

        var language = new Language(isoCode, cultureName) { IsDefault = isDefault };
        _localizationService.Save(language, Constants.Security.SuperUserId);
    }

    private static readonly (string Key, string De, string En, string Ar)[] Entries =
    {
        ("Nav.Home", "Startseite", "Home", "الرئيسية"),
        ("Nav.About", "Über uns", "About Us", "من نحن"),
        ("Nav.Menu", "Speisekarte", "Menu", "قائمة الطعام"),
        ("Nav.Gallery", "Galerie", "Gallery", "معرض الصور"),
        ("Nav.News", "Neuigkeiten", "News", "الأخبار"),
        ("Nav.Contact", "Kontakt", "Contact", "اتصل بنا"),
        ("Nav.LoyalGuests", "Stammgäste", "Loyal Guests", "الضيوف المميزون"),
        ("Btn.ViewMore", "Mehr erfahren", "View more", "اقرأ المزيد"),
        ("Btn.ReadMore", "Weiterlesen", "Read more", "اقرأ المزيد"),
        ("Btn.Submit", "Absenden", "Submit", "إرسال"),
        ("Btn.LoadMore", "Weitere anzeigen", "Load more", "عرض المزيد"),
        ("Footer.Rights", "Alle Rechte vorbehalten", "All rights reserved", "جميع الحقوق محفوظة"),
        ("Login.Title", "Anmelden", "Log in", "تسجيل الدخول"),
        ("Login.Username", "Benutzername", "Username", "اسم المستخدم"),
        ("Login.Password", "Passwort", "Password", "كلمة المرور"),
        ("Login.Action", "Anmelden", "Log in", "دخول"),
        ("Login.Error", "Anmeldung fehlgeschlagen. Bitte erneut versuchen.", "Login failed. Please try again.", "فشل تسجيل الدخول. حاول مرة أخرى."),
        ("Logout.Action", "Abmelden", "Log out", "تسجيل الخروج"),
        ("Language.Label", "Sprache", "Language", "اللغة"),
    };

    public void SeedDictionaryItems()
    {
        foreach ((string key, string de, string en, string ar) in Entries)
        {
            IDictionaryItem? item = _localizationService.GetDictionaryItemByKey(key);
            if (item == null)
            {
                item = new DictionaryItem(key);
                _localizationService.Save(item, Constants.Security.SuperUserId);
            }

            SetTranslation(item, German, de);
            SetTranslation(item, English, en);
            SetTranslation(item, Arabic, ar);
        }
    }

    private void SetTranslation(IDictionaryItem item, string isoCode, string value)
    {
        ILanguage? language = _localizationService.GetLanguageByIsoCode(isoCode);
        if (language == null) return;

        bool alreadyTranslated = item.Translations.Any(t => t.LanguageIsoCode == isoCode && t.Value == value);
        if (alreadyTranslated) return;

        _localizationService.AddOrUpdateDictionaryValue(item, language, value);
        _localizationService.Save(item, Constants.Security.SuperUserId);
    }
}
