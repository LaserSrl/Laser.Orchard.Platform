using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile {
    [OrchardFeature("Laser.Orchard.BannerAgent")]
    public class MigrationBannerAgent : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable(typeof(BannerAgentPartRecord).Name,
                        table => table
                        .ContentPartRecord()
                        .Column<string>("title", column => column.WithLength(400))
                        .Column<string>("author", column => column.WithLength(400))
                        .Column<string>("price", column => column.WithLength(400))
                        .Column<string>("price_suffix_apple", column => column.WithLength(400))
                        .Column<string>("price_suffix_google", column => column.WithLength(400))
                        .Column<string>("icon_apple", column => column.WithLength(400))
                        .Column<string>("icon_google", column => column.WithLength(400))
                        .Column<string>("button", column => column.WithLength(400))
                        .Column<string>("button_url_apple", column => column.WithLength(400))
                        .Column<string>("button_url_google", column => column.WithLength(400))
                        .Column<string>("enabled_platforms", column => column.WithLength(400))
                        .Column<string>("exclude_user_agent_regex", column => column.WithLength(400))
                        .Column<string>("include_user_agent_regex", column => column.WithLength(400))
                        .Column<string>("disable_positioning", column => column.WithLength(400))
                        .Column<string>("hide_ttl", column => column.WithLength(400))
                        .Column<string>("hide_path", column => column.WithLength(400))
                        .Column<string>("custom_design_modifier", column => column.WithLength(400))
                        );
            ContentDefinitionManager.AlterPartDefinition(typeof(BannerAgentPart).Name, cfg => cfg
                .Attachable(false));

            ContentDefinitionManager.AlterTypeDefinition("BannerAgentWidget", cfg => cfg
                .WithPart(typeof(BannerAgentPart).Name)
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));
            return 1;
        }
    }
}