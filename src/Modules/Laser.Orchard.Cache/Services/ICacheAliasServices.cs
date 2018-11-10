using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Cache.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment;

using Orchard.OutputCache.Models;

namespace Laser.Orchard.Cache.Services {

    public interface ICacheAliasServices : ISingletonDependency {

        CacheRouteConfig GetByUrl(string url);

        void RefreshCachedRouteConfig();

        CacheUrlSetting Setting();
    }

    public class CacheAliasServices : ICacheAliasServices {
        public static List<CacheRouteConfig> CachedRouteConfig;
        private CacheUrlSetting _setting;
        public CacheUrlSetting Setting() {
            return _setting;
        }
        public void RefreshCachedRouteConfig() {
            var defaultMaxAge = _orchardServices.WorkContext.CurrentSite.As<CacheSettingsPart>().DefaultMaxAge;
            IRepository<CacheUrlRecord> _tmpcacheUrlRepository;
            _orchardServices.WorkContext.TryResolve<IRepository<CacheUrlRecord>>(out _tmpcacheUrlRepository);
            IRepository<CacheUrlSetting> _tmpcacheUrlSettingRepository;
            _orchardServices.WorkContext.TryResolve<IRepository<CacheUrlSetting>>(out _tmpcacheUrlSettingRepository);
            if (_tmpcacheUrlSettingRepository.Fetch(x => true).Count() == 0) {
                _tmpcacheUrlSettingRepository.Create(new CacheUrlSetting {
                    ActiveLog = false,
                    PreventDefaultAuthenticatedCache = false,
                    PreventDefaultNotContentItemAuthenticatedCache = false
                });
                _tmpcacheUrlSettingRepository.Flush();
            }
            _setting = _tmpcacheUrlSettingRepository.Fetch(x => true).FirstOrDefault();
            if (_tmpcacheUrlRepository.Fetch(x => x.CacheURL == "/private/").FirstOrDefault() == null) {
                _tmpcacheUrlRepository.Create(new CacheUrlRecord {
                    CacheDuration = 0,
                    CacheGraceTime = 0,
                    Priority = 10,
                    CacheURL = "/private/",
                    CacheToken = "{User.Id}"
                });
                _tmpcacheUrlRepository.Flush();
            }

            if (_tmpcacheUrlRepository.Fetch(x => x.CacheURL == "user+info").FirstOrDefault() == null) {
                _tmpcacheUrlRepository.Create(new CacheUrlRecord {
                    CacheDuration = 0,
                    CacheGraceTime = 0,
                    Priority = 10,
                    CacheURL = "user+info",
                    CacheToken = "{User.Id}"
                });
                _tmpcacheUrlRepository.Flush();
            }
            if (_tmpcacheUrlRepository.Fetch(x => x.CacheURL == "user info").FirstOrDefault() == null) {
                _tmpcacheUrlRepository.Create(new CacheUrlRecord {
                    CacheDuration = 0,
                    CacheGraceTime = 0,
                    Priority = 10,
                    CacheURL = "user info",
                    CacheToken = "{User.Id}"
                });
                _tmpcacheUrlRepository.Flush();
            }
            CachedRouteConfig = _tmpcacheUrlRepository.Table.OrderByDescending(y => y.Priority).Select(w => new CacheRouteConfig {
                Duration = w.CacheDuration,
                GraceTime = w.CacheGraceTime,
                Priority = w.Priority,
                FeatureName = "CacheUrl",
                MaxAge = defaultMaxAge,
                RouteKey = w.CacheURL ?? "",
                Url = w.CacheToken ?? ""
            }).ToList();
        }

        public IOrchardServices _orchardServices { get; set; }

        public CacheAliasServices(OrchardServices orchardServices) {
            /// Important  this class and CacheURLAdminController class 
            /// are not on same pipeline 
            /// so don't use dependency injection for IRepository<CacheUrlRecord>
            _orchardServices = orchardServices;
            RefreshCachedRouteConfig();
        }

        public CacheRouteConfig GetByUrl(string url) {
            if (CachedRouteConfig != null) {
                url = HttpUtility.UrlDecode(url);
                return CachedRouteConfig.Where(x => url.ToLower().Contains(x.RouteKey)).OrderByDescending(w => w.Priority).FirstOrDefault();
            }
            return null;
        }
    }
}