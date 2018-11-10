using Laser.Orchard.Highlights.Enums;
using Laser.Orchard.Highlights.Models;
using Laser.Orchard.Highlights.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using System;
using Orchard.DisplayManagement;
using Laser.Orchard.Highlights.ViewModels;

namespace Laser.Orchard.Highlights.Drivers {

    public class HighlightsItemDriver : ContentPartCloningDriver<HighlightsItemPart> {

        private readonly IHighlightsService _HighlightsService;
        private readonly IWorkContextAccessor _workContext;
        private readonly IContentManager _contentManager;

        private readonly ContentIdentity.ContentIdentityEqualityComparer _identityComparer;
        private readonly Dictionary<ContentIdentity, int> _identities;
        private readonly IShapeFactory _shapeFactory;


        public HighlightsItemDriver(IHighlightsService HighlightsService, IWorkContextAccessor workContext, IContentManager contentManager, IShapeFactory shapeFactory) {
            _HighlightsService = HighlightsService;
            _workContext = workContext;
            _contentManager = contentManager;
            _identityComparer = new ContentIdentity.ContentIdentityEqualityComparer();
            _identities = new Dictionary<ContentIdentity, int>(_identityComparer);
            _shapeFactory = shapeFactory;

        }

        protected override string Prefix
        {
            get { return "HighlightsItem"; }
        }

        protected override DriverResult Display(HighlightsItemPart part, String displayType, dynamic shapeHelper) {
            ViewsInfos viewInfos = _HighlightsService.ChooseView(part);
            var dict = new Dictionary<string, object> {
                { "HighlightsItem", _HighlightsService.MapContentToHighlightsItemViewModel(part) },
                { "DisplayTemplate", displayType }
            };
            var args = Arguments.From(dict.Values, dict.Keys);

            return ContentShape(
                viewInfos.ResultShapeName,
                () => _shapeFactory.Create(viewInfos.ResultShapeName, args));
        }

        protected override DriverResult Editor(HighlightsItemPart part, dynamic shapeHelper) {

            if (part.HighlightsGroupPartRecordId == 0) {
                part.HighlightsGroupPartRecordId = Convert.ToInt32(_workContext.GetContext().HttpContext.Request.QueryString["HighlightsGroupId"]);
            }
            return ContentShape("Parts_HighlightsItem_Edit",
                                () => shapeHelper.EditorTemplate(
                                  TemplateName: "Parts/HighlightsItem_Edit",
                                  Model: part,
                                  Prefix: Prefix));
        }

        protected override DriverResult Editor(HighlightsItemPart part, global::Orchard.ContentManagement.IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(HighlightsItemPart part, ImportContentContext context) {
            part.LinkText = context.Attribute(part.PartDefinition.Name, "LinkText");
            part.LinkUrl = context.Attribute(part.PartDefinition.Name, "LinkUrl");
            part.Sottotitolo = context.Attribute(part.PartDefinition.Name, "Sottotitolo");
            part.TitleSize = context.Attribute(part.PartDefinition.Name, "TitleSize");

            var video = context.Attribute(part.PartDefinition.Name, "Video");
            if (video != null) {
                part.Video = Boolean.Parse(video);
            }

            var itemOrder = context.Attribute(part.PartDefinition.Name, "ItemOrder");
            if (itemOrder != null) {
                part.ItemOrder = int.Parse(itemOrder);
            }

            var linkTarget = context.Attribute(part.PartDefinition.Name, "LinkTarget");

            switch (linkTarget) {
                case "_blank":
                    part.LinkTarget = Enums.LinkTargets._blank;
                    break;
                case "_modal":
                    part.LinkTarget = Enums.LinkTargets._modal;
                    break;
                case "_self":
                    part.LinkTarget = Enums.LinkTargets._self;
                    break;
                default:
                    part.LinkTarget = Enums.LinkTargets._blank;
                    break;
            }

            var GroupIdentity = context.Attribute(part.PartDefinition.Name, "HighlightsGroupPartRecordId");

            var grpID = Get(GroupIdentity);

            if (grpID != null) {
                part.HighlightsGroupPartRecordId = (int)grpID;
            }

            //HighlightsGroupPartRecordId

        }

        //public ContentItem Get(string id, VersionOptions versionOptions, string contentTypeHint = null)
        public int? Get(string id) {
            var contentIdentity = new ContentIdentity(id);

            ContentItem existingItem = _contentManager.ResolveIdentity(contentIdentity);

            if (existingItem != null) {
                return existingItem.Id;
            }

            return null;
        }

        protected override void Exporting(HighlightsItemPart part, ExportContentContext context) {

            var container = _contentManager.Get(part.Record.Id);
            if (container != null) {
                var containerIdentity = _contentManager.GetItemMetadata(container).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Id", part.Record.Id.ToString());

                var parent = _contentManager.Get(part.HighlightsGroupPartRecordId);
                if (parent != null) {
                    var parentIdentity = _contentManager.GetItemMetadata(parent).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("HighlightsGroupPartRecordId", parentIdentity);
                }
            }

            if (part.LinkText != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("LinkText", part.LinkText);
            }

            if (part.LinkUrl != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("LinkUrl", part.LinkUrl);
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("Video", part.Video);

            if (part.Sottotitolo != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("Sottotitolo", part.Sottotitolo);
            }

            if (part.TitleSize != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("TitleSize", part.TitleSize);
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("ItemOrder", part.ItemOrder);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LinkTarget", part.LinkTarget);
        }

        protected override void Cloning(HighlightsItemPart originalPart, HighlightsItemPart clonePart, CloneContentContext context) {
            clonePart.Video = originalPart.Video;
            clonePart.Sottotitolo = originalPart.Sottotitolo;
            clonePart.TitleSize = originalPart.TitleSize;
            clonePart.HighlightsGroupPartRecordId = originalPart.HighlightsGroupPartRecordId;
            clonePart.LinkUrl = originalPart.LinkUrl;
            clonePart.LinkTarget = originalPart.LinkTarget;
            clonePart.LinkText = originalPart.LinkText;
            clonePart.ItemOrder = originalPart.ItemOrder;
        }
    }
}