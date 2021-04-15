using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Laser.Orchard.Translator.ViewModels;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers {
    public class TranslatorController : Controller {
        private readonly ITranslatorServices _translatorServices;

        public Localizer T { get; set; }

        public TranslatorController(ITranslatorServices translatorServices) {
            _translatorServices = translatorServices;
            T = NullLocalizer.Instance;
        }

        [Admin]
        public ActionResult Index(string language, string folderName, string folderType) {
            TranslationDetailViewModel translationDetailVM = new TranslationDetailViewModel();

            var messages = _translatorServices.GetTranslations()
                .Where(m => m.Language == language
                    && m.ContainerName == folderName
                    && m.ContainerType == folderType)
                .Select(x => new StringSummaryViewModel {
                    id = x.Id,
                    message = x.Message,
                    localized = !string.IsNullOrWhiteSpace(x.TranslatedMessage)
                });

            translationDetailVM.containerName = folderName;
            translationDetailVM.containerType = folderType;
            translationDetailVM.language = language;
            translationDetailVM.messages = messages.ToList().OrderBy(m => m.localized).ThenBy(x => x.message).ToList();

            return View(translationDetailVM);
        }

        public ActionResult TranslatorForm(int id) {
            TranslationRecord messageRecord = _translatorServices.GetTranslations().Where(m => m.Id == id).FirstOrDefault();
            if (messageRecord != null) {
                ViewBag.SuggestedTranslations = _translatorServices.GetSuggestedTranslations(messageRecord.Message, messageRecord.Language);
                return View(messageRecord);
            } else {
                return View(new TranslationRecord());
            }
        }

        public ActionResult TranslatorFolderSettings(string language, string folderName, string folderType) {
            TranslationFolderSettingsRecord settings = _translatorServices.GetTranslationFoldersSettings()
                .Where(m => m.ContainerName == folderName
                    && m.ContainerType == folderType /*&& m.Language == language*/)
                .FirstOrDefault();

            if (settings == null) {
                settings = new TranslationFolderSettingsRecord();
                settings.ContainerName = folderName;
                settings.ContainerType = folderType;
                //settings.Language = language;
            }

            return View(settings);
        }

        [HttpPost]
        [ActionName("TranslatorForm")]
        [FormValueRequired("saveTranslation")]
        public ActionResult SaveTranslation(TranslationRecord translation) {
            bool success = _translatorServices.TryAddOrUpdateTranslation(translation);
            ViewBag.SuggestedTranslations = _translatorServices.GetSuggestedTranslations(translation.Message, translation.Language);

            if (!success) {
                ModelState.AddModelError("SaveTranslationError", T("An error occurred while saving the translation. Please reload the page and retry.").ToString());
                ViewBag.SaveSuccess = false;
            } else {
                ViewBag.SaveSuccess = true;
            }

            return View(translation);
        }

        [HttpPost]
        [ActionName("TranslatorForm")]
        [FormValueRequired("deleteTranslation")]
        public ActionResult DeleteTranslation(int id) {
            TranslationRecord messageRecord = _translatorServices.GetTranslations().Where(m => m.Id == id).FirstOrDefault();

            bool success = _translatorServices.DeleteTranslation(messageRecord);

            if (!success) {
                ModelState.AddModelError("DeleteTranslationError", T("Unable to delete the translation.").ToString());
                ViewBag.DeleteSuccess = false;
                return View(messageRecord);
            } else {
                ViewBag.DeleteSuccess = true;
                return View(new TranslationRecord { Id = 0 });
            }
        }

        [HttpPost]
        [ActionName("TranslatorFolderSettings")]
        public ActionResult SaveTranslationFolderSettings(TranslationFolderSettingsRecord translationSettings) {
            bool success = _translatorServices.TryAddOrUpdateTranslationFolderSettings(translationSettings);

            if (!success) {
                ModelState.AddModelError("SaveTranslationError", T("An error occurred while saving the translation folder settings. Please reload the page and retry.").ToString());
                ViewBag.SaveSuccess = false;
            } else {
                ViewBag.SaveSuccess = true;
            }

            return View(translationSettings);
        }
    }
}