using Newtonsoft.Json.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public class FormDataExtendedRegistrationProvider : IExtendedRegistrationProvider {
        public void FillCustomFields(HttpRequestBase request, JObject registeredData) {
            if (request.Form !=  null && request.Form.Keys.Count > 0) {

            }
        }


        public int GetUserIdFromRegisteredData(JObject registeredData) {
            // registeredData json structure is the following:
            // "Data": {
            //  "Roles": [],
            //  "UserId": 80,
            //  "ContactId": 78
            // },
            var userId = 0;



            return userId;
        }
    }
}