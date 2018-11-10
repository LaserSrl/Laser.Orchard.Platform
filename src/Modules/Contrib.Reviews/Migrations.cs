using System;

using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Contrib.Reviews {
    
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ReviewsPart", builder => builder.Attachable());

            SchemaBuilder.CreateTable("ReviewRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<int>("ContentItemRecordId", c => c.NotNull())
                    .Column<int>("VoteRecordId", c => c.NotNull())
                    .Column<string>("Comment", c => c.WithLength(1200).Nullable())
                    .Column<DateTime>("CreatedUtc", c => c.NotNull())
                );

            return 1;
        }
    }
}