using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.Servicies;
using Laser.Orchard.ShareLink.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc.Html;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.ShareLink.Handlers {

    public class ShareLinkHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IShareLinkService _sharelinkService;
        private readonly IEnumerable<IShareLinkPriorityProvider> _priorityProviders;

        private int processedPriority;

        public ShareLinkHandler(
            IRepository<ShareLinkPartRecord> repository,
            IOrchardServices orchardServices,
            IShareLinkService sharelinkService,
            IEnumerable<IShareLinkPriorityProvider> priorityProviders) {

            Filters.Add(StorageFilter.For(repository));
            _orchardServices = orchardServices;
            _sharelinkService = sharelinkService;
            _priorityProviders = priorityProviders;

            processedPriority = int.MinValue;

            OnGetDisplayShape<ShareLinkPart>((context, part) => {
                if (context.DisplayType == "Detail") {
                    _sharelinkService.FillPart(part); //GetPriority may use info from the part
                    var thisPriority = _priorityProviders.Any() ? _priorityProviders.Max(pp => pp.GetPriority(part)) : 0;
                    if (thisPriority > processedPriority) {
                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                        var description = part.SharedBody ?? "";
                        if (description.Length > 290)
                            description = TruncateAtWord(part.SharedBody ?? "", 290) + " ...";
                        var getpart = _orchardServices.WorkContext.CurrentSite.As<ShareLinkModuleSettingPart>();
                        string fbappid = "";
                        if (getpart != null)
                            fbappid = getpart.Fb_App;

                        var openGraphVM = new OpenGraphVM {
                            Title = part.SharedText,
                            Image = part.SharedImage,
                            Url = _orchardServices.WorkContext.CurrentSite.BaseUrl + urlHelper.ItemDisplayUrl(part.ContentItem),
                            Site_name = _orchardServices.WorkContext.CurrentSite.SiteName,
                            Description = description,
                            Fbapp_id = fbappid, //Your page will appear in the "Likes and Interests" section of the user's profile, and you have the ability to publish updates to the user
                            #region Twitter
                            TwitterTitle = part.SharedText,
                            TwitterDescription = part.SharedBody,
                            TwitterImage = part.SharedImage,
                            TwitterSite = _orchardServices.WorkContext.CurrentSite.BaseUrl
                            #endregion
                        };
                        //remove previous sharelink metas
                        var layout = (dynamic)context.Layout;
                        if (layout.Head?.Items != null) {
                            var headShapes = (List<object>)layout.Head.Items;
                            headShapes.RemoveAll(sha => ((Shape)sha)
                              .Metadata
                              .Type == "ShareLinkMeta");
                        }
                        layout.Head.Add(context.New.ShareLinkMeta(OpenGraphVM: openGraphVM));
                        processedPriority = thisPriority;
                    }
                    
                }
            });
        }


        private string TruncateAtWord(string value, int length) {
            if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
                return value;

            return value.Substring(0, value.IndexOf(" ", length));
        }
    }
}