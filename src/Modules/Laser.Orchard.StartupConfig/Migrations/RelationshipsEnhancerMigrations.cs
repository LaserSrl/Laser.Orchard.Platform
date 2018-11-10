using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Migrations {
    [OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    public class RelationshipsEnhancerMigrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(ContentPickerConnectorPart).Name,
                cfg =>
                    cfg.Attachable());
            return 1;
        }


        // TODO: Gestire la configurazione dei filtri come Settings della parte
        //public int Create() {

        //    SchemaBuilder.CreateTable("ContentPickerConnectorSettingsRecord",
        //    table =>
        //            table
        //            .ContentPartRecord()
        //            .Column<string>("ContentTypesFilter", column => column.WithLength(1000))
        //            );
        //    ContentDefinitionManager.AlterPartDefinition(
        //        typeof(ContentPickerConnectorPart).Name, 
        //        cfg =>  
        //            cfg.Attachable());

        //    return 1;
        //}
    }
}