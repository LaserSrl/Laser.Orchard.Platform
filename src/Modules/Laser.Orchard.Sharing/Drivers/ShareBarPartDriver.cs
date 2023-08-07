using System;

using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Mvc;
using Orchard.Utility.Extensions;
using Laser.Orchard.Sharing.Models;
using Laser.Orchard.Sharing.Settings;
using Laser.Orchard.Sharing.ViewModels;
using Orchard.Environment.Configuration;
using System.Web;
using Orchard.Logging;
using Orchard.Tokens;
using System.Collections.Generic;

namespace Laser.Orchard.Sharing.Drivers {

    public class ShareBarPartDriver : ContentPartDriver<ShareBarPart> {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardServices _services;
        private readonly ITokenizer _tokenizer;

        public ILogger Logger { get; set; }

        public ShareBarPartDriver(IHttpContextAccessor httpContextAccessor, IOrchardServices services, ITokenizer tokenizer) {
            _httpContextAccessor = httpContextAccessor;
            _services = services;
            _tokenizer = tokenizer;
        }

        protected override DriverResult Display(ShareBarPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Share_ShareBar", () => {
                var shareSettings = _services.WorkContext.CurrentSite.As<ShareBarSettingsPart>();
                var httpContext = _httpContextAccessor.Current();

                string path = "";
                // Prevent share bar from showing when current item is not Routable and it's not possible to retrieve the url
                if (!part.Is<IAliasAspect>()) {
                    try {
                        path = HttpContext.Current.Request.Url.AbsoluteUri;
                        if (string.IsNullOrWhiteSpace(path)) {
                            return null;
                        }
                    }
                    catch (Exception e) {
                        Logger.Error(e.Message);
                        return null;
                    }

                }
                ShareBarTypePartSettings typeSettings;
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };

                typeSettings = part.Settings.GetModel<ShareBarTypePartSettings>();

                path = _tokenizer.Replace(typeSettings.Url, tokens);

                // The default sharing URL is the browser URL address.
                // It defines explicitly the URl only if the setttings sets a different URL 
                if (!String.IsNullOrWhiteSpace(path)) {
                    var baseUrl = httpContext.Request.ToApplicationRootUrlString();
                    // remove any application path from the base url
                    var applicationPath = httpContext.Request.ApplicationPath ?? String.Empty;

                    var urlPrefix = _services.WorkContext.Resolve<ShellSettings>().RequestUrlPrefix;

                    if (path.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase)) {
                        path = path.Substring(applicationPath.Length);
                    }

                    if (!string.IsNullOrWhiteSpace(urlPrefix))
                        urlPrefix = urlPrefix + "/";
                    else
                        urlPrefix = "";

                    baseUrl = baseUrl.TrimEnd('/');
                    path = path.TrimStart('/');

                    path = baseUrl + "/" + urlPrefix + path;
                }

                var title = _tokenizer.Replace(typeSettings.Title, tokens);
                var description = _tokenizer.Replace(typeSettings.Description, tokens);
                var media = _tokenizer.Replace(typeSettings.Media, tokens);
                var model = new ShareBarViewModel {
                    Link = path,
                    Title = !string.IsNullOrWhiteSpace(title) ? title : _services.ContentManager.GetItemMetadata(part).DisplayText,
                    Media = media,
                    Description = description,
                    Settings = typeSettings
                };
                return shapeHelper.Parts_Share_ShareBar(ViewModel: model);
            });
        }
    }
}