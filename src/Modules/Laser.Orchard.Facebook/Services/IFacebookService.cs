using Facebook;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.Facebook.Services {

    public interface IFacebookService : IDependency {

        ResponseAction PostFacebook(FacebookPostPart facebookpart);

        List<FacebookAccountPart> GetValidFacebookAccount();

        List<FacebookAccountPart> Facebook_GetAccessToken(FacebookPostPart facebookpart);
    }

    public class ResponseAction {

        public ResponseAction() {
            this.Success = true;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class FacebookService : IFacebookService {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContext;
        private readonly ShellSettings _shellSettings;

        public FacebookService(ShellSettings shellSettings, IOrchardServices orchardServices, INotifier notifier, IWorkContextAccessor workContext) {
            _orchardServices = orchardServices;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _workContext = workContext;
            _shellSettings = shellSettings;
        }

        public List<FacebookAccountPart> GetValidFacebookAccount() {
            List<FacebookAccountVM> listaccount = new List<FacebookAccountVM>();
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
            return _orchardServices.ContentManager.Query().ForType(new string[] { "SocialFacebookAccount" }).ForPart<FacebookAccountPart>().List().Where(x => x.Valid == true && (x.Shared || x.IdUser == currentiduser)).ToList();
        }





        public ResponseAction PostFacebook(FacebookPostPart facebookpart) {
            ResponseAction rsp = new ResponseAction();
            List<FacebookAccountPart> FacebookAccountSettings = Facebook_GetAccessToken(facebookpart);
            string accessToken = "";
            string pageId = "";
            foreach (FacebookAccountPart Faccount in FacebookAccountSettings) {
                try {
                    string idalbum = "";
                    string linktosend = "";
                    string PostInArea = "feed";
                    if (string.IsNullOrEmpty(Faccount.IdPage)) {
                        accessToken = Faccount.UserToken;
                        pageId = "";
                    }
                    else {
                        accessToken = Faccount.PageToken;
                        pageId = "";
                    }
                    var objFacebookClient = new FacebookClient(accessToken);

                    #region [Get List of Album and if no album create one]
                    if (facebookpart.ContentItem.ContentType == "CommunicationAdvertising") {
                        string TitoloAlbum = "Nessun Album";
                        Int32 idcampagna = (Int32)(((dynamic)facebookpart.ContentItem).CommunicationAdvertisingPart.CampaignId);
                        if (idcampagna > 0) {
                            ContentItem campagna = _orchardServices.ContentManager.Get(idcampagna);
                            TitoloAlbum = campagna.As<TitlePart>().Title;
                        }
                        else {
                            TitoloAlbum = "Flash";
                        }

                        
                        dynamic albums = objFacebookClient.Get("me/albums");
                        foreach (dynamic albumInfo in albums.data) {
                            if (TitoloAlbum == albumInfo.name) {
                                idalbum = albumInfo.id;
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(idalbum)) {
                            #region [crea album]
                            var albumParameters = new Dictionary<string, object>();
                            albumParameters["name"] = TitoloAlbum;
                            dynamic myalbum = objFacebookClient.Post("/me/albums", albumParameters);
                            idalbum = myalbum.id;
                            #endregion
                        }
                    }
                    #endregion

                    var parameters = new Dictionary<string, object>();
                    if (facebookpart.FacebookType == FacebookType.Post) {
                        #region [Post Style]
                        if (facebookpart.ContentItem.ContentType == "CommunicationAdvertising") {
                            ICommunicationService _communicationService;
                            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                            if (tryed) {
                                linktosend = _communicationService.GetCampaignLink("Facebook", facebookpart);
                            }
                            else
                                linktosend = "";
                        }
                        else
                            linktosend = facebookpart.FacebookLink;
                        if (!string.IsNullOrEmpty(linktosend))
                            parameters["message"] = facebookpart.FacebookMessageToPost + " " + linktosend;
                        else
                            parameters["message"] = facebookpart.FacebookMessageToPost;
                        if (facebookpart.HasImage) {
                            if (!string.IsNullOrEmpty(facebookpart.FacebookIdPicture)) {
                                var ContentImage = _orchardServices.ContentManager.Get(Convert.ToInt32(facebookpart.FacebookIdPicture), VersionOptions.Published);
                                string Media_MimeType = ContentImage.As<MediaPart>().MimeType;
                                string Media_FileName = ContentImage.As<MediaPart>().FileName;
                                //string contentTypeMediaUrl = urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                                var mediaPath = HostingEnvironment.IsHosted
                                             ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                             : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                                string folderpath = ContentImage.As<MediaPart>().FolderPath;
                                if (!string.IsNullOrEmpty(folderpath))
                                    folderpath += "\\";
                                string physicalPath = mediaPath + _shellSettings.Name + "\\" + folderpath + ContentImage.As<MediaPart>().FileName;
                                byte[] photo = System.IO.File.ReadAllBytes(physicalPath);
                                parameters["source"] = new FacebookMediaObject {
                                    ContentType = Media_MimeType, //"image/jpeg",
                                    FileName = Path.GetFileName(Media_FileName)
                                }.SetValue(photo);
                                parameters["access_token"] = accessToken;
                               // if (!string.IsNullOrEmpty(Faccount.IdPage))
                               //     pageId = Faccount.IdPage;

                                PostInArea = idalbum+"/photos";
                            }
                        }
                        #endregion
                    }
                    else {
                        #region [Share Link style]
                        if (!string.IsNullOrEmpty(facebookpart.FacebookMessage))
                            parameters["message"] = facebookpart.FacebookMessage;
                        if (!string.IsNullOrEmpty(facebookpart.FacebookCaption))
                            parameters["caption"] = facebookpart.FacebookCaption;
                        if (!string.IsNullOrEmpty(facebookpart.FacebookDescription))
                            parameters["description"] = facebookpart.FacebookDescription;
                        if (!string.IsNullOrEmpty(facebookpart.FacebookName))
                            parameters["name"] = facebookpart.FacebookName;

                        if (facebookpart.ContentItem.ContentType == "CommunicationAdvertising") {
                            ICommunicationService _communicationService;
                            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                            if (tryed) {
                                linktosend = _communicationService.GetCampaignLink("Facebook", facebookpart);
                            }
                            else
                                linktosend = "";
                        }
                        else
                            linktosend = facebookpart.FacebookLink;
                        if (!string.IsNullOrEmpty(linktosend)) {
                            parameters["link"] = linktosend;
                            if (!string.IsNullOrEmpty(facebookpart.FacebookPicture))
                                parameters["picture"] = facebookpart.FacebookPicture;
                        }
                        else {
                            if (!string.IsNullOrEmpty(facebookpart.FacebookIdPicture)) {
                                var ContentImage = _orchardServices.ContentManager.Get(Convert.ToInt32(facebookpart.FacebookIdPicture), VersionOptions.Published);
                                string Media_MimeType = ContentImage.As<MediaPart>().MimeType;
                                string Media_FileName = ContentImage.As<MediaPart>().FileName;
                                //string contentTypeMediaUrl = urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                                var mediaPath = HostingEnvironment.IsHosted
                                             ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                             : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                                string folderpath = ContentImage.As<MediaPart>().FolderPath;
                                if (!string.IsNullOrEmpty(folderpath))
                                    folderpath += "\\";
                                string physicalPath = mediaPath + _shellSettings.Name + "\\" + folderpath + ContentImage.As<MediaPart>().FileName;
                                byte[] photo = System.IO.File.ReadAllBytes(physicalPath);
                                parameters["source"] = new FacebookMediaObject {
                                    ContentType = Media_MimeType, //"image/jpeg",
                                    FileName = Path.GetFileName(Media_FileName)
                                }.SetValue(photo);
                                parameters["access_token"] = accessToken;
                              //  if (!string.IsNullOrEmpty(Faccount.IdPage))
                              //      pageId = Faccount.IdPage;
                                PostInArea = idalbum + "/photos";
                            }
                        }
                        #endregion
                    }


                    if (pageId != "") {
                        pageId += "/";
                    }
                    var objresponsePost = objFacebookClient.Post(pageId + PostInArea, parameters);
                    rsp.Message = "Facebook Posted";
                }
                catch (Exception ex) {
                    rsp.Success = false;
                    rsp.Message = "Facebook Posting Error Message: " + ex.Message;
                }
                _notifier.Add(NotifyType.Information, T(rsp.Message));
            }
            return rsp;
        }

        private void Postmultipleimages() {
            //POST /me/books.reads?
            //book=http://www.example.com/book/09485736/&amp;
            //image[0][url]=http://www.example.com/09485736-cover.jpg&amp;
            //image[0][user_generated]=true&amp;
            //image[1][url]=http://www.example.com/recipes/09485736-art.jpg&amp;
            //image[1][user_generated]=true&amp;
            //access_token=VALID_ACCESS_TOKEN
        }

        public List<FacebookAccountPart> Facebook_GetAccessToken(FacebookPostPart facebookpart) {
            List<FacebookAccountPart> allparts = _orchardServices.ContentManager.Query().ForType(new string[] { "SocialFacebookAccount" }).ForPart<FacebookAccountPart>().List().Where(x => x.Valid == true).ToList();
            return allparts.Where(x => facebookpart.AccountList.Contains(x.Id)).ToList();
        }
    }
}