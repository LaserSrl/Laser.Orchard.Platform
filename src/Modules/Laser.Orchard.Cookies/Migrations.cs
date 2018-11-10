using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Cookies
{
    public class Migrations : DataMigrationImpl
    {
        public const string cookiemsg = "We use cookies on this website, you can <a href=\"{{cookiePolicyLink}}\" title=\"read about our cookies\">read about them here</a>. To use the website as intended please...";
        public const string cookieanalyticsmsg = "We use cookies, just to track visits to our website, we store no personal details. To use the website as intended please...";
        public const string acceptmsg = "ACCEPT COOKIES";
        public const string declinemsg = "DECLINE COOKIES";
        public const string resetmsg = "RESET COOKIES FOR THIS WEBSITE";
        public const string whataremsg = "What are Cookies?";
        public const string discreetmsg = "Cookies?";
        public const string errormsg = "We're sorry, you declined the use of cookies on this website, this feature places cookies in your browser and has therefore been disabled.<br>To continue this functionality please";
        public const string whatarecookieslink = "http://www.allaboutcookies.org/";

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