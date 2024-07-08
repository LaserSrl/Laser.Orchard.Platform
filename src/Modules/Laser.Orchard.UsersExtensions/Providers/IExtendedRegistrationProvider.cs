using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public interface IExtendedRegistrationProvider : IDependency {
        void FillCustomFields(HttpRequestBase request, JObject registeredData);
        void FillCustomField(ContentItem user, ContentField field, string value);
        int GetUserIdFromRegisteredData(JObject registeredData);
    }
}
