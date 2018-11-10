using Ionic.Zip;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers
{
    public class ImportExportController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ITranslatorServices _translatorServices;
        private readonly IUtilsServices _utilsServices;

        private const string pattern = "msgctxt\\s+\"([^\\s]+)\"\\s+msgid\\s+\"([^\"]*)\"\\s+msgstr\\s+\"([^\"]*)\"";

        public ImportExportController(IOrchardServices orchardServices, ITranslatorServices translatorServices, IUtilsServices utilsServices)
        {
            _orchardServices = orchardServices;
            _translatorServices = translatorServices;
            _utilsServices = utilsServices;
        }

        public ActionResult ImportTranslations()
        {
            _translatorServices.DeleteAllTranslations();

            var translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();

            List<string> modulesToTranslate = translatorSettings.ModulesToTranslate.Replace(" ", "").Split(',').ToList();
            List<string> themesToTranslate = translatorSettings.ThemesToTranslate.Replace(" ", "").Split(',').ToList();

            if (modulesToTranslate.Any())
                ImportFromPO(modulesToTranslate, ElementToTranslate.Module);

            if (themesToTranslate.Any())
                ImportFromPO(themesToTranslate, ElementToTranslate.Theme);

            string returnUrl = this.Request.UrlReferrer.AbsolutePath;
            return Redirect(returnUrl);
        }

        public ActionResult ExportTranslations()
        {
            using (ZipFile zip = new ZipFile())
            {
                var filename = "ExportTranslations_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".zip";

                Response.Clear();
                Response.BufferOutput = false;
                Response.ContentType = "application/zip";
                Response.AppendHeader("content-disposition", "attachment; filename=" + filename);

                var deprecatedFolders = _translatorServices.GetTranslationFoldersSettings().Where(m => m.Deprecated).Select(s => new { s.ContainerName, s.ContainerType/*, s.Language*/ });

                var messagesToExport = _translatorServices.GetTranslations().Where(m => m.TranslatedMessage != null
                                                                                     && m.TranslatedMessage != string.Empty);

                var foldersToExport = messagesToExport.GroupBy(f => new { f.ContainerName, f.ContainerType, f.Language })
                                                      .Select(g => new { g.Key.ContainerName, g.Key.ContainerType, g.Key.Language });

                foreach (var folder in foldersToExport) {
                    if (!deprecatedFolders.Where(item => /*item.Language == folder.Language &&*/ item.ContainerName == folder.ContainerName && item.ContainerType == folder.ContainerType).Any()) {

                        var folderMessages = messagesToExport.Where(m => m.ContainerName == folder.ContainerName
                                                                      && m.ContainerType == folder.ContainerType
                                                                      && m.Language == folder.Language)
                                                             .OrderBy(m => m.Context).ThenBy(m => m.Message);

                        MemoryStream stream = new MemoryStream();
                        StreamWriter streamWriter = new StreamWriter(stream);

                        streamWriter.WriteLine("# Orchard resource strings - " + folder.Language);
                        streamWriter.WriteLine("# Copyright (c) " + DateTime.Now.Year + " Laser s.r.l.");
                        streamWriter.WriteLine(Environment.NewLine);

                        streamWriter.WriteLine("# > #: msgctxt { contesto del messaggio - Originale }");
                        streamWriter.WriteLine("# > #| msgid { identificativo del messaggio - Originale }");
                        streamWriter.WriteLine("# > msgctxt  \"{ contesto del messaggio }\"");
                        streamWriter.WriteLine("# > msgid \"{ identificativo del messaggio }\"");
                        streamWriter.WriteLine("# > msgstr \"{ messaggio }\"");
                        streamWriter.Write(Environment.NewLine);

                        foreach (var message in folderMessages) {
                            streamWriter.WriteLine("msgctxt \"" + message.Context + "\"");
                            streamWriter.WriteLine("msgid \"" + message.Message + "\"");
                            streamWriter.WriteLine("msgstr \"" + message.TranslatedMessage + "\"");
                            streamWriter.Write(Environment.NewLine);
                        }

                        streamWriter.Flush();
                        stream.Seek(0, SeekOrigin.Begin);

                        string parentFolder = "";
                        string fileName = "";

                        if (folder.ContainerType == "M") {
                            parentFolder = "Modules";
                            fileName = "orchard.module.po";
                        } else if (folder.ContainerType == "T") {
                            parentFolder = "Themes";
                            fileName = "orchard.theme.po";
                        }

                        if (!String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(parentFolder))
                            zip.AddEntry(parentFolder + "/" + folder.ContainerName + "/App_Data/Localization/" + folder.Language + "/" + fileName, stream);
                    }
                }

                zip.Save(Response.OutputStream);
            }

            Response.Flush();
            Response.End();

            return new EmptyResult();
        }

        private void ImportFromPO(List<string> foldersToImport, ElementToTranslate type)
        {
            string parentFolder = "";
            string fileName = "";

            if (type == ElementToTranslate.Module)
            {
                parentFolder = "Modules";
                fileName = "orchard.module.po";
            }
            else if (type == ElementToTranslate.Theme)
            {
                parentFolder = "Themes";
                fileName = "orchard.theme.po";
            }
            else
                return;

            foreach (var folder in foldersToImport)
            {
                var path = Path.Combine(_utilsServices.TenantPath, parentFolder, folder, "App_Data", "Localization");
                if (Directory.Exists(path))
                {
                    var languages = Directory.GetDirectories(path).Select(d => new DirectoryInfo(d).Name);
                    foreach (var language in languages)
                    {
                        var filePath = Path.Combine(path, language, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            string fileContent = System.IO.File.ReadAllText(filePath);
                            foreach (Match match in Regex.Matches(fileContent, pattern, RegexOptions.IgnoreCase))
                            {
                                TranslationRecord translation = new TranslationRecord();

                                translation.ContainerName = folder;

                                if (type == ElementToTranslate.Module)
                                    translation.ContainerType = "M";
                                else if (type == ElementToTranslate.Theme)
                                    translation.ContainerType = "T";

                                translation.Context = match.Groups[1].Value;
                                translation.Message = match.Groups[2].Value;
                                translation.TranslatedMessage = match.Groups[3].Value;
                                translation.Language = language;

                                _translatorServices.TryAddOrUpdateTranslation(translation);
                            }
                        }
                    }
                }
            }
        }
    }
}