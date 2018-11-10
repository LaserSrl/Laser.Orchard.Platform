using Laser.Orchard.SEO.Models;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;


namespace Laser.Orchard.SEO.Services {


  public interface IFaviconService : IDependency {
    string GetFaviconUrl();
  }


  [OrchardFeature("Laser.Orchard.Favicon")]
  public class FaviconService : IFaviconService {


    private readonly IWorkContextAccessor _wca;
    private readonly ICacheManager _cacheManager;
    private readonly ISignals _signals;


    public FaviconService(IWorkContextAccessor wca, ICacheManager cacheManager, ISignals signals) {
      _wca = wca;
      _cacheManager = cacheManager;
      _signals = signals;
    }


    public string GetFaviconUrl() {
        try {
            return _cacheManager.Get(
                "Laser.Orchard.Favicon.Url",
                ctx => {
                    ctx.Monitor(_signals.When("Laser.Orchard.Favicon.Changed"));
                    var workContext = _wca.GetContext();
                    var faviconSettings =
                        (FaviconSettingsPart)workContext
                                                  .CurrentSite
                                                  .ContentItem
                                                  .Get(typeof(FaviconSettingsPart));
                    return faviconSettings.FaviconUrl;
                });
        } catch {
            return null;
        }
    }


  }
}