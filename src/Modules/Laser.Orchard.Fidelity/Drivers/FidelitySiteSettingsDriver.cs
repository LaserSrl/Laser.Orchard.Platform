using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Text;

namespace Laser.Orchard.Fidelity.Drivers {
    public class FidelitySiteSettingsDriver : ContentPartDriver<FidelitySiteSettingsPart> {
        public Localizer T { get; set; }
        private readonly IEncryptionService _encryptionService;
        private const string TemplateName = "Parts/FidelitySiteSettings";

        public FidelitySiteSettingsDriver(IEncryptionService encryptionService) {
            T = NullLocalizer.Instance;
            _encryptionService = encryptionService;
        }

        protected override string Prefix { get { return "FidelitySiteSettings"; } }

        protected override DriverResult Editor(FidelitySiteSettingsPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_FidelitySiteSettings_Edit", () =>
                                {
                                    var model = new FidelitySiteSettingsViewModel
                                                    {
                                                        DeveloperKey = part.DeveloperKey,
                                                        ApiURL = part.ApiURL,
                                                        MerchantUsername = part.MerchantUsername,
                                                        MerchantPwd = "",
                                                        RegisterOnLogin = part.RegisterOnLogin
                                                    };

                                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                                }
                    ).OnGroup("Fidelity");
        }

        protected override DriverResult Editor(FidelitySiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            string oldPwd = part.MerchantPwd;
            FidelitySiteSettingsViewModel fidelityVM = new FidelitySiteSettingsViewModel();

            if (updater.TryUpdateModel(fidelityVM, Prefix, null, null)) {
                if (!String.IsNullOrWhiteSpace(fidelityVM.ApiURL)) { // trattandosi di settings aggiorno solo se un parametro obbligatorio è presente, altrimenti non deve aggiornare. L'effetto altrimento sarebbe dell'azzeramento dei settings per la fidelity
                    if (!String.IsNullOrEmpty(fidelityVM.MerchantPwd)) {
                        string encryptedPwd = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(fidelityVM.MerchantPwd)));

                        if (encryptedPwd != oldPwd) part.MerchantSessionId = "";

                        part.MerchantPwd = encryptedPwd;
                    } else
                        part.MerchantPwd = oldPwd;

                    part.ApiURL = fidelityVM.ApiURL;
                    part.DeveloperKey = fidelityVM.DeveloperKey;
                    part.MerchantUsername = fidelityVM.MerchantUsername;
                }
                part.RegisterOnLogin = fidelityVM.RegisterOnLogin;
            }

            return Editor(part, shapeHelper);
        }
    }
}