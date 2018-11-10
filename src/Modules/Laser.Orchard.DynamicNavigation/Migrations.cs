using Laser.Orchard.DynamicNavigation.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;


namespace Laser.Orchard.DynamicNavigation {


  public class Migrations : DataMigrationImpl {


    public int Create() {

      SchemaBuilder.CreateTable("DynamicMenuRecord",
          table => table
              .ContentPartRecord()
              .Column<int>("MenuId")
              .Column<int>("LevelsToShow")
          );

      return 1;
    }


    public int UpdateFrom1() {

      ContentDefinitionManager
        .AlterPartDefinition(typeof(DynamicMenuPart).Name, 
          builder => builder.Attachable());

      ContentDefinitionManager
        .AlterTypeDefinition("DynamicMenuWidget",
          cfg => cfg
              .WithPart("DynamicMenuPart")
              .WithPart("WidgetPart")
              .WithPart("CommonPart")
              .WithSetting("Stereotype", "Widget"));

      return 2;
    }

    public int UpdateFrom2() {
        SchemaBuilder.AlterTable("DynamicMenuRecord", table=>table.AddColumn<bool>("ShowFirstLevelBrothers"));
        return 3;
    }
  }
}