using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace BellaVista.NotificationHandlers;

/// <summary>
/// Chapter 9 "Event-Handling im Umbraco Backend": a Composer that hooks into
/// Umbraco's publish event via IComposer + INotificationHandler, exactly as taught -
/// no generic event bus or third-party library.
/// </summary>
public class EventHandlingComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentPublishedNotification, DishAndNewsPublishedNotificationHandler>();
    }
}

/// <summary>
/// Logs whenever a Menu page (which holds the dish Block Grid) or a News item gets
/// published, and flags whether it's the first time that node was ever published -
/// the same "IRememberBeingDirty / WasPropertyDirty(Id)" check shown in the course slides.
/// A real deployment could swap the logging call below for an email via IEmailSender.
/// </summary>
public class DishAndNewsPublishedNotificationHandler : INotificationHandler<ContentPublishedNotification>
{
    private readonly ILogger<DishAndNewsPublishedNotificationHandler> _logger;

    public DishAndNewsPublishedNotificationHandler(ILogger<DishAndNewsPublishedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public void Handle(ContentPublishedNotification notification)
    {
        foreach (IContent content in notification.PublishedEntities)
        {
            bool isNew = ((IRememberBeingDirty)content).WasPropertyDirty("Id");

            if (content.ContentType.Alias.InvariantEquals("newsItem"))
            {
                _logger.LogInformation(
                    "{Status} news item published: \"{Name}\"",
                    isNew ? "New" : "Updated",
                    content.Name);
            }
            else if (content.ContentType.Alias.InvariantEquals("menuPage"))
            {
                int dishCount = content.GetValue<string>("dishes")?.Contains("\"dishName\"") == true
                    ? content.GetValue<string>("dishes")!.Split("\"dishName\"").Length - 1
                    : 0;

                _logger.LogInformation(
                    "{Status} menu page published: \"{Name}\" ({DishCount} dishes in the Block Grid)",
                    isNew ? "New" : "Updated",
                    content.Name,
                    dishCount);
            }
        }
    }
}
