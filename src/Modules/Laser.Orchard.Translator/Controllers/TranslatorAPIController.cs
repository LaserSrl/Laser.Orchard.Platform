using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Orchard.Data;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Laser.Orchard.Translator.Controllers
{
    [WebApiKeyFilter(false)]
    public class TranslatorAPIController : ApiController
    {
        private readonly ITransactionManager _transactionManager;
        private readonly ITranslatorServices _translatorServices;
        public ILogger Log { get; set; }

        public TranslatorAPIController(ITransactionManager transactionManager, ITranslatorServices translatorServices)
        {
            _transactionManager = transactionManager;
            _translatorServices = translatorServices;
            Log = NullLogger.Instance;
        }

        [System.Web.Mvc.HttpPost]
        public bool AddRecords([FromBody] List<TranslationRecord> records)
        {
            try
            {
                if (records == null) {
                    Log.Error("TranslatorAPIController.AddRecords error - No data received in TranslationRecord list.");
                    return false;
                }
                if (records.Where(r => String.IsNullOrWhiteSpace(r.Message)
                                    || String.IsNullOrWhiteSpace(r.Language)
                                    || String.IsNullOrWhiteSpace(r.ContainerName)
                                    || String.IsNullOrWhiteSpace(r.ContainerType)).Any()) {
                    Log.Error("TranslatorAPIController.AddRecords error - TranslationRecord not valid. At least one of these field is empty: Message, Language, ContainerName and ContainerType. Please verify if T(\"\") is present in your code because it causes an empty Message.");
                    return false;
                }

                foreach (var record in records)
                {
                    var alreadyExistingRecords = _translatorServices.GetTranslations().Where(r => r.ContainerName == record.ContainerName
                                                                                              && r.ContainerType == record.ContainerType
                                                                                              && r.Context == record.Context
                                                                                              && r.Message == record.Message
                                                                                              && r.Language == record.Language);
                    var tryAddOrUpdateTranslation = true;
                    if (alreadyExistingRecords.Any())
                    {
                        // verifica maiuscole/minuscole del message
                        // aggiunto il for perchè nel caso in cui ci fosse più di una traduzione uguale 
                        // con differenza di maiuscolo o minuscole deve  effettuare il controllo su tutte
                        foreach (var item in alreadyExistingRecords)
                        {
                            if (record.Message.Equals(item.Message, StringComparison.InvariantCulture))
                            {
                                tryAddOrUpdateTranslation = false;
                                break;
                            }
                        }
                    }
                    if (tryAddOrUpdateTranslation)
                    {
                        bool success = _translatorServices.TryAddOrUpdateTranslation(record);
                        if (!success)
                        {
                            _transactionManager.Cancel();
                            Log.Error("TranslatorAPIController.AddRecords error - Id: {0}, Message: {1}", record.Id, record.Message);
                            return false;
                        }
                    }
                }

                var folderList = records.GroupBy(g => new { g.ContainerName, g.ContainerType })
                                            .Select(g => new { g.Key.ContainerName, g.Key.ContainerType });

                foreach (var folder in folderList)
                {
                    if (folder.ContainerType == "M")
                        _translatorServices.EnableFolderTranslation(folder.ContainerName, ElementToTranslate.Module);
                    else if (folder.ContainerType == "T")
                        _translatorServices.EnableFolderTranslation(folder.ContainerName, ElementToTranslate.Theme);
                }

                return true;
            }
            catch (Exception ex)
            {
                _transactionManager.Cancel();
                Log.Error(ex, "TranslatorAPIController.AddRecords error.");
                return false;
            }
        }
    }
}