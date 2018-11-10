using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.MediaExtension")]
    public class MediaLibraryPickerFieldGDPRContentHandler : ContentGDPRHandler {

        private readonly IContentGDPRManager _contentGDPRManager;
        private readonly IContentManager _contentManager;

        public MediaLibraryPickerFieldGDPRContentHandler(
            IContentGDPRManager contentGDPRManager,
            IContentManager contentManager) {

            _contentGDPRManager = contentGDPRManager;
            _contentManager = contentManager;

            // since we want to process a ContentField, we do this from the handlers for GDPRPart
            OnAnonymizing<GDPRPart>(FieldsAnonymizing);
            OnAnonymized<GDPRPart>(PostProcess);
            OnErasing<GDPRPart>(FieldsErasing);
            OnErased<GDPRPart>(PostProcess);
        }

        private void FieldsAnonymizing(AnonymizeContentContext context, GDPRPart part) {
            Processing(context, part, _contentGDPRManager.Anonymize);
        }

        private void FieldsErasing(EraseContentContext context, GDPRPart part) {
            Processing(context, part, _contentGDPRManager.Erase);
        }

        private void Processing(
            GDPRContentContext context, 
            GDPRPart part, 
            Action<ContentItem, GDPRContentContext> process) {

            // If there are MediaLibraryPickerFields whose selected items we should process, do so
            if (context.ShouldProcess(part)) {
                var items = AllItemsToProcessFromAllVersions(context);
                foreach (var item in items) {
                    if (!context.ChainOfItems.Any(ci => ci.Id == item.Id)) {
                        // We only process the item if it's not alredy being processed in the current processing "chain".
                        // This helps preventing recursion, and propagates information regarding the other items.
                        // Then, the Manager will check whether the process is possible.
                        process(item, context);
                    }
                }
            }
        }

        private void PostProcess(GDPRContentContext context, GDPRPart part) {
            // if we should detach items, this is the place
            if (context.ShouldProcess(part)) {
                // each version of the item will have their own
                foreach (var itemVersion in context.AllVersions) {
                    var mlpfs = AllMLPFsFromContent(context, itemVersion);
                    foreach (var mlpf in mlpfs) {
                        Func<ContentItem, bool> shouldRemain = ci => true;
                        if (ShoulDetachAll(context, mlpf)) { // remove all?
                            shouldRemain = ci => false;
                        } else if (ShouldDetachPersonal(context, mlpf)) { // remove personal info?
                            shouldRemain = ci => !ci.Is<GDPRPart>();
                        }
                        mlpf.Ids = RemainingIds(mlpf, shouldRemain);
                    }
                }
            }
        }

        private int[] RemainingIds(MediaLibraryPickerField field, Func<ContentItem, bool> shouldRemain) {
            var items = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.AllVersions, QueryHints.Empty);
            return items.Where(shouldRemain).Select(ci => ci.Id).Distinct().ToArray();
        }

        private bool ShouldDetachPersonal(GDPRContentContext context, MediaLibraryPickerField field) {
            return context.Erase
                ? Setting(field).DetachGDPRItemsOnErase
                : Setting(field).DetachGDPRItemsOnAnonymize;
        }

        private bool ShoulDetachAll(GDPRContentContext context, MediaLibraryPickerField field) {
            return context.Erase
                ? Setting(field).DetachAllItemsOnErase
                : Setting(field).DetachAllItemsOnAnonymize;
        }

        private MediaLibraryPickerFieldGDPRPartFieldSettings Setting(MediaLibraryPickerField field) {
            return field.PartFieldDefinition.Settings.GetModel<MediaLibraryPickerFieldGDPRPartFieldSettings>();
        }
        
        /// <summary>
        /// Returns for the ContentItem passed as parameter all MediaLibraryPickerFields that should
        /// be processed as well
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<MediaLibraryPickerField> AllMLPFsFromContent(
            GDPRContentContext context, ContentItem contentItem) {
            return contentItem.Parts
                .SelectMany(pa => pa.Fields.Where(fi =>
                    fi is MediaLibraryPickerField
                    && context.ShouldProcess(fi)))
                .Cast<MediaLibraryPickerField>();
        }

        /// <summary>
        /// Returns all ContentItems selected in MediaLibraryPickerFields that are set to have all their
        /// selected items processed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<ContentItem> AllItemsToProcessFromAllVersions(GDPRContentContext context) {

            return
                context.AllVersions.SelectMany(civ => civ.Parts) // from all parts
                    .SelectMany(pa => pa.Fields.Where(fi => // get their fields
                        fi is MediaLibraryPickerField // that are ContentPickerFields
                        && context.ShouldProcess(fi) // that should be processed
                        && CheckFlag(context, (MediaLibraryPickerField)fi))) // that should have selected items processed as well
                    .Cast<MediaLibraryPickerField>() // cast the fields to MediaLibraryPickerFields
                    .SelectMany(mlpf => mlpf.MediaParts) // select all the selected items of all those fields
                    .Distinct() // do not process them twice
                    .Select(mp => mp.ContentItem); // we want the actual ContentItem
        }

        private bool CheckFlag(GDPRContentContext context, MediaLibraryPickerField fi) {
            var settings = fi.PartFieldDefinition.Settings.GetModel<MediaLibraryPickerFieldGDPRPartFieldSettings>();
            return context.Erase ?
                settings.AttemptToEraseItems :
                settings.AttemptToAnonymizeItems;
        }
    }
}