using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml;

namespace Laser.Orchard.PaymentGestPay.Models {
    /// <summary>
    /// This class is used to store the information from the xml we get from GestPay.
    /// </summary>
    public class TransactionOutcome {
        [StringLength(7)]
        public string TransactionType { get; set; } //transaction tye request ("DECRYPT" or "ENCRYPT")
        [StringLength(2)]
        [Required]
        public string TransactionResult { get; set; } //"OK", or "KO", or "XX". In case of "XX" we are actually expecting a further call.
        [StringLength(50)]
        public string ShopTransactionID { get; set; } //Identifier attributed to transaction (the id we sent)
        [StringLength(9)]
        public string BankTransactionID { get; set; } //Identifier attributed to the transaction by GestPay
        [StringLength(6)]
        public string AuthorizationCode { get; set; } //transaction authorization code
        [StringLength(3)]
        public string Currency { get; set; } //code identifying the currency in which the transaction amount is denominated. see CodeTables.CurrencyCodes
        [StringLength(9)]
        public string Amount { get; set; } //Transaction amount. Do not insert thousands separator. Decimals, max 2 digits, are optional and separator is point
        [StringLength(30)]
        public string Country { get; set; } //nationality of institute issuing card
        [StringLength(1000)]
        public string CustomInfo { get; set; } //String containing specific infomation as configured in the merchant's profile
        public GenericOutcomeBuyer Buyer { get; set; }
        [StringLength(255)]
        public string TDLevel { get; set; } //level of authentication for VBV Visa/Mastercard securecode transactions
        [StringLength(9)]
        [Required]
        public string ErrorCode { get; set; } //error code
        [StringLength(255)]
        [Required]
        public string ErrorDescription { get; set; } //error description
        [StringLength(9)]
        public string AlertCode { get; set; } //alert code
        [StringLength(255)]
        public string AlertDescription { get; set; } //alert description in chosen language
        [StringLength(1)]
        public string CVVPresent { get; set; } //credit card security code flag
        [StringLength(25)]
        public string MaskedPAN { get; set; } //masked pan string
        [StringLength(100)]
        public string PaymentMethod { get; set; } //indicates the used payment method
        [StringLength(25)]
        public string TOKEN { get; set; } //string containing the token value
        [StringLength(100)]
        public string ProductType { get; set; } //string containing card type
        [StringLength(2)]
        public string TokenExpiryMonth { get; set; } //string containing the token expiry month
        [StringLength(2)]
        public string TokenExpiryYear { get; set; } //string containing the token expiry year
        [StringLength(18)]
        public string TransactionKey { get; set; } //transaction identifier for 3D transactions. Only useful in Server-Server transations
        [StringLength(2)]
        public string VbV { get; set; } //verified by visa
        public string VbVRisp { get; set; } //encrypted string containing info for 3D-secure transactions. Only useful in Server-Server transations
        [StringLength(2)]
        public string VbVBuyer { get; set; } //info about enrollment of the buyer's card to 3D-Secure protool: "OK" means enrolled; "KO" means not enrolled
        [StringLength(2)]
        public string VbVFlag { get; set; } //info aout the 3D-secure status. Only useful in Server-Server transations
        [StringLength(9)]
        public string RedResponseCode { get; set; } //RED fraud score of the transaction, or Error code
        [StringLength(400)]
        public string RedResponseDescription { get; set; } //RED description of the redresponsecode

        public TransactionOutcome() { }
        public TransactionOutcome(XmlNode xml) {
            TransactionType = ReadXmlNode(xml, "TransactionType");
            TransactionResult = ReadXmlNode(xml, "TransactionResult");
            ShopTransactionID = ReadXmlNode(xml, "ShopTransactionID");
            BankTransactionID = ReadXmlNode(xml, "BankTransactionID");
            AuthorizationCode = ReadXmlNode(xml, "AuthorizationCode");
            Currency = ReadXmlNode(xml, "Currency");
            Amount = ReadXmlNode(xml, "Amount");
            Country = ReadXmlNode(xml, "Country");
            CustomInfo = ReadXmlNode(xml, "CustomInfo");
            Buyer = xml.SelectSingleNode("Buyer") == null ? null : new GenericOutcomeBuyer(xml.SelectSingleNode("Buyer"));
            TDLevel = ReadXmlNode(xml, "TDLevel");
            ErrorCode = ReadXmlNode(xml, "ErrorCode");
            ErrorDescription = ReadXmlNode(xml, "ErrorDescription");
            AlertCode = ReadXmlNode(xml, "AlertCode");
            AlertDescription = ReadXmlNode(xml, "AlertDescription");
            CVVPresent = ReadXmlNode(xml, "CVVPresent");
            MaskedPAN = ReadXmlNode(xml, "MaskedPAN");
            PaymentMethod = ReadXmlNode(xml, "PaymentMethod");
            TOKEN = ReadXmlNode(xml, "TOKEN");
            ProductType = ReadXmlNode(xml, "ProductType");
            TokenExpiryMonth = ReadXmlNode(xml, "TokenExpiryMonth");
            TokenExpiryYear = ReadXmlNode(xml, "TokenExpiryYear");
            TransactionKey = ReadXmlNode(xml, "TransactionKey");
            VbV = ReadXmlNode(xml, "VbV");
            VbVRisp = ReadXmlNode(xml, "VbVRisp");
            VbVBuyer = ReadXmlNode(xml, "VbVBuyer");
            VbVFlag = ReadXmlNode(xml, "VbVFlag");
            RedResponseCode = ReadXmlNode(xml, "RedResponseCode");
            RedResponseDescription = ReadXmlNode(xml, "RedResponseDescription");
        }
        public string ReadXmlNode(XmlNode xml, string nodeName) {
            XmlNode node = xml.SelectSingleNode(nodeName);
            if (node == null)
                return null;
            else
                return node.InnerText;
        }

        public static TransactionOutcome InternalError(string error) {
            return new TransactionOutcome {
                TransactionResult = "KO",
                ErrorCode = "0",
                ErrorDescription = error
            };
        }
    }

    public class GenericOutcomeBuyer {
        [StringLength(50)]
        public string BuyerName { get; set; } //buyer's name and surname
        [StringLength(50)]
        public string BuyerEmail { get; set; } //buyer's e-mail address

        public GenericOutcomeBuyer() { }

        public GenericOutcomeBuyer(XmlNode xml) {
            XmlNode node;
            node = xml.SelectSingleNode("BuyerName");
            if (node != null) {
                BuyerName = node.InnerText;
            }
            node = xml.SelectSingleNode("BuyerEmail");
            if (node != null) {
                BuyerEmail = node.InnerText;
            }
        }
    }
}