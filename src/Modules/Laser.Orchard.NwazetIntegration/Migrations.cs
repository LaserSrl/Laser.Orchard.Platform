using Orchard.Data.Migration;
using System;

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
                .Column<int>("CountryId")
                .Column<int>("CityId")
                .Column<int>("ProvinceId")
                .Column<string>("FiscalCode")
                .Column<string>("VATNumber")
                .Column<string>("CustomerType", col=>col.WithLength(20))
                .Column<bool>("InvoiceRequest")

                );

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
                 .Column<int>("BillingProvinceId")
                 .Column<bool>("ShippingAddressIsOptional")
                 .Column<string>("BillingFiscalCode")
                 .Column<string>("BillingVATNumber")
                 .Column<string>("BillingCustomerType", col => col.WithLength(20))
                 .Column<bool>("BillingInvoiceRequest")
                 );

            SchemaBuilder.CreateTable("TerritoryAdministrativeTypePartRecord", table => table
                .ContentPartRecord()
                // TerritoryAdministrativeType enum stored as string
                .Column<string>("AdministrativeType")
                .Column<int>("TerritoryInternalRecord_Id")
                .Column<bool>("HasCities")
                .Column<bool>("HasProvinces")
                );
            SchemaBuilder.AlterTable("TerritoryAdministrativeTypePartRecord",
                table => table.CreateIndex("IDX_AdministrativeType", "AdministrativeType"));

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

            SchemaBuilder.CreateTable("TerritoryISO3166CodePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("ISO3166Code")
                .Column<int>("TerritoryInternalRecord_Id"));

            return 10;
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
        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("TerritoryAdministrativeTypePartRecord",
                table => table.AddColumn<bool>("HasCities"));
            SchemaBuilder.AlterTable("TerritoryAdministrativeTypePartRecord",
                table => table.AddColumn<bool>("HasProvinces"));

            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("AddressOrderPartRecord",
                table => table.AddColumn<bool>("ShippingAddressIsOptional"));

            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.CreateTable("TerritoryISO3166CodePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("ISO3166Code")
                .Column<int>("TerritoryInternalRecord_Id"));

            return 9;
        }

        public int UpdateFrom9() {
            //Adds VAT Number and Fiscal code for billing addresses
            SchemaBuilder.AlterTable("AddressRecord", table =>
                table.AddColumn<string>("FiscalCode"));
            SchemaBuilder.AlterTable("AddressRecord", table =>
                table.AddColumn<string>("VATNumber"));
            SchemaBuilder.AlterTable("AddressRecord", table =>
                table.AddColumn<string>("CustomerType"));
            SchemaBuilder.AlterTable("AddressRecord", table =>
                table.AddColumn<bool>("InvoiceRequest"));
            SchemaBuilder.AlterTable("AddressOrderPartRecord", table =>
                table.AddColumn<string>("BillingFiscalCode"));
            SchemaBuilder.AlterTable("AddressOrderPartRecord", table =>
                table.AddColumn<string>("BillingVATNumber"));
            SchemaBuilder.AlterTable("AddressOrderPartRecord", table =>
                table.AddColumn<string>("BillingCustomerType", col => col.WithLength(20)));
            SchemaBuilder.AlterTable("AddressOrderPartRecord", table =>
                table.AddColumn<bool>("BillingInvoiceRequest", col => col.WithLength(20)));
            return 10;
        }
    }
}