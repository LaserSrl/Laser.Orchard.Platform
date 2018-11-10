using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.Settings;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Tokens;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace Laser.Orchard.Twitter.Handlers {

    public class TwitterPostHandler : ContentHandler {
        private readonly ITwitterService _TwitterService;
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public TwitterPostHandler(ITokenizer tokenizer, IRepository<TwitterPostPartRecord> repository, ITwitterService TwitterService, IOrchardServices orchardServices, INotifier notifier) {
            _TwitterService = TwitterService;
            _orchardServices = orchardServices;
            _tokenizer = tokenizer;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));
            //    Filters.Add(new ActivatingFilter<TwitterPostPart>("CommunicationAdvertising"));
            OnUpdated<TwitterPostPart>((context, part) => {
                TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                if (!string.IsNullOrEmpty(setting.Description))
                    part.TwitterDescription = _tokenizer.Replace(setting.Description, tokens);
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                if (string.IsNullOrEmpty(setting.Image)) {
                    part.TwitterPicture = "";
                }
                else {
                    string listid = _tokenizer.Replace(setting.Image, tokens);
                    listid = listid.Replace("{", "").Replace("}", "");
                    Int32 idimage = 0;
                    Int32.TryParse(listid.Split(',')[0], out idimage);
                    if (idimage > 0) {
                        var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                        var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                        part.TwitterPicture = pathdocument;
                    }
                    else
                        part.TwitterPicture = "";
                }
                if (!string.IsNullOrEmpty(setting.Title))
                    part.TwitterTitle = _tokenizer.Replace(setting.Title, tokens);
            });
            OnPublished<TwitterPostPart>((context, Twitterpart) => {
                try {
                    PostToTwitterViewModel Fvm = new PostToTwitterViewModel();
                    Fvm.Message = Twitterpart.TwitterMessage;
                    if (Twitterpart.ContentItem.ContentType == "CommunicationAdvertising") {
                        ICommunicationService _communicationService;
                        bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                        if (tryed) {
                            Fvm.Link = _communicationService.GetCampaignLink("Twitter", Twitterpart);
                        }
                        else
                            Fvm.Link = "";
                    }
                    else
                        if (Twitterpart.TwitterCurrentLink) {
                            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                            Fvm.Link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(Twitterpart));// get current display link
                        }


                    Fvm.Picture = Twitterpart.TwitterPicture;

                    Fvm.AccountList = Twitterpart.AccountList;
                    if (Twitterpart.SendOnNextPublish && !Twitterpart.TwitterMessageSent) {
                        if (Twitterpart.AccountList.Length == 0) {
                            _notifier.Add(NotifyType.Warning, T("No Twitter account specified."));
                        }
                        else {
                            ResponseAction rsp = _TwitterService.PostTwitter(Fvm);
                            if (rsp.Success) {
                                Twitterpart.TwitterMessageSent = true;
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    _notifier.Add(NotifyType.Error, T("Twitter error:" + ex.Message));
                }
            });
        }
    }
}