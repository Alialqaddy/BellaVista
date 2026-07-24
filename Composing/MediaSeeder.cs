using Microsoft.AspNetCore.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace BellaVista.Composing;

/// <summary>
/// Copies a handful of the Modus-Versus theme's stock photos (already sitting in
/// wwwroot/images) into the Umbraco media library, so the demo content (slider,
/// dishes, gallery, news) has real Media items to pick from instead of Lorem Ipsum.
/// </summary>
public class MediaSeeder
{
    private readonly IMediaService _mediaService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly MediaFileManager _mediaFileManager;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

    private readonly Dictionary<string, Guid> _cache = new();

    public MediaSeeder(
        IMediaService mediaService,
        IWebHostEnvironment webHostEnvironment,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
    {
        _mediaService = mediaService;
        _webHostEnvironment = webHostEnvironment;
        _mediaFileManager = mediaFileManager;
        _mediaUrlGenerators = mediaUrlGenerators;
        _shortStringHelper = shortStringHelper;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
    }

    /// <summary>Returns the Media key for a theme image (under wwwroot/images), creating the Media item on first use.</summary>
    public Guid FromThemeImage(string fileName, string? displayName = null)
    {
        if (_cache.TryGetValue(fileName, out Guid cached)) return cached;

        string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException($"Theme image not found for media seeding: {path}");
        }

        IMedia media = _mediaService.CreateMediaWithIdentity(
            displayName ?? Path.GetFileNameWithoutExtension(fileName),
            -1,
            Constants.Conventions.MediaTypes.Image);

        using (FileStream stream = System.IO.File.OpenRead(path))
        {
            media.SetValue(
                _mediaFileManager,
                _mediaUrlGenerators,
                _shortStringHelper,
                _contentTypeBaseServiceProvider,
                Constants.Conventions.Media.File,
                fileName,
                stream);
        }

        _mediaService.Save(media, Constants.Security.SuperUserId);
        _cache[fileName] = media.Key;
        return media.Key;
    }
}
