﻿using Laser.Orchard.TenantBridges.Models;
using Laser.Orchard.TenantBridges.Services;
using Laser.Orchard.TenantBridges.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;

namespace Laser.Orchard.TenantBridges.Drivers {
    public class RemoteTenantContentSnippetWidgetPartDriver :
        ContentPartCloningDriver<RemoteTenantContentSnippetWidgetPart> {

        private readonly IRemoteContentService _remoteContentService;

        public RemoteTenantContentSnippetWidgetPartDriver(
            IRemoteContentService remoteContentService) {

            _remoteContentService = remoteContentService;
        }

        protected override DriverResult Display(
            
            //TODO: Add the possibility to call a generic url and get the markup to show it

            RemoteTenantContentSnippetWidgetPart part, string displayType, dynamic shapeHelper) {
            if (part.ShouldGetHtmlSnippet) {
                // Fetch the html snippet from the remote content controller on the remote tenant
                return ContentShape("Parts_RemoteTenantContentSnippetWidget_Html", () => {
                    return shapeHelper.Parts_RemoteTenantContentSnippetWidget_Html(
                        ViewModel: new RemoteTenantContentSnippetWidgetPartHtmlDisplayViewModel { 
                            Html= _remoteContentService.GetSnippet(part)
                        });
                });
            }
            else {
                // Get the content from the web API on the remote tenant
                // The Laser WebServices feature has to be enabled on the remote tenant for this to work
                // if it isn't we get a 404 error
                return ContentShape("Parts_RemoteTenantContentSnippetWidget_Json", () => {
                    string json = _remoteContentService.GetJson(part);
                    JObject jObject = default(JObject);
                    try {
                        jObject = JObject.Parse(json);
                    }
                    catch (Exception) { 
                        // TODO: Do we set a particular value so the shape can know there was an error?
                    }

                    return shapeHelper.Parts_RemoteTenantContentSnippetWidget_Json(
                        ViewModel: new RemoteTenantContentSnippetWidgetPartJsonDisplayViewModel {                            
                            Json = json,
                            JObject = jObject
                    });;                  
                });
            }
        }

        protected override string Prefix {
            get { return "RemoteTenantContentSnippetWidgetPart"; }
        }

        protected override DriverResult Editor(
            RemoteTenantContentSnippetWidgetPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(
            RemoteTenantContentSnippetWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {

            var vm = RemoteTenantContentSnippetWidgetPartEditorViewModel.FromPart(part);
            if (updater != null) {
                // TODO: validate the model: URLs must be urls, numbers must be numbers...
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    // if the update was OK we can update the part with what's in the ViewModel
                    vm.UpdatePart(part);
                }
            }

            return ContentShape("Parts_RemoteTenantContentSnippetWidget_Edit", () =>
                shapeHelper.EditorTemplate(
                    TemplateName: "Parts/RemoteTenantContentSnippetWidget",
                    Prefix: Prefix,
                    Model: vm
                    ));
        }

        protected override void Exporting(
            RemoteTenantContentSnippetWidgetPart part, ExportContentContext context) {

            var partName = part.PartDefinition.Name;
            context.Element(partName)
                .SetAttributeValue("RemoteTenantBaseUrl", part.RemoteTenantBaseUrl);
            context.Element(partName)
                .SetAttributeValue("ShouldGetHtmlSnippet", part.ShouldGetHtmlSnippet);
            context.Element(partName)
                .SetAttributeValue("RemoteContentId", part.RemoteContentId);
            context.Element(partName)
                .SetAttributeValue("RemoveRemoteWrappers", part.RemoveRemoteWrappers);
            context.Element(partName)
                 .SetAttributeValue("Alias", part.Alias);
        }

        protected override void Importing(
            RemoteTenantContentSnippetWidgetPart part, ImportContentContext context) {

            var partName = part.PartDefinition.Name;
            part.RemoteTenantBaseUrl = context.Attribute(partName, "RemoteTenantBaseUrl");
            part.ShouldGetHtmlSnippet = bool.Parse(context.Attribute(partName, "ShouldGetHtmlSnippet"));
            part.RemoteContentId = Int32.Parse(context.Attribute(partName, "RemoteContentId"));
            part.RemoveRemoteWrappers = bool.Parse(context.Attribute(partName, "RemoveRemoteWrappers"));
            part.Alias = context.Attribute(partName, "Alias");
        }

        protected override void Cloning(
            RemoteTenantContentSnippetWidgetPart originalPart, RemoteTenantContentSnippetWidgetPart clonePart, CloneContentContext context) {
            clonePart.RemoteTenantBaseUrl = originalPart.RemoteTenantBaseUrl;
            clonePart.ShouldGetHtmlSnippet = originalPart.ShouldGetHtmlSnippet;

            clonePart.RemoteContentId = originalPart.RemoteContentId;
            clonePart.RemoveRemoteWrappers = originalPart.RemoveRemoteWrappers;
            clonePart.Alias = originalPart.Alias;
        }
    }
}