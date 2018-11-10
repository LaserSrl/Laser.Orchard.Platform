using Laser.Orchard.Faq.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Faq.Handlers
{
    public class FaqHandler: ContentHandler
    {
        public FaqHandler(IRepository<FaqPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            OnRemoved<FaqPart>((context, part) => repository.Delete(part.Record));

            OnIndexing<FaqPart>((context, contactPart) => context.DocumentIndex.Add("faq_question", contactPart.Question).Analyze().Store());
        }
    }
}