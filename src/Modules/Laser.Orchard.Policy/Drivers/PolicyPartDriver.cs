using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.OutputCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.Policy.Drivers
{
    public class PolicyPartDriver : ContentPartCloningDriver<PolicyPart>, ICachingEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IPolicyServices _policyServices;
        private readonly ICurrentContentAccessor _currentContentAccessor;

        private string _additionalCacheKey;

        public PolicyPartDriver(IHttpContextAccessor httpContextAccessor,
                                IWorkContextAccessor workContextAccessor,
                                IControllerContextAccessor controllerContextAccessor,
                                IContentManager contentManager,
                                ICurrentContentAccessor currentContentAccessor,
                                IPolicyServices policyServices) {
            _httpContextAccessor = httpContextAccessor;
            _workContextAccessor = workContextAccessor;
            _controllerContextAccessor = controllerContextAccessor;
            _contentManager = contentManager;
            _policyServices = policyServices;
            _currentContentAccessor = currentContentAccessor;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Policy"; }
        }
        protected override DriverResult Display(PolicyPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                if (_policyServices.HasPendingPolicies(part.ContentItem) ?? false) {
                    var language = _workContextAccessor.GetContext().CurrentCulture;
                    UrlHelper url = new UrlHelper(_httpContextAccessor.Current().Request.RequestContext);

                    var associatedPolicies = _policyServices.GetPoliciesForContent(part);
                    var encodedAssociatedPolicies = "";
                    if (associatedPolicies != null)
                        encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", associatedPolicies)));
                    else
                        encodedAssociatedPolicies = Convert.ToBase64String(Encoding.UTF8.GetBytes(""));

                    var fullUrl = url.Action("Index", "Policies", new { area = "Laser.Orchard.Policy",
                                                                        lang = language,
                                                                        policies = encodedAssociatedPolicies,
                                                                        returnUrl = _httpContextAccessor.Current().Request.RawUrl,
                                                                        alias = part.As<AutoroutePart>()!=null? part.As<AutoroutePart>().DisplayAlias:""});
                    var cookie = _httpContextAccessor.Current().Request.Cookies["PoliciesAnswers"];
                    if (cookie != null && cookie.Value != null) {
                        _httpContextAccessor.Current().Response.Cookies.Add(_httpContextAccessor.Current().Request.Cookies["PoliciesAnswers"]);
                    }
                    _httpContextAccessor.Current().Response.Redirect(fullUrl, true);
                }
                else {
                }
            }
            else if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_Policy_SummaryAdmin",
                     () => shapeHelper.Parts_Policy_SummaryAdmin(IncludePendingPolicy: part.IncludePendingPolicy));

            }
            return null;
        }

        protected override DriverResult Editor(PolicyPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Policy_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/Policy_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }
        protected override DriverResult Editor(PolicyPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("PolicyPartError", T("PolicyPart Error"));
            }
            return Editor(part, shapeHelper);
        }



        #region [ Import/Export ]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="context"></param>
        protected override void Exporting(PolicyPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("IncludePendingPolicy", part.IncludePendingPolicy);

            List<string> PolicyTextReferencesIdentities = new List<string>();
            if (part.PolicyTextReferences != null) {
                if (part.PolicyTextReferences.Contains("{All}"))
                    PolicyTextReferencesIdentities.Add("{All}");
                else {
                    foreach (string PolicyTextReference in part.PolicyTextReferences) {
                        int PolicyTextReferenceId = 0;
                        if (Int32.TryParse(PolicyTextReference.TrimStart('{').TrimEnd('}'), out PolicyTextReferenceId)) {
                            var contentItem = _contentManager.Get(PolicyTextReferenceId);
                            if (contentItem != null) {
                                var containerIdentity = _contentManager.GetItemMetadata(contentItem).Identity;
                                PolicyTextReferencesIdentities.Add(containerIdentity.ToString());
                            }
                        }
                    }
                }
            }

            root.SetAttributeValue("PolicyTextReferencesCsv", String.Join(",", PolicyTextReferencesIdentities.ToArray()));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="context"></param>
        protected override void Importing(PolicyPart part, ImportContentContext context) {

            var root = context.Data.Element(part.PartDefinition.Name);
            var includePendingPolicy = IncludePendingPolicyOptions.Yes;

            List<string> policyTextReferencesList = new List<string>();
            var policyTextReferencesIdentities = root.Attribute("PolicyTextReferencesCsv").Value;

            if (policyTextReferencesIdentities != null) {
                if (policyTextReferencesIdentities.Contains("{All}"))
                    policyTextReferencesList.Add("{All}");
                else {
                    foreach (string policyTextReferencesIdentity in policyTextReferencesIdentities.Split(',')) {
                        var contentItem = context.GetItemFromSession(policyTextReferencesIdentity);
                        if (contentItem != null)
                            policyTextReferencesList.Add("{" + contentItem.Id + "}");
                    }
                }
            }

            Enum.TryParse<IncludePendingPolicyOptions>(root.Attribute("IncludePendingPolicy").Value, out includePendingPolicy);
            part.IncludePendingPolicy = includePendingPolicy;
            part.PolicyTextReferencesCsv = String.Join(",", policyTextReferencesList.ToArray());
        }
        #endregion

        /// <summary>
        /// Called by OutputCache after the default cache key has been defined.
        /// </summary>
        /// <param name="key">Default cache key such as defined in Orchard.OutputCache.</param>
        public void KeyGenerated(StringBuilder key) {
            var part = _currentContentAccessor.CurrentContentItem.As<PolicyPart>();
            if (part == null) return;

            if (_policyServices.HasPendingPolicies(part.ContentItem) ?? false) {
                _additionalCacheKey = "policy-not-accepted;";
                _additionalCacheKey += "pendingitempolicies=" + String.Join("_", _policyServices.PendingPolicies(part.ContentItem).Select(s => s.Id)) + ";";
            }
            else {
                _additionalCacheKey = "policy-accepted;";
            }

            key.Append(_additionalCacheKey);
        }

        protected override void Cloning(PolicyPart originalPart, PolicyPart clonePart, CloneContentContext context) {
            clonePart.IncludePendingPolicy = originalPart.IncludePendingPolicy;
            clonePart.PolicyTextReferencesCsv = originalPart.PolicyTextReferencesCsv;
        }
    }
}