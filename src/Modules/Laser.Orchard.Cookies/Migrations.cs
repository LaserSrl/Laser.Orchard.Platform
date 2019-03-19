using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Cookies
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("CookieLawPart", part => part
                .WithDescription("Renders the CookieLaw plugin."));

            ContentDefinitionManager.AlterTypeDefinition("CookieLawWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("CookieLawPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }
    }
}