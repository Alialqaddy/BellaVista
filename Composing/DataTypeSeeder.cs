using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Creates the custom Data Types Bella Vista needs on top of the ones Umbraco
/// already ships with (Textstring, Textarea, Richtext editor, True/false, ...).
/// Runs once on startup, see <see cref="BellaVistaDataSeeder"/>.
/// </summary>
public class DataTypeSeeder
{
    private readonly IDataTypeService _dataTypeService;
    private readonly DataEditorCollection _dataEditors;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public DataTypeSeeder(
        IDataTypeService dataTypeService,
        DataEditorCollection dataEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        _dataTypeService = dataTypeService;
        _dataEditors = dataEditors;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    private IDataEditor Editor(string alias) => _dataEditors.First(e => e.Alias == alias);

    public IDataType GetOrCreate(string name, Guid key, string editorAlias, object? configuration = null)
    {
        IDataType? existing = _dataTypeService.GetDataType(name);
        if (existing != null)
        {
            return existing;
        }

        var dataType = new DataType(Editor(editorAlias), _configurationEditorJsonSerializer)
        {
            Key = key,
            Name = name,
        };
        if (configuration != null)
        {
            dataType.Configuration = configuration;
        }

        _dataTypeService.Save(dataType, Constants.Security.SuperUserId);
        return dataType;
    }

    /// <summary>Dropdown of dish/gallery categories, shared between the Menu and the Gallery.</summary>
    public IDataType CreateCategoryDropDown() => GetOrCreate(
        "Bella Vista - Category",
        SchemaKeys.CategoryDropDown,
        Constants.PropertyEditors.Aliases.DropDownListFlexible,
        new DropDownFlexibleConfiguration
        {
            Multiple = false,
            Items = new List<ValueListConfiguration.ValueListItem>
            {
                new() { Id = 1, Value = "Starter" },
                new() { Id = 2, Value = "Main" },
                new() { Id = 3, Value = "Dessert" },
                new() { Id = 4, Value = "Drink" },
            },
        });

    /// <summary>Single image picker, restricted to images, reused across the site.</summary>
    public IDataType CreateSingleImagePicker(string name, Guid key) => GetOrCreate(
        name,
        key,
        Constants.PropertyEditors.Aliases.MediaPicker3,
        new MediaPicker3Configuration
        {
            Multiple = false,
            Filter = Constants.Conventions.MediaTypes.Image,
            ValidationLimit = new MediaPicker3Configuration.NumberRange { Min = 0, Max = 1 },
        });

    /// <summary>The custom spice-level property editor (App_Plugins/SpiceLevel), stores an int 0-3.</summary>
    public IDataType CreateSpiceLevelEditor() => GetOrCreate(
        "Bella Vista - Spice Level",
        SchemaKeys.SpiceLevelEditor,
        "BellaVista.SpiceLevel");

    /// <summary>NestedContent list of "slideItem" elements used for the homepage slider (Übung 4.2).</summary>
    public IDataType CreateSliderNestedContent() => GetOrCreate(
        "Bella Vista - Slider (Nested Content)",
        SchemaKeys.SliderNestedContent,
        Constants.PropertyEditors.Aliases.NestedContent,
        new NestedContentConfiguration
        {
            MinItems = 1,
            MaxItems = 6,
            ConfirmDeletes = true,
            ShowIcons = false,
            HideLabel = true,
            ContentTypes = new[]
            {
                new NestedContentConfiguration.ContentType
                {
                    Alias = "slideItem",
                    TabAlias = "Content",
                },
            },
        });

    /// <summary>Block List of gallery photos, used on the Gallery page.</summary>
    public IDataType CreateGalleryBlockList() => GetOrCreate(
        "Bella Vista - Gallery (Block List)",
        SchemaKeys.GalleryBlockList,
        Constants.PropertyEditors.Aliases.BlockList,
        new BlockListConfiguration
        {
            Blocks = new[]
            {
                new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = SchemaKeys.GalleryImage,
                    Label = "{{caption}}",
                },
            },
        });

    /// <summary>
    /// Block Grid used on the Menu page. "Menu Section" blocks (Starters, Mains, ...) live at
    /// the root; "Dish" blocks are only allowed inside a section's "dishes" Area, which is what
    /// gives staff the flexible, drag-and-drop menu layout asked for in the brief.
    /// </summary>
    public IDataType CreateMenuBlockGrid() => GetOrCreate(
        "Bella Vista - Menu (Block Grid)",
        SchemaKeys.MenuBlockGrid,
        Constants.PropertyEditors.Aliases.BlockGrid,
        new BlockGridConfiguration
        {
            GridColumns = 12,
            Blocks = new[]
            {
                new BlockGridConfiguration.BlockGridBlockConfiguration
                {
                    ContentElementTypeKey = SchemaKeys.MenuSection,
                    AllowAtRoot = true,
                    AllowInAreas = false,
                    Label = "{{sectionTitle}}",
                    Areas = new[]
                    {
                        new BlockGridConfiguration.BlockGridAreaConfiguration
                        {
                            Key = SchemaKeys.MenuSectionDishesArea,
                            Alias = "dishes",
                            ColumnSpan = 12,
                            RowSpan = 1,
                            MinAllowed = 0,
                        },
                    },
                },
                new BlockGridConfiguration.BlockGridBlockConfiguration
                {
                    ContentElementTypeKey = SchemaKeys.Dish,
                    AllowAtRoot = false,
                    AllowInAreas = true,
                    ColumnSpanOptions = new[]
                    {
                        new BlockGridConfiguration.BlockGridColumnSpanOption { ColumnSpan = 4 },
                    },
                    Label = "{{dishName}}",
                },
            },
        });
}
