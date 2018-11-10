using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Handlers {
    /// <summary>
    /// This abstract should generally be implemtend and provide filters and methods for those
    /// parts and fields that require special care in anonymization or erasure, and thus cannot
    /// be handled simply by the default handlers provided. Implementers should take care to
    /// correctly handle all versions of the contents they are processing.
    /// </summary>
    public abstract class ContentGDPRHandler : IContentGDPRHandler {

        protected ContentGDPRHandler() {
            Filters = new List<IContentFilter>();
            Logger = NullLogger.Instance;
        }

        public List<IContentFilter> Filters { get; set; }
        public ILogger Logger { get; set; }

        /// <summary>
        /// Provide an Action that will be executed while anonymizing a ContentItem that
        /// has a ContentPart of the type provided. The Action will be executed if any of
        /// the following conditions is verified:
        /// 1. The type of the part is GDPRPart
        /// 2. The part is configured to be anonymized
        /// </summary>
        /// <typeparam name="TPart"></typeparam>
        /// <param name="handler"></param>
        protected void OnAnonymizing<TPart>(Action<AnonymizeContentContext, TPart> handler)
            where TPart : ContentPart {

            Filters.Add(new InlineGDPRFilter<TPart> { OnAnonymizing = handler });
        }

        protected void OnAnonymized<TPart>(Action<AnonymizeContentContext, TPart> handler)
            where TPart : ContentPart {

            Filters.Add(new InlineGDPRFilter<TPart> { OnAnonymized = handler });
        }

        protected void OnErasing<TPart>(Action<EraseContentContext, TPart> handler)
            where TPart : ContentPart {

            Filters.Add(new InlineGDPRFilter<TPart> { OnErasing = handler });
        }

        protected void OnErased<TPart>(Action<EraseContentContext, TPart> handler)
            where TPart : ContentPart {

            Filters.Add(new InlineGDPRFilter<TPart> { OnErased = handler });
        }

        class InlineGDPRFilter<TPart> : GDPRFilterBase<TPart> where TPart : ContentPart {
            public Action<AnonymizeContentContext, TPart> OnAnonymizing { get; set; }
            public Action<AnonymizeContentContext, TPart> OnAnonymized { get; set; }
            public Action<EraseContentContext, TPart> OnErasing { get; set; }
            public Action<EraseContentContext, TPart> OnErased { get; set; }

            /*
             * In the methods below, the Actions should be called every time they exist, and the
             * checks on whether a part should actually be processed are left to the handlers.
             * This may seem weird, because for example at this stage we can already verify a lot
             * of information from the context: however, there are parts that should always be 
             * processed, because they are designed to hold personal identifiable information.
             * We cannot know what those parts are beforehand, because we cannot go through all
             * of Orchard and mark each one in a clearly identifiable way.
             * */
             
            protected override void Anonymizing(AnonymizeContentContext context, TPart instance) {
                if (OnAnonymizing != null)
                    OnAnonymizing(context, instance);
            }
            protected override void Anonymized(AnonymizeContentContext context, TPart instance) {
                if (OnAnonymized != null)
                    OnAnonymized(context, instance);
            }
            protected override void Erasing(EraseContentContext context, TPart instance) {
                if (OnErasing != null)
                    OnErasing(context, instance);
            }
            protected override void Erased(EraseContentContext context, TPart instance) {
                if (OnErased != null)
                    OnErased(context, instance);
            }
        }

        void IContentGDPRHandler.Anonymizing(AnonymizeContentContext context) {
            foreach (var filter in Filters.OfType<IContentGDPRFilter>()) {
                filter.Anonymizing(context);
            }
            Anonymizing(context);
        }

        void IContentGDPRHandler.Anonymized(AnonymizeContentContext context) {
            foreach (var filter in Filters.OfType<IContentGDPRFilter>()) {
                filter.Anonymized(context);
            }
            Anonymized(context);
        }

        void IContentGDPRHandler.Erasing(EraseContentContext context) {
            foreach (var filter in Filters.OfType<IContentGDPRFilter>()) {
                filter.Erasing(context);
            }
            Erasing(context);
        }

        void IContentGDPRHandler.Erased(EraseContentContext context) {
            foreach (var filter in Filters.OfType<IContentGDPRFilter>()) {
                filter.Erased(context);
            }
            Erased(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        protected virtual void Anonymizing(AnonymizeContentContext context) { }
        protected virtual void Anonymized(AnonymizeContentContext context) { }
        protected virtual void Erasing(EraseContentContext context) { }
        protected virtual void Erased(EraseContentContext context) { }
    }
}