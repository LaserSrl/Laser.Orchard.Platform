using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.ContactForm {
    public class Migrations : DataMigrationImpl 
    {

        private readonly IUtilsServices _utilServices;

        public Migrations(IUtilsServices utilsServices)
        {
            _utilServices = utilsServices;
        }

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() 
        {
			// Creating table ContactFormRecord
			SchemaBuilder.CreateTable("ContactFormRecord", table => table
				.ContentPartRecord()
                .Column<string>("RecipientEmailAddress", column => column.WithLength(800))
                .Column<string>("StaticSubjectMessage", column => column.WithLength(2000))
				.Column<bool>("UseStaticSubject")
                .Column<bool>("DisplayNameField")
                .Column<bool>("RequireNameField")
			);

            ContentDefinitionManager.AlterPartDefinition(
                typeof(ContactFormPart).Name, cfg => cfg.Attachable());

            return 1;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterTypeDefinition("ContactFormWidget", cfg => cfg
                .WithPart("ContactFormPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 2;
        }

        public int UpdateFrom2() 
        {
            SchemaBuilder.CreateTable("ContactFormSettingsRecord", table => table
               .ContentPartRecord()
               .Column<bool>("EnableSpamProtection")
               .Column<bool>("EnableSpamEmail")
               .Column<string>("SpamEmail")
              );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<int>("TemplateRecord_Id", column => column.WithDefault(-1))
            );
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<bool>("EnableUpload", column => column.WithDefault(false))
            );
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<string>("PathUpload")
            );

            _utilServices.EnableFeature("Laser.Orchard.TemplateManagement");
            _utilServices.EnableFeature("Laser.Orchard.TemplateManagement.Parsers.Razor");

            _utilServices.EnableFeature("Laser.Orchard.StartupConfig");

            return 4;
        }

        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<bool>("AttachFiles", column => column.WithDefault(false))
            );

            return 5;
        }

        public int UpdateFrom5()
        {
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<bool>("RequireAttachment", column => column.WithDefault(false))
            );

            return 6;
        }
        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<bool>("AcceptPolicy", column => column.WithDefault(false))
            );
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                 .AddColumn<string>("AcceptPolicyUrl")
            );
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                   .AddColumn<string>("AcceptPolicyUrlText")
            );
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
               .AddColumn<string>("AcceptPolicyText")
            );
            return 7;
        }
        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("ContactFormRecord", table => table
                .AddColumn<string>("ThankyouPage", column => column.WithLength(2000))
            );
            return 8;
        }
    }
}