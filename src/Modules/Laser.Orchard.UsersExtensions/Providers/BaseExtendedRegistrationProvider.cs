using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Fields.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public abstract class BaseExtendedRegistrationProvider : IExtendedRegistrationProvider {
        private IContentManager _contentManager;
        private ITaxonomyService _taxonomyService;

        public BaseExtendedRegistrationProvider(IContentManager contentManager,
            ITaxonomyService taxonomyService) {

            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        public virtual void FillCustomFields(HttpRequestBase request, JObject registeredData) {

        }

        public virtual void FillCustomField(ContentItem user, ContentField field, string value) {
            if (field != null) {
                switch (field.FieldDefinition.Name.ToLowerInvariant()) {
                    case "textfield":
                        ((TextField)field).Value = value;
                        break;

                    case "numericfield":
                        decimal numericValue = 0;
                        if (decimal.TryParse(value, out numericValue)) {
                            ((NumericField)field).Value = numericValue;
                        }
                        break;

                    case "taxonomyfield":
                        string[] values = value.Split(',');
                        var terms = new List<TermPart>();
                        foreach (var val in values) {
                            int intVal = 0;
                            if (int.TryParse(val, out intVal)) {
                                var term = _contentManager.Get(intVal);
                                if (term != null && term.Has<TermPart>()) {
                                    terms.Add(term.As<TermPart>());
                                }
                            }
                        }

                        _taxonomyService.UpdateTerms(user, terms, field.Name);

                        break;

                    default:
                        //((TextField)field).Value = request.Form[key.ToString()].ToString();
                        break;
                }
            }
        }

        public virtual int GetUserIdFromRegisteredData(JObject registeredData) {
            // registeredData json structure is the following:
            // "Data": {
            //  "Roles": [],
            //  "UserId": 80,
            //  "ContactId": 78
            // },
            var userId = registeredData["UserId"].ToString();

            var intId = 0;
            int.TryParse(userId, out intId);

            return intId;
        }
    }
}