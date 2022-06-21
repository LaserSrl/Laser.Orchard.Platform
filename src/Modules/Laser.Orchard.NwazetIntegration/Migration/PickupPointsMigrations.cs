using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Migration {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsMigrations : DataMigrationImpl {

        public int Create() {
            // Tables
            SchemaBuilder.CreateTable("PickupPointPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("CountryName")
                .Column<int>("CountryId")
                .Column<string>("ProvinceName")
                .Column<int>("ProvinceId")
                .Column<string>("CityName")
                .Column<int>("CityId"));
            // ContentTypes
            ContentDefinitionManager.AlterTypeDefinition(PickupPointPart.DefaultContentTypeName, cfg => cfg
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("PickupPointPart"));

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("PickupPointPartRecord", table => table
                .AddColumn<string>("AddressLine1")
            );
            SchemaBuilder.AlterTable("PickupPointPartRecord", table => table
                .AddColumn<string>("AddressLine2")
            );

            return 2;
        }
        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition(PickupPointPart.DefaultContentTypeName, cfg => cfg
                .WithPart("TitlePart"));

            return 3;
        }
    }
}