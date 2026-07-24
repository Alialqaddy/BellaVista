# Bella Vista

Bella Vista is a fictional, family-run Italian restaurant. This repository is an Umbraco CMS
website built for it: a homepage with a dish slider, a menu built from editable dish blocks,
a filterable photo gallery, a news/events section, table reservations, and a "Loyal Guests"
area with special offers behind a member login.

## Course context

- Module: **Konzepte und Werkzeuge der Softwaretechnik (.NET)**
- University: HTW Saar, Prof. Thomas Beckert
- Exam format: project (working, documented Umbraco CMS website), no oral presentation
- Deadline: **31 August 2026**
- Team: Ali, Zaid, Waseem

The visual design reuses the free "Modus-Versus" HTML theme (w3layouts, Bootstrap-based),
converted into Umbraco Document Types and Razor templates.

## Tech stack

- Umbraco CMS 13 (LTS) on ASP.NET Core / .NET 8
- Razor views, Bootstrap, jQuery, and the Modus-Versus theme's existing plugins (owl.carousel,
  jquery.fancybox, jquery.mixitup, fwslider)
- SQLite (Umbraco's local development database)

## Setup / how to run locally

**Prerequisites**

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (the project targets `net8.0`
  specifically - Umbraco 13 does not support .NET 9)
- Umbraco project templates: `dotnet new install Umbraco.Templates::13.*` (not required to run
  an already-created project, only if you want to scaffold a new one the same way this was made)

**Run it**

```bash
cd BellaVista
dotnet restore
dotnet run
```

Open **https://localhost:44300** or **http://localhost:5140** (whichever `dotnet run` prints).
There is no manual setup step needed: on first run, Umbraco creates the SQLite database, and
a startup routine (see [`Composing/`](Composing/)) creates every Document Type, Data Type and
Template in code, then seeds realistic demo content (dishes, gallery photos, news articles,
etc.) automatically. Just wait for the first request to finish (it does noticeably more work
than a normal Umbraco boot the very first time).

**Backoffice login** (`/umbraco`): `admin@bellavista.local` / `BellaVista2026!`
(configured via unattended install in `appsettings.Development.json` - a local dev credential,
not a production secret).

**Loyal Guests demo login** (`/login`): `guest@bellavista.local` / `LoyalGuest2026!`

## Why the schema is created in code, not by hand in the backoffice

Document Types, Data Types and Templates are normally clicked together in the Umbraco
backoffice. We built ours in C# instead (`Composing/DataTypeSeeder.cs`,
`Composing/ContentTypeSeeder.cs`, `Composing/TemplateSeeder.cs`, `Composing/ContentSeeder.cs`,
wired up by `Composing/BellaVistaComposer.cs`), running once on startup. This means:

- the whole schema is in version control and reviewable as source code, and
- anyone can clone the repo and get the exact same site, with demo content, after one
  `dotnet run` - no manual backoffice setup steps to follow or get wrong.

Every seeding step checks whether it already ran (by alias/name) before doing anything, so it's
safe on every restart. This is why `umbraco/Data/*.sqlite.db` and `wwwroot/media/` are
gitignored: they're fully regenerated from this code on first run, and a binary SQLite file in
git would just be a source of merge conflicts.

## Feature list

