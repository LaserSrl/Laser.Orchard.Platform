using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Mvc;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.MediaLibrary.Models;
using Orchard.Localization;
using System.Linq;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard;
using om = Orchard.MediaLibrary;
using Orchard.Environment.Extensions;


namespace Laser.Orchard.StartupConfig.Controllers
{
    [OrchardFeature("Laser.Orchard.StartupConfig.ExtendAdminControllerToFrontend")]

    public class FrontEndClientStorageController : Controller
    {

        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IMimeTypeProvider _mimeTypeProvider;

        public FrontEndClientStorageController(
            IMediaLibraryService mediaManagerService,
            IOrchardServices orchardServices,
            IMimeTypeProvider mimeTypeProvider)
        {
            _mediaLibraryService = mediaManagerService;
            _mimeTypeProvider = mimeTypeProvider;
            Services = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }


        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }


        [HttpPost]
        public JsonResult GetIdByName(string folder, List<string> imgName)
        {
            var list = Services.ContentManager
               .Query<MediaPart, MediaPartRecord>(VersionOptions.Latest).Where(x => x.FolderPath.Equals(folder) && imgName.Contains(x.FileName))
               .List()
               .Select(x => x.Id)
               .ToList<int>();
            return Json(list);
        }
        public ActionResult MediaItem(int id, string displayType = "SummaryAdmin")
        {
            var contentItem = Services.ContentManager.Get<MediaPart>(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!_mediaLibraryService.CheckMediaFolderPermission(om.Permissions.SelectMediaContent, contentItem.FolderPath))
            {
                // Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Cannot select media"));
                return new HttpUnauthorizedResult();
            }

            dynamic model = Services.ContentManager.BuildDisplay(contentItem, displayType);

            return new ShapeResult(this, model);
        }

        [HttpPost]
        public ActionResult Delete(int[] mediaItemIds)
        {
            if (!Services.Authorizer.Authorize(om.Permissions.ManageOwnMedia))
            {
                // Services.Notifier.Add(UI.Notify.NotifyType.Error, T("Couldn't delete media items"));
                return new HttpUnauthorizedResult();
            }

            var mediaItems = Services.ContentManager
                .Query(VersionOptions.Latest)
                .ForContentItems(mediaItemIds)
                .List()
                .Select(x => x.As<MediaPart>())
                .Where(x => x != null);

            try
            {
                foreach (var media in mediaItems)
                {
                    if (!_mediaLibraryService.CheckMediaFolderPermission(om.Permissions.DeleteMediaContent, media.FolderPath))
                    {
                        return Json(false);
                    }
                    Services.ContentManager.Remove(media.ContentItem);
                }

                return Json(true);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not delete media items.");
                return Json(false);
            }
        }
        public ActionResult Index(string folderPath, string type, int? replaceId = null)
        {

            if (!_mediaLibraryService.CheckMediaFolderPermission(om.Permissions.SelectMediaContent, folderPath))
            {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath))
            {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new ImportMediaViewModel
            {
                FolderPath = folderPath,
                Type = type,
            };

            if (replaceId != null)
            {
                var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId.Value);
                if (replaceMedia == null)
                    return HttpNotFound();

                viewModel.Replace = replaceMedia;
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Upload(string folderPath, string type)
        {
            if (!_mediaLibraryService.CheckMediaFolderPermission(om.Permissions.ImportMediaContent, folderPath))
            {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath))
            {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();
            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();

            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++)
            {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);

                // if the file has been pasted, provide a default name
                if (file.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                {
                    filename = "clipboard.png";
                }

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(filename))
                {
                    statuses.Add(new
                    {
                        error = T("This file is not allowed: {0}", filename).Text,
                        progress = 1.0,
                    });
                    continue;
                }

                try
                {
                    var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename, type);
                    Services.ContentManager.Create(mediaPart);

                    statuses.Add(new
                    {
                        id = mediaPart.Id,
                        name = mediaPart.Title,
                        type = mediaPart.MimeType,
                        size = file.ContentLength,
                        progress = 1.0,
                        url = mediaPart.FileName,
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unexpected exception when uploading a media.");
                    statuses.Add(new
                    {
                        error = T(ex.Message).Text,
                        progress = 1.0,
                    });
                }
            }

            // Return JSON
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Replace(int replaceId, string type)
        {
            if (!Services.Authorizer.Authorize(om.Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId);
            if (replaceMedia == null)
                return HttpNotFound();

            // Check permission
            if (!(_mediaLibraryService.CheckMediaFolderPermission(om.Permissions.EditMediaContent, replaceMedia.FolderPath) && _mediaLibraryService.CheckMediaFolderPermission(om.Permissions.ImportMediaContent, replaceMedia.FolderPath))
                && !_mediaLibraryService.CanManageMediaFolder(replaceMedia.FolderPath))
            {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();

            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();

            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++)
            {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);

                // if the file has been pasted, provide a default name
                if (file.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                {
                    filename = "clipboard.png";
                }

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(filename))
                {
                    statuses.Add(new
                    {
                        error = T("This file is not allowed: {0}", filename).Text,
                        progress = 1.0,
                    });
                    continue;
                }

                try
                {
                    var mimeType = _mimeTypeProvider.GetMimeType(filename);

                    string replaceContentType = _mediaLibraryService.MimeTypeToContentType(file.InputStream, mimeType, type) ?? type;
                    if (!replaceContentType.Equals(replaceMedia.TypeDefinition.Name, StringComparison.OrdinalIgnoreCase))
                        throw new Exception(T("Cannot replace {0} with {1}", replaceMedia.TypeDefinition.Name, replaceContentType).Text);

                    var mediaItemsUsingTheFile = Services.ContentManager.Query<MediaPart, MediaPartRecord>()
                                                                .ForVersion(VersionOptions.Latest)
                                                                .Where(x => x.FolderPath == replaceMedia.FolderPath && x.FileName == replaceMedia.FileName)
                                                                .Count();
                    if (mediaItemsUsingTheFile == 1)
                    { // if the file is referenced only by the deleted media content, the file too can be removed.
                        _mediaLibraryService.DeleteFile(replaceMedia.FolderPath, replaceMedia.FileName);
                    }
                    else
                    {
                        // it changes the media file name
                        replaceMedia.FileName = filename;
                    }
                    _mediaLibraryService.UploadMediaFile(replaceMedia.FolderPath, replaceMedia.FileName, file.InputStream);
                    replaceMedia.MimeType = mimeType;

                    // Force a publish event which will update relevant Media properties
                    replaceMedia.ContentItem.VersionRecord.Published = false;
                    Services.ContentManager.Publish(replaceMedia.ContentItem);

                    statuses.Add(new
                    {
                        id = replaceMedia.Id,
                        name = replaceMedia.Title,
                        type = replaceMedia.MimeType,
                        size = file.ContentLength,
                        progress = 1.0,
                        url = replaceMedia.FileName,
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unexpected exception when uploading a media.");

                    statuses.Add(new
                    {
                        error = T(ex.Message).Text,
                        progress = 1.0,
                    });
                }
            }

            return Json(statuses, JsonRequestBehavior.AllowGet);
        }
    }
}
