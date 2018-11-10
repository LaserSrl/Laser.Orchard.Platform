using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay {
    public class GestPayMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PaymentGestPaySettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("GestPayShopLogin")
                    .Column<bool>("UseTestEnvironment")
                );


            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("GestPayRedirectRecord",
                 table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("PaymentRecordId")
                    .Column("RedirectUrl", System.Data.DbType.String, x => x.Unlimited())
                    .Column("Schema", System.Data.DbType.String, x => x.Unlimited())
                 );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.DropTable("GestPayRedirectRecord");
            return 3;
        }
    }
}