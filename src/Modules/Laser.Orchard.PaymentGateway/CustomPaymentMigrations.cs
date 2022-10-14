using Laser.Orchard.AdvancedSettings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Linq;

namespace Laser.Orchard.PaymentGateway {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPaymentMigrations : DataMigrationImpl {
        IContentManager _contentManager;
        IMembershipService _membershipService;

        public CustomPaymentMigrations(IContentManager contentManager,
            IMembershipService membershipService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
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

            // Creating a ImpostazioniPagamenti content needs to check if there already is the same content inside the database.
            // In that case, take note of current settings before replacing the content with a new one, created by this migration.
            var currentIban = "";
            var currentEmail = "";
            var advancedSettingsPart = _contentManager.Query<AdvancedSettingsPart, AdvancedSettingsPartRecord>()
                .Where(asp => asp.Name.Equals("Impostazioni Pagamenti", StringComparison.OrdinalIgnoreCase))
                .List()
                .FirstOrDefault();
            if (advancedSettingsPart != null) {
                var currentSettings = _contentManager.Get(advancedSettingsPart.Id, VersionOptions.Published);
                if (currentSettings != null) {
                    var currentEmailField = currentSettings.Parts
                        .SelectMany(pa => pa.Fields)
                        .FirstOrDefault(fi => fi is TextField && fi.Name.Equals("BankTransferEmail", StringComparison.OrdinalIgnoreCase));
                    currentEmail = currentEmailField != null ? ((TextField)currentEmailField).Value : "";
                    var currentIbanField = currentSettings.Parts
                        .SelectMany(pa => pa.Fields)
                        .FirstOrDefault(fi => fi is TextField && fi.Name.Equals("IBAN", StringComparison.OrdinalIgnoreCase));
                    currentIban = currentIbanField != null ? ((TextField)currentIbanField).Value : "";
                }

                _contentManager.Remove(currentSettings);
            }

            var impostazioniPagamenti = _contentManager.New("ImpostazioniPagamenti");
            
            _contentManager.Create(impostazioniPagamenti, VersionOptions.Draft);
            impostazioniPagamenti.As<AdvancedSettingsPart>().Name = "Impostazioni Pagamenti";
            impostazioniPagamenti.As<CommonPart>().Owner = _membershipService.GetUser("admin");
            // Set BankTransferEmail and IBAN from the content that has just been replaced.
            TextField emailField = impostazioniPagamenti.Parts
                .SelectMany(pa => pa.Fields)
                .FirstOrDefault(fi => fi.Name.Equals("BankTransferEmail", StringComparison.OrdinalIgnoreCase))
                as TextField;
            emailField.Value = currentEmail;
            TextField ibanField = impostazioniPagamenti.Parts
                .SelectMany(pa => pa.Fields)
                .FirstOrDefault(fi => fi.Name.Equals("IBAN", StringComparison.OrdinalIgnoreCase))
                as TextField;
            ibanField.Value = currentIban;
            impostazioniPagamenti.VersionRecord.Published = false;
            _contentManager.Publish(impostazioniPagamenti);

            return 1;
        }
    }
}