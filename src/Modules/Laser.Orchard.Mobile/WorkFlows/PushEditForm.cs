using Laser.Orchard.Mobile.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Localization.Services;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.WorkFlows {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushEditForm : IFormProvider {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ICultureManager _cultureManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public PushEditForm(IShapeFactory shapeFactory, IPushNotificationService pushNotificationService, ICultureManager cultureManager) {
            Shape = shapeFactory;
            _pushNotificationService = pushNotificationService;
            T = NullLocalizer.Instance;
            _cultureManager = cultureManager;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ActionPush",
                        _Type: Shape.FieldSet(
                            Title: T("Send Push"),
                            _ddlDevice: Shape.SelectList(
                                Id: "allDevice",
                                Name: "allDevice",
                                Title: T("Device"),
                                Size: 10,
                                Multiple: false
                            ),
                            _UsersList: Shape.Textbox(
                                Id: "userId",
                                Name: "userId",
                                Title: T("Users list (ID / e-mail / username)"),
                                Description: T("Comma separated list of User IDs or e-mails or usernames (eg. 12,45,239). Tokenized."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                            _ddlLanguage: Shape.SelectList(
                                Id: "allLanguage",
                                Name: "allLanguage",
                                Title: T("Language"),
                                Size: 4,
                                Multiple: false
                            ),
                            _RecipientProd: Shape.Radio(
                                Id: "Produzione",
                                Name: "Produzione",
                                Value: "Produzione",
                                Title: T("Produzione")
                            ),
                            _RecipientProd2: Shape.Radio(
                                Id: "Produzione",
                                Name: "Produzione",
                                Value: "Sviluppo",
                                Title: T("Sviluppo")
                            ),
                            _Recipientidrelated: Shape.TextBox(
                                Id: "idRelated",
                                Name: "idRelated",
                                Value: "",
                                Title: T("Link the content as Related Content"),
                                Description: T("Content ID. Tokenized. Example: the current content item is {Content.Id}."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                            _Recipient5: Shape.Textbox(
                                Id: "PushMessage",
                                Name: "PushMessage",
                                Title: T("Push Message"),
                                Description: T("Push Message Tokenized."),
                                Classes: new[] { "large", "text", "tokenized" }
                            )
                        )
                    );
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "All", Text = "All Devices" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "Apple", Text = "All Apple's device" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "Android", Text = "All Android's device" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "WindowsMobile", Text = "All WindowsMobile's device" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "ContentOwner", Text = "Content's Owner" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "ContentCreator", Text = "Content's Creator" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "ContentLastModifier", Text = "Content's LastModifier" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "UserId", Text = "User specified by ID" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "UserEmail", Text = "User specified by e-mail" });
                    f._Type._ddlDevice.Add(new SelectListItem { Value = "UserName", Text = "User specified by username" });
                    f._Type._ddlLanguage.Add(new SelectListItem { Value = "All", Text = "All Languages" });
                    foreach (string up in _cultureManager.ListCultures()) {
                        f._Type._ddlLanguage.Add(new SelectListItem { Value = up.ToString(), Text = up.ToString() });
                    }
                    return f;
                };
            context.Form("ActivityMobileForm", form);
        }
    }
}