using Laser.Orchard.AdvancedSettings.Models;
using Laser.Orchard.AdvancedSettings.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Security;

namespace Laser.Orchard.PaymentGateway {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPaymentMigrations : DataMigrationImpl {
        IContentManager _contentManager;
        IMembershipService _membershipService;
        IAdvancedSettingsService _advancedSettingsService;

        public CustomPaymentMigrations(IContentManager contentManager,
            IMembershipService membershipService,
            IAdvancedSettingsService advancedSettingsService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _advancedSettingsService = advancedSettingsService;
        }

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("ImpostazioniPagamenti", cfg => cfg
                .Listable()
                .Creatable()
                .Draftable()
                .Securable()
                .WithPart("CommonPart")
                .WithPart("AdvancedSettingsPart")
                .WithIdentity()
                .WithPart("ImpostazioniPagamenti")
            );

            ContentDefinitionManager.AlterPartDefinition("ImpostazioniPagamenti", cfg => cfg
                .Attachable()
                .WithField("BankTransferEmail", b => b.OfType("TextField")
                    .WithDisplayName("Bank Transfer Email")
                    .WithSetting("TextFieldSettings.Flavor", "Large"))
                .WithField("IBAN", b => b.OfType("TextField")
                    .WithDisplayName("IBAN")
                    .WithSetting("TextFieldSettings.Hint", "IBAN used for custom payments")
                    .WithSetting("TextFieldSettings.Flavor", "Large"))
            );

            // Check if there already is the Impostazioni Pagamenti content item.
            var advancedSettings = _advancedSettingsService.GetCachedSetting("Impostazioni Pagamenti");
            if (advancedSettings == null) {
                var impostazioniPagamenti = _contentManager.New("ImpostazioniPagamenti");
                _contentManager.Create(impostazioniPagamenti, VersionOptions.Draft);
                impostazioniPagamenti.As<AdvancedSettingsPart>().Name = "Impostazioni Pagamenti";
                impostazioniPagamenti.As<CommonPart>().Owner = _membershipService.GetUser("admin");
                impostazioniPagamenti.VersionRecord.Published = false;
                _contentManager.Publish(impostazioniPagamenti);
            }
            
            return 1;
        }
    }
}