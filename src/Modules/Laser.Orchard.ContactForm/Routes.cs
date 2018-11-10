using System.Collections.Generic;
using Orchard.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.ContactForm
{
    public class Routes : IRouteProvider
    {
        #region Implementation of IRouteProvider

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            yield return new RouteDescriptor
            {
                Priority = 15,
                Route = new Route(
                    "contact-form/send-email",
                    new RouteValueDictionary 
                    { 
                        {"area", "Laser.Orchard.ContactForm"},    
                        {"controller","contactform"},
                        {"action","sendcontactemail"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary
                    {
                        {"area", "Laser.Orchard.ContactForm"}
                    },
                    new MvcRouteHandler())
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var descriptor in GetRoutes())
                routes.Add(descriptor);
        }

        #endregion
    }
}