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
    }
}