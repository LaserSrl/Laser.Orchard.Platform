using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.PartSettings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;

namespace Laser.Orchard.ShareLink.Servicies {

    public interface IShareLinkService : IDependency {
        string GetImgUrl(string idimg);
        void FillPart(ShareLinkPart part);
    }

    public class ShareLinkService : IShareLinkService {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;

        public ShareLinkService(IOrchardServices orchardServicies, ITokenizer tokenizer) {
            _orchardServices = orchardServicies;
            _tokenizer = tokenizer;
        }

        private string RemoveHtmlTag(string text) {
            if (string.IsNullOrEmpty(text))
                return "";
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(text);
            return (htmlDoc.DocumentNode.InnerText);
        }
        private string TruncateText(string value, int length = 0) {
            if (string.IsNullOrWhiteSpace(value) || value.Length < length || length <= 0) {
                return value;
            }

            value = value.Substring(0, value.IndexOf(" ", length));
            if (value.Length > length) {
                if (value.LastIndexOf(" ") != -1) { //try to be nice and truncate aftrer a word
                    value = value.Substring(0, value.LastIndexOf(" "));
                } else { //just shear it off to avoid breaking stuff db-side
                    value = value.Substring(0, length);
                }
            }
            return value;
        }
        private string ProcessString(string value, bool removeHtml) {
            return ProcessString(value, 0, removeHtml);
        }
        private string ProcessString(string value, int length = 0, bool removeHtml = false) {
            if (removeHtml) {
                value = HttpUtility.HtmlDecode(value);
                value = RemoveHtmlTag(value);
            }
            return TruncateText(value, length);
        }
        private string FillString(string first, string second, Dictionary<string, object> tokens) {
            if (!string.IsNullOrEmpty(first)) {
                return _tokenizer.Replace(first, tokens);
            }
            if (!string.IsNullOrEmpty(second)) {
                return _tokenizer.Replace(second, tokens);
            }
            return string.Empty;
        }

        public void FillPart(ShareLinkPart part) {
            var moduleSetting = _orchardServices.WorkContext.CurrentSite.As<ShareLinkModuleSettingPart>();
            var partSetting = part.Settings.GetModel<ShareLinkPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };

            if ((!partSetting.ShowBodyChoise) || string.IsNullOrWhiteSpace(part.SharedBody)) {
                var s = FillString(partSetting.SharedBody, moduleSetting.SharedBody, tokens);
                part.SharedBody = ProcessString(s, true);
            }
            if ((!partSetting.ShowTextChoise) || string.IsNullOrWhiteSpace(part.SharedText)) {
                var s = FillString(partSetting.SharedText, moduleSetting.SharedText, tokens);
                if (!string.IsNullOrWhiteSpace(s)) {
                    part.SharedText = ProcessString(s);
                }
            }
            if ((!partSetting.ShowLinkChoise) || string.IsNullOrWhiteSpace(part.SharedLink)) {
                var s = FillString(partSetting.SharedLink, moduleSetting.SharedLink, tokens);
                if (!string.IsNullOrWhiteSpace(s)) {
                    part.SharedLink = ProcessString(s);
                }
            }

            string ListId = "";
            if (!(partSetting.ShowImageChoise)) {
                if (!string.IsNullOrEmpty(partSetting.SharedImage)) {
                    ListId = _tokenizer.Replace(partSetting.SharedImage, tokens);
                    part.SharedImage = GetImgUrl(ListId);

                } else {
                    if (!string.IsNullOrEmpty(moduleSetting.SharedImage)) {
                        ListId = _tokenizer.Replace(moduleSetting.SharedImage, tokens);
                        part.SharedImage = GetImgUrl(ListId);
                    }
                }
                
                part.SharedIdImage = part.SharedImage?.Replace("{", "").Replace("}", "") ?? "";
                part.SharedImage = GetImgUrl(part.SharedIdImage);
            }
        }

        public string GetImgUrl(string idimg) {
            if (idimg != null) {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                Int32 idimage = 0;
                string[] contentListId = idimg.Replace("{", "").Replace("}", "").Split(',');
                for (int i = 0; i < contentListId.Length; i++) {
                    Int32.TryParse(contentListId[i], out idimage);
                    if (idimage > 0) {
                        var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                        if (ContentImage != null) {
                            return urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                        }
                    } else {
                        return idimg;   // non ho passato un id e quindi sarà un link  
                    }
                }
            }
            return ""; //idimg null o non ci sono immagini pubblicate
        }
    }
}