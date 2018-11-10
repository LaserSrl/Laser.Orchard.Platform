using System.Data;
using Laser.Orchard.Maps.Models;
using Orchard;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;

namespace Laser.Orchard.Maps {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("MapVersionRecord",
             table => table
              .ContentPartVersionRecord()
                .Column("Latitude", DbType.Single)
                .Column("Longitude", DbType.Single)
                .Column("LocationInfo", DbType.String, column => column.WithLength(100))
                .Column("LocationAddress", DbType.String, column => column.WithLength(255))
            );

            // Create a new widget content type with our map
            ContentDefinitionManager.AlterTypeDefinition("MapWidget", cfg => cfg
                .WithPart("MapPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterPartDefinition(typeof(MapPart).Name, cfg => cfg
                .WithField("MapSourceFile", fieldBuilder => fieldBuilder
                    .WithDisplayName("Map Source")
                    .OfType("MediaLibraryPickerField")
                    .WithSetting("MediaLibraryPickerFieldSettings.Required", "False")
                    .WithSetting(" MediaLibraryPickerFieldSettings.Multiple", "False"))
                .Attachable());

            return 5; //return 5 to stay aligned with old obsolete migrations
        }

        public int UpdateFrom5() {
            ContentDefinitionManager.AlterPartDefinition("CustomPinPart", builder => builder
                .Attachable(true));
            return 6;
        }

        #region Obsolete code
        //private readonly IRepository<MapRecord> _record;
        //private readonly IRepository<MapVersionRecord> _versionRecord;

        //public Migrations(IRepository<MapRecord> record, IRepository<MapVersionRecord> versionRecord) {
        //    _record = record;
        //    _versionRecord = versionRecord;
        //}

        //public int Create() {
        //    // Creating table MapRecord
        //    SchemaBuilder.CreateTable("MapRecord", table => table
        //        .ContentPartRecord()
        //        .Column("Latitude", DbType.Single)
        //        .Column("Longitude", DbType.Single)
        //    );

        //    ContentDefinitionManager.AlterPartDefinition(typeof(MapPart).Name, cfg => cfg
        //        .Attachable());

        //    return 1;
        //}

        //public int UpdateFrom1() {
        //    // Create a new widget content type with our map
        //    ContentDefinitionManager.AlterTypeDefinition("MapWidget", cfg => cfg
        //        .WithPart("MapPart")
        //        .WithPart("WidgetPart")
        //        .WithPart("CommonPart")
        //        .WithSetting("Stereotype", "Widget"));

        //    return 2;
        //}

        //public int UpdateFrom2() {
        //    SchemaBuilder.AlterTable("MapRecord", table => table.AddColumn("LocationInfo", DbType.String, column => column.WithLength(100)));
        //    SchemaBuilder.AlterTable("MapRecord", table => table.AddColumn("LocationAddress", DbType.String, column => column.WithLength(255)));
        //    return 3;
        //}

        //public int UpdateFrom3() {
        //    ContentDefinitionManager.AlterPartDefinition(typeof(MapPart).Name, cfg => cfg
        //        .WithField("MapSourceFile", fieldBuilder => fieldBuilder
        //            .WithDisplayName("Map Source")
        //            .OfType("MediaLibraryPickerField")
        //            .WithSetting("MediaLibraryPickerFieldSettings.Required", "False")
        //            .WithSetting(" MediaLibraryPickerFieldSettings.Multiple", "False"))
        //        .Attachable());
        //    return 4;
        //}

        ///// <summary>
        ///// Converte la Part in una Part Draftabile
        ///// </summary>
        ///// <returns></returns>
        //public int UpdateFrom4() {
        //    SchemaBuilder.CreateTable("MapVersionRecord",
        //     table => table
        //      .ContentPartVersionRecord()
        //        .Column("Latitude", DbType.Single)
        //        .Column("Longitude", DbType.Single)
        //        .Column("LocationInfo", DbType.String, column => column.WithLength(100))
        //        .Column("LocationAddress", DbType.String, column => column.WithLength(255))
        //    );

        //    foreach (var row in _record.Table) {
        //        foreach (var version in row.ContentItemRecord.Versions) {
        //            var newItem = new MapVersionRecord() {
        //                ContentItemRecord = row.ContentItemRecord,
        //                ContentItemVersionRecord = version,
        //                Latitude = row.Latitude,
        //                LocationAddress = row.LocationAddress,
        //                LocationInfo = row.LocationInfo,
        //                Longitude = row.Longitude
        //            };
        //            _versionRecord.Create(newItem);
        //        }
        //    }
        //    return 5;
        //}
        #endregion
    }
}
