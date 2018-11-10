using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.FidelityGateway.Models;

namespace Laser.Orchard.FidelityGateway
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("FidelityUserPartRecord", table => table
              .ContentPartRecord()
              .Column<string>("FidelityUsername")
              .Column<string>("FidelityPassword")
              .Column<string>("CustomerId")
          );

            ContentDefinitionManager.AlterPartDefinition(typeof(FidelityUserPart).Name, cfg => cfg
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition(
                "User",
                type => type
                            .WithPart("FidelityUserPart")
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterPartDefinition(typeof(FidelityUserPart).Name, cfg => cfg
                .WithDescription("Fidelity information about the user")
                .Attachable());

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateTable("ActionInCampaign",
            table =>
                    table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Action", column => column.WithLength(30))
                    .Column<string>("CampaignId", column => column.WithLength(30))
                    .Column<double>("Points")
                    );
            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.DropTable("ActionInCampaign");
            SchemaBuilder.CreateTable("ActionInCampaign",
            table =>
                    table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Action", column => column.WithLength(30))
                    .Column<string>("CampaignId", column => column.WithLength(30))
                    .Column<double>("Points")
                    );
            return 4;
        }

        public int UpdateFrom4()
        {
            SchemaBuilder.DropTable("ActionInCampaign");
            SchemaBuilder.CreateTable("ActionInCampaignRecord",
            table =>
                    table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Action", column => column.WithLength(30))
                    .Column<string>("CampaignId", column => column.WithLength(30))
                    .Column<double>("Points")
                    );
            return 5;
        }

        public int UpdateFrom5()
        {
            SchemaBuilder.DropTable("ActionInCampaignRecord");
            SchemaBuilder.CreateTable("ActionInCampaignRecord",
            table =>
                    table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Action", column => column.WithLength(30))
                    .Column<string>("CampaignId", column => column.WithLength(30))
                    .Column<string>("Provider", column => column.WithLength(30))
                    .Column<double>("Points")
                    );
            return 6;
        }
    }
}