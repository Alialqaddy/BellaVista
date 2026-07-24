using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace BellaVista.Composing;

/// <summary>
/// Builds the raw JSON Umbraco stores for NestedContent / Block List / Block Grid
/// properties. There is no public "add a block programmatically" API, so this
/// constructs the same <see cref="BlockValue"/> model the backoffice editors produce
/// and serializes it exactly like Umbraco does (camelCase properties, but the
/// "Umbraco.BlockList" / "Umbraco.BlockGrid" dictionary keys must stay as-is).
/// </summary>
public static class BlockValueBuilder
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(processDictionaryKeys: false, overrideSpecifiedNames: true),
        },
    };

    public static string ToJson(object value) => JsonConvert.SerializeObject(value, Settings);

    /// <summary>Raw value for a single-image Media Picker 3 property.</summary>
    public static JArray MediaPickerValue(Guid mediaKey) => new(
        new JObject(new JProperty("key", Guid.NewGuid()), new JProperty("mediaKey", mediaKey)));

    public static string MediaPickerJson(Guid mediaKey) => ToJson(MediaPickerValue(mediaKey));

    /// <summary>Raw value for a (single-select) Dropdown property.</summary>
    public static string DropDownJson(string selected) => ToJson(new JArray(selected));

    public record SlideItemInput(string Title, string TitleEn, string TitleAr, string SubTitle, string SubTitleEn, string SubTitleAr, Guid ImageMediaKey);

    /// <summary>Builds the JSON for a NestedContent "slider" property (Übung 4.2).</summary>
    public static string BuildSlider(IEnumerable<SlideItemInput> slides)
    {
        var array = new JArray();
        foreach (SlideItemInput slide in slides)
        {
            array.Add(new JObject(
                new JProperty("key", Guid.NewGuid()),
                new JProperty("name", slide.Title),
                new JProperty("ncContentTypeAlias", "slideItem"),
                new JProperty("title", slide.Title),
                new JProperty("titleEn", slide.TitleEn),
                new JProperty("titleAr", slide.TitleAr),
                new JProperty("subTitle", slide.SubTitle),
                new JProperty("subTitleEn", slide.SubTitleEn),
                new JProperty("subTitleAr", slide.SubTitleAr),
                new JProperty("bgImage", MediaPickerValue(slide.ImageMediaKey))));
        }

        return array.ToString(Formatting.None);
    }

    public record GalleryImageInput(string Caption, string CaptionEn, string CaptionAr, string Category, Guid ImageMediaKey);

    /// <summary>Builds the JSON for the Gallery page's "images" Block List property.</summary>
    public static string BuildGallery(IEnumerable<GalleryImageInput> images)
    {
        var value = new BlockValue { Layout = new Dictionary<string, JToken>() };
        var layoutItems = new List<BlockListLayoutItem>();

        foreach (GalleryImageInput image in images)
        {
            var udi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
            var data = new BlockItemData { ContentTypeKey = SchemaKeys.GalleryImage, Udi = udi };
            data.RawPropertyValues["caption"] = image.Caption;
            data.RawPropertyValues["captionEn"] = image.CaptionEn;
            data.RawPropertyValues["captionAr"] = image.CaptionAr;
            data.RawPropertyValues["category"] = new JArray(image.Category);
            data.RawPropertyValues["image"] = MediaPickerValue(image.ImageMediaKey);

            value.ContentData.Add(data);
            layoutItems.Add(new BlockListLayoutItem { ContentUdi = udi });
        }

        value.Layout[Constants.PropertyEditors.Aliases.BlockList] = JArray.FromObject(layoutItems, JsonSerializer.Create(Settings));
        return ToJson(value);
    }

    public record DishInput(string Name, string Description, string DescriptionEn, string DescriptionAr, string Price, string Category, Guid ImageMediaKey, int SpiceLevel, bool IsTodaysSpecial);

    public record MenuSectionInput(string Title, string TitleEn, string TitleAr, IReadOnlyList<DishInput> Dishes);

    /// <summary>Builds the JSON for the Menu page's "dishes" Block Grid property (sections at root, dishes inside each section's "dishes" Area).</summary>
    public static string BuildMenu(IEnumerable<MenuSectionInput> sections)
    {
        var value = new BlockValue { Layout = new Dictionary<string, JToken>() };
        var rootLayoutItems = new List<BlockGridLayoutItem>();

        foreach (MenuSectionInput section in sections)
        {
            var sectionUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
            var sectionData = new BlockItemData { ContentTypeKey = SchemaKeys.MenuSection, Udi = sectionUdi };
            sectionData.RawPropertyValues["sectionTitle"] = section.Title;
            sectionData.RawPropertyValues["sectionTitleEn"] = section.TitleEn;
            sectionData.RawPropertyValues["sectionTitleAr"] = section.TitleAr;
            value.ContentData.Add(sectionData);

            var dishLayoutItems = new List<BlockGridLayoutItem>();
            foreach (DishInput dish in section.Dishes)
            {
                var dishUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
                var dishData = new BlockItemData { ContentTypeKey = SchemaKeys.Dish, Udi = dishUdi };
                dishData.RawPropertyValues["dishName"] = dish.Name;
                dishData.RawPropertyValues["description"] = dish.Description;
                dishData.RawPropertyValues["descriptionEn"] = dish.DescriptionEn;
                dishData.RawPropertyValues["descriptionAr"] = dish.DescriptionAr;
                dishData.RawPropertyValues["price"] = dish.Price;
                dishData.RawPropertyValues["category"] = new JArray(dish.Category);
                dishData.RawPropertyValues["image"] = MediaPickerValue(dish.ImageMediaKey);
                dishData.RawPropertyValues["spiceLevel"] = dish.SpiceLevel;
                dishData.RawPropertyValues["isTodaysSpecial"] = dish.IsTodaysSpecial;
                value.ContentData.Add(dishData);

                dishLayoutItems.Add(new BlockGridLayoutItem { ContentUdi = dishUdi, ColumnSpan = 4, RowSpan = 1 });
            }

            rootLayoutItems.Add(new BlockGridLayoutItem
            {
                ContentUdi = sectionUdi,
                ColumnSpan = 12,
                RowSpan = 1,
                Areas = new[]
                {
                    new BlockGridLayoutAreaItem { Key = SchemaKeys.MenuSectionDishesArea, Items = dishLayoutItems.ToArray() },
                },
            });
        }

        value.Layout[Constants.PropertyEditors.Aliases.BlockGrid] = JArray.FromObject(rootLayoutItems, JsonSerializer.Create(Settings));
        return ToJson(value);
    }
}
