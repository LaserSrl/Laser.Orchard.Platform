using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml;

namespace Laser.Orchard.PaymentGestPay.Models {
    /// <summary>
    /// This class is used to store the information from the xml returned by the calls to the encryption servers
    /// </summary>
    public class EncryptDecryptTransactionResult {
        [StringLength(7)]
        public string TransactionType { get; set; } //"DECRYPT" or "ENCRYPT"
        [StringLength(2)]
        [Required]
        public string TransactionResult { get; set; } //"OK", or "KO"
        public string CryptDecryptString { get; set; } //encrypted string. May not be here in case o error
        [StringLength(9)]
        public string ErrorCode { get; set; } //error code
        [StringLength(255)]
        public string ErrorDescription { get; set; } //error description

        public EncryptDecryptTransactionResult(XmlNode xml) {
            XmlNode node;
            node = xml.SelectSingleNode("TransactionType");
            if (node != null) {
                TransactionType = node.InnerText;
            }
            node = xml.SelectSingleNode("TransactionResult");
            if (node != null) {
                TransactionResult = node.InnerText;
            }
            node = xml.SelectSingleNode("CryptDecryptString");
            if (node != null) {
                CryptDecryptString = node.InnerText;
            }
            node = xml.SelectSingleNode("ErrorCode");
            if (node != null) {
                ErrorCode = node.InnerText;
            }
            node = xml.SelectSingleNode("ErrorDescription");
            if (node != null) {
                ErrorDescription = node.InnerText;
            }
        }
    }
}