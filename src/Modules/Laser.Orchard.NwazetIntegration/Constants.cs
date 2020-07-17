namespace Laser.Orchard.NwazetIntegration {

    public struct CountryAlpha2 {
        public int TerritoryId { get; set; }
        public string ISOCode { get; set; }
    }
    public class Constants {
        public const string PaymentSucceeded = "Payment Succeeded";
        public const string PaymentFailed = "Payment Failed";

        public static string AddressConfigurationCacheEvictSignal = "AddressConfigurationSiteSettingsPart_Evict";
        public static string CheckoutSettingsCacheEvictSignal = "CheckoutSettingsPart_Evict";

        // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
        // also: Orchard.Users.Services.AccountValidationService
        public const string EmailPattern =
            @"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-\p{L}0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@([a-z0-9][\w-]*\.)+[a-z]{2,}$";
    }
}