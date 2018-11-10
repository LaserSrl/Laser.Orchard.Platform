using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Handlers {
    /// <summary>
    /// This is the main default handler whose methods will run every-time.
    /// Since every ContentItem can be Anonymized or Erased only if it has a GDPRPart,
    /// this handler will add filters to that.
    /// Mostly, these will have to run default stuff, especially based on the additional
    /// settings telling the behaviour using reflection.
    /// One reason to have that behaviour running here, on events for GDPRPart, is to handle
    /// all those parts that cannot have their own handler as they are not defined in code.
    /// Note that all processing should happen on all versions of the content.
    /// </summary>
    public class GDPRPartGDPRHandler : ContentGDPRHandler {

        public GDPRPartGDPRHandler() {
            
        }

        protected override void Anonymizing(AnonymizeContentContext context) {
            // this check is a trick to verify that the ContentItem has a GDPRPart and
            // that it is not protected or anything.
            if (context.ShouldProcess(context.GDPRPart)) {
                // go through every part in context.ContentItem
                foreach (var part in context.ContentItem.Parts) {
                    // get the subset of all the part's fields that include those that need processing
                    var fieldsToProcess = part.Fields.Where(fi =>
                        IsFieldToProcess(context, fi, set => set.AnonymizationPropertyValuePairs));
                    // we get the part's settings to check whether they areconfigured for reflection
                    var partSettings = part.TypePartDefinition
                        .Settings.GetModel<GDPRPartPartSettings>();
                    // Only do stuff if either the part should be processed and has a configuration for reflection,
                    // or that is true for any of its fields.
                    var IsPartToProcess = context.ShouldProcess(part)
                        && ValidDictionary(partSettings.AnonymizationPropertyValuePairs);
                    if (IsPartToProcess || fieldsToProcess.Any()) {
                        // We will need this part's definition to look for it in previous versions of the 
                        // content item
                        var partName = part.PartDefinition.Name;
                        // we get all the versions of this part. We get this here before checking whether we
                        // should process the part because we will need it if there are fields to process.
                        var partVersions = context.AllVersions
                            .Select(ci => ci.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == partName))
                            .Where(pa => pa != null);
                        var partVersionAction = new Action<ContentPart>(pv => { });
                        if (IsPartToProcess) {
                            partVersionAction = pv => SetAllProperties(pv, partSettings.AnonymizationPropertyValuePairs);
                        }
                        var fieldVersionAction = new Action<ContentPart>((pv) => { });
                        if (fieldsToProcess.Any()) {
                            fieldVersionAction = (pv) => {
                                foreach (var field in fieldsToProcess) {
                                    // get the default setting for reflection (we verified it exists in IsFieldToProcess)
                                    var fieldSettings = field.PartFieldDefinition
                                        .Settings.GetModel<GDPRPartFieldSettings>();
                                    //get the field
                                    var fieldVersion = pv.Fields
                                        .FirstOrDefault(fi => fi.Name == field.Name);
                                    // use reflection to set property values
                                    SetAllProperties(fieldVersion, fieldSettings.AnonymizationPropertyValuePairs);
                                }
                            };
                        }
                        foreach (var partVersion in partVersions) {
                            partVersionAction(partVersion);
                            fieldVersionAction(partVersion);
                        }
                    }
                }
            }
        }

        protected override void Erasing(EraseContentContext context) {
            // this check is a trick to verify that the ContentItem has a GDPRPart and
            // that it is not protected or anything.
            if (context.ShouldProcess(context.GDPRPart)) {
                // go through every part in context.ContentItem
                foreach (var part in context.ContentItem.Parts) {
                    // get the subset of all the part's fields that include those that need processing
                    var fieldsToProcess = part.Fields.Where(fi =>
                        IsFieldToProcess(context, fi, set => set.ErasurePropertyValuePairs));
                    var partSettings = part.TypePartDefinition
                        .Settings.GetModel<GDPRPartPartSettings>();
                    // Only do stuff if either the part should be processed and has a configuration for reflection,
                    // or that is true for any of its fields.
                    var IsPartToProcess = context.ShouldProcess(part)
                        && ValidDictionary(partSettings.ErasurePropertyValuePairs);
                    if (IsPartToProcess || fieldsToProcess.Any()) {
                        // We will need this part's definition to look for it in previous versions of the 
                        // content item
                        var partName = part.PartDefinition.Name;
                        // we get all the versions of this part
                        var partVersions = context.AllVersions
                            .Select(ci => ci.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == partName))
                            .Where(pa => pa != null);
                        var partVersionAction = new Action<ContentPart>(pv => { });
                        if (IsPartToProcess) {
                            partVersionAction = pv => SetAllProperties(pv, partSettings.ErasurePropertyValuePairs);
                        }
                        var fieldVersionAction = new Action<ContentPart>((pv) => { });
                        if (fieldsToProcess.Any()) {
                            fieldVersionAction = (pv) => {
                                foreach (var field in fieldsToProcess) {
                                    // get the default setting for reflection (we verified it exists in IsFieldToProcess)
                                    var fieldSettings = field.PartFieldDefinition
                                        .Settings.GetModel<GDPRPartFieldSettings>();
                                    //get the field
                                    var fieldVersion = pv.Fields
                                        .FirstOrDefault(fi => fi.Name == field.Name);
                                    // use reflection to set property values
                                    SetAllProperties(fieldVersion, fieldSettings.ErasurePropertyValuePairs);
                                }
                            };
                        }
                        foreach (var partVersion in partVersions) {
                            partVersionAction(partVersion);
                            fieldVersionAction(partVersion);
                        }
                    }
                }
            }
        }

        protected override void Anonymized(AnonymizeContentContext context) {
            context.Outcome = GDPRProcessOutcome.Anonymized;
        }

        protected override void Erased(EraseContentContext context) {
            context.Outcome = GDPRProcessOutcome.Erased;
        }

        #region private methods to make the processing methods more readable

        /// <summary>
        /// This method is used to verify whether a specific field requires processing.
        /// </summary>
        /// <param name="context">The context object.</param>
        /// <param name="field">The field we are testing.</param>
        /// <param name="dictionaryFunc">This Func parameter is used to tell which dictionary should be
        /// validated for the reflection settings. In practice, this means that this Func discriminates
        /// between a check for Anonymization and one for erasure.</param>
        /// <returns>True for a field that need processing and has something configured to be done in reflection.
        /// False otherwise.</returns>
        private bool IsFieldToProcess(
            GDPRContentContext context, 
            ContentField field, 
            Func<GDPRPartFieldSettings, Dictionary<string, string>> dictionaryFunc) {

            if (context.ShouldProcess(field)) {
                var settings = field.PartFieldDefinition
                    .Settings.GetModel<GDPRPartFieldSettings>();
                if (ValidDictionary(dictionaryFunc(settings))) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method checks whether the dictionary contains any element. It's just a shorthand
        /// for a check we have to perform a bunch of times.
        /// </summary>
        /// <param name="dictionary">The dictionary that we want to verify.</param>
        /// <returns>True i fthe dictionary has elements. False otherwise.</returns>
        private bool ValidDictionary(Dictionary<string, string> dictionary) {
            return dictionary != null && dictionary.Any();
        }

        /// <summary>
        /// This method loops through all elements in the dictionary and tries setting the corresponding
        /// Proerty-Value pair on the object passed. This is just a shorthand for an operation we'll have
        /// to do a bunch of times.
        /// </summary>
        /// <param name="obj">The object whose properties we are tryeing to set.</param>
        /// <param name="propertyValuePairs">A dictionary whose keys are the property names, and whose
        /// values are the values we are trying to set for those properties.</param>
        private void SetAllProperties(object obj, Dictionary<string, string> propertyValuePairs) {
            foreach (var propertyValuePair in propertyValuePairs) {
                TrySetProperty(obj, propertyValuePair.Key, propertyValuePair.Value);
            }
        }

        /// <summary>
        /// This method sets a property of an object to a specific value using reflection.
        /// </summary>
        /// <param name="obj">THe object whose property we are attempting to set.</param>
        /// <param name="propertyName">The name of the property that will be used.</param>
        /// <param name="value">The value we are trying to set.</param>
        /// <returns>True if successful. False otherwise.</returns>
        private bool TrySetProperty(object obj, string propertyName, object value) {
            try {
                // validation of stuff required
                if (obj != null) {
                    var prop = obj.GetType()
                        .GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (prop != null) {
                        var val = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(obj, val, null);
                    }
                }
                return true;
            } catch (Exception ex) {
                Logger.Error(ex.ToString());
                return false;
            }
        }
        #endregion
    }
}