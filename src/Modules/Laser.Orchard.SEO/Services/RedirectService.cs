using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.SEO.Models;
using Orchard.Data;
using Laser.Orchard.SEO.Exceptions;
using Orchard.Localization;
using Orchard.Caching;
using Orchard.Logging;

namespace Laser.Orchard.SEO.Services {
    public class RedirectService : IRedirectService {
        private readonly IRepository<RedirectRule> _repository;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private Dictionary<string, RedirectRule> _redirectCache;
        public RedirectService(
            IRepository<RedirectRule> repository,
            ICacheManager cacheManager,
            ISignals signals) {

            _repository = repository;
            _cacheManager = cacheManager;
            _signals = signals;
            _redirectCache = new Dictionary<string, RedirectRule>();
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IQueryable<RedirectRule> GetTable() {
            return _repository.Table;
        }
        public IEnumerable<RedirectRule> GetRedirects(int startIndex = 0, int pageSize = 0) {
            var result = _repository.Table.Skip(startIndex >= 0 ? startIndex : 0);

            if (pageSize > 0) {
                return RedirectRule.Copy(result.Take(pageSize));
            }
            return RedirectRule.Copy(result.ToList());
        }

        public IEnumerable<RedirectRule> GetRedirects(int[] itemIds) {
            return RedirectRule.Copy(_repository.Fetch(x => itemIds.Contains(x.Id)));
        }

        public RedirectRule Update(RedirectRule redirectRule) {
            //FixRedirect(redirectRule);
            if (GetSameSourceUrlIds(redirectRule).Any(id => id != redirectRule.Id)) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Update(redirectRule);
            _signals.Trigger("Laser.Orchard.Redirects.Changed");
            return redirectRule;
        }

        public RedirectRule Add(RedirectRule redirectRule) {
            //FixRedirect(redirectRule);
            if (GetSameSourceUrlIds(redirectRule).Any()) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Create(redirectRule);
            ClearCache();
            return redirectRule;
        }

        public void Delete(RedirectRule redirectRule) {
            Delete(redirectRule.Id);
            ClearCache();
        }

        public void Delete(int id) {
            var redirect = _repository.Get(id);
            _repository.Delete(redirect);
            ClearCache();
        }


        public IEnumerable<RedirectRule> GetCachedRedirects() {
            try {
                return _cacheManager.Get(
                    "Laser.Orchard.Redirects",
                    ctx => {
                        ctx.Monitor(_signals.When("Laser.Orchard.Redirects.Changed"));
                        return _repository.Table.ToList();
                    });
            }
            catch (Exception ex) {
                Logger.Error(ex, "An unexpected error occurred in GetRedirects method");
                return new List<RedirectRule>();
            }
        }

        public void ClearCache() {
            _signals.Trigger("Laser.Orchard.Redirects.Changed");
        }

        private IEnumerable<int> GetSameSourceUrlIds(RedirectRule redirectRule) {
            try {
                return _repository.Table
                    .Where(rr => rr.SourceUrl == redirectRule.SourceUrl)
                    .ToList() //need to force execution of the query, so that it can fail in sqlCE
                    .Select(rr => rr.Id);
            }
            catch (Exception) {
                //sqlCE doe not support using strings properly when their length is such that the column
                //in the record is of type ntext.
                var rules = _repository.Fetch(rr =>
                    rr.SourceUrl.StartsWith(redirectRule.SourceUrl) &&
                    rr.SourceUrl.EndsWith(redirectRule.SourceUrl));
                return rules.ToList()
                    .Where(rr => rr.SourceUrl == redirectRule.SourceUrl)
                    .Select(rr => rr.Id);
            }
        }

        private static void FixRedirect(RedirectRule redirectRule) {
            redirectRule.SourceUrl = redirectRule.SourceUrl.TrimStart('/');
            redirectRule.DestinationUrl = redirectRule.DestinationUrl.TrimStart('/');
        }


    }
}