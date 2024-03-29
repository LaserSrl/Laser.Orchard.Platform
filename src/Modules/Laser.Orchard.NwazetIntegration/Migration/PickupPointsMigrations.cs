﻿using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

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

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("PickupPointPartRecord", table => table
                .AddColumn<string>("PostalCode")
            );

            return 4;
        }

        public int UpdateFrom4() {
            // Tables
            SchemaBuilder.CreateTable("PickupPointOrderPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("CountryName")
                .Column<int>("CountryId")
                .Column<string>("ProvinceName")
                .Column<int>("ProvinceId")
                .Column<string>("CityName")
                .Column<int>("CityId")
                .Column<string>("AddressLine1")
                .Column<string>("AddressLine2")
                .Column<string>("PostalCode")
                .Column<bool>("IsOrderPickupPoint"));

            return 5;

        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("PickupPointOrderPartRecord", table => table
                .AddColumn<string>("PickupPointTitle")
            );

            return 6;
        }
    }
}