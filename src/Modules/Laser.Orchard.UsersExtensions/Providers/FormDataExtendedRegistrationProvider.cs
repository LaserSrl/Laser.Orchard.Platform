using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using System;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public class FormDataExtendedRegistrationProvider : IExtendedRegistrationProvider {
        private readonly IContentManager _contentManager;

        public FormDataExtendedRegistrationProvider(IContentManager contentManager) {
            _contentManager = contentManager;
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