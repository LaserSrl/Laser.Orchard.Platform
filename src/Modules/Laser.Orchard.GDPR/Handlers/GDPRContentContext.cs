using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Handlers {
    public abstract class GDPRContentContext : ContentContextBase {

        public GDPRContentContext(ContentItem contentItem) : base(contentItem) {

            ChainOfContexts = new List<GDPRContentContext>();
            Outcome = GDPRProcessOutcome.Unknown;
        }

        public GDPRContentContext(ContentItem contentItem, GDPRContentContext previousContext)
            : this(contentItem) {

            ChainOfContexts.Add(previousContext);
            if (previousContext?.ChainOfContexts != null) {
                // we called the other ctor, thus ChainOfContexts was initialized as empty. 
                // Then we add to it whatever was there already.
                // The result of this is that this.ChainOfContexts is ordered with the current item
                // being the first element, and the item on which the process began last.
                ChainOfContexts.AddRange(previousContext.ChainOfContexts);
            }
        }

        /// <summary>
        /// The GDPRPart for the ContentItem being processed. Since we will often use this, it's
        /// convenient to have a shorter form to access it.
        /// </summary>
        public GDPRPart GDPRPart { get { return ContentItem.As<GDPRPart>(); } }

        /// <summary>
        /// This flag tells whether we are performing an erasure rather than an anonymization.
        /// </summary>
        public bool Erase { get; protected set; }

        public GDPRProcessOutcome Outcome { get; set; }

        /// <summary>
        /// This represents the items that have been taken into consideration for the current
        /// anonymization/erasure process. The first item in the sequence is the current ContentItem
        /// The ProfileItem from which we have started the whole process is the last element in the 
        /// list.
        /// This can be useful when processing related or connected ContentItems, to prevent recursions.
        /// Moreover, it can be useful to carry information on the previous items in the chain to be
        /// used in custom processing.
        /// </summary>
        public List<ContentItem> ChainOfItems {
            get {
                var items = new List<ContentItem>() { ContentItem };
                items.AddRange(ChainOfContexts.Select(ctx => ctx.ContentItem));
                return items;
            }
        }

        /// <summary>
        /// This represents the items that have been taken into consideration for the current
        /// anonymization/erasure process, as well as any additional information about the process,
        /// such as knowledge as to whether it was an anonymization or an erasure. 
        /// This sequence may be empty, representing the case in which the current context is the
        /// first to have been created.
        /// The ProfileItem from which we have started the whole process is in the last element in the 
        /// list.
        /// This can be useful when processing related or connected ContentItems, to prevent recursions.
        /// Moreover, it can be useful to carry information on the previous items in the chain to be
        /// used in custom processing.
        /// </summary>
        public List<GDPRContentContext> ChainOfContexts { get; set; }

        /// <summary>
        /// These are all the versions that exist for the ContentItem currently being processed.
        /// </summary>
        public IEnumerable<ContentItem> AllVersions {
            get { return ContentManager.GetAllVersions(Id); }
        }

        /// <summary>
        /// This method tells us whether something should be done for a specific part.
        /// </summary>
        /// <typeparam name="TPart">The type of the ContentPart being tested.</typeparam>
        /// <returns>The method returns true if the part requires some processing. False if there
        /// is nothing to do.</returns>
        /// <remarks>The ShouldProcess generic methods are designed to be used in handlers to figure
        /// out whether the Part or Field has been configured for processing. The variants for the
        /// ContentParts are also used in the default implementation of the filters used in the 
        /// base handler.</remarks>
        public bool ShouldProcess<TPart>() where TPart : ContentPart {
            // if the item is protected, we should do nothing
            if (GDPRPart == null || GDPRPart.IsProtected) {
                return false;
            }

            return ShouldProcess(ContentItem.As<TPart>());
        }

        /// <summary>
        /// This method tells us whether something should be done for a specific part.
        /// </summary>
        /// <param name="part">The ContentPart being tested.</typeparam>
        /// <returns>The method returns true if the part requires some processing. False if there
        /// is nothing to do.</returns>
        /// <remarks>The ShouldProcess generic methods are designed to be used in handlers to figure
        /// out whether the Part or Field has been configured for processing. The variants for the
        /// ContentParts are also used in the default implementation of the filters used in the 
        /// base handler.</remarks>
        public bool ShouldProcess(ContentPart part) {
            // if the item is protected, we should do nothing
            if (GDPRPart == null || GDPRPart.IsProtected) {
                return false;
            }

            // can't anonymize the part if it does not exist
            if (part == null) {
                return false;
            }

            // we should always run the handlers for GDPRPart, because they are the ones
            // that allow us to run filters for the ContentFields, or generically for the
            // whole ContentItem
            if (part.GetType() == typeof(GDPRPart)) {
                return true;
            }

            // Check the TypePartDefinition for the part to see whether it's configured to be
            // processed.
            var settings = part.TypePartDefinition.Settings.GetModel<GDPRPartPartSettings>();

            return settings != null // sanity check
                && ((settings.ShouldAnonymize && !Erase) // anonymization
                || (settings.ShouldErase && Erase)); //erasure
        }

        /// <summary>
        /// This method tells us whether something should be done for the field specified by
        /// its type and its name.
        /// </summary>
        /// <typeparam name="TField">The type of the ContentField being tested.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="part">The ContentPart this field is in, or null if the ContentField is
        /// directly assigned to the ContentItem.</param>
        /// <returns>The method returns true if the field requires some processing. False if there
        /// is nothing to do.</returns>
        /// <remarks>The ShouldProcess generic methods are designed to be used in handlers to figure
        /// out whether the Part or Field has been configured for processing. The variants for the
        /// ContentParts are also used in the default implementation of the filters used in the 
        /// base handler.</remarks>
        public bool ShouldProcess<TField>(string fieldName, ContentPart part = null)
            where TField : ContentField {
            // if the item is protected, we should do nothing
            if (GDPRPart == null || GDPRPart.IsProtected) {
                return false;
            }

            // Check whether the field exists in the part. If part == null, search in
            // the fields configured for the ContentItem (they are in a ContentPart whose name
            // matches the ContentType)
            if (part == null) {
                part = ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == ContentItem.ContentType);
            }
            if (part == null) {
                // we should never be here. This is a bad error condition.
                return false; // we should probably throw an exception here, because this condition is messed up.
            }
            var field = part.Fields
                .FirstOrDefault(fi => fi is TField && fi.Name == fieldName);
            return ShouldProcess(field);
        }

        /// <summary>
        /// This method tells us whether something should be done for the field passed as
        /// argument.
        /// </summary>
        /// <param name="field">The ContentField we are testing for.</param>
        /// <returns>The method returns true if the field requires some processing. False if there
        /// is nothing to do.</returns>
        /// <remarks>The ShouldProcess generic methods are designed to be used in handlers to figure
        /// out whether the Part or Field has been configured for processing. The variants for the
        /// ContentParts are also used in the default implementation of the filters used in the 
        /// base handler.</remarks>
        public bool ShouldProcess(ContentField field) {
            // if the item is protected, we should do nothing
            if (GDPRPart == null || GDPRPart.IsProtected) {
                return false;
            }
            // can't anonymize the field if it's not there
            if (field == null) {
                return false;
            }

            // Check the settings to see whether the field should be anonymized
            var settings = field.PartFieldDefinition.Settings.GetModel<GDPRPartFieldSettings>();

            return settings != null // sanity check
                && ((settings.ShouldAnonymize && !Erase) // anonymization
                || (settings.ShouldErase && Erase)); //erasure
        }
    }
}