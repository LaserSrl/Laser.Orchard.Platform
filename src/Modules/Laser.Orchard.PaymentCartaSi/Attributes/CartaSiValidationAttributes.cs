using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Attributes {
    //TODO: add validation attributes based on codetables
    /// <summary>
    /// Validates the amount for transactions
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidAmount : ValidationAttribute {

        internal bool ValidateParamAmount(string parameter) {
            if (Regex.IsMatch(parameter, "[^0-9]")) {
                return false;
            }
            return true;
        }

        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param)) {
                return true;
            }
            return ValidateParamAmount(param);
        }

        public override string FormatErrorMessage(string name) {
            return string.Format(@"Invalid format in amount {0}.", name);
        }
    }
    /// <summary>
    /// Validation used when no # character may be present
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NoOctothorpe : ValidationAttribute {
        //octothorpe is the official name of the # character
        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param) || !param.Contains("#")) {
                return true;
            }
            return false;
        }

        public override string FormatErrorMessage(string name) {
            return string.Format(@"Parameter {0} may not contain the # character", name);
        }
    }
    /// <summary>
    /// Validate an URL by checking that it starts with either http:// or https://, and that the used ports are the standard ones
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class IsValidUrl : ValidationAttribute {

        internal bool IsUrl(string parameter) {
            //starts with either http:// or https://
            if (Regex.IsMatch(parameter, @"(^https?://)")) {
                var m = Regex.Matches(parameter, @"(?<=:)(\d+)");
                if (m.Count == 0) {
                    //there is no port number
                    return true;
                } else if (m.Count == 1) {
                    //there is a port number
                    if (parameter.StartsWith(@"http://") && m[0].Value == "80") { //http may only have port 80
                        return true;
                    } else if (parameter.StartsWith(@"https://") && m[0].Value == "443") { //https may only have port 443
                        return true;
                    }
                }
                //more than one port number or port not valid
                return false;
            }
            return false;
        }

        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param)) {
                return true; //because the url may not be required
            }
            return true;
        }

        public override string FormatErrorMessage(string name) {
            return string.Format(@"Parameter {0} failed to validate as a correct URL.", name);
        }
    }
    /// <summary>
    /// Validates the optional parameters by checking that they do not clash with a list of reserved ones.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class IsValidParametersDictionary : ValidationAttribute {

        internal static string[] InvalidKeys = { "TRANSACTION_TYPE", "return-ok", "tid", "INFO_PAGE", "RECALL_PAGE", 
                                                  "back_url", "ERROR_URL", @"$EMAIL", @"$NOME", @"$COGNOME", "EMAIL" };

        public override bool IsValid(object value) {
            Dictionary<string, string> dic = (Dictionary<string, string>)value;
            if (dic == null || dic.Count == 0) {
                //empty dictionaries are fine
                return true;
            }
            string[] keys = dic.Keys.ToArray();
            if (keys.Intersect(InvalidKeys).Count() > 0) {
                //the dictionary contains invalid keys
                return false;
            }
            return true;
        }

        public override string FormatErrorMessage(string name) {
            return string.Format(@"Dictionary {0} had some invalid keys or values", name);
        }
    }
}