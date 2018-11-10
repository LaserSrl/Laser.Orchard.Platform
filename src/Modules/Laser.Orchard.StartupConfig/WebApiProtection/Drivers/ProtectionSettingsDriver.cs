using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Drivers {
    public class ProtectionSettingsDriver : ContentPartDriver<ProtectionSettingsPart> {
        protected override void Exporting(ProtectionSettingsPart part, ExportContentContext context) {
            base.Exporting(part, context);
            var root = context.Element(part.PartDefinition.Name);
            foreach (var app in part.ExternalApplicationList.ExternalApplications) {
                XElement externalApp = new XElement("ExternalApplication");
                externalApp.SetAttributeValue("Name", app.Name);
                externalApp.SetAttributeValue("ApiKey", app.ApiKey);
                externalApp.SetAttributeValue("EnableTimeStampVerification", app.EnableTimeStampVerification);
                externalApp.SetAttributeValue("Validity", app.Validity);
                root.Add(externalApp);
            }
        }
        protected override void Importing(ProtectionSettingsPart part, ImportContentContext context) {
            base.Importing(part, context);
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            var settings = new List<ExternalApplication>();
            var webApiUtils = new WebApiUtils();
            settings.AddRange(part.ExternalApplicationList.ExternalApplications);
            var externalApps = root.Elements("ExternalApplication");
            foreach (var app in externalApps) {
                var name = app.Attribute("Name") != null ? app.Attribute("Name").Value : "";
                var externalApp = settings.FirstOrDefault(x => x.Name == name);
                if(externalApp == null) {
                    externalApp = new ExternalApplication { Name = name };
                    settings.Add(externalApp);
                }
                externalApp.ApiKey = app.Attribute("ApiKey") != null ? app.Attribute("ApiKey").Value : webApiUtils.RandomString(22);
                externalApp.EnableTimeStampVerification = app.Attribute("EnableTimeStampVerification") != null ? bool.Parse(app.Attribute("EnableTimeStampVerification").Value) : true; // default value: true
                externalApp.Validity = app.Attribute("Validity") != null ? int.Parse(app.Attribute("Validity").Value) : 10; // default value: 10
            }
            part.ExternalApplicationList = new ExternalApplicationList { ExternalApplications = settings };
        }
    }
}