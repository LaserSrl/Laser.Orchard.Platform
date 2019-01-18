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
using Laser.Orchard.Cookies.Services;
using Laser.Orchard.Sharing.Services;

namespace Laser.Orchard.Sharing.Drivers {

    public class ShareBarPartDriver : ContentPartDriver<ShareBarPart> {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardServices _services;
        private readonly ITokenizer _tokenizer;
        private readonly IGDPRScript _gdprScript;

        public ILogger Logger { get; set; }

        public ShareBarPartDriver(IHttpContextAccessor httpContextAccessor, IOrchardServices services, ITokenizer tokenizer, IGDPRScript gdprScript) {
            _httpContextAccessor = httpContextAccessor;
            _services = services;
            _tokenizer = tokenizer;
            _gdprScript = gdprScript;
        }

        protected override DriverResult Display(ShareBarPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Share_ShareBar", () => {
                var shareSettings = _services.WorkContext.CurrentSite.As<ShareBarSettingsPart>();
                var httpContext = _httpContextAccessor.Current();

                string path;
                ShareBarTypePartSettings typeSettings;

                // check user choice according to GDPR
                if(_gdprScript.IsAcceptableForUser(new Laser.Orchard.Sharing.Services.CookieGDPR()) == false) {
                    return null;
                }

                // Prevent share bar from showing if account is not set
                if (shareSettings == null || string.IsNullOrWhiteSpace(shareSettings.AddThisAccount)) {
                    return null;
                }

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
                else {

                    path = part.As<IAliasAspect>().Path;

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

                typeSettings = part.Settings.GetModel<ShareBarTypePartSettings>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };

                var title = _tokenizer.Replace(typeSettings.Title, tokens);
                var description = _tokenizer.Replace(typeSettings.Description, tokens);
                var media = _tokenizer.Replace(typeSettings.Media, tokens);
                var model = new ShareBarViewModel {
                    Link = path,
                    Title = !string.IsNullOrWhiteSpace(title) ? title : _services.ContentManager.GetItemMetadata(part).DisplayText,
                    Media = media,
                    Description = description,
                    Account = shareSettings.AddThisAccount,
                    Mode = typeSettings.Mode
                };
                return shapeHelper.Parts_Share_ShareBar(ViewModel: model);
            });
        }
    }
}