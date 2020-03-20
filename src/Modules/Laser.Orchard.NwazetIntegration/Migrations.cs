using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Laser.Orchard.NwazetIntegration {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("NwazetContactPartRecord",
                table => table
                .ContentPartRecord()
            );
            SchemaBuilder.CreateTable("AddressRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<int>("NwazetContactPartRecord_Id")
                .Column<DateTime>("TimeStampUTC", c => c.NotNull())
                .Column<string>("AddressType", col => col.WithLength(40))
                .Column<string>("Honorific")
                .Column<string>("FirstName")
                .Column<string>("LastName")
                .Column<string>("Company")
                .Column<string>("Address1")
                .Column<string>("Address2")
                .Column<string>("City")
                .Column<string>("Province")
                .Column<string>("PostalCode")
                .Column<string>("Country")
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("AddressRecord", table => table
                .AddColumn<int>("CountryId")
            );
            SchemaBuilder.AlterTable("AddressRecord", table => table
                .AddColumn<int>("CityId")
            );
            SchemaBuilder.AlterTable("AddressRecord", table => table
                .AddColumn<int>("ProvinceId")
            );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("AddressOrderPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("ShippingCountryName")
                .Column<int>("ShippingCountryId")
                .Column<string>("ShippingCityName")
                .Column<int>("ShippingCityId")
                .Column<string>("ShippingProvinceName")
                .Column<int>("ShippingProvinceId")
                .Column<string>("BillingCountryName")
                .Column<int>("BillingCountryId")
                .Column<string>("BillingCityName")
                .Column<int>("BillingCityId")
                .Column<string>("BillingProvinceName")
                .Column<int>("BillingProvinceId"));

            return 3;
        }

        public int UpdateFrom3() {

            SchemaBuilder.CreateTable("TerritoryAdministrativeTypePartRecord", table => table
                .ContentPartRecord()
                // TerritoryAdministrativeType enum stored as string
                .Column<string>("AdministrativeType"));
            return 4;
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("TerritoryAdministrativeTypePartRecord",
                table => table.AddColumn<int>("TerritoryInternalRecord_Id"));
            SchemaBuilder.AlterTable("TerritoryAdministrativeTypePartRecord",
                table => table.CreateIndex("IDX_AdministrativeType", "AdministrativeType"));

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.CreateTable("TerritoryAddressTypePartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("Shipping")
                .Column<bool>("Billing")
                .Column<int>("TerritoryInternalRecord_Id"));
            // indexes because we will search over those booleans
            SchemaBuilder.AlterTable("TerritoryAddressTypePartRecord",
                table => table.CreateIndex("IDX_Shipping", "Shipping"));
            SchemaBuilder.AlterTable("TerritoryAddressTypePartRecord",
                table => table.CreateIndex("IDX_Billing", "Billing"));

            return 6;
        }
    }
}