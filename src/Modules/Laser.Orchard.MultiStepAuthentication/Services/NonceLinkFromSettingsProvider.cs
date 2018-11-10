using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Mvc.Extensions;
using Orchard.Logging;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLinkFromSettingsProvider : INonceLinkProvider {
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICommonsServices _commonsServices;
        public ILogger Logger { get; set; }
        public NonceLinkFromSettingsProvider(
            IWorkContextAccessor workContextAccessor,
            ICommonsServices commonsServices,
            ShellSettings shellSettings) {
            Logger = NullLogger.Instance;
            _workContextAccessor = workContextAccessor;
            _commonsServices = commonsServices;
            _shellSettings = shellSettings;
        }

        public string FormatURI(string nonce) {
            return FormatURI(nonce, null);
        }
        public string FormatURI(string nonce, FlowType? flow) {
            if (flow == FlowType.App) {
                var urlHelper = _commonsServices.GetUrlHelper();
                string protocol = "http://";
                if (HttpContext.Current!=null && HttpContext.Current.Request.Url.ToString().StartsWith("https"))
                    protocol = "https://";
                var sitebase = _workContextAccessor.GetContext().CurrentSite.BaseUrl.Replace("http://","").Replace("https://", "");
                var site = "/" + _shellSettings.RequestUrlPrefix + "/NonceAppCamouflage";
                site = protocol+ sitebase + site.Replace("//", "/").Replace("//", "/") + string.Format("?n={0}",urlHelper.Encode(nonce));
                return site;
           }
            else
                return string.Format(GetFormat(), nonce);
        }
        public string GetSchema() {
            var format = GetFormat();
            if (format.Contains(@"://")) {
                return format.Substring(0, format.IndexOf(@"://"));
            }
            else {
                return format;
            }
        }

        private string GetFormat() {
            return _workContextAccessor.GetContext().CurrentSite.As<NonceLoginSettingsPart>().LoginLinkFormat;
        }
    }
}