using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using Laser.Orchard.SEO.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.UI.Resources;
using System;

namespace Laser.Orchard.SEO.Drivers {


    public class SeoDriver : ContentPartCloningDriver<SeoPart> {

        private readonly IOrchardServices _orchardServices;
        private readonly ISEOServices _seoServices;
        private readonly IWorkContextAccessor _workContextAccessor;

        public SeoDriver(IOrchardServices orchardServices, ISEOServices seoServices, IWorkContextAccessor workContextAccessor) {
            _orchardServices = orchardServices;
            _seoServices = seoServices;
            _workContextAccessor = workContextAccessor;
        }


        /// <summary>
        /// GET Display.
        /// </summary>
        protected override DriverResult Display(SeoPart part, string displayType, dynamic shapeHelper) {

            if (displayType != "Detail")
                return null;

            var resourceManager = _workContextAccessor.GetContext().Resolve<IResourceManager>();

            if (!string.IsNullOrWhiteSpace(part.Description)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "description",
                    Content = part.Description
                });
            }

            if (!string.IsNullOrWhiteSpace(part.Keywords)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "keywords",
                    Content = part.Keywords
                });
            }

            string metaRobots = "";
            metaRobots += part.RobotsNoIndex ? "noindex," : "";
            metaRobots += part.RobotsNoFollow ? "nofollow," : "";
            metaRobots += part.RobotsNoSnippet ? "nosnippet," : "";
            metaRobots += part.RobotsNoOdp ? "noodp," : "";
            metaRobots += part.RobotsNoArchive ? "noarchive," : "";
            metaRobots += part.RobotsUnavailableAfter ? "unavailable_after:" + part.RobotsUnavailableAfterDate.Value.ToUniversalTime().ToString("r") + "," : ""; //date in rfc850 format
            metaRobots += part.RobotsNoImageIndex ? "noimageindex," : "";
            if (!string.IsNullOrWhiteSpace(metaRobots)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "robots",
                    Content = metaRobots.Substring(0, metaRobots.Length - 1) //remove trailing comma
                });
            }

            string metaGoogle = "";
             metaGoogle += part.GoogleNoSiteLinkSearchBox ? "nositelinkssearchbox," : "";
            metaGoogle += part.GoogleNoTranslate ? "notranslate," : "";
            if (!string.IsNullOrWhiteSpace(metaGoogle)) {
                resourceManager.SetMeta(new MetaEntry {
                    Name = "google",
                    Content = metaGoogle.Substring(0, metaGoogle.Length - 1)
                });
            }
            return null;
        }

        /// <summary>
        /// GET Editor.
        /// </summary>
        protected override DriverResult Editor(SeoPart part, dynamic shapeHelper) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageSEO))
                return null;

            return ContentShape("Parts_SEO_Edit",
                                () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/SEO",
                                  Model: new SeoPartViewModel(part, _seoServices), //use a viewmodel to show times in local base, while keeping UTC on the server side
                                  Prefix: Prefix));
        }

        /// <summary>
        /// POST Editor.
        /// </summary>
        protected override DriverResult Editor(SeoPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (_orchardServices.Authorizer.Authorize(Permissions.ManageSEO)) {
                var vm = new SeoPartViewModel(_seoServices);
                updater.TryUpdateModel(vm, Prefix, null, null);
                vm.UpdatePart(part);
                return Editor(part, shapeHelper);
            } else
                return null;
        }

        protected override void Cloning(SeoPart originalPart, SeoPart clonePart, CloneContentContext context) {
            clonePart.TitleOverride = originalPart.TitleOverride;
            clonePart.Keywords = originalPart.Keywords;
            clonePart.Description = originalPart.Description;
            clonePart.RobotsNoIndex = originalPart.RobotsNoIndex;
            clonePart.RobotsNoFollow = originalPart.RobotsNoFollow;
            clonePart.RobotsNoSnippet = originalPart.RobotsNoSnippet;
            clonePart.RobotsNoOdp = originalPart.RobotsNoOdp;
            clonePart.RobotsNoArchive = originalPart.RobotsNoArchive;
            clonePart.RobotsUnavailableAfter = originalPart.RobotsUnavailableAfter;
            clonePart.RobotsUnavailableAfterDate = originalPart.RobotsUnavailableAfterDate;
            clonePart.RobotsNoImageIndex = originalPart.RobotsNoImageIndex;
            clonePart.GoogleNoSiteLinkSearchBox = originalPart.GoogleNoSiteLinkSearchBox;
            clonePart.GoogleNoTranslate = originalPart.GoogleNoTranslate;
            clonePart.HideDetailMicrodata = originalPart.HideDetailMicrodata;
            clonePart.HideAggregatedMicrodata = originalPart.HideAggregatedMicrodata;
            clonePart.CanonicalUrl = originalPart.CanonicalUrl;
        }

        protected override void Importing(SeoPart part, ImportContentContext context) {
            var partDefinitionName = part.PartDefinition.Name;
            if (context.Data.Element(partDefinitionName) == null) {
                return;
            }

            // ImportAttribute handles null checks internally
            context.ImportAttribute(partDefinitionName, "TitleOverride", s =>
                part.TitleOverride = s
            );
            context.ImportAttribute(partDefinitionName, "CanonicalUrl", s =>
                part.CanonicalUrl = s
            );
            context.ImportAttribute(partDefinitionName, "Keywords", s =>
                part.Keywords = s
            );
            context.ImportAttribute(partDefinitionName, "Description", s =>
                part.Description = s
            );

            context.ImportAttribute(partDefinitionName, "RobotsNoIndex", s =>
                part.RobotsNoIndex = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsNoFollow", s =>
                part.RobotsNoFollow = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsNoSnippet", s =>
                part.RobotsNoSnippet = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsNoOdp", s =>
                part.RobotsNoOdp = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsNoArchive", s =>
                part.RobotsNoArchive = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsUnavailableAfter", s =>
                part.RobotsUnavailableAfter = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsUnavailableAfterDate", s =>
                part.RobotsUnavailableAfterDate = DateTime.Parse(s)
            );
            context.ImportAttribute(partDefinitionName, "RobotsNoImageIndex", s =>
                part.RobotsNoImageIndex = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "GoogleNoSiteLinkSearchBox", s =>
                part.GoogleNoSiteLinkSearchBox = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "GoogleNoTranslate", s =>
                part.GoogleNoTranslate = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "HideDetailMicrodata", s =>
                part.HideDetailMicrodata = Convert.ToBoolean(s)
            );
            context.ImportAttribute(partDefinitionName, "HideAggregatedMicrodata", s =>
                part.HideAggregatedMicrodata = Convert.ToBoolean(s)
            );
        }

        protected override void Exporting(SeoPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("TitleOverride", part.TitleOverride);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CanonicalUrl", part.CanonicalUrl);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Keywords", part.Keywords);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Description", part.Description);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoIndex", part.RobotsNoIndex);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoFollow", part.RobotsNoFollow);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoSnippet", part.RobotsNoSnippet);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoOdp", part.RobotsNoOdp);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoArchive", part.RobotsNoArchive);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsUnavailableAfter", part.RobotsUnavailableAfter);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsUnavailableAfterDate", part.RobotsUnavailableAfterDate);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RobotsNoImageIndex", part.RobotsNoImageIndex);
            context.Element(part.PartDefinition.Name).SetAttributeValue("GoogleNoSiteLinkSearchBox", part.GoogleNoSiteLinkSearchBox);
            context.Element(part.PartDefinition.Name).SetAttributeValue("GoogleNoTranslate", part.GoogleNoTranslate);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HideDetailMicrodata", part.HideDetailMicrodata);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HideAggregatedMicrodata", part.HideAggregatedMicrodata);
        }


    }
}