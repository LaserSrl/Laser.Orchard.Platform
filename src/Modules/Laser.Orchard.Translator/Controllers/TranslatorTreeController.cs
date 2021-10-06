using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Laser.Orchard.Translator.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers {
    [Admin]
    public class TranslatorTreeController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ITranslatorServices _translatorServices;
        private readonly IUtilsServices _utilsServices;
        private TranslatorSettingsPart _translatorSettings;
        private IEnumerable<TranslationFolderSettingsRecord> _translationFolderSettingsRecords;

        public Localizer T { get; set; }

        public TranslatorTreeController(IOrchardServices orchardServices, ITranslatorServices translatorServices, IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _translatorServices = translatorServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index(string selectedCulture, string selectedFolderName, string selectedFolderType) {
            TranslatorViewModel treeVM = new TranslatorViewModel();

            treeVM.CultureList = _translatorServices.GetCultureList();
            treeVM.selectedCulture = selectedCulture;
            treeVM.selectedFolderName = selectedFolderName;
            treeVM.selectedFolderType = selectedFolderType;

            return View(treeVM);
        }

        public JsonResult CreateJsonForTree(string language) {
            List<TranslationTreeNodeViewModel> tree = new List<TranslationTreeNodeViewModel>();

            if (!String.IsNullOrWhiteSpace(language)) {
                // Reading all messages without a valid translation (or with Undefined container type), grouped by container type and name.
                var messagesToTranslate = _translatorServices.GetTranslations()
                    .Where(t => t.Language == language)
                    .AsParallel()
                    .GroupBy(t => new { t.ContainerType, t.ContainerName, unTranslated = (t.ContainerType == "U" || string.IsNullOrWhiteSpace(t.TranslatedMessage)) }, (key, group)
                        => new { containerType = key.ContainerType, containerName = key.ContainerName, unTranslated = key.unTranslated, count = group.Count() })
                    .Where(t => t.unTranslated)
                    .Select(t => new ContainerMessage { ContainerType = t.containerType, ContainerName = t.containerName, Count = t.count })
                    .ToList();

                // Laser Modules
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-M",
                    text = T("Modules").ToString(),
                    children = CreateListForTree(language, 
                        Path.Combine(_utilsServices.TenantPath, "Modules"), 
                        ElementToTranslate.Module, 
                        messagesToTranslate
                            .Where(m => m.ContainerType == "M")
                            .ToDictionary(cm => cm.ContainerName)),
                    data = new Dictionary<string, string>() { { "type", "M" } }
                });

                // Laser Themes
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-T",
                    text = T("Themes").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Themes"), 
                        ElementToTranslate.Theme, 
                        messagesToTranslate
                            .Where(m => m.ContainerType == "T")
                            .ToDictionary(cm => cm.ContainerName)),
                    data = new Dictionary<string, string>() { { "type", "T" } }
                });

                // Tenants
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-A",
                    text = T("Tenants").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Tenant"),
                        ElementToTranslate.Tenant, 
                        messagesToTranslate
                            .Where(m => m.ContainerType == "A")
                            .ToDictionary(cm => cm.ContainerName)),
                    data = new Dictionary<string, string>() { { "type", "A" } }
                });

                // Orchard modules
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-W",
                    text = T("Orchard modules").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Orchard modules"), 
                        ElementToTranslate.OrchardModule,
                        messagesToTranslate
                            .Where(m => m.ContainerType == "W")
                            .ToDictionary(cm => cm.ContainerName)),
                    data = new Dictionary<string, string>() { { "type", "W" } }
                });

                // Orchard themes
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-X",
                    text = T("Orchard themes").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Orchard themes"), 
                        ElementToTranslate.OrchardTheme, 
                        messagesToTranslate
                            .Where(m => m.ContainerType == "X")
                            .ToDictionary(cm => cm.ContainerName)),
                    data = new Dictionary<string, string>() { { "type", "X" } }
                });

                // Orchard core
                int labels = 0;
                if (messagesToTranslate.Any(m => m.ContainerType == "Y" && m.ContainerName == "Orchard.Core")) {
                    labels = messagesToTranslate.FirstOrDefault(m => m.ContainerType == "Y" && m.ContainerName == "Orchard.Core").Count;
                }
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-Y",
                    text = T("Orchard.Core").ToString(),
                    data = new Dictionary<string, string>() { { "type", "Y" }, { "percent", T("{0} labels to translate", labels.ToString()).Text } }
                });

                // Orchard framework
                labels = 0;
                if (messagesToTranslate.Any(m => m.ContainerType == "Z" && m.ContainerName == "Orchard.Framework")) {
                    labels = messagesToTranslate.FirstOrDefault(m => m.ContainerType == "Z" && m.ContainerName == "Orchard.Framework").Count;
                }
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-Z",
                    text = T("Orchard.Framework").ToString(),
                    data = new Dictionary<string, string>() { { "type", "Z" }, { "percent", T("{0} labels to translate", labels.ToString()).Text } }
                });

                // Undefined
                labels = 0;
                if (messagesToTranslate.Any(m => m.ContainerType == "U")) {
                    labels = messagesToTranslate.FirstOrDefault(m => m.ContainerType == "U").Count;
                }
                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-U",
                    text = T("Undefined").ToString(),
                    data = new Dictionary<string, string>() { { "type", "U" }, { "percent", T("{0} labels to translate", labels.ToString()).Text } }
                });
            }

            return Json(tree, JsonRequestBehavior.AllowGet);
        }

        private int GetLabelsToTranslate(List<TranslationRecord> messages, string containerName, string containerType) {
            int result = 0;

            result = messages.Where(t => (t.ContainerType == "U" || (t.ContainerType != "U" && t.ContainerName == containerName))
                    && t.ContainerType == containerType).Count();

            return result;
        }

        /// <summary>
        /// Get translator settings if they've not read from database yet.
        /// </summary>
        /// <returns></returns>
        private TranslatorSettingsPart GetSettings() {
            if (_translatorSettings == null) {
                _translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();
            }

            return _translatorSettings;
        }

        private List<TranslationTreeNodeViewModel> CreateListForTree(string language, string parentFolder, ElementToTranslate elementType, IDictionary<string, ContainerMessage> messages) {
            var translatorSettings = GetSettings();

            List<string> elementsToTranslate = new List<string>();
            var folderType = "";
            switch (elementType) {
                case ElementToTranslate.Module:
                    elementsToTranslate = translatorSettings.ModulesToTranslate != null
                        ? translatorSettings.ModulesToTranslate.Replace(" ", "").Split(',').ToList()
                        : new List<string>();
                    folderType = "M";
                    break;
                case ElementToTranslate.Theme:
                    elementsToTranslate = translatorSettings.ThemesToTranslate != null
                        ? translatorSettings.ThemesToTranslate.Replace(" ", "").Split(',').ToList()
                        : new List<string>();
                    folderType = "T";
                    break;
                case ElementToTranslate.Tenant:
                    elementsToTranslate = translatorSettings.TenantsToTranslate != null
                        ? translatorSettings.TenantsToTranslate.Replace(" ", "").Split(',').ToList()
                        : new List<string>();
                    folderType = "A";
                    break;
                case ElementToTranslate.OrchardModule:
                    elementsToTranslate = translatorSettings.OrchardModulesToTranslate != null
                        ? translatorSettings.OrchardModulesToTranslate.Replace(" ", "").Split(',').ToList()
                        : new List<string>();
                    folderType = "W";
                    break;
                case ElementToTranslate.OrchardTheme:
                    elementsToTranslate = translatorSettings.OrchardThemesToTranslate != null
                        ? translatorSettings.OrchardThemesToTranslate.Replace(" ", "").Split(',').ToList()
                        : new List<string>();
                    folderType = "X";
                    break;
            }

            // rimossa questa verifica in quanto se non presente la cartella su server (cosa possibile in quanto potrei voler tradurre un tema o un modulo non ancora deployato) non presenta il nodo da tradurre
            //var list = new List<string>(Directory.GetDirectories(parentFolder));
            //list = list.Select(dir => dir.Remove(0, dir.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToList();
            //list = list.Where(dir => elementsToTranslate.Any(x => x == dir)).ToList();
            //list.Sort((x, y) => string.Compare(x, y));

            var list = elementsToTranslate.Where(x => !String.IsNullOrWhiteSpace(x)).OrderBy(x => x);
            
            List<TranslationTreeNodeViewModel> treeList = new List<TranslationTreeNodeViewModel>();
            foreach (var item in list) {
                Dictionary<string, string> additionalData = new Dictionary<string, string>();

                int labels = 0;
                if (messages.ContainsKey(item)) {
                    labels = messages[item].Count;
                }

                additionalData.Add("percent", T("{0} labels to translate", labels).Text);
                additionalData.Add("to_translate", "true");
                additionalData.Add("type", folderType);

                string deprecatedType = "";
                if (IsDeprecated(item, folderType))
                    deprecatedType = "deprecated";

                treeList.Add(new TranslationTreeNodeViewModel { id = "translatortree-child-" + item.Replace('.', '-'), text = item, data = additionalData, type = deprecatedType });
            }

            return treeList;
        }
        
        private IEnumerable<TranslationFolderSettingsRecord> GetFolderSettings() {
            if (_translationFolderSettingsRecords == null) {
                _translationFolderSettingsRecords = _translatorServices.GetTranslationFoldersSettings().ToList();
            }

            return _translationFolderSettingsRecords;
        }

        private bool IsDeprecated(string containerName, string containerType) {
            //return false;

            var folderSettings = GetFolderSettings().FirstOrDefault(t => t.ContainerName == containerName && t.ContainerType == containerType);

            return folderSettings == null ? false : folderSettings.Deprecated;
        }
    }
}