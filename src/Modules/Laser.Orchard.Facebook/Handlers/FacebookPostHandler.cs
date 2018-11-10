using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Localization;
using System;
using System.Web.Hosting;
using System.IO;
using Orchard.Environment.Configuration;
using System.Web.Mvc;
using Orchard.Mvc.Extensions;
using System.Collections.Generic;
using Laser.Orchard.Facebook.Settings;
using Orchard.Tokens;
using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.Facebook.Handlers {

    public class FacebookPostHandler : ContentHandler {
        private readonly IFacebookService _facebookService;
        private readonly ITokenizer _tokenizer;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }
        private readonly IOrchardServices _orchardServices;
        public FacebookPostHandler(ITokenizer tokenizer,IRepository<FacebookPostPartRecord> repository, IFacebookService facebookService, IOrchardServices orchardServices,INotifier notifier) {
            _facebookService = facebookService;
            _orchardServices = orchardServices;
            _tokenizer = tokenizer;
            _notifier=notifier;
              T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));
            
            OnUpdated<FacebookPostPart>((context, part) => {
                #region [calcola i token e li sovrascrive messo qui e non nell'edit perchè non si sa in che ordine viene eseguita la parte e di conseguenza se i valori dei token riferiti ad altre parti sono aggiornati]
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                FacebookPostPartSettingVM setting = part.Settings.GetModel<FacebookPostPartSettingVM>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                if (!string.IsNullOrEmpty(setting.FacebookCaption)) {
                    part.FacebookCaption = _tokenizer.Replace(setting.FacebookCaption, tokens);
                }
                if (!string.IsNullOrEmpty(setting.FacebookDescription)) {
                    part.FacebookDescription = _tokenizer.Replace(setting.FacebookDescription, tokens);
                }
                if (!string.IsNullOrEmpty(setting.FacebookLink)) {
                    part.FacebookLink = _tokenizer.Replace(setting.FacebookLink, tokens);
                                   }
                if (!string.IsNullOrEmpty(setting.FacebookMessage)) {
                    part.FacebookMessage = _tokenizer.Replace(setting.FacebookMessage, tokens);
                     }
                if (!string.IsNullOrEmpty(setting.FacebookName)) {
                    part.FacebookName = _tokenizer.Replace(setting.FacebookName, tokens);
                 
                }
                if (!string.IsNullOrEmpty(setting.FacebookPicture)) {
                 
                    string idimg = _tokenizer.Replace(setting.FacebookPicture, tokens);
                    Int32 idimage = 0;

                    Int32.TryParse(idimg.Replace("{", "").Replace("}", "").Split(',')[0], out idimage); ;
                    if (idimage > 0) {
                        var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                        part.FacebookIdPicture = idimage.ToString();
                        part.FacebookPicture = urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                    }
                    else {
                        part.FacebookPicture = "";
                        part.FacebookIdPicture = "";
                    }

                }
                #endregion;
            });

            OnPublished<FacebookPostPart>((context, facebookpart) => {
                try {
                    bool publishEnabled = true;
                  //  string linktosend = "";
                    if (facebookpart.ContentItem.ContentType == "CommunicationAdvertising") {
                            ICommunicationService _communicationService;
                            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                            publishEnabled = _communicationService.AdvertisingIsAvailable(facebookpart.Id);
                            if (!publishEnabled) {
                                _notifier.Add(NotifyType.Error, T("Advertising can't be published, see campaign validation date"));
                            }
                    }

                    if (publishEnabled) {
                          if (facebookpart.SendOnNextPublish && !facebookpart.FacebookMessageSent) {
                              List<FacebookAccountPart> FacebookAccountSettings = _facebookService.Facebook_GetAccessToken(facebookpart);
                              if (FacebookAccountSettings.Count > 0) {
                                  ResponseAction rsp = _facebookService.PostFacebook(facebookpart);
                                  if (rsp.Success) {
                                      facebookpart.FacebookMessageSent = true;
                                  }
                                  else
                                      _notifier.Add(NotifyType.Error, T("Facebook error:" + rsp.Message));
                              }
                        }
                    }
                }
                catch(Exception ex) {
                    _notifier.Add(NotifyType.Error,T("Facebook error:"+ex.Message));
                }
            }
         );
        }
    }
}