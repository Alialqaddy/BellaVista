namespace BellaVista.Composing;

/// <summary>
/// Fixed GUIDs for the document/element types and data types we create in code
/// (see SchemaSeeder). Keeping them as constants lets later seeding steps (e.g. the
/// Block Grid data type, which has to reference element types by Key) point at the
/// right content type without having to look it up again first.
/// </summary>
public static class SchemaKeys
{
    // Element type used as a composition (chapter 5.4 "Composition von Doc-/Elementtypes")
    public static readonly Guid SeoComposition = new("9a13f1a0-0a1b-4a1a-8a1a-000000000001");

    // Document types
    public static readonly Guid Master = new("9a13f1a0-0a1b-4a1a-8a1a-000000000002");
    public static readonly Guid Home = new("9a13f1a0-0a1b-4a1a-8a1a-000000000003");
    public static readonly Guid ContentPage = new("9a13f1a0-0a1b-4a1a-8a1a-000000000004");
    public static readonly Guid MenuPage = new("9a13f1a0-0a1b-4a1a-8a1a-000000000005");
    public static readonly Guid GalleryPage = new("9a13f1a0-0a1b-4a1a-8a1a-000000000006");
    public static readonly Guid NewsPage = new("9a13f1a0-0a1b-4a1a-8a1a-000000000007");
    public static readonly Guid NewsItem = new("9a13f1a0-0a1b-4a1a-8a1a-000000000008");

    // Element types for NestedContent / Block List / Block Grid
    public static readonly Guid SlideItem = new("9a13f1a0-0a1b-4a1a-8a1a-000000000010");
    public static readonly Guid GalleryImage = new("9a13f1a0-0a1b-4a1a-8a1a-000000000011");
    public static readonly Guid MenuSection = new("9a13f1a0-0a1b-4a1a-8a1a-000000000012");
    public static readonly Guid Dish = new("9a13f1a0-0a1b-4a1a-8a1a-000000000013");
    public static readonly Guid Highlightable = new("9a13f1a0-0a1b-4a1a-8a1a-000000000014");

    // Data types
    public static readonly Guid CategoryDropDown = new("9a13f1a0-0a1b-4a1a-8a1a-000000000020");
    public static readonly Guid SliderNestedContent = new("9a13f1a0-0a1b-4a1a-8a1a-000000000021");
    public static readonly Guid GalleryBlockList = new("9a13f1a0-0a1b-4a1a-8a1a-000000000022");
    public static readonly Guid MenuBlockGrid = new("9a13f1a0-0a1b-4a1a-8a1a-000000000023");
    public static readonly Guid SpiceLevelEditor = new("9a13f1a0-0a1b-4a1a-8a1a-000000000024");
    public static readonly Guid SingleImagePicker = new("9a13f1a0-0a1b-4a1a-8a1a-000000000025");

    // Area key used inside the Menu Section block-grid area config
    public static readonly Guid MenuSectionDishesArea = new("9a13f1a0-0a1b-4a1a-8a1a-000000000030");

    // Member
    public static readonly Guid LoyalGuestMemberType = new("9a13f1a0-0a1b-4a1a-8a1a-000000000040");
}
