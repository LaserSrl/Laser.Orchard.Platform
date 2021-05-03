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

namespace Laser.Orchard.Translator.Controllers
{
    [Admin]
    public class TranslatorTreeController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ITranslatorServices _translatorServices;
        private readonly IUtilsServices _utilsServices;

        public Localizer T { get; set; }

        public TranslatorTreeController(IOrchardServices orchardServices, ITranslatorServices translatorServices, IUtilsServices utilsServices)
        {
            _orchardServices = orchardServices;
            _translatorServices = translatorServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index(string selectedCulture, string selectedFolderName, string selectedFolderType)
        {
            TranslatorViewModel treeVM = new TranslatorViewModel();

            treeVM.CultureList = _translatorServices.GetCultureList();
            treeVM.selectedCulture = selectedCulture;
            treeVM.selectedFolderName = selectedFolderName;
            treeVM.selectedFolderType = selectedFolderType;

            return View(treeVM);
        }

        public JsonResult CreateJsonForTree(string language)
        {
            List<TranslationTreeNodeViewModel> tree = new List<TranslationTreeNodeViewModel>();

            if (!String.IsNullOrWhiteSpace(language))
            {
                tree.Add(new TranslationTreeNodeViewModel
                {
                    id = "translatortree-parent-M",
                    text = T("Modules").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Modules"), ElementToTranslate.Module),
                    data = new Dictionary<string, string>() { { "type", "M" } }
                });

                tree.Add(new TranslationTreeNodeViewModel
                {
                    id = "translatortree-parent-T",
                    text = T("Themes").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Themes"), ElementToTranslate.Theme),
                    data = new Dictionary<string, string>() { { "type", "T" } }
                });

                tree.Add(new TranslationTreeNodeViewModel {
                    id = "translatortree-parent-A",
                    text = T("Tenants").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Tenant"), ElementToTranslate.Tenant),
                    data = new Dictionary<string, string>() { { "type", "T" } }
                });
            }

            return Json(tree, JsonRequestBehavior.AllowGet);
        }

        private List<TranslationTreeNodeViewModel> CreateListForTree(string language, string parentFolder, ElementToTranslate elementType)
        {
            var translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();

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
            }

            // rimossa questa verifica in quanto se non presente la cartella su server (cosa possibile in quanto potrei voler tradurre un tema o un modulo non ancora deployato) non presenta il nodo da tradurre
            //var list = new List<string>(Directory.GetDirectories(parentFolder));
            //list = list.Select(dir => dir.Remove(0, dir.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToList();
            //list = list.Where(dir => elementsToTranslate.Any(x => x == dir)).ToList();
            //list.Sort((x, y) => string.Compare(x, y));

            var list = elementsToTranslate.Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
            list.Sort((x, y) => string.Compare(x, y)); 
            List<TranslationTreeNodeViewModel> treeList = new List<TranslationTreeNodeViewModel>();
            foreach (var item in list)
            {
                Dictionary<string, string> additionalData = new Dictionary<string, string>();

                int percent = GetCompletionPercent(language, item, folderType);
                if (percent < 0)
                {
                    //additionalData.Add("percent", T("N/D").ToString());
                    additionalData.Add("to_translate", "false");
                }
                else
                {
                    additionalData.Add("percent", GetCompletionPercent(language, item, folderType).ToString() + "%");
                    additionalData.Add("to_translate", "true");
                }

                additionalData.Add("type", folderType);

                string deprecatedType = "";
                if (IsDeprecated(item, folderType))
                    deprecatedType = "deprecated";

                treeList.Add(new TranslationTreeNodeViewModel { id = "translatortree-child-" + item.Replace('.','-'), text = item, data = additionalData, type = deprecatedType });
            }

            return treeList;
        }

        private int GetCompletionPercent(string language, string containerName, string containerType)
        {
            var translationCount = _translatorServices.GetTranslations().Where(t => t.Language == language
                                                                                 && t.ContainerName == containerName
                                                                                 && t.ContainerType == containerType)
                                                                        .AsParallel()
                                                                        .GroupBy(t => !String.IsNullOrWhiteSpace(t.TranslatedMessage))
                                                                        .Select(t => new { translated = t.Key, count = t.Count() });

            var countDictionary = translationCount.ToDictionary(g => g.translated, g => g.count);

            if (!countDictionary.ContainsKey(true))
                return !countDictionary.ContainsKey(false) ? -1 : 0;
            else
            {
                return !countDictionary.ContainsKey(false) ? 100 : (int)Math.Floor((double)countDictionary[true] / (countDictionary[true] + countDictionary[false]) * 100);
            }
        }

        private bool IsDeprecated(string containerName, string containerType) {
            var folderSettings = _translatorServices.GetTranslationFoldersSettings().Where(t => t.ContainerName == containerName && t.ContainerType == containerType).FirstOrDefault();

            return folderSettings == null ? false : folderSettings.Deprecated;
        }
    }
}