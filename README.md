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

Open **https://localhost:44300** or **http://localhost:5140** (whichever `dotnet run` prints) -
a bare `/` request redirects to `/de/`, since the site has no un-prefixed domain (see the
Multilingual row below). There is no manual setup step needed: on first run, Umbraco creates the SQLite database, and
a startup routine (see [`Composing/`](Composing/)) creates every Document Type, Data Type and
Template in code, then seeds realistic demo content (dishes, gallery photos, news articles,
etc.) automatically. Just wait for the first request to finish (it does noticeably more work
than a normal Umbraco boot the very first time).

**Backoffice login** (`/umbraco`): `admin@bellavista.local` / `BellaVista2026!`
(configured via unattended install in `appsettings.Development.json` - a local dev credential,
not a production secret).

**Loyal Guests demo login** (linked from the header on every page - the URL itself is
culture-variant, e.g. `/de/anmelden/` or `/en/log-in/`): `guest@bellavista.local` / `LoyalGuest2026!`

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
| **Multilingual (DE/EN/AR)** | German, English and Arabic registered as Umbraco languages, with real **Culture Variance** (chapter 7, slide 71) on every page Document Type and a **path-suffix domain** per language (slide 72: `/de/`, `/en/`, `/ar/`, all pointing at the same Home node) instead of separate hostnames - well suited to a localhost setup, exactly as the slides describe. SEO fields, headings, body text, intros and teasers are real culture-variant properties, entered through Umbraco's native multi-language content editing (slide 73) and picked up automatically by `Model.Value(...)` for whichever culture the current domain resolves to - no manual language-picking code needed for those. UI chrome (nav, footer, buttons, forms, gallery filters) still comes from **Dictionary Items** via `Umbraco.GetDictionaryValue(key, CultureInfo)`. The four fields that live *inside* a Block List / Block Grid / Nested Content element (dish description, gallery caption, menu section title, slide title/subtitle) are the one exception - see "Why four fields still use parallel properties" below. The language switcher (Übung 7.1/7.2) links to each culture's real translated URL via `Model.Url(culture: "de-DE")` etc., with only the switcher's own DE/EN/AR label text still a plain Dictionary lookup. Arabic sets `dir="rtl"` on `<html>`; only genuinely untranslated content (Italian dish names, prices) stays explicitly `dir="ltr"`. | `Composing/ContentTypeSeeder.cs` (`PropV`, `Variations = ContentVariation.Culture`), `Composing/ContentSeeder.cs` (`SetV`, `SetName`, `SeedDomains`), `Composing/LanguageAndDictionarySeeder.cs`, `Helpers/LanguageHelper.cs`, language switcher in `Views/Master.cshtml` |
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

## Why four fields still use parallel properties, not Umbraco culture variance

Almost every translatable field uses Umbraco's real `ContentVariation.Culture` (see the
multilingual row above). Four fields are the exception and still use parallel invariant
properties (`title` / `titleEn` / `titleAr`, picked at render time via
`Helpers/LanguageHelper.Pick`): `Dish.description` and `MenuSection.sectionTitle` (both inside
the Menu's Block Grid), `GalleryImage.caption` (inside the Gallery's Block List), and
`SlideItem.title`/`subTitle` (inside the homepage's Nested Content slider).

This isn't a design preference - it's a verified platform limitation. We inspected Umbraco
13.16.0's actual storage model for these three editors directly (`Umbraco.Cms.Core.Models.Blocks.BlockValue`
/ `BlockItemData` via reflection against the real `Umbraco.Infrastructure` assembly): there is no
per-culture dimension anywhere in it - `BlockItemData` is a flat `Dictionary<string, object>`
with no `Culture`/`Segment`/"Expose" concept at all (that came to Block-based editors in a later
Umbraco major version, not 13). Nested Content, being older still, has the same limitation.
Since native variance for element-type properties inside these editors is simply not possible on
this Umbraco version - not something that would work with more careful code - these four fields
keep the parallel-property approach, while everything else on the site (including the content
Name, which now drives a real per-language URL segment) uses native variance.

Two smaller, related implementation notes:

- **`Controllers/Api/SpecialsController.cs`** and the `Descendants(alias)` call in
  `Views/Master.cshtml`'s "latest news" footer both needed a fix once culture-variant domain
  routing was introduced: the filtered `Descendants("newsItem")`/`DescendantsOrSelf("menuPage")`
  extension overloads stopped matching correctly under domain-based culture resolution, so both
  now walk `.Descendants()` unfiltered and filter by `ContentType.Alias` explicitly instead. The
  REST API additionally has no ambient culture at all (it isn't a culture-prefixed content URL),
  so the calling page passes its already-resolved language as a `lang` query parameter (see
  `wwwroot/js/specials.js`), and the controller sets `IVariationContextAccessor.VariationContext`
  from it manually before querying - without that, Umbraco can't tell which culture's published
  version of the (now variant) Menu page to read.
- Pages that share the `contentPage` Document Type (About Us, Contact, Loyal Guests, Login,
  Members Only) can no longer be looked up by a hardcoded German name now that Name varies by
  culture, so `Composing/ContentKeys.cs` gives each a fixed GUID, looked up via
  `Umbraco.Content(ContentKeys.X)` wherever a template needs to link to one of them directly.

## Known limitations

- **RTL visual polish**: Arabic sets `dir="rtl"` on `<html>`, and everything genuinely
  untranslated (Italian dish names, prices) stays explicitly `dir="ltr"` so it isn't
  bidi-reordered inside an RTL page. We did not do a full RTL layout/spacing pass on every
  theme component, so some visual polish (icon spacing, alignment) may still look better
  suited to LTR.
- **Images are stock placeholders, not real photos**: the Modus-Versus theme's own photos
  (`s1.jpg`-`s8.jpg`, `blog*.jpg`, `det_pic.jpg`, testimonial logos, etc.) were unrelated to a
  restaurant, so every image in `wwwroot/images/placeholders/` was replaced with a real,
  category-appropriate photo (pasta for a pasta dish, a wine glass for the drinks section,
  etc.), originally sourced one-time from **[LoremFlickr](https://loremflickr.com/)** (free, no
  API key or attribution required, explicitly built for placeholder use) and then **downloaded
  and committed to the repo as regular `.jpg` files** - the app never contacts LoremFlickr, or
  any external service, at build or run time. `Composing/MediaSeeder.cs` only ever does a local
  `File.OpenRead` from `wwwroot/images/`; there is no `HttpClient`/`WebClient` call anywhere in
  the seeding path. This means the site builds, seeds, and serves every image correctly with no
  internet connection at all - important for a graded submission that may be opened offline or
  on an unreliable network. These remain **stand-in photos for a fictional restaurant**, not
  real photos of a real place, used here purely for a student course project.
- **Reservation form / event-handling** just logs (`ILogger`) instead of sending real email -
  no SMTP/email service is configured for this course project.
- **No named image crops**: images are served through Umbraco's default ImageSharp pipeline
  without configured named crops (e.g. a "Slider" crop); this keeps the Media Picker Data Type
  simpler at the cost of not art-directing each image size.
- **Schema created in code, not the backoffice**: see "Why the schema is created in code" above.
  Everything is still fully editable afterwards through the normal backoffice UI.
