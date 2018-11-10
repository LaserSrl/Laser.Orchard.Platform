using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Newsletters {
    public class Migrations : DataMigrationImpl {
        private readonly IUtilsServices _utilServices;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContextAccessor;
        public Migrations(IUtilsServices utilServices, IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor) {
            _utilServices = utilServices;
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
        }
        public int Create() {

            #region [ Records ]
            SchemaBuilder.CreateTable("NewsletterDefinitionPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("TemplateRecord_Id")
                .Column<int>("ConfirmSubscrptionTemplateRecord_Id", col => col.NotNull())
                .Column<int>("DeleteSubscrptionTemplateRecord_Id", col => col.NotNull()));

            SchemaBuilder.CreateTable("NewsletterEditionPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("NewsletterDefinitionPartRecord_Id")
                .Column<int>("Number")
                .Column<bool>("Dispatched")
                .Column<DateTime>("DispatchDate")
                .Column<string>("AnnouncementIds", col => col.WithLength(200))
                );

            SchemaBuilder.CreateTable("SubscriberRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("Name", col => col.WithLength(150).NotNull())
                .Column<string>("Guid", col => col.WithLength(255).NotNull())
                .Column<string>("Email", col => col.WithLength(150).NotNull())
                .Column<DateTime>("SubscriptionDate", col => col.NotNull().WithDefault(DateTime.Now))
                .Column<bool>("Confirmed")
                .Column<DateTime>("ConfirmationDate", col => col.WithDefault(DateTime.MaxValue))
                .Column<DateTime>("UnsubscriptionDate", col => col.WithDefault(DateTime.MaxValue))
                .Column<int>("NewsletterDefinition_Id", col => col.NotNull())
                .Column<int>("UserRecord_Id", col => col.NotNull()));

            SchemaBuilder.CreateTable("AnnouncementPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("AnnouncementTitle", col => col.WithLength(150))
                .Column<string>("AttachToNextNewsletterIds", col => col.WithLength(50)));
            #endregion


            #region [ Parts ]
            ContentDefinitionManager.AlterPartDefinition("NewsletterDefinitionPart", part => part.Attachable());
            ContentDefinitionManager.AlterPartDefinition("NewsletterEditionPart", part => part.Attachable());
            ContentDefinitionManager.AlterPartDefinition("AnnouncementPart", part => part
                .WithField("Abstract", fld => fld.OfType("TextField").WithSetting("TextFieldSettings.Flavor", "html"))
                .WithField("PreviewImage", fld => fld
                    .OfType("MediaLibraryPickerField")
                    .WithSetting("MediaLibraryPickerField.Hint", "Insert a preview picture for your content").WithSetting("MediaLibraryPickerFieldSettings.Required", "False")
                    .WithSetting("MediaLibraryPickerFieldSettings.Multiple", "False"))
                .Attachable());
            #endregion


            #region [ Types ]
            ContentDefinitionManager.AlterTypeDefinition("NewsletterDefinition",
                item => item
                    .WithPart("CommonPart").WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithPart("TitlePart")
                    .WithPart("BodyPart").WithSetting("BodyTypePartSettings.Flavor", "text")
                    .WithPart("NewsletterDefinitionPart"));

            ContentDefinitionManager.AlterTypeDefinition("NewsletterEdition",
                item => item
                    .WithPart("CommonPart").WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithPart("TitlePart")
                    .WithPart("BodyPart").WithSetting("BodyTypePartSettings.Flavor", "html")
                    .WithPart("NewsletterEditionPart"));
            #endregion

            return 1;
        }

        public int UpdateFrom1() {
            #region [ Records ]
            SchemaBuilder.CreateTable("SubscriberRegistrationPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("PermitCumulativeRegistrations", col => col.WithDefault(true))
                .Column<string>("NewsletterDefinitionIds", col => col.WithLength(150)));

            #endregion
            #region [ Parts ]
            ContentDefinitionManager.AlterPartDefinition("SubscriberRegistrationPart", part => part.Attachable());
            #endregion
            #region [ Types ]
            ContentDefinitionManager.AlterTypeDefinition("SubscriberRegistrationWidget",
                item => item
                    .WithPart("CommonPart").WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithPart("WidgetPart")
                    .WithPart("IdentityPart")
                    .WithPart("SubscriberRegistrationPart")
                    .WithSetting("Stereotype", "Widget"));
            #endregion

            return 2;
        }

        public int UpdateFrom2() {
            _utilServices.EnableFeature("Laser.Orchard.TemplateManagement");
            dynamic t1Content = _orchardServices.ContentManager.New("CustomTemplate");
            _orchardServices.ContentManager.Create(t1Content);
            // Template Sottoscrizione
            t1Content.TemplatePart.Title = "Newsletter - Subscription Template";
            t1Content.TemplatePart.Text = "";// File.ReadAllText(_workContextAccessor.GetContext().HttpContext.Server.MapPath("~/Modules/Laser.Orchard.Newsletters/Templates/Subscription.html"));
            t1Content.TemplatePart.Subject = "Newsletter - Conferma iscrizione";
            t1Content = null;
            dynamic t2Content = _orchardServices.ContentManager.New("CustomTemplate");
            _orchardServices.ContentManager.Create(t2Content);
            // Template Annullamento sottoscrizione
            t2Content.TemplatePart.Title = "Newsletter - Unsubscription Template";
            t2Content.TemplatePart.Text = ""; // File.ReadAllText(_workContextAccessor.GetContext().HttpContext.Server.MapPath("~/Modules/Laser.Orchard.Newsletters/Templates/Unsubscription.html"));
            t2Content.TemplatePart.Subject = "Newsletter - Conferma cancellazione";
            t2Content = null;
            // Template Newsletter
            dynamic t3Content = _orchardServices.ContentManager.New("CustomTemplate");
            _orchardServices.ContentManager.Create(t3Content);
            t3Content.TemplatePart.Title = "Newsletter - Edition Template";
            t3Content.TemplatePart.Text = ""; //File.ReadAllText(_workContextAccessor.GetContext().HttpContext.Server.MapPath("~/Modules/Laser.Orchard.Newsletters/Templates/Newsletter.html"));
            t3Content.TemplatePart.Subject = "Newsletter";
            t3Content = null;
            return 3;
        }

        public int UpdateFrom3() {
            _utilServices.EnableFeature("Laser.Orchard.MailerUtility");
            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("SubscriberRecord",
                table => table.AddColumn<string>("SubscriptionKey", column => column.WithLength(500))
               );
            SchemaBuilder.AlterTable("SubscriberRecord",
                table => table.AddColumn<string>("UnsubscriptionKey", column => column.WithLength(500))
               );
            return 5;
        }

    }
}