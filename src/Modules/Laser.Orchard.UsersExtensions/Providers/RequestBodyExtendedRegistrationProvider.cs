using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Services;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public class RequestBodyExtendedRegistrationProvider : BaseExtendedRegistrationProvider {
        private readonly IContentManager _contentManager;

        public RequestBodyExtendedRegistrationProvider(IContentManager contentManager,
            ITaxonomyService taxonomyService) : base(contentManager, taxonomyService) {

            _contentManager = contentManager;
        }

        public override void FillCustomFields(HttpRequestBase request, JObject registeredData) {
            if (request.InputStream.Length > 0) {
                var user = _contentManager.Get(GetUserIdFromRegisteredData(registeredData));
                if (user != null) {
                    var allFields = user.Parts.SelectMany(pa => pa.Fields);
                    using (var reader = new StreamReader(request.InputStream)) {
                        // Force / reset the stream position
                        request.InputStream.Position = 0;
                        string respJson = reader.ReadToEnd();
                        var json = JObject.Parse(respJson);

                        // Request body is expected be in the following format:
                        // {
                        //    "Cognome": "Bianchi",
                        //    "KCal": "1000",
                        //    "RestrizioniAlimentari": "63,65"
                        // }
                        foreach (var p in json.Properties()) {
                            var field = allFields.FirstOrDefault(fi => fi.PartFieldDefinition.Name.Equals(p.Name.ToString(), StringComparison.OrdinalIgnoreCase));
                            FillCustomField(user, field, p.Value.ToString());
                        }
                    }

                }
            }
        }
    }
}