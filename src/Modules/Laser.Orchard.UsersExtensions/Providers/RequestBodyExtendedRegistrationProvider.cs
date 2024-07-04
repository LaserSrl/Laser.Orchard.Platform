using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public class RequestBodyExtendedRegistrationProvider : IExtendedRegistrationProvider {
        private readonly IContentManager _contentManager;

        public RequestBodyExtendedRegistrationProvider(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void FillCustomFields(HttpRequestBase request, JObject registeredData) {
            if (request.InputStream.Length > 0) {
                var user = _contentManager.Get(GetUserIdFromRegisteredData(registeredData));
                if (user != null) {
                    var allFields = user.Parts.SelectMany(pa => pa.Fields);
                    using (var reader = new StreamReader(request.InputStream)) {
                        // Force / reset the stream position
                        request.InputStream.Position = 0;
                        string respJson = reader.ReadToEnd();
                        var json = JObject.Parse(respJson);

                        for (int i = 0; i <= json.Count - 1; i++) {
                            //var t = json[i];
                            
                        }

                        //foreach (var t in json.ChildrenTokens) {
                        //    var field = allFields.FirstOrDefault(fi => fi.PartFieldDefinition.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase));
                        //    if (field != null) {
                        //        switch (field.FieldDefinition.Name) {
                        //            default:
                        //                ((TextField)field).Value = t.Value;
                        //                break;
                        //        }
                        //    }
                        //}
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