using Orchard.Data.Migration;

namespace Orchard.Captcha
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Creating table MapRecord
            SchemaBuilder.CreateTable("CaptchaSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("PublicKey")
                .Column<string>("PrivateKey")
                .Column<string>("Theme")
                .Column<string>("CustomCaptchaMarkup", c => c.WithLength(4000).Nullable())
            );
            return 1;
        }
    }
}