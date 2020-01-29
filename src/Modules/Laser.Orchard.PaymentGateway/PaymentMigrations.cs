using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.PaymentGateway.Models;

namespace Laser.Orchard.PaymentGateway {
    public class PaymentMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PaymentRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("PosName")
                    .Column<string>("Reason")
                    .Column<DateTime>("CreationDate")
                    .Column<DateTime>("UpdateDate")
                    .Column<string>("PosUrl")
                    .Column<decimal>("Amount")
                    .Column<string>("Currency")
                    .Column<bool>("Success")
                    .Column<string>("Error")
                    .Column<string>("TransactionId")
                    .Column<string>("ReturnUrl")
                    .Column("Info", System.Data.DbType.String, x => x.Unlimited())
                    .Column<int>("ContentItemId")
                );
            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(PayButtonPart).Name,
                p => p.Attachable()
            );
            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.DropColumn("ReturnUrl"));
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AlterColumn("PosUrl", col => col.WithType(System.Data.DbType.String).Unlimited()));
            return 4;
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn("UserId", System.Data.DbType.Int32));
            return 5;
        }
        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn("CustomRedirectUrl", System.Data.DbType.String, x => x.Unlimited()));
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn("CustomRedirectSchema", System.Data.DbType.String, x => x.Unlimited()));
            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn<bool>("PaymentTransactionComplete")
                );
            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn("Guid", System.Data.DbType.String, x => x.Unlimited()));
            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn("APIFilters", System.Data.DbType.String, x => x.Unlimited()));
            return 9;
        }
        public int UpdateFrom9() {
            SchemaBuilder.AlterTable("PaymentRecord",
                table => table.AddColumn("PaymentUniqueKey", System.Data.DbType.String));
            return 10;
        }
    }
}
