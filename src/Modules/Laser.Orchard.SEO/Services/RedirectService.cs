using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.SEO.Models;
using Orchard.Data;
using Laser.Orchard.SEO.Exceptions;
using Orchard.Localization;

namespace Laser.Orchard.SEO.Services {
    public class RedirectService : IRedirectService {
        private readonly IRepository<RedirectRule> _repository;
        public RedirectService(
            IRepository<RedirectRule> repository) {

            _repository = repository;

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

        private IEnumerable<int> GetSameSourceUrlIds(RedirectRule redirectRule) {
            try {
                return _repository.Table
                    .Where(rr => rr.SourceUrl == redirectRule.SourceUrl)
                    .ToList() //need to force execution of the query, so that it can fail in sqlCE
                    .Select(rr => rr.Id);
            } catch (Exception) {
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
            return redirectRule;
        }

        public RedirectRule Add(RedirectRule redirectRule) {
            //FixRedirect(redirectRule);
            if (GetSameSourceUrlIds(redirectRule).Any()) {
                throw new RedirectRuleDuplicateException(T("Rules with same SourceURL are not valid."));
            }
            _repository.Create(redirectRule);
            return redirectRule;
        }

        public void Delete(RedirectRule redirectRule) {
            Delete(redirectRule.Id);
        }

        public void Delete(int id) {
            var redirect = _repository.Get(id);

            _repository.Delete(redirect);
        }

        public RedirectRule GetRedirect(string path) {
            path = path.TrimStart('/');
            try {
                var rule = _repository.Get(x => x.SourceUrl == path);
                return rule == null ? null :
                    RedirectRule.Copy(rule);
            } catch (Exception) {
                //sqlCE doe not support using strings properly when their length is such that the column
                //in the record is of type ntext.
                var rules = _repository.Fetch(rr => 
                    rr.SourceUrl.StartsWith(path) && rr.SourceUrl.EndsWith(path));
                var rule = rules.ToList().Where(rr => rr.SourceUrl == path).FirstOrDefault();
                return rule == null ? null :
                    RedirectRule.Copy(rule);
            }
        }

        public RedirectRule GetRedirect(int id) {
            var rule = _repository.Get(id);
            return rule == null ? null :
                RedirectRule.Copy(rule);
        }
        
        private static void FixRedirect(RedirectRule redirectRule) {
            redirectRule.SourceUrl = redirectRule.SourceUrl.TrimStart('/');
            redirectRule.DestinationUrl = redirectRule.DestinationUrl.TrimStart('/');
        }

    }
}