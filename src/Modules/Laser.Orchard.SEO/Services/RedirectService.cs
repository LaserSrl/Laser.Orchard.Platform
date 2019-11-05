using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.SEO.Models;
using Orchard.Data;
using Laser.Orchard.SEO.Exceptions;
using Orchard.Localization;

namespace Laser.Orchard.SEO.Services {
    public class RedirectService : IRedirectService {
        private readonly IRepository<RedirectRule> _repository;
        private Dictionary<string, RedirectRule> _redirectCache;
        public RedirectService(
            IRepository<RedirectRule> repository) {

            _repository = repository;
            _redirectCache = new Dictionary<string, RedirectRule>();
            ReloadRedirectsCache();
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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

        public int GetRedirectsTotalCount() {
            return _repository.Table.Count();
        }

        private void ReloadRedirectsCache() {
            _redirectCache.Clear();
            foreach (var rule in _repository.Table) {
                _redirectCache.Add(rule.SourceUrl, rule);
            }
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

        public RedirectRule Update(RedirectRule redirectRule) {
            //FixRedirect(redirectRule);
            if (GetSameSourceUrlIds(redirectRule).Any(id => id != redirectRule.Id)) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Update(redirectRule);
            ReloadRedirectsCache();
            return redirectRule;
        }

        public RedirectRule Add(RedirectRule redirectRule) {
            //FixRedirect(redirectRule);
            if (GetSameSourceUrlIds(redirectRule).Any()) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Create(redirectRule);
            ReloadRedirectsCache();
            return redirectRule;
        }

        public void Delete(RedirectRule redirectRule) {
            Delete(redirectRule.Id);
        }

        public void Delete(int id) {
            var redirect = _repository.Get(id);
            _repository.Delete(redirect);
            ReloadRedirectsCache();
        }

        public RedirectRule GetRedirect(string path) {
            if (_redirectCache.ContainsKey(path)) {
                return _redirectCache[path];
            }
            else {
                return null;
            }
        }

        public RedirectRule GetRedirect(int id) {
            var rule = _repository.Get(id);
            return rule == null ? null :
                RedirectRule.Copy(rule);
        }

        public void ClearCache() {
            ReloadRedirectsCache();
        }

        public int CountCached() {
            return _redirectCache.Count;
        }

        private static void FixRedirect(RedirectRule redirectRule) {
            redirectRule.SourceUrl = redirectRule.SourceUrl.TrimStart('/');
            redirectRule.DestinationUrl = redirectRule.DestinationUrl.TrimStart('/');
        }

    }
}