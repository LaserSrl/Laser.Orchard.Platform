using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Twitterizer;

namespace Laser.Orchard.Twitter.Services {

    public interface ITwitterService : IDependency {

        ResponseAction PostTwitter(PostToTwitterViewModel message);

        List<TwitterAccountPart> GetValidTwitterAccount();
    }

    public class ResponseAction {

        public ResponseAction() {
            this.Success = true;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class TwitterService : ITwitterService {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContext;
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly IStorageProvider _storageProvider;
        private readonly ShellSettings _shellsettings;
        public ILogger Logger { get; set; }

        public TwitterService(ShellSettings shellsettings, IStorageProvider storageProvider, IOrchardServices orchardServices, INotifier notifier, IWorkContextAccessor workContext, IProviderConfigurationService providerConfigurationService) {
            _shellsettings = shellsettings;
            _providerConfigurationService = providerConfigurationService;
            _storageProvider = storageProvider;
            _orchardServices = orchardServices;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _workContext = workContext;
            Logger = NullLogger.Instance;
        }

        public List<TwitterAccountPart> GetValidTwitterAccount() {
            List<TwitterAccountVM> listaccount = new List<TwitterAccountVM>();
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
            return _orchardServices.ContentManager.Query().ForType(new string[] { "SocialTwitterAccount" }).ForPart<TwitterAccountPart>().List().Where(x => x.Valid == true && (x.Shared || x.IdUser == currentiduser)).ToList();
        }

        public ResponseAction PostTwitter(PostToTwitterViewModel message) {
            ResponseAction rsp = new ResponseAction();
            rsp.Success = true;
            bool trysended = false;
            List<TwitterAccountPart> TwitterAccountSettings = Twitter_GetAccessToken(message.AccountList);
            var pcr = _providerConfigurationService.Get("Twitter");
            foreach (TwitterAccountPart Faccount in TwitterAccountSettings) {
                try {
                    trysended = true;
                    OAuthTokens accesstoken = new OAuthTokens() {
                        AccessToken = Faccount.UserToken,
                        AccessTokenSecret = Faccount.UserTokenSecret,
                        ConsumerKey = pcr.ProviderIdKey,
                        ConsumerSecret = pcr.ProviderSecret
                    };
                    TwitterResponse<TwitterStatus> response;
                    string realmessage = message.Message;
                    if (!string.IsNullOrEmpty(message.Link))
                        realmessage += " " + message.Link;
                    if (string.IsNullOrEmpty(message.Picture)) {
                        response = TwitterStatus.Update(accesstoken, realmessage.Trim() );
                    } else {
                        var mediaPath = HostingEnvironment.IsHosted
                                ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                        string physicalPath = mediaPath + _shellsettings.Name + "\\" + message.Picture;
                        byte[] photo = System.IO.File.ReadAllBytes(physicalPath);
                        response = TwitterStatus.UpdateWithMedia(accesstoken, realmessage.Trim(), photo, new StatusUpdateOptions() {  UseSSL = true, APIBaseAddress = "http://api.twitter.com/1.1/" });
                    }
                    if (response.Result != RequestResult.Success) {
                        if (response.Content != null)
                            Logger.Error("response.Content:" + response.Content);
                        rsp.Success = false;
                        if (response.ErrorMessage != null) {
                            Logger.Error("response.ErrorMessage:" + response.ErrorMessage);
                            _notifier.Add(NotifyType.Error, T("Can't post on twitter: {0} {1}", Faccount.DisplayAs, response.ErrorMessage));
                        } else {
                            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            var jsondict = serializer.Deserialize<Dictionary<string, object>>(response.Content);
                            ArrayList errors = (ArrayList)jsondict["errors"];
                            foreach (System.Collections.Generic.Dictionary<string, object> error in errors) {
                                string errormsg = "";
                                foreach (var errordict in error) {
                                    errormsg += " " + errordict.Key.ToString() + ":" + errordict.Value.ToString();
                                }
                                _notifier.Add(NotifyType.Error, T("Can't post on twitter: {0} {1}", Faccount.DisplayAs, errormsg));
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error("Twitter Posting Error Message::" + ex.Message);
                    rsp.Success = false;
                    rsp.Message = "Twitter Posting Error Message: " + ex.Message;
                    _notifier.Add(NotifyType.Error, T("Twitter Posting {0}  Error Message: {1}", Faccount.DisplayAs, ex.Message));
                }
            }
            if (trysended && rsp.Success)
                _notifier.Add(NotifyType.Information, T("Twitter posted"));
            return rsp;
        }

        private List<TwitterAccountPart> Twitter_GetAccessToken(Int32[] AccountList) {
            List<TwitterAccountPart> allparts = _orchardServices.ContentManager.Query().ForType(new string[] { "SocialTwitterAccount" }).ForPart<TwitterAccountPart>().List().Where(x => x.Valid == true).ToList();
            return allparts.Where(x => AccountList.Contains(x.Id)).ToList();
        }
    }
}