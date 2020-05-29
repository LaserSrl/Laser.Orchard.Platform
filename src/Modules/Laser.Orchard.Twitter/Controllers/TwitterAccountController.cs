using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.Twitter.Helpers;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using Twitterizer;

namespace Laser.Orchard.Twitter.Controllers {

    public class TwitterAccountController : Controller, IUpdateModel {
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "SocialTwitterAccount";
        private readonly dynamic TestPermission = Permissions.ManageTwitterAccount;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public TwitterAccountController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager
                , IProviderConfigurationService providerConfigurationService
            , ShellSettings shellSettings
            ) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _providerConfigurationService = providerConfigurationService;
            _shellSettings = shellSettings;
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                model = _contentManager.BuildEditor(newContent);
            } else
                model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent, VersionOptions.Draft);
                content = newContent;
            } else
                content = _orchardServices.ContentManager.Get(id, VersionOptions.DraftRequired);
            var model = _orchardServices.ContentManager.UpdateEditor(content, this);

            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }
            _contentManager.Publish(content);
            _notifier.Add(NotifyType.Information, T("Twitter Account Added"));
            return RedirectToAction("Edit", new { id = content.Id });
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            _orchardServices.ContentManager.Remove(content);
            return RedirectToAction("Index", "TwitterAccount");
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search, bool ShowVideo = false) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search, ShowVideo);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search, bool ShowVideo = false) {
            dynamic Options = new System.Dynamic.ExpandoObject();
            Options.ShowVideo = false;
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
            IEnumerable<ContentItem> ListContent;
            bool hasAdminPermission = _orchardServices.Authorizer.Authorize(Permissions.AdminTwitterAccount);
            if (hasAdminPermission)
                ListContent = contentQuery.List();
            else
                ListContent = contentQuery.List().Where(x => x.As<TwitterAccountPart>().IdUser == currentiduser);

            if (!string.IsNullOrEmpty(search.Expression))
                ListContent = from content in ListContent
                              where
                              ((content.As<TitlePart>().Title ?? "").Contains(expression, StringComparison.InvariantCultureIgnoreCase))
                              select content;
            IEnumerable<ContentIndexVM> listVM = ListContent.Select(p => new ContentIndexVM {
                Id = p.Id,
                Title = p.As<TwitterAccountPart>().AccountType + " - " + p.As<TwitterAccountPart>().DisplayAs,// string.IsNullOrEmpty(p.As<TwitterAccountPart>().PageName) ? "User Account" : " Page -> " + p.As<TwitterAccountPart>().PageName,
                ModifiedUtc = p.As<CommonPart>().ModifiedUtc,
                UserName = p.As<CommonPart>().Owner.UserName,
                Option = new {
                    Valid = p.As<TwitterAccountPart>().Valid,
                    Shared = p.As<TwitterAccountPart>().Shared,
                    Image = Url.Content("~/Media/" + _shellSettings.Name + "/twitter_" + p.As<TwitterAccountPart>().DisplayAs + ".jpg")
                }

            });
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(listVM.Count());
            var list = listVM.Skip(pager.GetStartIndex())
                                .Take(pager.PageSize);
            var model = new SearchIndexVM(list, search, pagerShape, Options);
            return View((object)model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }

        public class TwitAuthenticateResponse {
            public string token_type { get; set; }
            public string access_token { get; set; }
        }

        #region twitterizer

        public ActionResult GetPostTokenTwitter() {
            var pcr = _providerConfigurationService.Get("Twitter");
            if (pcr == null) {
                _notifier.Add(NotifyType.Error, T("No twitter account setting added, add one in Settings -> Open Authentication"));
                return RedirectToAction("Index", "TwitterAccount", new { area = "Laser.Orchard.Twitter", id = -10 });
            }

            string consumerKey = pcr.ProviderIdKey;
            string consumerSecret = pcr.ProviderSecret;
            // il meccanismo utilizzato è il 3-Legged oAuth
            if (Request["oauth_token"] == null) {
                string tmpreq = Request.Url.AbsoluteUri;
                OAuthTokenResponse reqToken = OAuthUtility.GetRequestToken(consumerKey, consumerSecret, tmpreq);
                Response.Redirect(string.Format("https://api.twitter.com/oauth/authorize?oauth_token={0}", reqToken.Token));
            } else {
                string requestToken = Request["oauth_token"].ToString();
                string verifier = Request["oauth_verifier"].ToString();
                var tokens = OAuthUtility.GetAccessToken(consumerKey, consumerSecret, requestToken, verifier);
                TwitterAccountVM vm = new TwitterAccountVM();
                vm.DisplayAs = tokens.ScreenName;
                vm.UserToken = tokens.Token;
                vm.UserTokenSecret = tokens.TokenSecret; // conterrà l'account_token_secret
                #region [recupero immagine]
                OAuthTokens accessToken = new OAuthTokens();
                accessToken.AccessToken = vm.UserToken;
                accessToken.AccessTokenSecret = vm.UserTokenSecret;
                accessToken.ConsumerKey = consumerKey;
                accessToken.ConsumerSecret = consumerSecret;

                TwitterResponse<TwitterUser> myTwitterUser = TwitterUser.Show(accessToken, tokens.ScreenName);
                TwitterUser user = myTwitterUser.ResponseObject;
                var profilePictureUrl = user.ProfileImageLocation;
                var mediaPath = HostingEnvironment.IsHosted
                 ? HostingEnvironment.MapPath("~/Media/") ?? ""
                 : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                WebClient webClient = new WebClient();
                webClient.DownloadFile(profilePictureUrl, mediaPath + _shellSettings.Name + @"\twitter_" + vm.DisplayAs + ".jpg");
                #endregion
                // var avatarFormat = "https://api.twitter.com/1.1/users/show.json?screen_name={0}";
                // var avatarUrl = string.Format(avatarFormat, vm.DisplayAs);
                // HttpWebRequest avatarRequest = (HttpWebRequest)WebRequest.Create(avatarUrl);
                // var timelineHeaderFormat = "{0} {1}";
                // avatarRequest.Headers.Add("Authorization", String.Format("Bearer {0}", vm.UserToken));
                //// avatarRequest.Headers.Add("Authorization",
                ////                             string.Format(timelineHeaderFormat, "oauth_token", requestToken));
                // avatarRequest.Method = "Get";
                // WebResponse timeLineResponse = avatarRequest.GetResponse();

                // var reader = new StreamReader(timeLineResponse.GetResponseStream());
                // var avatarJson = string.Empty;
                //using (authResponse) {
                //    using (var reader = new StreamReader(timeLineResponse.GetResponseStream())) {
                //        avatarJson = reader.ReadToEnd();
                //    }
                //}

                // Uri profilePictureUrl = new Uri(string.Format("https://api.twitter.com/1.1/users/show.json?screen_name={1}", vm.DisplayAs ));

                OrchardRegister(vm);
            }
            return RedirectToAction("Index", "TwitterAccount", new { area = "Laser.Orchard.Twitter", id = -10 });
        }

        private void OrchardRegister(TwitterAccountVM fvm) {
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType);
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;

            Int32 elementi = contentQuery.List().Where(x => x.As<TwitterAccountPart>().IdUser == currentiduser && x.As<TwitterAccountPart>().DisplayAs == fvm.DisplayAs).Count();
            if (elementi > 0) {
                _notifier.Add(NotifyType.Warning, T("User Twitter Account can't be added, is duplicated"));
            } else {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                newContent.As<TwitterAccountPart>().AccountType = "User";
                newContent.As<TwitterAccountPart>().IdUser = currentiduser;
                newContent.As<TwitterAccountPart>().DisplayAs = fvm.DisplayAs;
                newContent.As<TwitterAccountPart>().SocialName = "Twitter";
                newContent.As<TwitterAccountPart>().UserToken = fvm.UserToken;
                newContent.As<TwitterAccountPart>().Valid = true;
                newContent.As<TwitterAccountPart>().Shared = false;
                newContent.As<TwitterAccountPart>().UserTokenSecret = fvm.UserTokenSecret;
                _notifier.Add(NotifyType.Warning, T("User Twitter Account added"));
            }
        }

        #endregion twitterizer
    }
}