using Laser.Orchard.ContactForm.Services;
using Laser.Orchard.ContactForm.Models;
using Orchard.MediaLibrary.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.ContactForm.ViewModels;
using Orchard.Themes;
using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.ContactForm.Controllers {

    [Themed(false)]
    public class FileUploadController : Controller {

        public Localizer T { get; set; }

        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IContactFormService _contactFormService;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;

        public FileUploadController(INotifier notifier, IContentManager contentManager, IMediaLibraryService mediaLibraryService, IContactFormService contactFormService) {
            _mediaLibraryService = mediaLibraryService;
            _contactFormService = contactFormService;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public ActionResult Add(int ContactFormID) {

            var model = new FileUploadModel
            {
                ParentID = ContactFormID,
                MediaData = null
            };

            if (Request != null) {

                if (Request.Files.Count > 0) {

                    if (string.IsNullOrWhiteSpace(Path.GetFileName(Request.Files[0].FileName))) {
                        _notifier.Error(T("Please select a file."));
                    } else {
                        ContactFormRecord contactForm = _contactFormService.GetContactForm(ContactFormID);

                        var folderPath = contactForm.PathUpload;
                        for (int i = 0; i < Request.Files.Count; i++) {

                            var file = Request.Files[i];
                            var filename = Path.GetFileName(file.FileName);

                            if (_contactFormService.FileAllowed(filename)) {

                                var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename);
                                _contentManager.Create(mediaPart);
                                var fullPart = _contentManager.Get(mediaPart.Id).As<MediaPart>();
                                model.MediaData = fullPart;
                            } else {
                                _notifier.Error(T("The file extension in the filename is not allowed or has not been provided."));
                            }
                        }
                    }
                }
            }

            return View(model);
        }
    }
}
