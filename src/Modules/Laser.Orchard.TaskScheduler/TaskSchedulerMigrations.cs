using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Core.Contents.Extensions;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.TaskScheduler.Models;

namespace Laser.Orchard.TaskScheduler {
    public class TaskSchedulerMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ScheduledTaskRecord",
                table => table.ContentPartRecord()
                    .Column<string>("SignalName")
                    .Column<DateTime>("ScheduledStartUTC")
                    .Column<int>("PeriodicityTime")
                    .Column<string>("PeriodicityUnit")
                    .Column<int>("ContentItemId")
                    .Column<int>("RunningTask_id")
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.DropTable("ScheduledTaskRecord");

            SchemaBuilder.CreateTable("LaserTaskSchedulerRecord",
                table => table.ContentPartRecord()
                    .Column<string>("SignalName")
                    .Column<DateTime>("ScheduledStartUTC")
                    .Column<int>("PeriodicityTime")
                    .Column<string>("PeriodicityUnit")
                    .Column<int>("ContentItem_id")
                    .Column<int>("RunningTask_id")
                );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.DropTable("LaserTaskSchedulerRecord");

            SchemaBuilder.CreateTable("LaserTaskSchedulerRecord",
                table => table.ContentPartRecord()
                    .Column<string>("SignalName")
                    .Column<DateTime>("ScheduledStartUTC")
                    .Column<int>("PeriodicityTime")
                    .Column<string>("PeriodicityUnit")
                    .Column<int>("ContentItemId")
                    .Column<int>("RunningTaskId")
                );
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("LaserTaskSchedulerRecord", table => table
                .AddColumn("Autodestroy", DbType.Boolean)
            );
            return 4;
        }
        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition(typeof(ScheduledTaskPart).Name, cfg => cfg
                .Attachable(false)
            );
            ContentDefinitionManager.AlterTypeDefinition("ScheduledTask", cfg => cfg
                .WithPart(typeof(ScheduledTaskPart).Name)
                .Creatable(false)
                .Draftable(false)
            );
            return 5;
        }
        public int UpdateFrom5() {
            ContentDefinitionManager.AlterTypeDefinition("ScheduledTask", cfg => cfg
                .RemovePart(typeof(ScheduledTaskPart).Name)
                .Creatable(false)
                .Draftable(false)
            );
            return 6;
        }
        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("LaserTaskSchedulerRecord", cfg => cfg
                .AddColumn<string>("ExecutionType", c => c.WithDefault("WorkFlow")));
            return 7;
        }
        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("LaserTaskSchedulerRecord", cfg => cfg
                .AddColumn<bool>("LongTask", c => c.WithDefault(false)));
            return 8;
        }
        
    }
}