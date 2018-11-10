using Orchard;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Services {
    public interface IWebApiService : IDependency {
        ActionResult Terms(string alias, int maxLevel = 10);
        ActionResult Display(string alias, int page = 1, int pageSize = 10, int maxLevel = 10);
    }

}