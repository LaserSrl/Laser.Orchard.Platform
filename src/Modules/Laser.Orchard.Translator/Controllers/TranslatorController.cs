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
                    && (m.ContainerName == folderName || folderType == "U")
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

        public ActionResult TranslatorForm(int id, string containerName = "", string containerType = "", string language = "") {
            TranslationRecord messageRecord = _translatorServices.GetTranslations().Where(m => m.Id == id).FirstOrDefault();
            if (messageRecord != null) {
                ViewBag.SuggestedTranslations = _translatorServices.GetSuggestedTranslations(messageRecord.Message, messageRecord.Language);
                var viewModel = new TranslationRecordViewModel(messageRecord) {
                    CultureList = _translatorServices.GetCultureList()
                };
                return View(viewModel);
            } else {
                var model = new TranslationRecord {
                    ContainerName = containerName,
                    ContainerType = containerType,
                    Language = language
                };

                var viewModel = new TranslationRecordViewModel(model) {
                    CultureList = _translatorServices.GetCultureList()
                };

                return View(viewModel);
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
        public ActionResult SaveTranslation(TranslationRecordViewModel translationVM) {
            TranslationRecord translation = translationVM.ToTranslationRecord();
            // I need to check if parent page needs to be refreshed.
            // If I'm creating a new record, I have to refresh parent page.
            // I also need to refresh parent page if I changed the ContainerType of my record.
            // I also need to refresh parent page if I changed the Language of my record.
            bool refreshParent = (translationVM.Id == 0 || !translation.ContainerType.Equals(translationVM.ContainerType, System.StringComparison.InvariantCulture) || !translationVM.OriginalLanguage.Equals(translationVM.Language, System.StringComparison.InvariantCulture));
            
            // If I'm saving a new translation, I must not overwrite an existing matching translation.
            bool success = _translatorServices.TryAddOrUpdateTranslation(translation, translation.Id == 0 ? false : true);
            ViewBag.SuggestedTranslations = _translatorServices.GetSuggestedTranslations(translation.Message, translation.Language);

            if (!success) {
                ModelState.AddModelError("SaveTranslationError", T("An error occurred while saving the translation. Please reload the page and retry.").ToString());
                ViewBag.RefreshParent = false;
                ViewBag.SaveSuccess = false;
            } else if (refreshParent) {
                ViewBag.RefreshParent = true;
                ViewBag.SaveSuccess = false;
            } else {
                ViewBag.RefreshParent = false;
                ViewBag.SaveSuccess = true;
            }

            translationVM.CultureList = _translatorServices.GetCultureList();

            return View(translationVM);
        }

        [HttpPost]
        [ActionName("TranslatorForm")]
        [FormValueRequired("saveNewTranslation")]
        public ActionResult SaveNewTranslation(TranslationRecordViewModel translationVM) {
            // I set Id to zero to force the fact this must be a new translation.
            translationVM.Id = 0;

            return SaveTranslation(translationVM);
        }

        [HttpPost]
        [ActionName("TranslatorForm")]
        [FormValueRequired("deleteTranslation")]
        public ActionResult DeleteTranslation(int id) {
            TranslationRecord messageRecord = _translatorServices.GetTranslations().Where(m => m.Id == id).FirstOrDefault();

            bool success = _translatorServices.DeleteTranslation(messageRecord);

            if (!success) {
                ModelState.AddModelError("DeleteTranslationError", T("Unable to delete the translation.").ToString());
                ViewBag.RefreshParent = false;
                var viewModel = new TranslationRecordViewModel(messageRecord) {
                    CultureList = _translatorServices.GetCultureList()
                };
                return View(viewModel);
            } else {
                ViewBag.RefreshParent = true;
                return Content(T("The translation has been deleted.").Text);
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