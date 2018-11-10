using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Migrations {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class MultiStepAuthenticationMigrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("OTPRecord", table=>table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("UserRecord_Id")
                .Column<string>("Password", col => col.Unlimited())
                .Column<string>("PasswordType")
                .Column<string>("AdditionalData", col => col.Unlimited())
                .Column<DateTime>("ExpirationUTCDate"));

            return 1;
        }
    }
}