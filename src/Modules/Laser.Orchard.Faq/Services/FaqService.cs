using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Faq.Models;
using Laser.Orchard.Faq.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Laser.Orchard.Faq.Services
{
    public class FaqService : IFaqService
    {
        private readonly IRepository<FaqPartRecord> _faqRepository;
        private readonly IContentManager _contentManager;
        private int _typeIdFilter = -1;

        public FaqService(IRepository<FaqPartRecord> faqRepository, IContentManager contentManager)
        {
            _faqRepository = faqRepository;
            _contentManager = contentManager;
        }

        public int FilterByTypeId
        {
            get { return _typeIdFilter; }
            set { _typeIdFilter = value; }
        }

        //public List<FaqPart> GetFaqs()
        //{
        //    return _FaqRepository.Table.ToList();
        //}

        public IEnumerable<FaqPart> GetLastFaqs(int? count =null, int? page = null)
        {
            IContentQuery<FaqPart, FaqPartRecord> query = _contentManager.Query<FaqPart, FaqPartRecord>(VersionOptions.Published);

            if (_typeIdFilter != -1)
            {
                query = query.Where<FaqPartRecord>(t => t.FaqTypeId == _typeIdFilter);
            }


            var publishedNews = query
                .Where<CommonPartRecord>(p => p.PublishedUtc != null)
                .OrderByDescending(p => p.PublishedUtc)
                .List<FaqPart>();




            if (page != null)
            {
                if (count != null)
                {
                    var temp = publishedNews.Skip((page.Value - 1)*count.Value)
                        .Take(count.Value);
                    return temp.ToList();
                }
                else
                {
                    var temp = publishedNews;
                    return temp.ToList();
                }
            }

            if (count != null)
                return publishedNews.Take(count.Value);
            return publishedNews;
        }

        public IEnumerable<FaqPart> GetTypedFaqs(int faqTypeId)
        {
            IContentQuery<FaqPart, FaqPartRecord> query = _contentManager.Query<FaqPart, FaqPartRecord>(VersionOptions.Published);

            var publishedNews = query.Where<CommonPartRecord>(p => p.PublishedUtc != null)
                                        .OrderByDescending(p => p.PublishedUtc)
                                        .Where<FaqPartRecord>(fp => fp.FaqTypeId == faqTypeId)
                                        .List<FaqPart>();

                return publishedNews;

        }

        public double GetCountOfPage(int count)
        {
            IContentQuery<FaqPart, FaqPartRecord> query = _contentManager.Query<FaqPart, FaqPartRecord>(VersionOptions.Published);

            if (_typeIdFilter != -1)
            {
                query = query.Where<FaqPartRecord>(t => t.FaqTypeId == _typeIdFilter);
            }

            var publishedNews = query
                .Where<CommonPartRecord>(p => p.PublishedUtc != null)
                .OrderBy(p => p.PublishedUtc)
                .List<FaqPart>();


            return Math.Ceiling(publishedNews.Count() / (float)count / 1.0);
        }

        public void UpdateFaqForContentItem(ContentItem item, EditFaqViewModel model)
        {
            var FaqPart = item.As<FaqPart>();
            FaqPart.Question = model.Question;
            FaqPart.FaqTypeId = model.FaqType;
            _faqRepository.Flush();
        }
    }
}