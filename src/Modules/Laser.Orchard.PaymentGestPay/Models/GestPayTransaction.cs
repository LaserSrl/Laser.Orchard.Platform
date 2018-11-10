using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGestPay.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;


//The first tests with GestPay return on 2016/09/13 error 1107: Unexpected parameter.
//We change things so that if the parameters are not populated, we return null when converting the
//objects to the GestPay representations.

namespace Laser.Orchard.PaymentGestPay.Models {
    //this class' functionality has been replaced by the validation attributes
    //public abstract class TransactionBase {
    //    /// <summary>
    //    /// Verifies that the parameter string contains no invalid characters. If invalid parameters are found, an exception is raised.
    //    /// </summary>
    //    /// <param name="parameter"></param>
    //    public void ValidateGestPayParameter(string parameter, string paramName = "") {
    //        if (Regex.IsMatch(parameter, "[& §()*<>,;:\\[ \\]/%?=]")) {
    //            throw new FormatException(string.Format("Invalid character in parameter{0}.", string.IsNullOrWhiteSpace(paramName) ? string.Empty : paramName));
    //        }
    //    }
    //    /// <summary>
    //    /// Verifies that the parameter string contains no invalid characters. If invalid parameters are found, an exception is raised.
    //    /// </summary>
    //    /// <param name="parameter"></param>
    //    public void ValidateRedParameter(string parameter, string paramName = "") {
    //        ValidateGestPayParameter(parameter, paramName);
    //        if (Regex.IsMatch(parameter, "[$!^~#]")) {
    //            throw new FormatException(string.Format("Invalid character in parameter{0}.", string.IsNullOrWhiteSpace(paramName) ? string.Empty : paramName));
    //        }
    //    }
    //}

    /// <summary>
    /// this class keeps all the information we send to gestpay in a single object. A lot of the information in it is not currently used, but it's included
    /// in case we decide to implement the rest of gestpay's functionalities
    /// </summary>
    public class GestPayTransaction {
        #region Mandatory properties
        [Required]
        [StringLength(3)]
        [ValidGestPayParameter]
        public string uicCode { get; set; } //code identifying currency in which transaction amount is denominated. THis is the number code, not the alpha3
        [Required]
        [StringLength(9)]
        [ValidGestPayParameter]
        public string amount { get; set; } //Transaction amount. Do not insert thousands separator. Decimals, max 2 digits, are optional and separator is point
        [Required]
        [StringLength(50)]
        [ValidGestPayParameter]
        public string shopTransactionID { get; set; } //Identifier attributed to merchant's transaction
        #endregion
        #region Optional properties
        [ValidGestPayParameter]
        public string cardNumber { get; set; }
        [ValidGestPayParameter]
        public string expiryMonth { get; set; }
        [ValidGestPayParameter]
        public string expiryYear { get; set; }
        [StringLength(50)]
        [ValidGestPayParameter]
        public string buyerName { get; set; } //Buyer's name and surname
        [StringLength(50)]
        [ValidGestPayParameter]
        public string buyerEmail { get; set; } //Buyer's email address
        [StringLength(2)]
        [ValidGestPayParameter]
        public string languageId { get; set; } //Code identifying language used in communication with buyer
        [ValidGestPayParameter]
        public string cvv { get; set; }
        [StringLength(1000)]
        [ValidGestPayParameter]
        public string customInfo { get; set; } //String containing specific infomation as configured in the merchant's profile
        [StringLength(25)]
        [ValidGestPayParameter]
        public string requestToken { get; set; } //"MASKEDPAN" for a standard token, any other value for Custom token. Using :FORCED: before the token, it's possible to have the token even if the transaction is not authorized
        [StringLength(1)]
        [ValidGestPayParameter]
        public string ppSellerProtection { get; set; } //Parameter to set the use of a confirmed address
        public GenericShippingDetails shippingDetails { get; set; }
        [ValidGestPayListParameter]
        public List<string> paymentTypes { get; set; } //set of tags to set the visibility of payment systems on payment page (see CodeTables.PaymentTypeCodes)
        public GenericPaymentTypeDetail paymentTypeDetail { get; set; }
        [StringLength(1)]
        [ValidRedGestPayParameter]
        public string redFraudPrevention { get; set; } //flag to activate Red Fraud Prevention (redFraudPrevention = "1")
        public GenericRedCustomerInfo Red_CustomerInfo { get; set; }
        public GenericRedShippingInfo Red_ShippingInfo { get; set; }
        public GenericRedBillingInfo Red_BillingInfo { get; set; }
        public GenericRedCustomerData Red_CustomerData { get; set; }
        [ValidRedGestPayListParameter]
        public List<string> Red_CustomInfo { get; set; }
        public GenericRedItems Red_Items { get; set; }
        [StringLength(3)]
        [ValidGestPayParameter]
        public string Consel_MerchantPro { get; set; } //merchant promotional code (mandatory to show consel in the pagam's payment method)
        public GenericConselCustomerInfo Consel_CustomerInfo { get; set; }
        [StringLength(127)]
        [ValidGestPayParameter]
        public string payPalBillingAgreementDescription { get; set; } //description of the goods, terms and conditions of the billing agreement
        public GenericEcommGestpayPaymentDetails OrderDetails { get; set; }
        #endregion

        public PaymentRecord record { get; set; }

