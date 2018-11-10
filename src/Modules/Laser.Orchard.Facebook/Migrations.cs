using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Facebook {

    public class FacebookMigrations : DataMigrationImpl {
        //  private readonly IUtilsServices _utilServices;

        //public Migrations(IUtilsServices utilsServices)
        //{
        //    _utilServices = utilsServices;
        //}

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() {
            SchemaBuilder.CreateTable("FacebookPostPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("FacebookMessage", col => col.Unlimited())
                .Column<string>("FacebookDescription", col => col.Unlimited())
                .Column<string>("FacebookCaption")
                .Column<string>("FacebookName")
                .Column<string>("FacebookPicture")
                .Column<string>("FacebookLink")
                .Column<string>("AccountList")
                .Column<bool>("FacebookMessageSent", col => col.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition(
                "FacebookPostPart",
                b => b
                .Attachable(true)
                );

            ContentDefinitionManager.AlterPartDefinition(
                "FacebookAccountPart",
                b => b
                .Attachable(false)
                );
            ContentDefinitionManager.AlterTypeDefinition(
                "SocialFacebookAccount",
                type => type
                .WithPart("FacebookAccountPart")
                .WithPart("IdentityPart")
                .WithPart("CommonPart")
                .Creatable(false)
                .Draftable(false)
          );

            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .WithPart("FacebookPostPart", p => p
                    .WithSetting("FacebookPostPartSettingVM.FacebookMessage", "{Content.DisplayText}")
                    .WithSetting("FacebookPostPartSettingVM.FacebookPicture", "{Content.Fields.CommunicationAdvertisingPart.Gallery}")
                     .WithSetting("FacebookPostPartSettingVM.FacebookLink", "Get from content")
                //.WithSetting("FacebookPostPartSettingVM.FacebookDescription", "{Content.Body}")
                    )
                );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("FacebookPostPartRecord", table => table
                .AddColumn<bool>("SendOnNextPublish", col => col.WithDefault(false)));
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("FacebookPostPartRecord", table => table
                     .AddColumn<string>("FacebookIdPicture"));
            return 4;
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("FacebookPostPartRecord", table => table
                     .AddColumn<string>("FacebookType"));
            return 5;
        }
        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("FacebookPostPartRecord", table => table
                     .AddColumn<string>("FacebookMessageToPost", col => col.Unlimited()));
            SchemaBuilder.AlterTable("FacebookPostPartRecord", table => table
                .AddColumn<bool>("HasImage", x => x.WithDefault(true)));
            return 6;
        }

    }
}