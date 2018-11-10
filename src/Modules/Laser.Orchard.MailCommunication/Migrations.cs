using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MailCommunication {
    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class Migrations : DataMigrationImpl {

        private readonly IUtilsServices _utilServices;
        private readonly IOrchardServices _orchardServices;

        public Migrations(IUtilsServices utilsServices, IOrchardServices orchardServices) {
            _utilServices = utilsServices;
            _orchardServices = orchardServices;
        }

        public int Create() {
            SchemaBuilder.CreateTable("MailCommunicationPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("MailMessageSent", col => col.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition("MailCommunicationPart", part => part
                .WithField("Message", cfg => cfg.OfType("TextField").WithSetting("TextFieldSettings.Flavor", "html"))
                .WithField("RelatedMailContent", cfg => cfg.OfType("ContentPickerField")
                    .WithSetting("ContentPickerFieldSettings.Multiple", "True"))
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("CommunicationAdvertising", type => type
                .WithPart("MailCommunicationPart")
                .WithPart("CustomTemplatePickerPart"));
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("CustomTemplate",
                part => part.WithField("ForEmailCommunication",
                    cfg => cfg.OfType("BooleanField"))
                    );

            ContentDefinitionManager.AlterTypeDefinition("CustomTemplate",
                type => type.WithPart("CustomTemplate"));
            return 2;
        }

        public int UpdateFrom2() {
            _utilServices.EnableFeature("Laser.Orchard.MailerUtility");
            return 3;
        }

        public int UpdateFrom3() {
            _utilServices.EnableFeature("Laser.Orchard.TemplateManagement");
            // Template Mail Communication
            dynamic t1Content = _orchardServices.ContentManager.New("CustomTemplate");
            _orchardServices.ContentManager.Create(t1Content);
            t1Content.TemplatePart.Title = "Mail Communication - Template";
            t1Content.TemplatePart.Text = "";
            t1Content.TemplatePart.Subject = "Email Commerciale";
            t1Content.CustomTemplate.ForEmailCommunication.Value = true;
            t1Content = null;
            // Template Mail Communication - Unsubscribe
            dynamic t2Content = _orchardServices.ContentManager.New("CustomTemplate");
            _orchardServices.ContentManager.Create(t2Content);
            t2Content.TemplatePart.Title = "Mail Communication - Unsubscription Template";
            t2Content.TemplatePart.Text = "";
            t2Content.TemplatePart.Subject = "Email Commerciale - Conferma cancellazione";
            t2Content.CustomTemplate.ForEmailCommunication.Value = true;
            t2Content = null;

            return 4;
        }
    }
}