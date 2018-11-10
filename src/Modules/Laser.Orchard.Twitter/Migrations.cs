using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Twitter {

    public class TwitterMigrations : DataMigrationImpl {

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() {
            SchemaBuilder.CreateTable("TwitterPostPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TwitterMessage", col => col.Unlimited())
                .Column<string>("TwitterDescription", col => col.Unlimited())
                .Column<string>("TwitterTitle")
                .Column<string>("TwitterName")
                .Column<string>("TwitterPicture")
                .Column<string>("TwitterLink")
                .Column<string>("AccountList")
                .Column<bool>("TwitterMessageSent", col => col.WithDefault(false))
                .Column<bool>("TwitterCurrentLink", col => col.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition(
                "TwitterPostPart",
                b => b
                .Attachable(true)
                //    .WithField("TwitterImage",
                //        field => field
                //        .OfType("MediaLibraryPickerField")
                //        .WithDisplayName("Twitter Image")
                //        .WithSetting("MediaLibraryPickerFieldSettings.Multiple", "false")
                //        .WithSetting("MediaLibraryPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
                //        )
               );

            ContentDefinitionManager.AlterPartDefinition(
                "TwitterAccountPart",
                b => b
                .Attachable(false)
                );
            ContentDefinitionManager.AlterTypeDefinition(
                "SocialTwitterAccount",
                type => type
                .WithPart("TwitterAccountPart")
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
                .WithPart("TwitterPostPart", pa => pa
                .WithSetting("TwitterPostPartSettingVM.Title", "{Content.DisplayText}")
                .WithSetting("TwitterPostPartSettingVM.Image", "{Content.Fields.CommunicationAdvertisingPart.Gallery}")
                .WithSetting("TwitterPostPartSettingVM.Description", "{Content.Body}")
                )
                );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("TwitterPostPartRecord", table => table
                .AddColumn<bool>("SendOnNextPublish", col => col.WithDefault(false)));
            return 3;
        }

    }
}