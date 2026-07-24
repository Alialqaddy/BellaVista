using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace BellaVista.Controllers.Surface;

/// <summary>
/// Handles the table reservation form on the Contact page. Kept intentionally simple for
/// a course project: no email service is configured, so the request is just logged -
/// see the README for what a real deployment would add here.
/// </summary>
public class ContactSurfaceController : SurfaceController
{
    private readonly ILogger<ContactSurfaceController> _logger;

    public ContactSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        ILogger<ContactSurfaceController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult HandleReservation(string name, string email, string phone, string? requestedDateTime, string? message)
    {
        _logger.LogInformation(
            "Table reservation request from {Name} ({Email}, {Phone}) for {RequestedDateTime}: {Message}",
            name, email, phone, requestedDateTime, message);

        TempData["ReservationSent"] = true;
        return RedirectToCurrentUmbracoPage();
    }
}
