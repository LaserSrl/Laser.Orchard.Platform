using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Tokens;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class SiteTokens : ITokenProvider {
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteTokens(
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IContentDefinitionManager contentDefinitionManager,
            IHttpContextAccessor httpContextAccessor) {

            _shellSettings = shellSettings;
            _workContextAccessor = workContextAccessor;
            _contentDefinitionManager = contentDefinitionManager;
            _httpContextAccessor = httpContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Site", T("Site Settings"), T("Tokens for Site Settings"))
                // {Site.RequestUrlPrefix} returns the strig from the settings or an empty string
                // if none is set there.
                .Token("RequestUrlPrefix",
                    T("Request Url Prefix"), 
                    T("The URL Prefix configured in the tenant's settings."), "Text")
                // {Site.TenantTopLevel} is used to "start" urls so that they are based
                // on the root. e.g.: if the goal is to have a link to /foo relative
                // to the base of the tenant, this token will resolve to "/" if no prefix
                // is set, or "/prefix/" if a prefix is set. If a different application path
                // would be used, for example when on localhost, it is also added here, so
                // the result would be "/appPath/prefix/"
                .Token("TenantTopLevel", 
                    T("Top Level for Tenant"),
                    T("A string intended to be used to start URLs. It resolves to either \"/\" if no prefix is set or \"/prefix/\" if there is a prefix for the tenant. If there is an application path that is correctly prepended."), "Text")
                ;
        }

        public void Evaluate(EvaluateContext context) {
            var forContent = context.For<ISite>("Site", (Func<ISite>)(() => _workContextAccessor.GetContext().CurrentSite));

            var urlPrefixFunc = (Func<ISite, object>)(content => _shellSettings.RequestUrlPrefix ?? "");
            var topLevelPrefixFunc = (Func<ISite, object>)(content => {
                var httpContext = _httpContextAccessor.Current();
                var appPath = httpContext.Request.ApplicationPath;
                if (appPath == "/")
                    appPath = "";
                if (!string.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix)) {
                    appPath = string.Concat(appPath, "/", _shellSettings.RequestUrlPrefix, "/");
                }
                if (!appPath.StartsWith("/")) {
                    appPath = "/" + appPath;
                }
                if (!appPath.EndsWith("/")) {
                    appPath = appPath + "/";
                }
                return appPath;
            });

            forContent
                .Token("RequestUrlPrefix", urlPrefixFunc)
                .Chain("RequestUrlPrefix", "Text", urlPrefixFunc)
                .Token("TenantTopLevel", topLevelPrefixFunc)
                .Chain("TenantTopLevel", "Text", topLevelPrefixFunc)
                ;

            // same as Orchard.Core.Settings.Tokens.SettingsTokens
            if (context.Target == "Site") {
                // is there a content available in the context ?
                if (forContent.Data != null && forContent.Data.ContentItem != null) {
                    var customSettingsPart = _contentDefinitionManager.GetTypeDefinition("Site");
                    foreach (var partField in customSettingsPart.Parts.SelectMany(x => x.PartDefinition.Fields)) {
                        var field = partField;
                        var tokenName = partField.Name;
                        forContent.Token(
                            tokenName,
                            (Func<IContent, object>)(content => LookupField(content, field.Name).Storage.Get<string>()));
                        forContent.Chain(
                            tokenName,
                            partField.FieldDefinition.Name,
                            (Func<IContent, object>)(content => LookupField(content, field.Name)));
                    }
                }
            }
        }
        // same as Orchard.Core.Settings.Tokens.SettingsTokens
        private static ContentField LookupField(IContent content, string fieldName) {
            return content.ContentItem.Parts
                .Where(part => part.PartDefinition.Name == "Site")
                .SelectMany(part => part.Fields.Where(field => field.Name == fieldName))
                .FirstOrDefault();
        }
    }
}