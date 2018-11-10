using Laser.Orchard.BikeSharing.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.BikeSharing
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition(typeof(BikeStationPart).Name, cfg => cfg
                .Attachable());
            return 1;
        }
    }
}
