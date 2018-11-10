using Laser.Orchard.Highlights.Models;
using Laser.Orchard.Highlights.Services;
using Laser.Orchard.Highlights.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;


//using Orchard;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Drivers;
//using Orchard.Localization;
//using Orchard.Localization.Services;
//using Orchard.Mvc;
using System;
using System.Linq;


namespace Laser.Orchard.Highlights.Handlers {

    public class HighlightsGroupHandler : ContentHandler {

        private readonly IHighlightsService _HighlightsService;

        public HighlightsGroupHandler(IRepository<HighlightsGroupPartRecord> repository, IHighlightsService HighlightsService) {
            Filters.Add(StorageFilter.For(repository));

            _HighlightsService = HighlightsService;

            // Register alias as identity
            OnGetContentItemMetadata<HighlightsGroupPart>((ctx, part) => {
                // nel caso in cui il gruppo widget non "battezzato" (ora è sempre così, ma nel caso in cui avesse già un "identity" valorizzato non lo sovrascriviamo)
                if (ctx.Metadata.Identity.ToString() == "") {
                    ctx.Metadata.Identity.Add("MWGroupId", part.Id.ToString());
                }
            });

            OnRemoving<HighlightsGroupPart>((ctx, part) => {

                var HighlightsItems = _HighlightsService.GetHighlightsItemsByGroupId(part.Id);

                //var group = new HighlightsGroup
                //{
                //    Id = part.Id,
                //    ContentId = (part.ContentItem != null) ? part.ContentItem.Id : 0,
                //    DisplayPlugin = part.DisplayPlugin,
                //    DisplayTemplate = part.DisplayTemplate,
                //    HighlightsItems = HighlightsItems.ToList(),
                //    HighlightsItemsOrder = String.Join(",", HighlightsItems.Select(s => s.Id).ToArray())
                //};

                //var comments = commentService.GetCommentsForCommentedContent(context.ContentItem.Id).List();

                foreach (var mediaItem in HighlightsItems) {
                    _HighlightsService.Remove(mediaItem);
                }
            });

        }


    }

}