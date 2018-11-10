using Laser.Orchard.Meteo.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Meteo {
    public class Migration : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
                        typeof(MeteoPart).Name,
                            cfg => cfg.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("MeteoWidget",
                            item => item
                                .WithPart("CommonPart").WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                                .WithPart("WidgetPart")
                                .WithPart("IdentityPart")
                                .WithPart("MeteoPart")
                                .WithSetting("Stereotype", "Widget"));
            return 1;
        }

    }
}