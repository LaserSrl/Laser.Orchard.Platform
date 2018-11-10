using Laser.Orchard.Accessibility.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Accessibility
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition(typeof(AccessibilityPart).Name, cfg => cfg
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("AccessibilityWidget", cfg => cfg
                .WithPart("AccessibilityPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 1;
        }
    }
}
