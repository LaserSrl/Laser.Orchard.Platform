using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Fields.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public class FormDataExtendedRegistrationProvider : IExtendedRegistrationProvider {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public FormDataExtendedRegistrationProvider(IContentManager contentManager,
            ITaxonomyService taxonomyService) {

            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        public void FillCustomFields(HttpRequestBase request, JObject registeredData) {
            if (request.Form != null && request.Form.Keys.Count > 0) {
                var user = _contentManager.Get(GetUserIdFromRegisteredData(registeredData));
                if (user != null) {

                    var allFields = user.Parts.SelectMany(pa => pa.Fields);

                    foreach (var key in request.Form.Keys) {
                        var field = allFields.FirstOrDefault(fi => fi.PartFieldDefinition.Name.Equals(key.ToString(), StringComparison.OrdinalIgnoreCase));

                        if (field != null) {
                            switch (field.FieldDefinition.Name.ToLowerInvariant()) {
                                case "textfield":
                                    ((TextField)field).Value = request.Form[key.ToString()].ToString();
                                    break;

                                case "numericfield":
                                    decimal numericValue = 0;
                                    if (decimal.TryParse(request.Form[key.ToString()].ToString(), out numericValue)) {
                                        ((NumericField)field).Value = numericValue;
                                    }
                                    break;

                                case "contentpickerfield":

                                    break;

                                case "taxonomyfield":
                                    string[] values = request.Form[key.ToString()].ToString().Split(',');
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
                }
            }
        }


        public int GetUserIdFromRegisteredData(JObject registeredData) {
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