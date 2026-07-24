namespace BellaVista.Composing;

/// <summary>
/// Fixed GUIDs for the singleton content nodes (one per page, created once at seed time).
/// Several pages share the "contentPage" Document Type (About Us, Contact, Loyal Guests,
/// Login, Members Only), and since Name now varies by culture (see ContentTypeSeeder), the
/// content tree can no longer look a specific page up by its German name. Templates and
/// controllers that need to link to one of these pages look it up by this fixed Key instead,
/// via Umbraco.Content(key) / IPublishedContentQuery.Content(key), then call .Url() on it so
/// the link still resolves to the correct URL for whichever culture is currently rendering.
/// </summary>
public static class ContentKeys
{
    public static readonly Guid Home = new("9a13f1a0-0a1b-4a1a-8a1a-000000000101");
    public static readonly Guid About = new("9a13f1a0-0a1b-4a1a-8a1a-000000000102");
    public static readonly Guid Menu = new("9a13f1a0-0a1b-4a1a-8a1a-000000000103");
    public static readonly Guid Gallery = new("9a13f1a0-0a1b-4a1a-8a1a-000000000104");
    public static readonly Guid News = new("9a13f1a0-0a1b-4a1a-8a1a-000000000105");
    public static readonly Guid Contact = new("9a13f1a0-0a1b-4a1a-8a1a-000000000106");
    public static readonly Guid LoyalGuests = new("9a13f1a0-0a1b-4a1a-8a1a-000000000107");
    public static readonly Guid Login = new("9a13f1a0-0a1b-4a1a-8a1a-000000000108");
    public static readonly Guid AccessDenied = new("9a13f1a0-0a1b-4a1a-8a1a-000000000109");
}
