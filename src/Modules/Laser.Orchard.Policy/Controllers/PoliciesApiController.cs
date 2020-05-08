using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Linq;
using System.Web.Http;

namespace Laser.Orchard.Policy.Controllers {
    [WebApiKeyFilter(false)]
    public class PoliciesApiController : ApiController {
        private readonly IPolicyServices _policyServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;

        public ILogger Log { get; set; }

        public PoliciesApiController(IOrchardServices orchardServices, IPolicyServices policyServices, ICsrfTokenHelper csrfTokenHelper, IUtilsServices utilsServices) {
            _policyServices = policyServices;
            _csrfTokenHelper = csrfTokenHelper;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
            _utilsServices = utilsServices;
        }

        public Localizer T { get; set; }

        // GET api/policiesapi
        public PoliciesApiModel Get(string lang) {
            PoliciesForUserViewModel model = _policyServices.GetPoliciesForCurrentUser(false, lang);

            return new PoliciesApiModel {
                Language = lang,
                PoliciesForUser = new SimplePoliciesForUserViewModel {
                    Policies = model.Policies.Select(s => new SimplePolicyForUserViewModel {
                        Accepted = s.Accepted,
                        AnswerDate = s.AnswerDate,
                        AnswerId = s.AnswerId,
                        OldAccepted = s.OldAccepted,
                        PolicyTextId = s.PolicyTextId,
                        UserId = s.UserId
                    }).ToList()
                }
            };
        }

        // POST api/policiesapi
        [System.Web.Mvc.HttpPost]
        public Response Post([FromBody]PoliciesApiModel policiesModel) {
            try {
                if ((_orchardServices.WorkContext.CurrentUser != null && _csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) || _orchardServices.WorkContext.CurrentUser == null) {
                    var fullModel = policiesModel.PoliciesForUser.Policies.Select(s => new PolicyForUserViewModel {
                        Accepted = s.Accepted,
                        AnswerDate = s.AnswerDate,
                        AnswerId = s.AnswerId,
                        OldAccepted = s.OldAccepted,
                        PolicyTextId = s.PolicyTextId,
                        PolicyText = null, // non serve per l'update è sufficinete l'ID
                        UserId = s.UserId
                    }).ToList();
                    _policyServices.PolicyForUserMassiveUpdate(fullModel);
                    return (_utilsServices.GetResponse(ResponseType.Success));// { Message = "Ok", Success = true });
                }
                else {
                    return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false });
                }
            }
            catch (Exception ex) {
                Log.Error("Policy -> PoliciesApi -> Post : " + ex.Message + " <Stack> " + ex.StackTrace);
                return (_utilsServices.GetResponse(ResponseType.None, "Error:" + ex.Message));
            }
        }

        // PUT api/policiesapi/5
        public void Put() {
        }

        // DELETE api/policiesapi/5
        public void Delete() {
        }
    }
}