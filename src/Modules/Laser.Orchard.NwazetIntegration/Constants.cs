namespace Laser.Orchard.NwazetIntegration {

    public struct CountryAlpha2 {
        public int TerritoryId { get; set; }
        public string ISOCode { get; set; }
    }
    public class Constants {
        public const string PaymentSucceeded = "Payment Succeeded";
        public const string PaymentFailed = "Payment Failed";
        public static string CacheEvictSignal = "AddressConfigurationSiteSettingsPart_Evict";
    }
}