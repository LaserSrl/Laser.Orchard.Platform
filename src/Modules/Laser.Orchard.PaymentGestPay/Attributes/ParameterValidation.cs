using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Attributes {
    /// <summary>
    /// Verifies that the parameter string contains no invalid characters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidGestPayParameter : ValidationAttribute {

        /// <summary>
        /// Verifies that the parameter string contains no invalid characters.
        /// </summary>
        /// <param name="parameter"></param>
        internal bool ValidateGestPayParameter(string parameter) {
            if (Regex.IsMatch(parameter, "[& §()*<>,;:\\[ \\]/%?=]") || parameter.Contains(@"*P1*")) {
                return false;
            }
            return true;
        }

        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param))
                return true;
            return ValidateGestPayParameter(param);
        }

        public override string FormatErrorMessage(string name) {
            return string.Format("Invalid character in parameter {0}.", name);
        }
    }
    /// <summary>
    /// Validates parameters in an array 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidGestPayArrayParameter : ValidGestPayParameter {

        public override bool IsValid(object value) {
            string[] paramArray = (string[])value;
            if (paramArray == null || paramArray.Length == 0)
                return true;
            foreach (string param in paramArray) {
                if (!ValidateGestPayParameter(param)) {
                    return false;
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Validates parameters in a list
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidGestPayListParameter : ValidGestPayParameter {

        public override bool IsValid(object value) {
            List<string> paramArray = (List<string>)value;
            if (paramArray == null || paramArray.Count == 0)
                return true;
            foreach (string param in paramArray) {
                if (!ValidateGestPayParameter(param)) {
                    return false;
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Verifies that the parameter string contains no invalid characters. Red has additional requirements.
    /// </summary
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidRedGestPayParameter : ValidGestPayParameter {

        /// <summary>
        /// Verifies that the parameter string contains no invalid characters.
        /// </summary>
        /// <param name="parameter"></param>
        internal bool ValidateRedParameter(string parameter) {
            if (Regex.IsMatch(parameter, "[$!^~#]")) {
                return false;
            }
            return ValidateGestPayParameter(parameter);
        }

        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param))
                return true;
            return ValidateRedParameter(param);
        }

        public override string FormatErrorMessage(string name) {
            return string.Format("Invalid character in parameter {0}.", name);
        }
    }
    /// <summary>
    /// Validates parameters in an array 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidRedGestPayArrayParameter : ValidRedGestPayParameter {

        public override bool IsValid(object value) {
            string[] paramArray = (string[])value;
            if (paramArray == null || paramArray.Length == 0)
                return true;
            foreach (string param in paramArray) {
                if (!ValidateRedParameter(param)) {
                    return false;
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Validates parameters in a list
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidRedGestPayListParameter : ValidRedGestPayParameter {

        public override bool IsValid(object value) {
            List<string> paramArray = (List<string>)value;
            if (paramArray == null || paramArray.Count == 0)
                return true;
            foreach (string param in paramArray) {
                if (!ValidateRedParameter(param)) {
                    return false;
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Verifies that the parameter string contains no invalid characters. Emailshave additional requirements.
    /// </summary
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidRedGestPayEmailParameter : ValidRedGestPayParameter {
        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param))
                return true;
            if (!ValidateRedParameter(param))
                return false;
            if (!param.Contains("@"))
                return false;
            return true;
        }

        public override string FormatErrorMessage(string name) {
            return string.Format("Parameter {0} must contain @ character.", name);
        }
    }
    /// <summary>
    /// Verifies that the parameter string contains no invalid characters. These may not contain whitespace
    /// </summary
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidRedGestPayNoSpaceParameter : ValidRedGestPayParameter {
        public override bool IsValid(object value) {
            string param = (string)value;
            if (string.IsNullOrWhiteSpace(param))
                return true;
            if (!ValidateRedParameter(param))
                return false;
            string tmp = Regex.Replace(param, "[\n\r\t/s]", " ");
            if (tmp.Contains(" "))
                return false;
            return true;
        }

        public override string FormatErrorMessage(string name) {
            return string.Format("Parameter {0} must not contain whitespace.", name);
        }
    }
}