        public GestPayTransaction() {
            shippingDetails = new GenericShippingDetails();
            paymentTypes = new List<string>();
            paymentTypeDetail = new GenericPaymentTypeDetail();
            Red_CustomerInfo = new GenericRedCustomerInfo();
            Red_ShippingInfo = new GenericRedShippingInfo();
            Red_BillingInfo = new GenericRedBillingInfo();
            Red_CustomerData = new GenericRedCustomerData();
            Red_CustomInfo = new List<string>();
            Red_Items = new GenericRedItems();
            Consel_CustomerInfo = new GenericConselCustomerInfo();
            OrderDetails = new GenericEcommGestpayPaymentDetails();
        }
        public GestPayTransaction(PaymentRecord pr)
            : this() {
            record = pr;
            //uicCode = pr.Currency;
            var curCode = CodeTables.CurrencyCodes.Where(cc => cc.isoCode.ToUpperInvariant() == pr.Currency.ToUpperInvariant()).SingleOrDefault();
            if (curCode == null) {
                //TODO: this is currently just a workaround
                //unable to identify currency. Set to EURO for now
                curCode = CodeTables.CurrencyCodes.Where(cc => cc.isoCode.ToUpperInvariant() == "EUR").SingleOrDefault();
            }
            uicCode = curCode.codeUIC.ToString();
            amount = pr.Amount.ToString("0.##", CultureInfo.InvariantCulture);
            shopTransactionID = pr.Id.ToString();
            //customInfo = pr.Reason;
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ShippingDetails classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericShippingDetails {
        [StringLength(32)]
        [ValidGestPayParameter]
        public string shipToName { get; set; } //string containing the shipping name
        [StringLength(100)]
        [ValidGestPayParameter]
        public string shipToStreet { get; set; } //string containing the shipping address
        [StringLength(40)]
        [ValidGestPayParameter]
        public string shipToCity { get; set; } //string containing the shipping city
        [StringLength(40)]
        [ValidGestPayParameter]
        public string shipToState { get; set; } //string containing the shipping state (see CodeTables.StateCodes)
        [StringLength(2)]
        [ValidGestPayParameter]
        public string shipToCountryCode { get; set; } //string containing the shipping country code (see CodeTables.ISOCountryCodes)
        [StringLength(20)]
        [ValidGestPayParameter]
        public string shipToZip { get; set; } //string containing the shipping zip
        [StringLength(100)]
        [ValidGestPayParameter]
        public string shipToStreet2 { get; set; } //string containing a shipping address additional field

        /// <summary>
        /// This method computes the object used to provide shipping details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ShippingDetails TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.ShippingDetails {
                shipToName = this.shipToName,
                shipToStreet = this.shipToStreet,
                shipToCity = this.shipToCity,
                shipToState = this.shipToState,
                shipToCountryCode = this.shipToCountryCode,
                shipToZip = this.shipToZip,
                shipToStreet2 = this.shipToStreet2
            };
        }
        /// <summary>
        /// This method computes the object used to provide shipping details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ShippingDetails ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.ShippingDetails {
                shipToName = this.shipToName,
                shipToStreet = this.shipToStreet,
                shipToCity = this.shipToCity,
                shipToState = this.shipToState,
                shipToCountryCode = this.shipToCountryCode,
                shipToZip = this.shipToZip,
                shipToStreet2 = this.shipToStreet2
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { shipToName, shipToStreet, shipToCity, shipToState, shipToCountryCode, shipToZip, shipToStreet2 };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }

    }
    /// <summary>
    /// This class contains the same exact information of the PaymentTypeDetail classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericPaymentTypeDetail {
        [StringLength(25)]
        [ValidGestPayParameter]
        public string MyBankBankCode { get; set; } //tag to set the Bank to show on payment page (the bank list is retrieved form WsS2S.CallMyBankListS2S)
        [StringLength(25)]
        [ValidGestPayParameter]
        public string IdealBankCode { get; set; } //tag to set the Bank to show on payment page (the bank list is retrieved form WsS2S.CallMyBankListS2S)

        /// <summary>
        /// This method computes the object used to provide payment type details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.PaymentTypeDetail TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.PaymentTypeDetail {
                MyBankBankCode = this.MyBankBankCode,
                IdealBankCode = this.IdealBankCode
            };
        }
        /// <summary>
        /// This method computes the object used to provide payment type details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.PaymentTypeDetail ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.PaymentTypeDetail {
                MyBankBankCode = this.MyBankBankCode,
                IdealBankCode = this.IdealBankCode
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { MyBankBankCode, IdealBankCode };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedCustomerInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedCustomerInfo {
        [StringLength(5)]
        [ValidRedGestPayParameter]
        public string Customer_Title { get; set; } //customer title
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Customer_Name { get; set; } //customer first name
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Customer_Surname { get; set; } //customer last name
        [StringLength(45)]
        [ValidRedGestPayEmailParameter]
        public string Customer_Email { get; set; } //customer email address - value must contain @
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Customer_Address { get; set; } //customer address line 1
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Customer_Address2 { get; set; } //customer address line 2
        [StringLength(20)]
        [ValidRedGestPayParameter]
        public string Customer_City { get; set; } //customer address city
        [StringLength(2)]
        [ValidRedGestPayParameter]
        public string Customer_StateCode { get; set; } //customer address state code
        [StringLength(3)]
        [ValidRedGestPayParameter]
        public string Customer_Country { get; set; } //customer country code - ISO-Alpha 3 (see CodeTables.ISOCountryCodes)
        [StringLength(9)]
        [ValidRedGestPayParameter]
        public string Customer_PostalCode { get; set; } //customer post/zip code
        [StringLength(19)]
        [ValidRedGestPayEmailParameter]
        public string Customer_Phone { get; set; } //Customer phone - no spaces

        /// <summary>
        /// This method computes the object used to provide customer info details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedCustomerInfo TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.RedCustomerInfo {
                Customer_Title = this.Customer_Title,
                Customer_Name = this.Customer_Name,
                Customer_Surname = this.Customer_Surname,
                Customer_Email = this.Customer_Email,
                Customer_Address = this.Customer_Address,
                Customer_Address2 = this.Customer_Address2,
                Customer_City = this.Customer_City,
                Customer_StateCode = this.Customer_StateCode,
                Customer_Country = this.Customer_Country,
                Customer_PostalCode = this.Customer_PostalCode,
                Customer_Phone = this.Customer_Phone
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer info details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedCustomerInfo ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.RedCustomerInfo {
                Customer_Title = this.Customer_Title,
                Customer_Name = this.Customer_Name,
                Customer_Surname = this.Customer_Surname,
                Customer_Email = this.Customer_Email,
                Customer_Address = this.Customer_Address,
                Customer_Address2 = this.Customer_Address2,
                Customer_City = this.Customer_City,
                Customer_StateCode = this.Customer_StateCode,
                Customer_Country = this.Customer_Country,
                Customer_PostalCode = this.Customer_PostalCode,
                Customer_Phone = this.Customer_Phone
            };
        }
        
        private bool ParamsAreAllNull() {
            string[] para = { Customer_Title, Customer_Name, Customer_Surname, Customer_Email, Customer_Address, Customer_Address2, Customer_City, Customer_StateCode, Customer_Country, Customer_PostalCode, Customer_Phone };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedShippingInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedShippingInfo {
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Shipping_Name { get; set; } //shipping first name
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Shipping_Surname { get; set; } //shipping last name
        [StringLength(45)]
        [ValidRedGestPayEmailParameter]
        public string Shipping_Email { get; set; } //customer email address - value must contain @
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Shipping_Address { get; set; } //shipping address line 1
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Shipping_Address2 { get; set; } //shipping address line 2
        [StringLength(20)]
        [ValidRedGestPayParameter]
        public string Shipping_City { get; set; } //shipping address city
        [StringLength(2)]
        [ValidRedGestPayParameter]
        public string Shipping_StateCode { get; set; } //shipping address state code
        [StringLength(3)]
        [ValidRedGestPayParameter]
        public string Shipping_Country { get; set; } //shipping country code  - ISO-Alpha 3 (see CodeTables.ISOCountryCodes)
        [StringLength(9)]
        [ValidRedGestPayParameter]
        public string Shipping_PostalCode { get; set; } //shipping post/zip code
        [StringLength(19)]
        [ValidRedGestPayNoSpaceParameter]
        public string Shipping_HomePhone { get; set; } //Customer home phone - no spaces
        [StringLength(12)]
        [ValidRedGestPayNoSpaceParameter]
        public string Shipping_FaxPhone { get; set; } //Customer fax phone - no spaces
        [StringLength(19)]
        [ValidRedGestPayParameter]
        public string Shipping_MobilePhone { get; set; } //Customer mobile phone
        [StringLength(19)]
        [ValidRedGestPayParameter]
        public string Shipping_TimeToDeparture { get; set; } //shipping time to departure

        /// <summary>
        /// This method computes the object used to provide customer shipping details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedShippingInfo TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.RedShippingInfo {
                Shipping_Name = this.Shipping_Name,
                Shipping_Surname = this.Shipping_Surname,
                Shipping_Email = this.Shipping_Email,
                Shipping_Address = this.Shipping_Address,
                Shipping_Address2 = this.Shipping_Address2,
                Shipping_City = this.Shipping_City,
                Shipping_StateCode = this.Shipping_StateCode,
                Shipping_Country = this.Shipping_Country,
                Shipping_PostalCode = this.Shipping_PostalCode,
                Shipping_HomePhone = this.Shipping_HomePhone,
                Shipping_FaxPhone = this.Shipping_FaxPhone,
                Shipping_MobilePhone = this.Shipping_MobilePhone,
                Shipping_TimeToDeparture = this.Shipping_TimeToDeparture
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer shipping details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedShippingInfo ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.RedShippingInfo {
                Shipping_Name = this.Shipping_Name,
                Shipping_Surname = this.Shipping_Surname,
                Shipping_Email = this.Shipping_Email,
                Shipping_Address = this.Shipping_Address,
                Shipping_Address2 = this.Shipping_Address2,
                Shipping_City = this.Shipping_City,
                Shipping_StateCode = this.Shipping_StateCode,
                Shipping_Country = this.Shipping_Country,
                Shipping_PostalCode = this.Shipping_PostalCode,
                Shipping_HomePhone = this.Shipping_HomePhone,
                Shipping_FaxPhone = this.Shipping_FaxPhone,
                Shipping_MobilePhone = this.Shipping_MobilePhone,
                Shipping_TimeToDeparture = this.Shipping_TimeToDeparture
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { Shipping_Name, Shipping_Surname, Shipping_Email, Shipping_Address, Shipping_Address2,  Shipping_City, Shipping_StateCode, Shipping_Country, Shipping_PostalCode, 
                Shipping_HomePhone,  Shipping_FaxPhone, Shipping_MobilePhone, Shipping_TimeToDeparture };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedBillingInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedBillingInfo {
        [StringLength(16)]
        [ValidRedGestPayParameter]
        public string Billing_Id { get; set; } //billing id
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Billing_Name { get; set; } //Billing first name
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Billing_Surname { get; set; } //Billing last name
        [StringLength(10)]
        [ValidRedGestPayParameter]
        public string Billing_DateOfBirth { get; set; } //billing date of birth - format YYYYMMDD
        [StringLength(45)]
        [ValidRedGestPayEmailParameter]
        public string Billing_Email { get; set; } //billing email address - value must contain @
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Billing_Address { get; set; } //Billing address line 1
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Billing_Address2 { get; set; } //Billing address line 2
        [StringLength(20)]
        [ValidRedGestPayParameter]
        public string Billing_City { get; set; } //Billing address city
        [StringLength(2)]
        [ValidRedGestPayParameter]
        public string Billing_StateCode { get; set; } //Billing address state code
        [StringLength(3)]
        [ValidRedGestPayParameter]
        public string Billing_Country { get; set; } //Billing country code  - ISO-Alpha 3 (see CodeTables.ISOCountryCodes)
        [StringLength(9)]
        [ValidRedGestPayParameter]
        public string Billing_PostalCode { get; set; } //Billing post/zip code
        [StringLength(19)]
        [ValidRedGestPayNoSpaceParameter]
        public string Billing_HomePhone { get; set; } //billing home phone - no spaces
        [StringLength(19)]
        [ValidRedGestPayNoSpaceParameter]
        public string Billing_WorkPhone { get; set; } //billing work phone - no spaces
        [StringLength(19)]
        [ValidRedGestPayParameter]
        public string Billing_MobilePhone { get; set; } //billing mobile phone

        /// <summary>
        /// This method computes the object used to provide customer billing details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedBillingInfo TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.RedBillingInfo {
                Billing_Id = this.Billing_Id,
                Billing_Name = this.Billing_Name,
                Billing_Surname = this.Billing_Surname,
                Billing_DateOfBirth = this.Billing_DateOfBirth,
                Billing_Email = this.Billing_Email,
                Billing_Address = this.Billing_Address,
                Billing_Address2 = this.Billing_Address2,
                Billing_City = this.Billing_City,
                Billing_StateCode = this.Billing_StateCode,
                Billing_Country = this.Billing_Country,
                Billing_PostalCode = this.Billing_PostalCode,
                Billing_HomePhone = this.Billing_HomePhone,
                Billing_WorkPhone = this.Billing_WorkPhone,
                Billing_MobilePhone = this.Billing_MobilePhone
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer billing details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedBillingInfo ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.RedBillingInfo {
                Billing_Id = this.Billing_Id,
                Billing_Name = this.Billing_Name,
                Billing_Surname = this.Billing_Surname,
                Billing_DateOfBirth = this.Billing_DateOfBirth,
                Billing_Email = this.Billing_Email,
                Billing_Address = this.Billing_Address,
                Billing_Address2 = this.Billing_Address2,
                Billing_City = this.Billing_City,
                Billing_StateCode = this.Billing_StateCode,
                Billing_Country = this.Billing_Country,
                Billing_PostalCode = this.Billing_PostalCode,
                Billing_HomePhone = this.Billing_HomePhone,
                Billing_WorkPhone = this.Billing_WorkPhone,
                Billing_MobilePhone = this.Billing_MobilePhone
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { Billing_Id, Billing_Name, Billing_Surname, Billing_DateOfBirth, Billing_Email, Billing_Address, Billing_Address2,  Billing_City, Billing_StateCode, Billing_Country, 
                Billing_PostalCode, Billing_HomePhone, Billing_WorkPhone, Billing_MobilePhone };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedCustomerData classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedCustomerData {
        [StringLength(60)]
        [ValidRedGestPayParameter]
        public string MerchantWebSite { get; set; } //transaction source website
        [StringLength(45)]
        [ValidRedGestPayParameter]
        public string Customer_IpAddress { get; set; } //ip of customer - Format: nnn.nnn.nnn.nnn
        [StringLength(4000)]
        [ValidRedGestPayParameter]
        public string PC_FingerPrint { get; set; } //PC Finger Print. If the RED configuration is defined with the chance to fill this valu, but for some reasons it's left empty, then fill Red=ServiceType="N" to avoid error
        [StringLength(1)]
        [ValidRedGestPayParameter]
        public string PreviousCustomer { get; set; } //previous customer flag - format "Y" or "N"
        [StringLength(12)]
        [ValidRedGestPayParameter]
        public string Red_Merchant_ID { get; set; } //optional only for merchant with a specific set of rules (code provided by Sella)
        [StringLength(1)]
        [ValidRedGestPayParameter]
        public string Red_ServiceType { get; set; } //optional only for merchant with a specific set of rules (code provided by Sella)

        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedCustomerData TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.RedCustomerData {
                MerchantWebSite = this.MerchantWebSite,
                Customer_IPAddress = this.Customer_IpAddress,
                PC_FingerPrint = this.PC_FingerPrint,
                PreviousCustomer = this.PreviousCustomer,
                Red_Merchant_ID = this.Red_Merchant_ID,
                Red_ServiceType = this.Red_ServiceType
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedCustomerData ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.RedCustomerData {
                MerchantWebSite = this.MerchantWebSite,
                Customer_IPAddress = this.Customer_IpAddress,
                PC_FingerPrint = this.PC_FingerPrint,
                PreviousCustomer = this.PreviousCustomer,
                Red_Merchant_ID = this.Red_Merchant_ID,
                Red_ServiceType = this.Red_ServiceType
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { MerchantWebSite, Customer_IpAddress, PC_FingerPrint, PreviousCustomer, Red_Merchant_ID, Red_ServiceType };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedItems classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedItems {
        [StringLength(2)]
        [ValidRedGestPayParameter]
        public string NumberOfItems { get; set; }
        public List<GenericRedItem> Red_Item { get; set; }

        public GenericRedItems() {
            Red_Item = new List<GenericRedItem>();
        }

        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedItems TestVersion() {
            if (string.IsNullOrWhiteSpace(NumberOfItems) &&
                this.Red_Item.Where(ri => ri.TestVersion() != null).Count() == 0) {
                return null;
            }
            return new CryptDecryptTest.RedItems {
                NumberOfItems = this.NumberOfItems,
                Red_Item = this.Red_Item.Where(ri => ri.TestVersion() != null).Select(ri => ri.TestVersion()).ToArray()
            };
        }
        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedItems ProdVersion() {
            if (string.IsNullOrWhiteSpace(NumberOfItems) &&
                this.Red_Item.Where(ri => ri.ProdVersion() != null).Count() == 0) {
                return null;
            }
            return new CryptDecryptProd.RedItems {
                NumberOfItems = this.NumberOfItems,
                Red_Item = this.Red_Item.Where(ri => ri.ProdVersion() != null).Select(ri => ri.ProdVersion()).ToArray()
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the RedItem classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericRedItem {
        [StringLength(12)]
        [ValidRedGestPayParameter]
        public string Item_ProductCode { get; set; } //item product code
        [StringLength(12)]
        [ValidRedGestPayParameter]
        public string Item_StockKeepingUnit { get; set; } //item stock keeping unit
        [StringLength(26)]
        [ValidRedGestPayParameter]
        public string Item_Description { get; set; } //item description
        [StringLength(12)]
        [ValidRedGestPayParameter]
        public string Item_Quantity { get; set; } //item quantity - 1 should be sent as "10000"
        [StringLength(12)]
        [ValidRedGestPayParameter]
        public string Item_UnitCost { get; set; } //item cost amount - €5.00 should be sent as 50000
        [StringLength(12)]
        [ValidRedGestPayParameter]
        public string Item_TotalCost { get; set; } //total item amount (item qty * item cost), no decimal
        [StringLength(19)]
        [ValidRedGestPayParameter]
        public string Item_ShippingNumber { get; set; } //item shippping/tracking numberr
        [StringLength(160)]
        [ValidRedGestPayParameter]
        public string Item_GiftMessage { get; set; } //item gift message
        [StringLength(30)]
        [ValidRedGestPayParameter]
        public string Item_PartEAN_Number { get; set; } //item Park or EAN number
        [StringLength(160)]
        [ValidRedGestPayParameter]
        public string Item_ShippingComments { get; set; } //item shipping comments

        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.RedItem TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.RedItem {
                Item_ProductCode = this.Item_ProductCode,
                Item_StockKeepingUnit = this.Item_StockKeepingUnit,
                Item_Description = this.Item_Description,
                Item_Quantity = this.Item_Quantity,
                Item_UnitCost = this.Item_UnitCost,
                Item_TotalCost = this.Item_TotalCost,
                Item_ShippingNumber = this.Item_ShippingNumber,
                Item_GiftMessage = this.Item_GiftMessage,
                Item_PartEAN_Number = this.Item_PartEAN_Number,
                Item_ShippingComments = this.Item_ShippingComments
            };
        }
        /// <summary>
        /// This method computes the object used to provide item details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.RedItem ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.RedItem {
                Item_ProductCode = this.Item_ProductCode,
                Item_StockKeepingUnit = this.Item_StockKeepingUnit,
                Item_Description = this.Item_Description,
                Item_Quantity = this.Item_Quantity,
                Item_UnitCost = this.Item_UnitCost,
                Item_TotalCost = this.Item_TotalCost,
                Item_ShippingNumber = this.Item_ShippingNumber,
                Item_GiftMessage = this.Item_GiftMessage,
                Item_PartEAN_Number = this.Item_PartEAN_Number,
                Item_ShippingComments = this.Item_ShippingComments
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { Item_ProductCode, Item_StockKeepingUnit, Item_Description, Item_Quantity, Item_UnitCost, Item_TotalCost, Item_ShippingNumber, Item_GiftMessage, Item_PartEAN_Number, Item_ShippingComments };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ConselCustomerInfo classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericConselCustomerInfo {
        [StringLength(30)]
        [ValidGestPayParameter]
        public string Surname { get; set; } //customer surname
        [StringLength(30)]
        [ValidGestPayParameter]
        public string Name { get; set; } //customer name
        [StringLength(16)]
        [ValidGestPayParameter]
        public string TaxationCode { get; set; } //customer taxation code
        [StringLength(60)]
        [ValidGestPayParameter]
        public string Address { get; set; } //customer address
        [StringLength(30)]
        [ValidGestPayParameter]
        public string City { get; set; } //customer city
        [StringLength(2)]
        [ValidGestPayParameter]
        public string StateCode { get; set; } //customer state code
        [StringLength(10)]
        [ValidGestPayParameter]
        public string DateAddress { get; set; } //date since the customer lives in the declared address dd/mm/yyyy
        [StringLength(15)]
        [ValidGestPayParameter]
        public string Phone { get; set; } //customer phone
        [StringLength(15)]
        [ValidGestPayParameter]
        public string MobilePhone { get; set; } //customer mobile phone
        [ValidGestPayParameter]
        public string MunicipalCode { get; set; }
        [ValidGestPayParameter]
        public string StateBirthDate { get; set; }
        [ValidGestPayParameter]
        public string BirthDate { get; set; }
        [ValidGestPayParameter]
        public string Mail { get; set; }
        [ValidGestPayParameter]
        public string MunicipalDocumentCode { get; set; }
        [ValidGestPayParameter]
        public string Employment { get; set; }
        [ValidGestPayParameter]
        public string WorkingAddress { get; set; }
        [ValidGestPayParameter]
        public string MunicipalWorkingCode { get; set; }
        [ValidGestPayParameter]
        public string DocumentState { get; set; }
        [ValidGestPayParameter]
        public string DocumentNumber { get; set; }
        [ValidGestPayParameter]
        public string MunicipalBirthCode { get; set; }
        [ValidGestPayParameter]
        public string VisaExpiryDate { get; set; }
        [ValidGestPayParameter]
        public string Iban { get; set; }
        [ValidGestPayParameter]
        public string DocumentDate { get; set; }
        [ValidGestPayParameter]
        public string WorkingTelNumber { get; set; }
        [ValidGestPayParameter]
        public string WorkingState { get; set; }
        [ValidGestPayParameter]
        public string MonthlyPay { get; set; }

        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ConselCustomerInfo TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.ConselCustomerInfo {
                Surname = this.Surname,
                Name = this.Name,
                TaxationCode = this.TaxationCode,
                Address = this.Address,
                City = this.City,
                StateCode = this.StateCode,
                DateAddress = this.DateAddress,
                Phone = this.Phone,
                MobilePhone = this.MobilePhone,
                MunicipalCode = this.MunicipalCode,
                StateBirthDate = this.StateBirthDate,
                BirthDate = this.BirthDate,
                Mail = this.Mail,
                MunicipalDocumentCode = this.MunicipalDocumentCode,
                Employment = this.Employment,
                WorkingAddress = this.WorkingAddress,
                MunicipalWorkingCode = this.MunicipalWorkingCode,
                DocumentState = this.DocumentState,
                DocumentNumber = this.DocumentNumber,
                MunicipalBirthCode = this.MunicipalBirthCode,
                VisaExpiryDate = this.VisaExpiryDate,
                Iban = this.Iban,
                DocumentDate = this.DocumentDate,
                WorkingTelNumber = this.WorkingTelNumber,
                WorkingState = this.WorkingState,
                MonthlyPay = this.MonthlyPay
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ConselCustomerInfo ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.ConselCustomerInfo {
                Surname = this.Surname,
                Name = this.Name,
                TaxationCode = this.TaxationCode,
                Address = this.Address,
                City = this.City,
                StateCode = this.StateCode,
                DateAddress = this.DateAddress,
                Phone = this.Phone,
                MobilePhone = this.MobilePhone,
                MunicipalCode = this.MunicipalCode,
                StateBirthDate = this.StateBirthDate,
                BirthDate = this.BirthDate,
                Mail = this.Mail,
                MunicipalDocumentCode = this.MunicipalDocumentCode,
                Employment = this.Employment,
                WorkingAddress = this.WorkingAddress,
                MunicipalWorkingCode = this.MunicipalWorkingCode,
                DocumentState = this.DocumentState,
                DocumentNumber = this.DocumentNumber,
                MunicipalBirthCode = this.MunicipalBirthCode,
                VisaExpiryDate = this.VisaExpiryDate,
                Iban = this.Iban,
                DocumentDate = this.DocumentDate,
                WorkingTelNumber = this.WorkingTelNumber,
                WorkingState = this.WorkingState,
                MonthlyPay = this.MonthlyPay
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { Surname, Name, TaxationCode, Address, City, StateCode, DateAddress, Phone, MobilePhone, MunicipalCode,  StateBirthDate, BirthDate, Mail, MunicipalDocumentCode, 
                Employment, WorkingAddress, MunicipalWorkingCode, DocumentState, DocumentNumber, MunicipalBirthCode, VisaExpiryDate, Iban, DocumentDate, WorkingTelNumber,  WorkingState, MonthlyPay };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the EcommGestpayPaymentDetails classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericEcommGestpayPaymentDetails {
        public GenericFraudPrevention FraudPrevention { get; set; }
        public GenericCustomerDetail CustomerDetail { get; set; }
        public GenericShippingAddress ShippingAddress { get; set; }
        public GenericBillingAddress BillingAddress { get; set; }
        public List<GenericProductDetail> ProductDetails { get; set; }
        public List<GenericDiscountCode> DiscountCodes { get; set; }
        public List<GenericShippingLine> ShippingLines { get; set; }

        public GenericEcommGestpayPaymentDetails() {
            FraudPrevention = new GenericFraudPrevention();
            CustomerDetail = new GenericCustomerDetail();
            ShippingAddress = new GenericShippingAddress();
            BillingAddress = new GenericBillingAddress();
            ProductDetails = new List<GenericProductDetail>();
            DiscountCodes = new List<GenericDiscountCode>();
            ShippingLines = new List<GenericShippingLine>();
        }

        /// <summary>
        /// This method computes the object used to provide payment details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.EcommGestpayPaymentDetails TestVersion() {
            if (this.FraudPrevention.TestVersion() == null && this.CustomerDetail.TestVersion() == null && this.ShippingAddress.TestVersion() == null &&
                this.BillingAddress.TestVersion() == null && this.ProductDetails.Where(pd => pd.TestVersion() != null).Count() == 0 &&
                this.DiscountCodes.Where(dc => dc.TestVersion() != null).Count() == 0 &&
                this.ShippingLines.Where(sl => sl.TestVersion() != null).Count() == 0) {
                return null;
            }
            return new CryptDecryptTest.EcommGestpayPaymentDetails {
                FraudPrevention = this.FraudPrevention.TestVersion(),
                CustomerDetail = this.CustomerDetail.TestVersion(),
                ShippingAddress = this.ShippingAddress.TestVersion(),
                BillingAddress = this.BillingAddress.TestVersion(),
                ProductDetails = this.ProductDetails.Where(pd => pd.TestVersion() != null).Select(pd => pd.TestVersion()).ToArray(),
                DiscountCodes = this.DiscountCodes.Where(dc => dc.TestVersion() != null).Select(dc => dc.TestVersion()).ToArray(),
                ShippingLines = this.ShippingLines.Where(sl => sl.TestVersion() != null).Select(sl => sl.TestVersion()).ToArray()
            };
        }
        /// <summary>
        /// This method computes the object used to provide payment details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.EcommGestpayPaymentDetails ProdVersion() {
            if (this.FraudPrevention.ProdVersion() == null && this.CustomerDetail.ProdVersion() == null && this.ShippingAddress.ProdVersion() == null &&
                this.BillingAddress.ProdVersion() == null && this.ProductDetails.Where(pd => pd.ProdVersion() != null).Count() == 0 &&
                this.DiscountCodes.Where(dc => dc.ProdVersion() != null).Count() == 0 &&
                this.ShippingLines.Where(sl => sl.ProdVersion() != null).Count() == 0) {
                return null;
            }
            return new CryptDecryptProd.EcommGestpayPaymentDetails {
                FraudPrevention = this.FraudPrevention.ProdVersion(),
                CustomerDetail = this.CustomerDetail.ProdVersion(),
                ShippingAddress = this.ShippingAddress.ProdVersion(),
                BillingAddress = this.BillingAddress.ProdVersion(),
                ProductDetails = this.ProductDetails.Where(pd => pd.ProdVersion() != null).Select(pd => pd.ProdVersion()).ToArray(),
                DiscountCodes = this.DiscountCodes.Where(dc => dc.ProdVersion() != null).Select(dc => dc.ProdVersion()).ToArray(),
                ShippingLines = this.ShippingLines.Where(sl => sl.ProdVersion() != null).Select(sl => sl.ProdVersion()).ToArray()
            };
        }
    }
    /// <summary>
    /// This class contains the same exact information of the FraudPrevention classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericFraudPrevention {
        [ValidGestPayParameter]
        public string SubmitForReview { get; set; }
        [ValidGestPayParameter]
        public string OrderDateTime { get; set; }
        [ValidGestPayParameter]
        public string OrderNote { get; set; }
        [ValidGestPayParameter]
        public string Source { get; set; }
        [ValidGestPayParameter]
        public string SubmissionReason { get; set; }
        [ValidGestPayParameter]
        public string BeaconSessionID { get; set; }

        /// <summary>
        /// This method computes the object used to provide fraud prevention details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.FraudPrevention TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.FraudPrevention {
                SubmitForReview = this.SubmitForReview,
                OrderDateTime = this.OrderDateTime,
                OrderNote = this.OrderNote,
                Source = this.Source,
                SubmissionReason = this.SubmissionReason,
                BeaconSessionID = this.BeaconSessionID
            };
        }
        /// <summary>
        /// This method computes the object used to provide fraud prevention details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.FraudPrevention ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.FraudPrevention {
                SubmitForReview = this.SubmitForReview,
                OrderDateTime = this.OrderDateTime,
                OrderNote = this.OrderNote,
                Source = this.Source,
                SubmissionReason = this.SubmissionReason,
                BeaconSessionID = this.BeaconSessionID
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { SubmitForReview, OrderDateTime, OrderNote, Source, SubmissionReason, BeaconSessionID };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the CustomerDetail classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericCustomerDetail {
        [StringLength(12)]
        [ValidGestPayParameter]
        public string ProfileID { get; set; } //customer profile ID
        [StringLength(50)]
        [ValidGestPayParameter]
        public string MerchantCustomerID { get; set; } //merchant customer ID
        [StringLength(65)]
        [ValidGestPayParameter]
        public string FirstName { get; set; } //customer first name
        [StringLength(65)]
        [ValidGestPayParameter]
        public string MiddleName { get; set; } //customer middle name
        [StringLength(65)]
        [ValidGestPayParameter]
        public string Lastname { get; set; } //customer last name
        [StringLength(100)]
        [ValidGestPayParameter]
        public string PrimaryEmail { get; set; } //customer primary email
        [StringLength(100)]
        [ValidGestPayParameter]
        public string SecondaryEmail { get; set; } //customer secondary email
        [StringLength(20)]
        [ValidGestPayParameter]
        public string PrimaryPhone { get; set; } //customer's phone including prefix
        [StringLength(20)]
        [ValidGestPayParameter]
        public string SecondaryPhone { get; set; } //customer's phone including prefix
        [StringLength(10)]
        [ValidGestPayParameter]
        public string DateOfBirth { get; set; } //customer date of birth dd/mm/yyyy
        [StringLength(1)]
        [ValidGestPayParameter]
        public string Gender { get; set; } //customer gender ("0"=Male "1"=Female)
        [StringLength(20)]
        [ValidGestPayParameter]
        public string SocialSecurityNumber { get; set; } //customer's social or fiscal identifier (for klarna use)
        [StringLength(255)]
        [ValidGestPayParameter]
        public string Company { get; set; } //customer company
        [ValidGestPayParameter]
        public string CreatedAtDate { get; set; }
        [ValidGestPayParameter]
        public string VerifiedEmail { get; set; }
        [ValidGestPayParameter]
        public string AccountType { get; set; }
        public GenericCustomerSocial Social { get; set; }

        public GenericCustomerDetail() {
            Social = new GenericCustomerSocial();
        }

        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.CustomerDetail TestVersion() {
            if (this.ParamsAreAllNull() && this.Social.TestVersion() == null) {
                return null;
            }
            return new CryptDecryptTest.CustomerDetail {
                ProfileID = this.ProfileID,
                MerchantCustomerID = this.MerchantCustomerID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                PrimaryEmail = this.PrimaryEmail,
                SecondaryEmail = this.SecondaryEmail,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                SocialSecurityNumber = this.SocialSecurityNumber,
                Company = this.Company,
                CreatedAtDate = this.CreatedAtDate,
                VerifiedEmail = this.VerifiedEmail,
                AccountType = this.AccountType,
                Social = this.Social.TestVersion()
            };
        }
        /// <summary>
        /// This method computes the object used to provide customer details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.CustomerDetail ProdVersion() {
            if (this.ParamsAreAllNull() && this.Social.ProdVersion() == null) {
                return null;
            }
            return new CryptDecryptProd.CustomerDetail {
                ProfileID = this.ProfileID,
                MerchantCustomerID = this.MerchantCustomerID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                PrimaryEmail = this.PrimaryEmail,
                SecondaryEmail = this.SecondaryEmail,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                SocialSecurityNumber = this.SocialSecurityNumber,
                Company = this.Company,
                CreatedAtDate = this.CreatedAtDate,
                VerifiedEmail = this.VerifiedEmail,
                AccountType = this.AccountType,
                Social = this.Social.ProdVersion()
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { ProfileID, MerchantCustomerID, FirstName, MiddleName, Lastname, PrimaryEmail, SecondaryEmail, PrimaryPhone, SecondaryPhone, DateOfBirth, Gender, SocialSecurityNumber, Company, CreatedAtDate, VerifiedEmail };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the CustomerSocial classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericCustomerSocial {
        [ValidGestPayParameter]
        public string Network { get; set; }
        [ValidGestPayParameter]
        public string PublicUsername { get; set; }
        [ValidGestPayParameter]
        public string CommunityScore { get; set; }
        [ValidGestPayParameter]
        public string ProfilePicture { get; set; }
        [ValidGestPayParameter]
        public string Email { get; set; }
        [ValidGestPayParameter]
        public string Bio { get; set; }
        [ValidGestPayParameter]
        public string AccountUrl { get; set; }
        [ValidGestPayParameter]
        public string Following { get; set; }
        [ValidGestPayParameter]
        public string Followed { get; set; }
        [ValidGestPayParameter]
        public string Posts { get; set; }
        [ValidGestPayParameter]
        public string Id { get; set; }
        [ValidGestPayParameter]
        public string AuthToken { get; set; }
        [ValidGestPayParameter]
        public string SocialData { get; set; }

        /// <summary>
        /// This method computes the object used to provide social data details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.CustomerSocial TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.CustomerSocial {
                Network = this.Network,
                PublicUsername = this.PublicUsername,
                CommunityScore = this.CommunityScore,
                ProfilePicture = this.ProfilePicture,
                Email = this.Email,
                Bio = this.Bio,
                AccountUrl = this.AccountUrl,
                Following = this.Following,
                Followed = this.Followed,
                Posts = this.Posts,
                Id = this.Id,
                AuthToken = this.AuthToken,
                SocialData = this.SocialData
            };
        }
        /// <summary>
        /// This method computes the object used to provide social data details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.CustomerSocial ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.CustomerSocial {
                Network = this.Network,
                PublicUsername = this.PublicUsername,
                CommunityScore = this.CommunityScore,
                ProfilePicture = this.ProfilePicture,
                Email = this.Email,
                Bio = this.Bio,
                AccountUrl = this.AccountUrl,
                Following = this.Following,
                Followed = this.Followed,
                Posts = this.Posts,
                Id = this.Id,
                AuthToken = this.AuthToken,
                SocialData = this.SocialData
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { Network, PublicUsername, CommunityScore, ProfilePicture, Email, Bio, AccountUrl, Following, Followed, Posts, Id, AuthToken, SocialData };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ShippingAddress classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericShippingAddress {
        [StringLength(12)]
        [ValidGestPayParameter]
        public string ProfileID { get; set; } //profile ID
        [StringLength(65)]
        [ValidGestPayParameter]
        public string FirstName { get; set; } //first name
        [StringLength(65)]
        [ValidGestPayParameter]
        public string MiddleName { get; set; } //middle name
        [StringLength(65)]
        [ValidGestPayParameter]
        public string Lastname { get; set; } //last name
        [StringLength(100)]
        [ValidGestPayParameter]
        public string StreetName { get; set; } //shipping street
        [StringLength(100)]
        [ValidGestPayParameter]
        public string Streetname2 { get; set; } //shipping street second line
        [StringLength(5)]
        [ValidGestPayParameter]
        public string HouseNumber { get; set; } //
        [StringLength(5)]
        [ValidGestPayParameter]
        public string HouseExtension { get; set; } //
        [StringLength(50)]
        [ValidGestPayParameter]
        public string City { get; set; } //shipping city
        [StringLength(50)]
        [ValidGestPayParameter]
        public string ZipCode { get; set; } //shipping zip code
        [StringLength(50)]
        [ValidGestPayParameter]
        public string State { get; set; } //shipping state
        [StringLength(2)]
        [ValidGestPayParameter]
        public string CountryCode { get; set; } //alpha-2 country code (see CodeTables.ISOCountryCodes)
        [StringLength(100)]
        [ValidGestPayParameter]
        public string Email { get; set; } //shipping contact email
        [StringLength(20)]
        [ValidGestPayParameter]
        public string PrimaryPhone { get; set; } //shipping primary phone
        [StringLength(20)]
        [ValidGestPayParameter]
        public string SecondaryPhone { get; set; } //shipping secondary phone
        [ValidGestPayParameter]
        public string Company { get; set; }
        [ValidGestPayParameter]
        public string StateCode { get; set; }

        /// <summary>
        /// This method computes the object used to provide shipping address details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ShippingAddress TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.ShippingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }
        /// <summary>
        /// This method computes the object used to provide shipping address details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ShippingAddress ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.ShippingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { ProfileID, FirstName, MiddleName, Lastname, StreetName, Streetname2, HouseNumber, HouseExtension, City, ZipCode, State, CountryCode, Email, PrimaryPhone, SecondaryPhone, Company, StateCode };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the BillingAddress classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericBillingAddress {
        [StringLength(12)]
        [ValidGestPayParameter]
        public string ProfileID { get; set; } //profile ID
        [StringLength(65)]
        [ValidGestPayParameter]
        public string FirstName { get; set; } //first name
        [StringLength(65)]
        [ValidGestPayParameter]
        public string MiddleName { get; set; } //middle name
        [StringLength(65)]
        [ValidGestPayParameter]
        public string Lastname { get; set; } //last name
        [StringLength(100)]
        [ValidGestPayParameter]
        public string StreetName { get; set; } //shipping street
        [StringLength(100)]
        [ValidGestPayParameter]
        public string Streetname2 { get; set; } //shipping street second line
        [StringLength(5)]
        [ValidGestPayParameter]
        public string HouseNumber { get; set; } //
        [StringLength(5)]
        [ValidGestPayParameter]
        public string HouseExtension { get; set; } //
        [StringLength(50)]
        [ValidGestPayParameter]
        public string City { get; set; } //billing city
        [StringLength(50)]
        [ValidGestPayParameter]
        public string ZipCode { get; set; } //billing zip code
        [StringLength(50)]
        [ValidGestPayParameter]
        public string State { get; set; } //billing state
        [StringLength(2)]
        [ValidGestPayParameter]
        public string CountryCode { get; set; } //alpha-2 country code (see CodeTables.ISOCountryCodes)
        [StringLength(100)]
        [ValidGestPayParameter]
        public string Email { get; set; }
        [StringLength(20)]
        [ValidGestPayParameter]
        public string PrimaryPhone { get; set; } //
        [StringLength(20)]
        [ValidGestPayParameter]
        public string SecondaryPhone { get; set; } //
        [ValidGestPayParameter]
        public string Company { get; set; }
        [ValidGestPayParameter]
        public string StateCode { get; set; }

        /// <summary>
        /// This method computes the object used to provide billing address details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.BillingAddress TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.BillingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }
        /// <summary>
        /// This method computes the object used to provide billing address details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.BillingAddress ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.BillingAddress {
                ProfileID = this.ProfileID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                Lastname = this.Lastname,
                StreetName = this.StreetName,
                Streetname2 = this.Streetname2,
                HouseNumber = this.HouseNumber,
                HouseExtention = this.HouseExtension,
                City = this.City,
                ZipCode = this.ZipCode,
                State = this.State,
                CountryCode = this.CountryCode,
                Email = this.Email,
                PrimaryPhone = this.PrimaryPhone,
                SecondaryPhone = this.SecondaryPhone,
                Company = this.Company,
                StateCode = this.StateCode
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { ProfileID, FirstName, MiddleName, Lastname, StreetName, Streetname2, HouseNumber, HouseExtension, City, ZipCode, State, CountryCode, Email, PrimaryPhone, SecondaryPhone, Company, StateCode };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ProductDetail classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericProductDetail {
        [StringLength(12)]
        [ValidGestPayParameter]
        public string ProductCode { get; set; } //Article's product code
        [StringLength(50)]
        [ValidGestPayParameter]
        public string SKU { get; set; } //article's stock keeping unit
        [StringLength(100)]
        [ValidGestPayParameter]
        public string Name { get; set; } //article's name
        [StringLength(255)]
        [ValidGestPayParameter]
        public string Description { get; set; } //article's description
        [StringLength(3)]
        [ValidGestPayParameter]
        public string Quantity { get; set; } //the number of products
        [StringLength(12)]
        [ValidGestPayParameter]
        public string Price { get; set; } //the number of products
        [StringLength(12)]
        [ValidGestPayParameter]
        public string UnitPrice { get; set; } //article's unit price
        [StringLength(2)]
        [ValidGestPayParameter]
        public string Type { get; set; } //the type of article: 1-product,, 2-shipping, 3-handling
        [StringLength(2)]
        [ValidGestPayParameter]
        public string Vat { get; set; } //value added tax (the value of the tax)
        [StringLength(2)]
        [ValidGestPayParameter]
        public string Discount { get; set; } //the amount offered by you as discount
        [ValidGestPayParameter]
        public string RequiresShipping { get; set; }
        [ValidGestPayParameter]
        public string Condition { get; set; }
        [ValidGestPayParameter]
        public string Seller { get; set; }
        [ValidGestPayParameter]
        public string Category { get; set; }
        [ValidGestPayParameter]
        public string SubCategory { get; set; }
        [ValidGestPayParameter]
        public string Brand { get; set; }
        [ValidGestPayParameter]
        public string DeliveryAt { get; set; }

        /// <summary>
        /// This method computes the object used to provide product details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ProductDetail TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.ProductDetail {
                ProductCode = this.ProductCode,
                SKU = this.SKU,
                Name = this.Name,
                Description = this.Description,
                Quantity = this.Quantity,
                Price = this.Price,
                UnitPrice = this.UnitPrice,
                Type = this.Type,
                Vat = this.Vat,
                Discount = this.Discount,
                RequiresShipping = this.RequiresShipping,
                Condition = this.Condition,
                Seller = this.Seller,
                Category = this.Category,
                SubCategory = this.SubCategory,
                Brand = this.Brand,
                DeliveryAt = this.DeliveryAt
            };
        }
        /// <summary>
        /// This method computes the object used to provide product details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ProductDetail ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.ProductDetail {
                ProductCode = this.ProductCode,
                SKU = this.SKU,
                Name = this.Name,
                Description = this.Description,
                Quantity = this.Quantity,
                Price = this.Price,
                UnitPrice = this.UnitPrice,
                Type = this.Type,
                Vat = this.Vat,
                Discount = this.Discount,
                RequiresShipping = this.RequiresShipping,
                Condition = this.Condition,
                Seller = this.Seller,
                Category = this.Category,
                SubCategory = this.SubCategory,
                Brand = this.Brand,
                DeliveryAt = this.DeliveryAt
            };
        }

        private bool ParamsAreAllNull() {
            string[] para = { ProductCode, SKU, Name, Description, Quantity, Price, UnitPrice, Type, Vat, Discount, RequiresShipping, Condition, Brand, DeliveryAt };
            if (string.IsNullOrWhiteSpace(string.Join("", para))) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the DiscountCode classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericDiscountCode {
        [ValidGestPayParameter]
        public string Amount { get; set; }
        [ValidGestPayParameter]
        public string Code { get; set; }

        /// <summary>
        /// This method computes the object used to provide discount code details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.DiscountCode TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.DiscountCode {
                Amount = this.Amount,
                Code = this.Code
            };
        }
        /// <summary>
        /// This method computes the object used to provide discount code details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.DiscountCode ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.DiscountCode {
                Amount = this.Amount,
                Code = this.Code
            };
        }

        private bool ParamsAreAllNull() {
            if (string.IsNullOrWhiteSpace(Amount) && string.IsNullOrWhiteSpace(Code)) {
                return true;
            } else {
                return false;
            }
        }
    }
    /// <summary>
    /// This class contains the same exact information of the ShippingLine classes from both the Test and Prod
    /// remote GestPay services. By using this, we can carry the info without resorting to either specific implementation.
    /// </summary>
    public partial class GenericShippingLine {
        [ValidGestPayParameter]
        public string Price { get; set; }
        [ValidGestPayParameter]
        public string Title { get; set; }
        [ValidGestPayParameter]
        public string Code { get; set; }

        /// <summary>
        /// This method computes the object used to provide shipping line details to the encrypt methods in the Test
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptTest.ShippingLine TestVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptTest.ShippingLine {
                Price = this.Price,
                Title = this.Title,
                Code = this.Code
            };
        }
        /// <summary>
        /// This method computes the object used to provide shipping line details to the encrypt methods in the Prod
        /// GestPay remote service.
        /// </summary>
        /// <returns></returns>
        public CryptDecryptProd.ShippingLine ProdVersion() {
            if (this.ParamsAreAllNull()) {
                return null;
            }
            return new CryptDecryptProd.ShippingLine {
                Price = this.Price,
                Title = this.Title,
                Code = this.Code
            };
        }

        private bool ParamsAreAllNull() {
            if (string.IsNullOrWhiteSpace(Price) && string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Code)) {
                return true;
            } else {
                return false;
            }
        }
    }


}