| Feature | What it does | Where |
|---|---|---|
| **Master/Home/Content Page templates** | Shared header/nav/footer in `Master.cshtml`, used as `Layout` by every page template. `Home` is the homepage-only doctype (slider); `ContentPage` is the generic page used for About/Contact/Loyal Guests/Login/Members-only. | `Views/Master.cshtml`, `Views/Home.cshtml`, `Views/ContentPage.cshtml`, `Composing/ContentTypeSeeder.cs` |
| **SEO composition** | `pageTitle` / `metaDescription` / `metaKeywords` on a shared Composition, composed into every page type (About/Contact/Menu/Gallery/News/dish pages all reuse it). | `Composing/ContentTypeSeeder.cs` (`CreateSeoComposition`), rendered in `Views/Master.cshtml` |
| **Homepage slider** | Editable via **Nested Content** (per the course's Übung 4.2), one `slideItem` element per slide (title, subtitle, image), rendered through the theme's `fwslider` JS. | `Composing/DataTypeSeeder.cs` (`CreateSliderNestedContent`), `Composing/ContentTypeSeeder.cs` (`CreateSlideItem`), rendered in `Views/Master.cshtml` |
| **Dynamic navigation** | A plain Partial View (not a macro - matches the course's Übung 4) that walks the content tree, builds the nav labels from Dictionary Items, and respects the built-in "hide from navigation" flag. | `Views/Partials/Navigation.cshtml` |
| **Blockgrid menu** | Each dish is a Block Grid element (`dish`: name, description, price, category, image, spice level). `menuSection` blocks (Starters/Mains/Desserts/Drinks) sit at the grid root and each expose a "dishes" **Area** so staff can drop dishes into whichever section they want. `dish` composes a small `highlightable` element type (Composition) for the "today's special" flag. | `Composing/DataTypeSeeder.cs` (`CreateMenuBlockGrid`), `Composing/ContentTypeSeeder.cs` (`CreateMenuSection`, `CreateDish`, `CreateHighlightable`), `Views/MenuPage.cshtml`, `Views/Partials/blockgrid/Components/menuSection.cshtml`, `dish.cshtml` |
| **Custom "Spice Level" property editor** | A 0-3 chili-icon picker, built the way the course teaches: `App_Plugins/SpiceLevel/package.manifest` + an AngularJS view/controller, registered server-side via a small `[DataEditor]` class so it can be used both from the backoffice and from our code-first Data Type seeding. | `App_Plugins/SpiceLevel/`, `Composing/SpiceLevelDataEditor.cs` |
| **Gallery with filtering** | Photos are a **Block List** of `galleryImage` elements (image, caption, category). Rendered with the theme's existing mixitup + fancybox markup/JS for the Starters/Mains/Desserts/Drinks filter buttons and lightbox. | `Composing/DataTypeSeeder.cs` (`CreateGalleryBlockList`), `Views/GalleryPage.cshtml`, `Views/Partials/blocklist/Components/galleryImage.cshtml` |
| **Multilingual (DE/EN/AR)** | German, English and Arabic registered as Umbraco languages. UI chrome (nav, footer, buttons, login/contact form, gallery filters) comes from **Dictionary Items**, resolved via `Umbraco.GetDictionaryValue(key, CultureInfo)`. All seeded page content (About Us story, menu dish descriptions, gallery captions, news articles, homepage slider/testimonials, contact/loyal-guests text) is fully translated too, stored as parallel invariant properties (`fieldName` / `fieldNameEn` / `fieldNameAr`) picked at render time via `LanguageHelper.Pick(...)` - see "Why parallel properties, not Umbraco culture variance" below. Language is picked via a plain-text `?lang=de/en/ar` switcher in the main nav (Übung 7.1/7.2), stored in a cookie. Arabic flips the page to `dir="rtl"`; only genuinely untranslated content (Italian dish names, article headlines kept as authored) stays explicitly `dir="ltr"` so it doesn't get bidi-reordered inside an RTL page. | `Composing/LanguageAndDictionarySeeder.cs`, `Helpers/LanguageHelper.cs`, `Composing/ContentSeeder.cs`, language switcher + `Dict()`/`Pick()` calls throughout `Views/` |
| **Members area** | Umbraco's built-in Member system (Identity-based): a "Loyal Guests" member group, a `/login` page (its own template, reusing the `ContentPage` doctype), and the "Loyal Guests" content node protected via Public Access so only logged-in members of that group can see it. | `Composing/MemberAndAccessSeeder.cs`, `Views/Login.cshtml`, `Views/Partials/Members/LoginStatus.cshtml`, `Controllers/Surface/MemberSurfaceController.cs` |
| **Event-handling** | A Composer + `INotificationHandler<ContentPublishedNotification>` logs whenever a Menu page (dishes) or a News item is published, and flags whether it's the first publish (same `IRememberBeingDirty` check taught in class). | `NotificationHandlers/DishAndNewsPublishedNotificationHandler.cs` |
| **REST API for daily specials** | An auto-routed `UmbracoApiController` (`/umbraco/api/specials/getspecials`) returns dishes flagged "today's special" across the menu, through a small Mapper class (`SpecialDto`), with the same `startFrom`/paging shape the course's News "load more" exercise uses. The homepage loads and paginates it via plain jQuery `$.get`, no full page reload. | `Controllers/Api/SpecialsController.cs`, `wwwroot/js/specials.js`, rendered into `Views/Home.cshtml` |
| **Reservation form** | A simple Surface Controller handles the Contact page's table-reservation form (logs the request - see Known Limitations). | `Controllers/Surface/ContactSurfaceController.cs`, form in `Views/ContentPage.cshtml` |

## Team & responsibilities

- **Ali** - Document Types & Templates (Master/Home/ContentPage), SEO composition, dynamic
  navigation, multilingual setup (DE/EN/AR + Dictionary Items).
  `Composing/ContentTypeSeeder.cs`, `Composing/TemplateSeeder.cs`, `Composing/LanguageAndDictionarySeeder.cs`,
  `Views/Master.cshtml`, `Views/Home.cshtml`, `Views/ContentPage.cshtml`, `Views/Partials/Navigation.cshtml`.

- **Zaid** - Blockgrid menu system (MenuPage, dish blocks, Areas/Compositions), custom
  spice-level Property Editor, gallery page with category filtering.
  `Composing/DataTypeSeeder.cs` (Blockgrid/Blocklist config), `Views/MenuPage.cshtml`,
  `Views/GalleryPage.cshtml`, `Views/Partials/blockgrid/`, `Views/Partials/blocklist/`,
  `App_Plugins/SpiceLevel/`.

- **Waseem** - Members area & login (loyal guests / reservation gating), event-handling
  notification on new dish/news publish, REST API controller for daily specials + frontend JS.
  `Composing/MemberAndAccessSeeder.cs`, `Views/Login.cshtml`, `Views/Partials/Members/`,
  `Controllers/Surface/`, `NotificationHandlers/`, `Controllers/Api/SpecialsController.cs`,
  `wwwroot/js/specials.js`.

**Shared/foundational work** (not cleanly one person's): the overall code-first schema/content
seeding architecture (`Composing/SchemaKeys.cs`, `Composing/BellaVistaComposer.cs`,
`Composing/MediaSeeder.cs`, `Composing/BlockValueBuilder.cs`, `Composing/ContentSeeder.cs`) and
importing the Modus-Versus theme assets into `wwwroot/` - all three of us touched these as the
project came together.

## Why parallel properties, not Umbraco culture variance

Translated fields are NOT implemented with Umbraco's native `ContentVariation.Culture` on
Document Types. Reason: Name variance is coupled to a content type's culture variance - making
properties vary by culture would force the Name to vary too, and since this site has no
per-culture domains/hostnames (language is a `?lang=` cookie switch, not separate hostnames),
the default culture's Name is what drives every URL segment (`/menu`, `/gallery`, `/about-us`,
etc.). Making Name vary risked breaking those hardcoded English routes. Instead, each
translatable field gets two extra invariant properties (`title` / `titleEn` / `titleAr`) and
`Helpers/LanguageHelper.Pick(langCode, de, en, ar)` selects the right one at render time. This
was a deliberate, pragmatic call by the team, not something taught in the course exercises.

## Known limitations

- **RTL visual polish**: Arabic sets `dir="rtl"` on `<html>`, and everything genuinely
  untranslated (Italian dish names, prices, news article headlines) stays explicitly
  `dir="ltr"` so it isn't bidi-reordered inside an RTL page. We did not do a full RTL
  layout/spacing pass on every theme component, so some visual polish (icon spacing, alignment)
  may still look better suited to LTR.
- **Images are stock placeholders, not real photos**: the Modus-Versus theme's own photos
  (`s1.jpg`-`s8.jpg`, `blog*.jpg`, `det_pic.jpg`, testimonial logos, etc.) were unrelated to a
  restaurant, so every image in `wwwroot/images/placeholders/` was replaced with real photos
  pulled from **[LoremFlickr](https://loremflickr.com/)** (`loremflickr.com/{w}/{h}/{keywords}`,
  redirects to a matching real Flickr photo) - chosen because it's free, needs no API key or
  attribution, and is explicitly built for placeholder use in projects like this one. These are
  **stand-in photos for a fictional restaurant**, not real photos of a real place, and are used
  here purely for a student course project.
- **Reservation form / event-handling** just logs (`ILogger`) instead of sending real email -
  no SMTP/email service is configured for this course project.
- **No named image crops**: images are served through Umbraco's default ImageSharp pipeline
  without configured named crops (e.g. a "Slider" crop); this keeps the Media Picker Data Type
  simpler at the cost of not art-directing each image size.
- **Schema created in code, not the backoffice**: see "Why the schema is created in code" above.
  Everything is still fully editable afterwards through the normal backoffice UI.
