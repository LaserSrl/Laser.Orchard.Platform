using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.Models
{
    /// <summary>
    /// Use this class to access settings in a readonly manner.
    /// This object can be safely cached to avoid fetching data
    /// from the db several times unless it changed.
    /// </summary>
    public class BraintreeSettings {
        public BraintreeSettings(BraintreeSiteSettingsPart part) {
            if (part != null) {
                ProductionEnvironment = part.ProductionEnvironment;
                MerchantId = part.MerchantId;
                CurrencyCode = part.CurrencyCode;
                PublicKey = part.PublicKey;
                PrivateKey = part.PrivateKey;
                MerchantAccountId = part.MerchantAccountId;
                AutomaticPayment = part.AutomaticPayment;
                GooglePayMerchantId = part.GooglePayMerchantId;
                ApplePayDisplayName = part.ApplePayDisplayName;
                ApplePayLabel = part.ApplePayLabel;
            }
        }
        #region Braintree merchant settings
        public bool ProductionEnvironment { get; private set; }
        public string MerchantId { get; private set; }
        public string CurrencyCode { get; private set; }
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }
        public string MerchantAccountId { get; private set; }
        public bool AutomaticPayment { get; private set; }
        #endregion
        #region GooglePay
        public string GooglePayMerchantId { get; private set; }
        #endregion
        #region ApplePay
        public string ApplePayDisplayName { get; private set; }
        public string ApplePayLabel { get; private set; }
        #endregion
    }
    public class BraintreeSiteSettingsPart : ContentPart
    {
        public const string CacheKey = "BraintreeSiteSettingsPart";
        #region Braintree merchant settings
        public bool ProductionEnvironment
        {
            get { return this.Retrieve(x => x.ProductionEnvironment); }
            set { this.Store(x => x.ProductionEnvironment, value); }
        }
        public string MerchantId
        {
            get { return this.Retrieve(x => x.MerchantId); }
            set { this.Store(x => x.MerchantId, value); }
        }
        public string CurrencyCode {
            get { return this.Retrieve(x => x.CurrencyCode); }
            set { this.Store(x => x.CurrencyCode, value); }
        }
        public string PublicKey
        {
            get { return this.Retrieve(x => x.PublicKey); }
            set { this.Store(x => x.PublicKey, value); }
        }
        public string PrivateKey
        {
            get { return this.Retrieve(x => x.PrivateKey); }
            set { this.Store(x => x.PrivateKey, value); }
        }

        public string MerchantAccountId {
            get { return this.Retrieve(x => x.MerchantAccountId); }
            set { this.Store(x => x.MerchantAccountId, value); }
        }

        public bool AutomaticPayment {
            get { return this.Retrieve(x => x.AutomaticPayment); }
            set { this.Store(x => x.AutomaticPayment, value); }
        }
        #endregion
        #region GooglePay
        public string GooglePayMerchantId {
            get { return this.Retrieve(x => x.GooglePayMerchantId); }
            set { this.Store(x => x.GooglePayMerchantId, value); }
        }
        #endregion
        #region ApplePay
        public string ApplePayDisplayName {
            get { return this.Retrieve(x => x.ApplePayDisplayName); }
            set { this.Store(x => x.ApplePayDisplayName, value); }
        }
        public string ApplePayLabel {
            get { return this.Retrieve(x => x.ApplePayLabel); }
            set { this.Store(x => x.ApplePayLabel, value); }
        }
        #endregion
    }
}