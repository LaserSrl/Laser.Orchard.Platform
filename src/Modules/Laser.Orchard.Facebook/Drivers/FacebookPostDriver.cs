using AutoMapper;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.Settings;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.MediaLibrary.Models;
using Orchard.Environment.Configuration;
using Orchard.UI.Admin;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;

namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookPostDriver : ContentPartCloningDriver<FacebookPostPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSettings;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookPostDriver(ShellSettings shellSettings, IOrchardServices orchardServices, IFacebookService facebookService,
                                  ITokenizer tokenizer, IContentManager contentManager) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            _shellSettings = shellSettings;
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
             _contentManager=contentManager;
        }

        protected override DriverResult Display(FacebookPostPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Summary") {
                    return ContentShape("Parts_FacebookPost",
                        () => shapeHelper.Parts_FacebookPost(SendOnNextPublish: part.SendOnNextPublish, Sent: part.FacebookMessageSent));
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }

        protected override DriverResult Editor(FacebookPostPart part, dynamic shapeHelper) {


            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            FacebookPostVM vm = new FacebookPostVM();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<FacebookPostPart, FacebookPostVM>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map(part, vm);
            if (string.IsNullOrEmpty(vm.FacebookType.ToString()))
                vm.FacebookType = FacebookType.Post;
            FacebookPostPartSettingVM setting = part.Settings.GetModel<FacebookPostPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            if (!string.IsNullOrEmpty(setting.FacebookCaption)) {
                vm.ShowFacebookCaption = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookDescription)) {
                vm.ShowFacebookDescription = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookLink)) {
                vm.ShowFacebookLink = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookMessage)) {
                vm.ShowFacebookMessage = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookName)) {
                vm.ShowFacebookName = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookPicture)) {
                vm.ShowFacebookPicture = false;
            }
            else
                vm.FacebookPicture = part.FacebookPicture;
            List<FacebookAccountPart> listaccount = _facebookService.GetValidFacebookAccount();
            //   List<SelectListItem> lSelectList = new List<SelectListItem>();
            List<OptionList> optionList = new List<OptionList>();
            foreach (FacebookAccountPart fa in listaccount) {
                //  lSelectList.Insert(0, new SelectListItem() { Value = fa.Id.ToString(), Text = fa.AccountType + " - " + fa.DisplayAs });
                OptionList ol = new OptionList {
                    Value = fa.Id.ToString(),
                    Text = fa.AccountType + " - " + fa.DisplayAs,
                    ImageUrl = urlHelper.Content("~/Media/" + _shellSettings.Name + "/facebook_" + fa.UserIdFacebook + ".jpg"),
                    Selected = part.AccountList.Contains(fa.Id) ? "selected=\"selected\"" : ""
                };
                optionList.Add(ol);
            }
            vm.ListOption = optionList;

            return ContentShape("Parts_FacebookPost",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/FacebookPost",
                                    Model: vm,
                                    Prefix: Prefix));
        }



        protected override DriverResult Editor(FacebookPostPart part, IUpdateModel updater, dynamic shapeHelper) {
            FacebookPostPartSettingVM setting = part.Settings.GetModel<FacebookPostPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            FacebookPostVM vm = new FacebookPostVM();
            updater.TryUpdateModel(vm, Prefix, null, null);

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<FacebookPostVM, FacebookPostPart>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map(vm, part);
            if (_orchardServices.WorkContext.HttpContext.Request.Form["FacebookType"] != null && _orchardServices.WorkContext.HttpContext.Request.Form["FacebookType"] == "1")
                part.FacebookType = FacebookType.Post;
            else
                part.FacebookType = FacebookType.ShareLink;

            if (vm.SelectedList != null && vm.SelectedList.Count() > 0) {
                part.AccountList = vm.SelectedList.Select(x => Int32.Parse(x)).ToArray();
            }
            return Editor(part, shapeHelper);
        }

        protected override void Cloning(FacebookPostPart originalPart, FacebookPostPart clonePart, CloneContentContext context) {
            clonePart.FacebookMessage = originalPart.FacebookMessage;
            //do not clone FacebookMessageSent so that we can send it in the cloned post
            clonePart.FacebookMessageSent = false;
            clonePart.FacebookCaption = originalPart.FacebookCaption;
            clonePart.FacebookDescription = originalPart.FacebookDescription;
            clonePart.FacebookName = originalPart.FacebookName;
            clonePart.FacebookPicture = originalPart.FacebookPicture;
            clonePart.FacebookIdPicture = originalPart.FacebookIdPicture;
            clonePart.FacebookLink = originalPart.FacebookLink;
            clonePart.SendOnNextPublish = false;
            clonePart.AccountList = originalPart.AccountList;
            clonePart.FacebookType = originalPart.FacebookType;
            clonePart.FacebookMessageToPost = originalPart.FacebookMessageToPost;
            clonePart.HasImage = originalPart.HasImage;
        }

        protected override void Exporting(FacebookPostPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("FacebookMessage", part.FacebookMessage);
            //FacebookMessageSent non serve per l'import
            root.SetAttributeValue("FacebookCaption", part.FacebookCaption);
            root.SetAttributeValue("FacebookDescription", part.FacebookDescription);
            root.SetAttributeValue("FacebookName", part.FacebookName);
            root.SetAttributeValue("FacebookPicture", part.FacebookPicture);
            root.SetAttributeValue("FacebookIdPicture", part.FacebookIdPicture);
            root.SetAttributeValue("FacebookLink", part.FacebookLink);
            //SendOnNextPublish non serve per l'import
            root.SetAttributeValue("FacebookType", part.FacebookType.ToString());
            root.SetAttributeValue("FacebookMessageToPost", part.FacebookMessageToPost);
            root.SetAttributeValue("HasImage", part.HasImage);
            if (part.AccountList.Count() > 0) {
                XElement accountElement = null;
                foreach (var accountId in part.AccountList) {
                    var fbAccount = _contentManager.Get<FacebookAccountPart>(accountId);
                    if (fbAccount != null) {
                        accountElement = new XElement("Account", _contentManager.GetItemMetadata(fbAccount.ContentItem).Identity.ToString());
                        root.Add(accountElement);
                    }
                }
            }  
        }   

        protected override void Importing(FacebookPostPart part, ImportContentContext context) 
        {
            var root = context.Data.Element(part.PartDefinition.Name);
            var FacebookMessage = root.Attribute("FacebookMessage");
            if (FacebookMessage != null) {
                part.FacebookMessage = FacebookMessage.Value;
            }
            part.FacebookMessageSent = false;
            var FacebookCaption = root.Attribute("FacebookCaption");
            if (FacebookCaption != null) {
                part.FacebookCaption = FacebookCaption.Value;
            }
            var FacebookDescription = root.Attribute("FacebookDescription");
            if (FacebookDescription != null) {
                part.FacebookDescription = FacebookDescription.Value;
            }
            var FacebookName = root.Attribute("FacebookName");
            if (FacebookName != null) {
                part.FacebookName = FacebookName.Value;
            }
            var FacebookPicture = root.Attribute("FacebookPicture");
            if (FacebookPicture != null) {
                part.FacebookPicture = FacebookPicture.Value;
            }
            var FacebookIdPicture = root.Attribute("FacebookIdPicture");
            if (FacebookIdPicture != null) {
                part.FacebookIdPicture = FacebookIdPicture.Value;
            }
            var FacebookLink = root.Attribute("FacebookLink");
            if (FacebookLink != null) {
                part.FacebookLink = FacebookLink.Value;
            }
            part.SendOnNextPublish = false;
            var FacebookMessageToPost = root.Attribute("FacebookMessageToPost");
            if (FacebookMessageToPost != null) {
                part.FacebookMessageToPost = FacebookMessageToPost.Value;
            }
            var HasImage = root.Attribute("HasImage");
            if (HasImage != null) {
                part.HasImage = bool.Parse(HasImage.Value);
            }
            var FacebookTypeElement = root.Attribute("FacebookType");
            if (FacebookTypeElement != null) {
                part.FacebookType = (FacebookType)(Enum.Parse(typeof(FacebookType), FacebookTypeElement.Value));
            }
            if (root.HasElements) {
                List<int> accountList = new List<int>();
                foreach (var userElement in root.Elements("Account")) {
                    var fbAccount = context.GetItemFromSession(userElement.Value);
                    if (fbAccount != null && fbAccount.Has<FacebookAccountPart>()) {
                        accountList.Add(fbAccount.Id);
                    }
                }
                if (accountList.Count > 0) {
                    part.AccountList = accountList.ToArray();
                }
            }
            //// old version
            //var importedAccountSelected = context.Data.Element(part.PartDefinition.Name).Elements("AccountList");

            //if (importedAccountSelected != null && importedAccountSelected.Count() > 0) {
                
            //    var listaccount = _facebookService.GetValidFacebookAccount();
            //    int countAcc= 0;
            //    int[] accSel = new int[importedAccountSelected.Count()];
                
            //    foreach (var selectedAcc in importedAccountSelected) {                   
            //        var tempPartFromid = selectedAcc.Attribute("AccountListSelected").Value;
            //        var addltsin =  listaccount.Where(x => _contentManager.GetItemMetadata(_contentManager.Get(x.Id)).Identity.ToString() == tempPartFromid.ToString()).Select(x => x.Id).ToArray();
            //        accSel[countAcc]=addltsin[0];
            //        countAcc=countAcc + 1;
                    
            //    }
            //    part.AccountList = accSel;
            //}
        }
    }
}