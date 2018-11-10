using Laser.Orchard.GDPR.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.GDPR.Handlers {
    public abstract class GDPRFilterBase<TPart> : IContentGDPRFilter where TPart : ContentPart {

        protected virtual void Anonymizing(AnonymizeContentContext context, TPart instance) { }
        protected virtual void Anonymized(AnonymizeContentContext context, TPart instance) { }

        protected virtual void Erasing(EraseContentContext context, TPart instance) { }
        protected virtual void Erased(EraseContentContext context, TPart instance) { }

        void IContentGDPRFilter.Anonymized(AnonymizeContentContext context) {
            if (context.ContentItem.Is<GDPRPart>() && context.ContentItem.Is<TPart>()) {
                Anonymized(context, context.ContentItem.As<TPart>());
            }
        }

        void IContentGDPRFilter.Anonymizing(AnonymizeContentContext context) {
            if (context.ContentItem.Is<GDPRPart>() && context.ContentItem.Is<TPart>()) {
                Anonymizing(context, context.ContentItem.As<TPart>());
            }
        }

        void IContentGDPRFilter.Erased(EraseContentContext context) {
            if (context.ContentItem.Is<GDPRPart>() && context.ContentItem.Is<TPart>()) {
                Erased(context, context.ContentItem.As<TPart>());
            }
        }

        void IContentGDPRFilter.Erasing(EraseContentContext context) {
            if (context.ContentItem.Is<GDPRPart>() && context.ContentItem.Is<TPart>()) {
                Erasing(context, context.ContentItem.As<TPart>());
            }
        }
    }
}