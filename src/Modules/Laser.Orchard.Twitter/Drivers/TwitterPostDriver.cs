using AutoMapper;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.Settings;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Mvc.Html;
using System.Globalization;
using Orchard.MediaProcessing.Services;
using Orchard.MediaProcessing.Models;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Linq.Expressions;
using Orchard.Environment.Configuration;
using Orchard.UI.Admin;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Twitter.Drivers {

    public class TwitterPostDriver : ContentPartCloningDriver<TwitterPostPart> {
        private readonly IImageProfileManager _imageProfileManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ITwitterService _TwitterService;
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly ITokenizer _tokenizer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IStorageProvider _storageProvider;
        private readonly ShellSettings _shellSettings;
       
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;

        protected override string Prefix {
            get { return "Laser.Orchard.Twitter"; }
        }

        public TwitterPostDriver(IImageProfileManager imageProfileManager, IStorageProvider storageProvider, IOrchardServices orchardServices, ITwitterService TwitterService, 
                                 IProviderConfigurationService providerConfigurationService, ITokenizer tokenizer, 
                                 IHttpContextAccessor httpContextAccessor, IControllerContextAccessor controllerContextAccessor,
                                 ShellSettings shellSettings, IContentManager contentManager) {
            _storageProvider = storageProvider;
            _imageProfileManager = imageProfileManager;
            _httpContextAccessor = httpContextAccessor;
            _controllerContextAccessor = controllerContextAccessor;
            _tokenizer = tokenizer;
            _orchardServices = orchardServices;
            _TwitterService = TwitterService;
            _providerConfigurationService = providerConfigurationService;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(TwitterPostPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin)
            {
                if ((displayType == "Detail") || (displayType == "Summary"))
                {
                    var pcr = _providerConfigurationService.Get("Twitter");
                    TwitterOgVM vm = new TwitterOgVM();
                    if (pcr != null)
                        vm.Site = pcr.UserIdentifier;
                    TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
                    var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                    if (!string.IsNullOrEmpty(setting.Description))
                        vm.Description = _tokenizer.Replace(setting.Description, tokens);
                    else
                        vm.Description = part.TwitterDescription;
                    if (!string.IsNullOrEmpty(setting.Image))
                    {
                        string ids = _tokenizer.Replace(setting.Image, tokens);

                        int idimage;
                        Int32.TryParse(ids.Replace("{", "").Replace("}", "").Split(',')[0], out idimage); ;
                        if (idimage > 0)
                        {
                            // _orchardServices.ContentManager.Get(id);
                            // vm.Image = Url.ItemDisplayUrl(_orchardServices.ContentManager.Get(id));
                            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                            //       vm.Image = urlHelper.ItemDisplayUrl(_orchardServices.ContentManager.Get(id));// get current display link
                            //   Fvm.Link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(Twitterpart));// get current display link
                            var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                            //   var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                            //  part.TwitterPicture = pathdocument;// 
                            vm.Image = urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                            //   .ResizeMediaUrl(Width: previewWidth, Height: previewHeight, Mode: "crop", Alignment: "middlecenter", Path: Model.MediaData.MediaUrl)');
                        }
                        else
                            vm.Image = "";
                    }
                    else
                        vm.Image = part.TwitterPicture;
                    if (!string.IsNullOrEmpty(setting.Title))
                        vm.Title = _tokenizer.Replace(setting.Title, tokens);

                    else
                        vm.Title = part.TwitterTitle;
                    return ContentShape("Parts_TwitterPost_Detail",
                        () => shapeHelper.Parts_TwitterPost_Detail(Twitter: vm, SendOnNextPublish: part.SendOnNextPublish, TwitterMessageSent: part.TwitterMessageSent));
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
   
        protected override DriverResult Editor(TwitterPostPart part, dynamic shapeHelper) {
            TwitterPostVM vm = new TwitterPostVM();
            Mapper.Initialize(cfg => {
                cfg.CreateMap<TwitterPostPart, TwitterPostVM>();
            });
            Mapper.Map<TwitterPostPart, TwitterPostVM>(part, vm);
            TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
            if (!string.IsNullOrEmpty(setting.Title))
                vm.ShowTitle = false;
            if (!string.IsNullOrEmpty(setting.Description))
                vm.ShowDescription = false;
            if (!string.IsNullOrEmpty(setting.Image))
                vm.ShowPicture = false;
            if (setting.ShowTwitterCurrentLink)
                vm.ShowTwitterCurrentLink = true;
            else
                vm.ShowTwitterCurrentLink = false;
               List<TwitterAccountPart> listaccount = _TwitterService.GetValidTwitterAccount();
             List<OptionList> optionList=new List<OptionList>();
              var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            foreach (TwitterAccountPart fa in listaccount) {
                  OptionList ol = new OptionList {
                    Value = fa.Id.ToString(),
                    Text = fa.AccountType + " - " + fa.DisplayAs,
                    ImageUrl =  urlHelper.Content("~/Media/" + _shellSettings.Name + "/twitter_" +fa.DisplayAs + ".jpg"),
                    Selected = part.AccountList.Contains(fa.Id)?"selected=\"selected\"":""
                };
                optionList.Add(ol);
            }
            vm.ListOption = optionList;
           vm.SelectedList = part.AccountList.Select(x => x.ToString()).ToArray();
            return ContentShape("Parts_TwitterPost",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/TwitterPost",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(TwitterPostPart part, IUpdateModel updater, dynamic shapeHelper) {
            TwitterPostVM vm = new TwitterPostVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.Initialize(cfg => {
                cfg.CreateMap< TwitterPostVM, TwitterPostPart > ();
            });
            Mapper.Map<TwitterPostVM, TwitterPostPart>(vm, part);
            if (vm.SelectedList != null)
                part.AccountList = vm.SelectedList.Select(x => Int32.Parse(x)).ToArray();
            else
                part.AccountList = new Int32[] { };
            return Editor(part, shapeHelper);
        }

        protected override void Cloning(TwitterPostPart originalPart, TwitterPostPart clonePart, CloneContentContext context) {
            clonePart.TwitterMessage = originalPart.TwitterMessage;
            //TwitterMessageSent is set by handlers and if cloned would prevent sending the message
            clonePart.TwitterMessageSent = false;
            clonePart.TwitterTitle = originalPart.TwitterTitle;
            clonePart.TwitterDescription = originalPart.TwitterDescription;
            clonePart.TwitterPicture = originalPart.TwitterPicture;
            clonePart.TwitterCurrentLink = originalPart.TwitterCurrentLink;
            clonePart.TwitterLink = originalPart.TwitterLink;
            clonePart.SendOnNextPublish = false;
            clonePart.AccountList = originalPart.AccountList;
        }
        protected override void Importing(TwitterPostPart part, ImportContentContext context) {
            var importedTwitterMessage = context.Attribute(part.PartDefinition.Name, "TwitterMessage");
            if (importedTwitterMessage != null) {
                part.TwitterMessage = importedTwitterMessage;
            }

            part.TwitterMessageSent = false;

            var importedTwitterTitle = context.Attribute(part.PartDefinition.Name, "TwitterTitle");
            if (importedTwitterTitle != null) {
                part.TwitterTitle = importedTwitterTitle;
            }

            var importedTwitterDescription = context.Attribute(part.PartDefinition.Name, "TwitterDescription");
            if (importedTwitterDescription != null) {
                part.TwitterDescription = importedTwitterDescription;
            }

            var importedTwitterPicture = context.Attribute(part.PartDefinition.Name, "TwitterPicture");
            if (importedTwitterPicture != null) {
                part.TwitterPicture = importedTwitterPicture;
            }

            var importedTwitterCurrentLink = context.Attribute(part.PartDefinition.Name, "TwitterCurrentLink");
            if (importedTwitterCurrentLink != null) {
                part.TwitterCurrentLink = Convert.ToBoolean(importedTwitterCurrentLink);
            }

            var importedTwitterLink = context.Attribute(part.PartDefinition.Name, "TwitterLink");
            if (importedTwitterLink != null) {
                part.TwitterLink = importedTwitterLink;
            }
            part.SendOnNextPublish = false;

            var root = context.Data.Element(part.PartDefinition.Name);
            if (root.HasElements) {
                List<int> accountList = new List<int>();
                foreach (var userElement in root.Elements("Account")) {
                    var twAccount = context.GetItemFromSession(userElement.Value);
                    if (twAccount != null && twAccount.Has<TwitterAccountPart>()) {
                        accountList.Add(twAccount.Id);
                    }
                }
                if (accountList.Count > 0) {
                    part.AccountList = accountList.ToArray();
                }
            }
        }
        protected override void Exporting(TwitterPostPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("TwitterMessage", part.TwitterMessage);
            //TwitterMessageSent non serve per l'import
            root.SetAttributeValue("TwitterTitle", part.TwitterTitle);
            root.SetAttributeValue("TwitterDescription", part.TwitterDescription);
            root.SetAttributeValue("TwitterPicture", part.TwitterPicture);
            root.SetAttributeValue("TwitterCurrentLink", part.TwitterCurrentLink);
            root.SetAttributeValue("TwitterLink", part.TwitterLink);
            //SendOnNextPublish non serve per l'import
            if (part.AccountList.Count() > 0) {
                XElement accountElement = null;
                foreach (var accountId in part.AccountList) {
                    var twAccount = _contentManager.Get<TwitterAccountPart>(accountId);
                    if (twAccount != null) {
                        accountElement = new XElement("Account", _contentManager.GetItemMetadata(twAccount.ContentItem).Identity.ToString());
                        root.Add(accountElement);
                    }
                }
            }  
        }
    }
}