using Newtonsoft.Json.Linq;
using Orchard;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Providers {
    public interface IExtendedRegistrationProvider : IDependency {
        void FillCustomFields(HttpRequestBase request, JObject registeredData);
        int GetUserIdFromRegisteredData(JObject registeredData);
    }
}
