using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard;
using Laser.Orchard.Mobile.Models;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Laser.Orchard.Mobile.ViewModels;
using System.ServiceModel.Channels;

namespace Laser.Orchard.Mobile.Services
{

    public interface IManifestAppFileServices : IDependency
    {
        ManifestAppFileRecord Get();

        Tuple<bool, IEnumerable<string>> Save(ManifestAppFileViewModel vieModel);
    }

    public class ManifestAppFileServices : IManifestAppFileServices
    {
        private readonly IRepository<ManifestAppFileRecord> _repository;
        private readonly ISignals _signals;

        public ManifestAppFileServices(IRepository<ManifestAppFileRecord> repository, ISignals signals)
        {
            _repository = repository;
            _signals = signals;
        }

        public ManifestAppFileRecord Get()
        {
            var manifestAppFileRecord = _repository.Table.FirstOrDefault();
            if (manifestAppFileRecord == null)
            {
                manifestAppFileRecord = new ManifestAppFileRecord() {
                    FileContent = "",
                    Enable = false,
                    GoogleFileContent = "",
                    GoogleEnable = false,
                    DeveloperDomainText = "",
                    EnableDeveloperDomain = false
                };
                _repository.Create(manifestAppFileRecord);
            }
            return manifestAppFileRecord;
        }

        public Tuple<bool, IEnumerable<string>> Save(ManifestAppFileViewModel vieModel)
        {
            var validationResult = Validate(vieModel);
            if (validationResult.Item1)
            {
                var manifestAppFileRecord = Get();
                manifestAppFileRecord.FileContent = vieModel.Text;
                manifestAppFileRecord.Enable = vieModel.Enable;
                manifestAppFileRecord.GoogleFileContent = vieModel.GoogleText;
                manifestAppFileRecord.GoogleEnable = vieModel.GoogleEnable;
                manifestAppFileRecord.DeveloperDomainText = vieModel.DeveloperDomainText;
                manifestAppFileRecord.EnableDeveloperDomain = vieModel.EnableDeveloperDomain;
                _signals.Trigger("ManifestAppFile.SettingsChanged");
            }
            return validationResult;
        }

        private Tuple<bool, IEnumerable<string>> Validate(ManifestAppFileViewModel vieModel)
        {

            if (String.IsNullOrEmpty(vieModel.Text) && String.IsNullOrEmpty(vieModel.GoogleText))
            {
                return new Tuple<bool, IEnumerable<string>>(true, new List<string>() { });
            }

            string schemaJson = @"{ }";
            JSchema schema = JSchema.Parse(schemaJson);
            var isValid = true;
            var messages = new List<string>();
            if (!String.IsNullOrEmpty(vieModel.Text))
            {
                try
                {
                    isValid = isValid && JsonValidate(vieModel.Text, messages);
                }
                catch {
                    isValid = false;
                }
            }
            if (!String.IsNullOrEmpty(vieModel.GoogleText))
            {
                try
                {
                    isValid = isValid && JsonValidate(vieModel.GoogleText, messages);
                }
                catch
                {
                    isValid = false;
                }
            }

            return new Tuple<bool, IEnumerable<string>>(isValid, messages);

        }

        private bool JsonValidate(string jsonString, List<string> messages)
        {
            string schemaJson = @"{ }";
            JSchema schema = JSchema.Parse(schemaJson);
            try
            {
                JToken jText = JToken.Parse(jsonString);
                IList<string> message;
                return jText.IsValid(schema, out message);
            }
            catch (JsonReaderException ex)
            {
                messages.Add(ex.Message);
                return false;
            }

        }

    }
}