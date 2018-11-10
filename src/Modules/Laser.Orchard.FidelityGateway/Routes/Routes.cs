using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.FidelityGateway.Routes
{
    public class Routes : IHttpRouteProvider
    {

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (RouteDescriptor routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                  new HttpRouteDescriptor {
                Name = "Test",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/Test/{optional}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "Test"
                }
            },

            
            new HttpRouteDescriptor {
                Name = "CustomerRegistration",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CustomerRegistration/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CustomerRegistration"
                }
            },
         
              new HttpRouteDescriptor {
                Name = "CustomerRegistrationInDefaultCampaign",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CustomerRegistrationInDefaultCampaign",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CustomerRegistrationInDefaultCampaign"
                }
            },
        
             new HttpRouteDescriptor {
                Name = "CustomerDetails",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CustomerDetails",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CustomerDetails"
                }
            },

             new HttpRouteDescriptor {
                Name = "CustomerDetailsFromId",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CustomerDetailsFromId/{customerId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CustomerDetailsFromId"
                }
            },

             new HttpRouteDescriptor {
                Name = "CampaignList",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CampaignList",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CampaignList"
                }
            },

            new HttpRouteDescriptor {
                Name = "GetCampaignData",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GetCampaignData/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GetCampaignData"
                }
            },

            new HttpRouteDescriptor {
                Name = "GetDefaultCampaignData",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GetDefaultCampaignData",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GetDefaultCampaignData"
                }
            },

            new HttpRouteDescriptor {
                Name = "AddPoints",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPoints/{amount}/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPoints"
                }
            },

             new HttpRouteDescriptor {
                Name = "AddPointsInDefaultCampaign",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPointsInDefaultCampaign/{amount}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPointsInDefaultCampaign"
                }
            },

            new HttpRouteDescriptor {
                Name = "GiveReward",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GiveReward/{rewardId}/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GiveReward"
                }
            },

             new HttpRouteDescriptor {
                Name = "GiveRewardInDefaultCampaign",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GiveRewardInDefaultCampaign/{rewardId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GiveRewardInDefaultCampaign"
                }
            },
         

            new HttpRouteDescriptor {
                Name = "GiveRewardToCustomerInDefaultCampaign",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GiveRewardToCustomerInDefaultCampaign/{rewardId}/{customerId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GiveRewardToCustomerInDefaultCampaign"
                }
            },

            new HttpRouteDescriptor {
                Name = "GiveRewardToCustomer",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GiveRewardToCustomer/{rewardId}/{campaignId}/{customerId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GiveRewardToCustomer"
                }
            },

            new HttpRouteDescriptor {
                Name = "AddPointsFromActionToCustomer",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPointsFromActionToCustomer/{actionId}/{customerId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPointsFromActionToCustomer"
                }
            },

            new HttpRouteDescriptor {
                Name = "AddPointsToCustomerInDefaultCampaign",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPointsToCustomerInDefaultCampaign/{amount}/{customerId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPointsToCustomerInDefaultCampaign"
                }
            },

              new HttpRouteDescriptor {
                Name = "AddPointsToCustomer",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPointsToCustomer/{amount}/{campaignId}/{customerId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPointsToCustomer"
                }
            },

            new HttpRouteDescriptor {
                Name = "GetActions",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GetActions",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GetActions"
                }
            },

             new HttpRouteDescriptor {
                Name = "AddPointsFromAction",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPointsFromAction/{actionId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPointsFromAction"
                }
            }
        };
        }
    }
}