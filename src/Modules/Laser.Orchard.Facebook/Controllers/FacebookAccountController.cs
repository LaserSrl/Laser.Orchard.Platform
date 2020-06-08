using Facebook;
using Laser.Orchard.Facebook.Helpers;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.ViewModels;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Orchard.FileSystems.Media;
using System.Web.Hosting;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.Facebook.Controllers {

    public class FacebookAccountController : Controller, IUpdateModel {
        private readonly ShellSettings _shellSettings;
        private readonly IStorageProvider _storageProvider;
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "SocialFacebookAccount";
        private readonly dynamic TestPermission = Permissions.ManageFacebookAccount;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public FacebookAccountController(
             ShellSettings shellSettings,
            IStorageProvider storageProvider,
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
            IProviderConfigurationService providerConfigurationService) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _providerConfigurationService = providerConfigurationService;
            _storageProvider = storageProvider;
            _shellSettings = shellSettings;

        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                //  model = _orchardServices.ContentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
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
            _notifier.Add(NotifyType.Information, T("Facebook Account Added"));
            return RedirectToAction("Edit", new { id = content.Id });
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            _orchardServices.ContentManager.Remove(content);

            return RedirectToAction("Index", "FacebookAccount");
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
            bool hasAdminPermission = _orchardServices.Authorizer.Authorize(Permissions.AdminFacebookAccount);
            if (hasAdminPermission)
                ListContent = contentQuery.List();
            else
                ListContent = contentQuery.List().Where(x => x.As<FacebookAccountPart>().IdUser == currentiduser);

            if (!string.IsNullOrEmpty(search.Expression))
                ListContent = from content in ListContent
                              where
                              ((content.As<FacebookAccountPart>().AccountType + " - " + content.As<FacebookAccountPart>().DisplayAs ?? "").Contains(expression, StringComparison.InvariantCultureIgnoreCase))
                              select content;
            IEnumerable<ContentIndexVM> listVM = ListContent.Select(p => new ContentIndexVM {
                Id = p.Id,
                Title = p.As<FacebookAccountPart>().AccountType + " - " + p.As<FacebookAccountPart>().DisplayAs,// string.IsNullOrEmpty(p.As<FacebookAccountPart>().PageName) ? "User Account" : " Page -> " + p.As<FacebookAccountPart>().PageName,
                ModifiedUtc = p.As<CommonPart>().ModifiedUtc,
                UserName = p.As<CommonPart>().Owner.UserName,
                Option = new { Valid = p.As<FacebookAccountPart>().Valid, Shared = p.As<FacebookAccountPart>().Shared, Image = Url.Content("~/Media/" + _shellSettings.Name + "/facebook_" + p.As<FacebookAccountPart>().UserIdFacebook + ".jpg") }
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

        [Admin]
        public ActionResult GetPostTokenFacebook() {
            var pcr = _providerConfigurationService.Get("Facebook");
            if (pcr == null) {
                _notifier.Add(NotifyType.Error, T("No facebook account setting added, add one in Settings -> Open Authentication"));
                return RedirectToAction("Index", "FacebookAccount", new { area = "Laser.Orchard.Facebook", id = -10 });
            }
            string app_id = pcr.ProviderIdKey;
            string app_secret = pcr.ProviderSecret;
            string scope = "publish_actions,manage_pages,publish_pages,user_photos";//user_status status_updated nelle extended permission

            if (Request["code"] == null) {
                string url = string.Format(
                    "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}",
                    app_id, Request.Url.AbsoluteUri, scope);
                Response.Redirect(url, false);
            } else {
                Dictionary<string, string> tokens = new Dictionary<string, string>();

                string url = string.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}&code={3}&client_secret={4}",
                    app_id, Request.Url.AbsoluteUri, scope, Request["code"].ToString(), app_secret);

                HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
                string access_token = "";
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string vals = reader.ReadToEnd();
                    var json = JObject.Parse(vals);
                    if ((json["access_token"]).Type != JTokenType.Null)
                         access_token =( json["access_token"]??"").ToString();
                }
                var client = new FacebookClient(access_token);

                //  FacebookPostSettingPart getpart = _orchardServices.WorkContext.CurrentSite.As<FacebookPostSettingPart>();
                //  getpart.FacebookAccessToken = access_token;
                object taskresult = client.Get("/me");
                var result = (IDictionary<string, object>)taskresult;
                string facebookUserId = (string)result["id"];
                string facebookUserName = (string)result["name"];

                FacebookAccountVM fvm = new FacebookAccountVM();
                fvm.UserToken = access_token;
                fvm.UserIdFacebook = facebookUserId;
                fvm.UserName = facebookUserName;
                OrchardRegister(fvm);


                JsonObject jsonResponse = client.Get("me/accounts") as JsonObject;

                Uri profilePictureUrl = new Uri(string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", facebookUserId, "small", access_token));
                var mediaPath = HostingEnvironment.IsHosted
                    ? HostingEnvironment.MapPath("~/Media/") ?? ""
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                WebClient webClient = new WebClient();
                webClient.DownloadFile(profilePictureUrl, mediaPath + _shellSettings.Name + @"\facebook_" + facebookUserId + ".jpg");

                Dictionary<string, string> ElencoPagine = new Dictionary<string, string>();
                foreach (var account in (JsonArray)jsonResponse["data"]) {
                    string accountName = (string)(((JsonObject)account)["name"]);
                    fvm = new FacebookAccountVM();
                    fvm.UserToken = access_token;
                    fvm.PageName = accountName;
                    fvm.PageToken = (string)(((JsonObject)account)["access_token"]);
                    fvm.IdPage = (string)(((JsonObject)account)["id"]);
                    profilePictureUrl = new Uri(string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", fvm.IdPage, "small", access_token));
                    webClient = new WebClient();
                    webClient.DownloadFile(profilePictureUrl, mediaPath + _shellSettings.Name + @"\facebook_" + fvm.IdPage + ".jpg");
                    fvm.UserIdFacebook = fvm.IdPage;
                    fvm.UserName = accountName;
                    OrchardRegister(fvm);
                }
                return RedirectToAction("Index", "FacebookAccount", new { area = "Laser.Orchard.Facebook", id = -10 });
            }
            return null;
        }

        private void OrchardRegister(FacebookAccountVM fvm) {
            string displayas = "";
            string AccountType = "";
            if (string.IsNullOrEmpty(fvm.PageName)) {
                string json = new WebClient().DownloadString("https://graph.facebook.com/me?access_token=" + fvm.UserToken);
                displayas = (JObject.Parse(json))["name"].ToString();
                AccountType = "User";
            } else {
                displayas = fvm.PageName;
                AccountType = "Page";
            }

            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType);
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
            fvm.IdPage = fvm.IdPage ?? "";
            Int32 elementi = contentQuery.List().Where(x => x.As<FacebookAccountPart>().IdUser == currentiduser && (x.As<FacebookAccountPart>().DisplayAs == displayas)).Count();
            if (elementi > 0) {
                if (string.IsNullOrEmpty(fvm.IdPage)) {
                    _notifier.Add(NotifyType.Warning, T("User Facebook Account can't be added, is duplicated"));
                } else {
                    _notifier.Add(NotifyType.Warning, T("Facebook Page {0} can't be added, is duplicated", fvm.PageName));
                }
            } else {


                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                newContent.As<FacebookAccountPart>().IdUser = currentiduser;
                newContent.As<FacebookAccountPart>().AccountType = AccountType;
                newContent.As<FacebookAccountPart>().DisplayAs = displayas;
                newContent.As<FacebookAccountPart>().SocialName = "Facebook";
                newContent.As<FacebookAccountPart>().UserToken = fvm.UserToken;
                newContent.As<FacebookAccountPart>().Valid = false;
                newContent.As<FacebookAccountPart>().PageName = fvm.PageName;
                newContent.As<FacebookAccountPart>().PageToken = fvm.PageToken;
                newContent.As<FacebookAccountPart>().IdPage = fvm.IdPage ?? "";
                newContent.As<FacebookAccountPart>().Shared = false;
                newContent.As<FacebookAccountPart>().UserIdFacebook = fvm.UserIdFacebook ?? "";
                newContent.As<FacebookAccountPart>().UserName = fvm.UserName ?? "";

                if (string.IsNullOrEmpty(fvm.IdPage)) {
                    _notifier.Add(NotifyType.Warning, T("User Facebook Account added"));
                } else {
                    _notifier.Add(NotifyType.Warning, T("Facebook Page {0} added", fvm.PageName));
                }
            }
        }
    }
}