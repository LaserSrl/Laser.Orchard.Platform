using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.ContentPickerFieldExtension")]
    public class ContentPickerFieldGDPRContentHandler : ContentGDPRHandler {

        private readonly IContentGDPRManager _contentGDPRManager;
        private readonly IContentManager _contentManager;

        public ContentPickerFieldGDPRContentHandler(
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

            // If there are ContentPickerFields whose selected items we should process, do so
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
                    var cpfs = AllCPFsFromContent(context, itemVersion);
                    foreach (var cpf in cpfs) {
                        Func<ContentItem, bool> shouldRemain = ci => true;
                        if (ShoulDetachAll(context, cpf)) { // remove all?
                            shouldRemain = ci => false;
                        } else if (ShouldDetachPersonal(context, cpf)) { // remove personal info?
                            shouldRemain = ci => !ci.Is<GDPRPart>();
                        }
                        cpf.Ids = RemainingIds(cpf, shouldRemain);
                    }
                }
            }
        }

        private int[] RemainingIds(ContentPickerField field, Func<ContentItem, bool> shouldRemain) {
            var items = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.AllVersions, QueryHints.Empty);
            return items.Where(shouldRemain).Select(ci => ci.Id).Distinct().ToArray();
        }

        private bool ShouldDetachPersonal(GDPRContentContext context, ContentPickerField field) {
            return context.Erase
                ? Setting(field).DetachGDPRItemsOnErase
                : Setting(field).DetachGDPRItemsOnAnonymize;
        }

        private bool ShoulDetachAll(GDPRContentContext context, ContentPickerField field) {
            return context.Erase
                ? Setting(field).DetachAllItemsOnErase
                : Setting(field).DetachAllItemsOnAnonymize;
        }

        private ContentPickerFieldGDPRPartFieldSettings Setting(ContentPickerField field) {
            return field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldGDPRPartFieldSettings>();
        }
        
        /// <summary>
        /// Returns for the ContentItem passed as parameter all ContentPickerFields that should
        /// be processed as well
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<ContentPickerField> AllCPFsFromContent(
            GDPRContentContext context, ContentItem contentItem) {
            return contentItem.Parts
                .SelectMany(pa => pa.Fields.Where(fi =>
                    fi is ContentPickerField
                    && context.ShouldProcess(fi)))
                .Cast<ContentPickerField>();
        }

        /// <summary>
        /// Returns all ContentItems selected in ContentPickerFields that are set to have all their
        /// selected items processed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<ContentItem> AllItemsToProcessFromAllVersions(GDPRContentContext context) {
            var allParts = context.AllVersions.SelectMany(civ => civ.Parts);
            var allFields = allParts
                .SelectMany(pa => pa.Fields.Where(fi => // get their fields
                    fi is ContentPickerField // that are ContentPickerFields
                    && context.ShouldProcess(fi) // that should be processed
                    && CheckFlag(context, (ContentPickerField)fi))) // that should have selected items processed as well
                .Cast<ContentPickerField>(); // cast the fields to ContentPickerFields

            var allItems = allFields
                .SelectMany(cpf => cpf.ContentItems) // select all the selected items of all those fields
                .GroupBy(ci => ci.Id)
                .Select(group => group.First()); // do not process them twice

            return allItems;

            //return 
            //    context.AllVersions.SelectMany(civ => civ.Parts) // from all parts
            //        .SelectMany(pa => pa.Fields.Where(fi => // get their fields
            //            fi is ContentPickerField // that are ContentPickerFields
            //            && context.ShouldProcess(fi) // that should be processed
            //            && CheckFlag(context, (ContentPickerField)fi))) // that should have selected items processed as well
            //        .Cast<ContentPickerField>() // cast the fields to ContentPickerFields
            //        .SelectMany(cpf => cpf.ContentItems) // select all the selected items of all those fields
            //        .Distinct(); // do not process them twice
        }

        private bool CheckFlag(GDPRContentContext context, ContentPickerField fi) {
            var settings = fi.PartFieldDefinition.Settings.GetModel<ContentPickerFieldGDPRPartFieldSettings>();
            return context.Erase ?
                settings.AttemptToEraseItems :
                settings.AttemptToAnonymizeItems;
        }
    }
}