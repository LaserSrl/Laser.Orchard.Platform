using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi {
    public class CartaSiMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("PaymentCartaSiSettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("CartaSiShopAlias")
                    .Column<bool>("UseTestEnvironment")
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("PaymentCartaSiSettingsPartRecord",
                table => table
                    .AddColumn<string>("CartaSiSecretKey")
                );

            return 2;
        }
    }
}