using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Orchard.Data;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace Laser.Orchard.Translator.Controllers {
    public class MessageTranslatorAPIController : ApiController {
        private readonly ITransactionManager _transactionManager;
        private readonly ITranslatorServices _translatorServices;
        public ILogger Log { get; set; }

        public MessageTranslatorAPIController(ITransactionManager transactionManager, ITranslatorServices translatorServices) {
            _transactionManager = transactionManager;
            _translatorServices = translatorServices;
            Log = NullLogger.Instance;
        }

        private readonly string[] _validContainerTypes = new string[] { "A", "M", "T", "U", "W", "X", "Y", "Z" };
        [System.Web.Mvc.HttpPost, ActionName("AddTranslatedRecords")]
        public string AddTranslatedRecords([FromBody] List<TranslationRecord> records) {
            var result = new StringBuilder();

            try {
                if (records == null) {
                    Log.Error("TranslatorAPIController.AddTranslatedRecords error - No data received in TranslationRecord list.");
                    result.AppendLine("TranslatorAPIController.AddTranslatedRecords error - No data received in TranslationRecord list.");
                    return result.ToString();
                }
                if (records.Where(r => string.IsNullOrWhiteSpace(r.Message)
                                    || string.IsNullOrWhiteSpace(r.Language)
                                    || string.IsNullOrWhiteSpace(r.ContainerName)
                                    || string.IsNullOrWhiteSpace(r.ContainerType)
                                    || string.IsNullOrWhiteSpace(r.TranslatedMessage)).Any()) {
                    Log.Error("TranslatorAPIController.AddTranlsatedRecords error - TranslationRecord not valid. At least one of these field is empty: Message, Language, ContainerName, ContainerType and TranslatedMessage. Please verify if T(\"\") is present in your code because it causes an empty Message.");
                    result.AppendLine("TranslatorAPIController.AddTranlsatedRecords error - TranslationRecord not valid. At least one of these field is empty: Message, Language, ContainerName, ContainerType and TranslatedMessage. Please verify if T(\"\") is present in your code because it causes an empty Message.");
                    return result.ToString();
                }

                foreach (var record in records) {
                    var alreadyExistingRecords = _translatorServices.GetTranslations().Where(r => r.ContainerName == record.ContainerName
                                                                                              && r.ContainerType == record.ContainerType
                                                                                              && r.Context == record.Context
                                                                                              && r.Message == record.Message
                                                                                              && r.Language == record.Language);
                    var tryAddOrUpdateTranslation = true;
                    if (alreadyExistingRecords.Any()) {
                        // verifica maiuscole/minuscole del message
                        // aggiunto il for perchè nel caso in cui ci fosse più di una traduzione uguale 
                        // con differenza di maiuscolo o minuscole deve  effettuare il controllo su tutte
                        // devo inoltre verificare che non sia già presente una traduzione
                        foreach (var item in alreadyExistingRecords) {
                            if (record.Message.Equals(item.Message, StringComparison.InvariantCulture) && !string.IsNullOrWhiteSpace(item.TranslatedMessage)) {
                                // Append to result there already is the translated message.
                                result.AppendLine(string.Format("The message '{0}' already has a translation, which isn't going to be overwritten.", record.Message));
                                tryAddOrUpdateTranslation = false;
                                break;
                            }
                        }
                    }
                    if (tryAddOrUpdateTranslation) {
                        bool success = _translatorServices.TryAddOrUpdateTranslation(record, false);
                        if (!success) {
                            _transactionManager.Cancel();
                            Log.Error("TranslatorAPIController.AddRecords error - Id: {0}, Message: {1}", record.Id, record.Message);
                            result.AppendLine(string.Format("TranslatorAPIController.AddRecords error - Id: {0}, Message: {1}", record.Id, record.Message));
                            return result.ToString();
                        }
                    }
                }

                var folderList = records.GroupBy(g => new { g.ContainerName, g.ContainerType })
                                            .Select(g => new { g.Key.ContainerName, g.Key.ContainerType });

                if (folderList.Any(f => !_validContainerTypes.Contains(f.ContainerType))) {
                    Log.Error("TranslatorAPIController.AddTranslatedRecords error - Some record has invalid ContainerType");
                    result.AppendLine("TranslatorAPIController.AddTranslatedRecords error - Some record has invalid ContainerType");
                    return result.ToString();
                }
                foreach (var folder in folderList) {
                    var folderType = ElementToTranslate.Module;
                    switch (folder.ContainerType) {
                        case "M":
                            folderType = ElementToTranslate.Module;
                            break;
                        case "T":
                            folderType = ElementToTranslate.Theme;
                            break;
                        case "A":
                            folderType = ElementToTranslate.Tenant;
                            break;
                        case "U":
                            folderType = ElementToTranslate.Undefined;
                            break;
                        case "W":
                            folderType = ElementToTranslate.OrchardModule;
                            break;
                        case "X":
                            folderType = ElementToTranslate.OrchardTheme;
                            break;
                        case "Y":
                            folderType = ElementToTranslate.OrchardCore;
                            break;
                        case "Z":
                            folderType = ElementToTranslate.OrchardFramework;
                            break;
                    }
                    _translatorServices.EnableFolderTranslation(folder.ContainerName, folderType);

                }

                result.AppendLine("AddTranslatedRecords completed.");
                return result.ToString();
            } catch (Exception ex) {
                _transactionManager.Cancel();
                Log.Error(ex, "TranslatorAPIController.AddTranslatedRecords error.");
                result.AppendLine("TranslatorAPIController.AddTranslatedRecords exception.");
                result.AppendLine(ex.ToString());
                return result.ToString();
            }
        }
    }
}