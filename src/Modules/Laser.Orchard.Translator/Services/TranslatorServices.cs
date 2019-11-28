using Laser.Orchard.Translator.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Laser.Orchard.Translator.Services {
    public interface ITranslatorServices : IDependency {
        IEnumerable<string> GetCultureList();
        IQueryable<TranslationRecord> GetTranslations();
        IQueryable<TranslationFolderSettingsRecord> GetTranslationFoldersSettings();
        IList<string> GetSuggestedTranslations(string message, string language);
        bool TryAddOrUpdateTranslation(TranslationRecord translation);
        bool TryAddOrUpdateTranslationFolderSettings(TranslationFolderSettingsRecord translation);
        void EnableFolderTranslation(string folderName, ElementToTranslate folderType);
        bool DeleteTranslation(TranslationRecord record);
        void DeleteAllTranslations();
    }

    public class TranslatorServices : ITranslatorServices {
        private readonly ICultureManager _cultureManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<TranslationRecord> _translationRecordRepository;
        private readonly IRepository<TranslationFolderSettingsRecord> _translationFoldersSettingsRecordRepository;

        public Localizer T { get; set; }
        public ILogger Log { get; set; }

        public TranslatorServices(ICultureManager cultureManager, IOrchardServices orchardServices, IRepository<TranslationRecord> translationRecordRepository, IRepository<TranslationFolderSettingsRecord> translationFoldersSettingsRecordRepository) {
            _cultureManager = cultureManager;
            _orchardServices = orchardServices;
            _translationRecordRepository = translationRecordRepository;
            _translationFoldersSettingsRecordRepository = translationFoldersSettingsRecordRepository;
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
        }

        public IEnumerable<string> GetCultureList() {
            //Lista completa da usare in produzione
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(c => c.Name).OrderBy(c => c);

            //Lista ridotta a scopo di test
            //return _cultureManager.ListCultures();
        }

        public IQueryable<TranslationRecord> GetTranslations() {
            return _translationRecordRepository.Table;
        }

        public IQueryable<TranslationFolderSettingsRecord> GetTranslationFoldersSettings() {
            return _translationFoldersSettingsRecordRepository.Table;
        }

        public bool TryAddOrUpdateTranslation(TranslationRecord translation) {
            try {
                AddOrUpdateTranslation(translation);
                return true;
            } catch (Exception ex) {
                Log.Error(ex, "TranslatorServices.TryAddOrUpdateTranslation error.");
                return false;
            }
        }

        public bool TryAddOrUpdateTranslationFolderSettings(TranslationFolderSettingsRecord translation) {
            try {
                AddOrUpdateTranslationFolderSettings(translation);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private void AddOrUpdateTranslation(TranslationRecord translation) {
            List<TranslationRecord> existingTranslations = new List<TranslationRecord>();
            bool searchById = translation.Id != 0;

            if (translation.TranslatedMessage != null)
                translation.TranslatedMessage = translation.TranslatedMessage.Trim();

            if (searchById) {
                existingTranslations = GetTranslations().Where(t => t.Id == translation.Id).ToList();
            } else {
                existingTranslations = GetTranslations().Where(t => t.Language == translation.Language
                                                                    && t.ContainerName == translation.ContainerName
                                                                    && t.ContainerType == translation.ContainerType
                                                                    && t.Context == translation.Context
                                                                    && t.Message == translation.Message).ToList();
            }

            var updateRecord = false;
            if (existingTranslations.Any())
            {
                // nel caso in cui dall'api controller viene richiesto l'inserimento o l'aggiornamento
                // faccio un ulteriore verifica maiuscole/minuscole del message
                foreach (var item in existingTranslations)
                {
                    if (translation.Message.Equals(item.Message, StringComparison.InvariantCulture))
                    {
                        updateRecord = true;
                        break;
                    }
                }
            }

            if (updateRecord)
            {
                TranslationRecord existingTranslation = existingTranslations.FirstOrDefault();

                existingTranslation.Context = translation.Context;
                existingTranslation.TranslatedMessage = translation.TranslatedMessage;
                existingTranslation.Message = translation.Message;  // #GM 2015-09-22

                _translationRecordRepository.Update(existingTranslation);
                _translationRecordRepository.Flush();
            } else {
                if (searchById) {
                    throw new Exception(T("The requested translation does not exists.").ToString());
                } else {
                    _translationRecordRepository.Create(translation);
                    _translationRecordRepository.Flush();
                }
            }
        }

        private void AddOrUpdateTranslationFolderSettings(TranslationFolderSettingsRecord translationSettings) {
            List<TranslationFolderSettingsRecord> existingSettings = new List<TranslationFolderSettingsRecord>();

            existingSettings = GetTranslationFoldersSettings().Where(t => t.ContainerName == translationSettings.ContainerName
                                                                  && t.ContainerType == translationSettings.ContainerType
                                                                  /*&& t.Language == translationSettings.Language*/).ToList();

            if (existingSettings.Any()) {
                TranslationFolderSettingsRecord existingFolderSettings = existingSettings.FirstOrDefault();

                existingFolderSettings.Deprecated = translationSettings.Deprecated;
                existingFolderSettings.OutputPath = translationSettings.OutputPath;
                _translationFoldersSettingsRecordRepository.Update(existingFolderSettings);
                _translationFoldersSettingsRecordRepository.Flush();
            } else {
                _translationFoldersSettingsRecordRepository.Create(translationSettings);
                _translationFoldersSettingsRecordRepository.Flush();
            }
        }

        public bool DeleteTranslation(TranslationRecord record) {
            try {
                _translationRecordRepository.Delete(record);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public void DeleteAllTranslations() {
            List<TranslationRecord> translations = GetTranslations().ToList();

            foreach (TranslationRecord translation in translations) {
                if (!String.IsNullOrEmpty(translation.Language))
                    _translationRecordRepository.Delete(translation);
            }
        }

        public void EnableFolderTranslation(string folderName, ElementToTranslate folderType)
        {
            var translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();
            // missing settings
            translatorSettings.ModulesToTranslate = translatorSettings.ModulesToTranslate ?? "";
            translatorSettings.ThemesToTranslate = translatorSettings.ThemesToTranslate ?? "";

            List<string> enabledFolders = new List<string>();
            if (folderType == ElementToTranslate.Module)
                enabledFolders = translatorSettings.ModulesToTranslate.Replace(" ", "").Split(',').ToList();
            else if (folderType == ElementToTranslate.Theme)
                enabledFolders = translatorSettings.ThemesToTranslate.Replace(" ", "").Split(',').ToList();

            if (!enabledFolders.Contains(folderName))
            {
                if (folderType == ElementToTranslate.Module)
                {
                    if (!String.IsNullOrWhiteSpace(translatorSettings.ModulesToTranslate))
                        translatorSettings.ModulesToTranslate += ",";

                    translatorSettings.ModulesToTranslate += folderName;
                }
                else if (folderType == ElementToTranslate.Theme)
                {
                    if (!String.IsNullOrWhiteSpace(translatorSettings.ThemesToTranslate))
                        translatorSettings.ThemesToTranslate += ",";

                    translatorSettings.ThemesToTranslate += folderName;
                }
            }
        }

        public IList<string> GetSuggestedTranslations(string message, string language) {
            return GetTranslations().Where(w => w.Message == message
                                             && w.Language == language
                                             && w.TranslatedMessage != null
                                             && w.TranslatedMessage != string.Empty)
                                    .Take(5)
                                    .Select(x => x.TranslatedMessage)
                                    .AsParallel()
                                    .Distinct()
                                    .ToList();
        }
    }
}