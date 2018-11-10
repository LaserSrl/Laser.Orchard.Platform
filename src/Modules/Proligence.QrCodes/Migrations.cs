namespace Proligence.QrCodes
{
    using Orchard.ContentManagement.MetaData;
    using Orchard.Core.Contents.Extensions;
    using Orchard.Data.Migration;

    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("QrCodeSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Value")
                .Column<int>("Size")
            );

            SchemaBuilder.CreateTable("QrCodePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Value")
                .Column<int>("Size")
            );

            ContentDefinitionManager.AlterPartDefinition("QrCodePart", cfg => cfg.Attachable());
            
            return 1;
        }
    }
}