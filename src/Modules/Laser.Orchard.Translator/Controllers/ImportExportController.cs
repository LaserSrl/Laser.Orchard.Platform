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
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers {
    public class ImportExportController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ITranslatorServices _translatorServices;
        private readonly IUtilsServices _utilsServices;

        private const string pattern = "msgctxt\\s+\"([^\\s]+)\"\\s+msgid\\s+\"([^\"]*)\"\\s+msgstr\\s+\"([^\"]*)\"";

        public ImportExportController(IOrchardServices orchardServices, ITranslatorServices translatorServices, IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _translatorServices = translatorServices;
            _utilsServices = utilsServices;
        }

        public ActionResult ExportTranslations() {
            using (ZipFile zip = new ZipFile()) {
                var zipFileName = "ExportTranslations_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".zip";

                Response.Clear();
                Response.BufferOutput = false;
                Response.ContentType = "application/zip";
                Response.AppendHeader("content-disposition", "attachment; filename=" + zipFileName);
                var settings = _translatorServices.GetTranslationFoldersSettings();
                var deprecatedFolders = settings.Where(m => m.Deprecated)
                    .Select(s => new { s.ContainerName, s.ContainerType/*, s.Language*/ });

                var messagesToExport = _translatorServices.GetTranslations()
                    .Where(m => m.TranslatedMessage != null
                        && m.TranslatedMessage != string.Empty);

                var foldersToExport = messagesToExport
                    .GroupBy(f => new { f.ContainerName, f.ContainerType, f.Language })
                    .Select(g => new { g.Key.ContainerName, g.Key.ContainerType, g.Key.Language });

                foreach (var folder in foldersToExport) {
                    if (!deprecatedFolders
                        .Where(item => /*item.Language == folder.Language &&*/
                            item.ContainerName == folder.ContainerName
                            && item.ContainerType == folder.ContainerType).Any()) {

                        var settingsForFolder = settings
                            .SingleOrDefault(item => item.ContainerName == folder.ContainerName && item.ContainerType == folder.ContainerType);
                        var folderMessages = messagesToExport
                            .Where(m => m.ContainerName == folder.ContainerName
                                && m.ContainerType == folder.ContainerType
                                && m.Language == folder.Language)
                            .OrderBy(m => m.Context).ThenBy(m => m.Message);

                        MemoryStream stream = new MemoryStream();
                        StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
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
                        string outputPath = "Sources";
                        var localizationFolderBase = Path.Combine("App_Data", "Localization");
                        switch (folder.ContainerType) {
                            case "M":
                                parentFolder = Path.Combine("Deploy", "Modules");
                                fileName = "orchard.module.po";
                                break;
                            case "T":
                                parentFolder = Path.Combine("Deploy", "Themes");
                                fileName = "orchard.theme.po";
                                break;
                            case "A":
                                parentFolder = Path.Combine("Deploy", "App_Data", "Sites");
                                fileName = "orchard.po";
                                localizationFolderBase = "Localization";
                                break;
                            case "W":   // Orchard modules
                                parentFolder = Path.Combine("Deploy", "Modules");
                                fileName = "orchard.module.po";
                                break;
                            case "X":   // Orchard themes
                                parentFolder = Path.Combine("Deploy", "Themes");
                                fileName = "orchard.theme.po";
                                break;
                            case "Y":   // Orchard core
                                parentFolder = Path.Combine("Deploy", "Core");
                                fileName = "orchard.core.po";
                                break;
                            case "Z":   // Orchard framework
                                parentFolder = "Deploy";
                                fileName = "orchard.root.po";
                                break;
                        }

                        if (settingsForFolder != null && !string.IsNullOrWhiteSpace(settingsForFolder.OutputPath)) {
                            outputPath = Path.Combine(
                                outputPath,
                                settingsForFolder.OutputPath.StartsWith("/")
                                    ? settingsForFolder.OutputPath.Substring(1)
                                    : settingsForFolder.OutputPath);
                        }
                        if (!String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(parentFolder)) {
                            StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                            var finalContent = streamReader.ReadToEnd();
                            // Special cases for Orchard Core and Orchard Framework containers: they don't require the ContainerName subfolder.
                            if (folder.ContainerType == "Y" || folder.ContainerType == "Z") {
                                zip.AddEntry(
                                    Path.Combine(
                                        parentFolder,
                                        localizationFolderBase,
                                        folder.Language,
                                        fileName),
                                    finalContent, Encoding.UTF8);
                                zip.AddEntry(
                                    Path.Combine(
                                        outputPath,
                                        localizationFolderBase,
                                        folder.Language,
                                        fileName),
                                    finalContent, Encoding.UTF8);
                            } else {
                                zip.AddEntry(
                                    Path.Combine(
                                        parentFolder,
                                        folder.ContainerName,
                                        localizationFolderBase,
                                        folder.Language,
                                        fileName),
                                    finalContent, Encoding.UTF8);
                                zip.AddEntry(
                                    Path.Combine(
                                        outputPath,
                                        folder.ContainerName,
                                        localizationFolderBase,
                                        folder.Language,
                                        fileName),
                                    finalContent, Encoding.UTF8);
                            }

                            streamReader.Dispose();
                        }
                        streamWriter.Dispose();
                        stream.Dispose();
                    }
                }

                zip.Save(Response.OutputStream);
            }

            Response.Flush();
            Response.End();

            return new EmptyResult();
        }
    }
}