using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGestPay {

    //Things here are not enums, but conceptually we use them as if they were
    public static class CodeTables {
        //Lazy collections to store the tables from the GestPay documentation
        
        private static StreamReader GetReaderForResource(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            return new StreamReader(
                assembly.GetManifestResourceStream(resourceName)
            );
        }

        private static readonly string[] SplitSeparator = new string[] { "," };

        //Currency codes
        private static Lazy<List<CurrencyCode>> _currencyCodes = 
            new Lazy<List<CurrencyCode>>(() => ReadCurrencyCodesFromFile());
        private const string currencyCodesFileName = "Laser.Orchard.PaymentGestPay.CurrencyCodeTable.txt";
        private static List<CurrencyCode> ReadCurrencyCodesFromFile() {
            //Read the CurrencyCodeTable.txt file that stores the currency codes in csv format
            //Each line is like "N,C,D", where N is the numeric code for the currency, C is the 3 characters
            //string code, and D is the currency name.
            //Create all the currency codes and return them in a list
            List<CurrencyCode> allCodes = new List<CurrencyCode>();
            using (StreamReader reader = GetReaderForResource(currencyCodesFileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 3, StringSplitOptions.RemoveEmptyEntries);
                    int tmpCode = 0;
                    if (int.TryParse(parts[0], out tmpCode)) {
                        allCodes.Add(new CurrencyCode(tmpCode, parts[1], parts[2]));
                    }
                }
            }
            return allCodes;
        }
        public static List<CurrencyCode> CurrencyCodes {
            get { return _currencyCodes.Value; }
        }

        //Error codes
        private static Lazy<Dictionary<int, string>> _errorCodes = 
            new Lazy<Dictionary<int, string>>(() => ReadErrorCodesFromFile());
        private const string errorCodesFileName = "Laser.Orchard.PaymentGestPay.ErrorCodeTable.txt";
        private static Dictionary<int, string> ReadErrorCodesFromFile() {
            //Read the ErrorCodeTable.txt file that stores the error codes documented by GestPay
            //Each line is like "NN,XXXX", where NN is an int, XXXX is a string of unknown length,
            //that may contain further commas.
            //Fill a Dictionary<int, string> with the error codes from the file and return it
            Dictionary<int, string> allErrors = new Dictionary<int, string>();
            using (StreamReader reader = GetReaderForResource(errorCodesFileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
                    int tmpCode = 0;
                    if (int.TryParse(parts[0], out tmpCode)) {
                        allErrors.Add(tmpCode, parts[1]);
                    }
                }
            }
            return allErrors;
        }
        public static Dictionary<int, string> ErrorCodes {
            get { return _errorCodes.Value; }
        }

        //State codes (used for Italian provinces)
        private static Lazy<Dictionary<string, string>> _stateCodes = 
            new Lazy<Dictionary<string, string>>(() => ReadStateCodesFromFile());
        private const string stateCodesFileName = "Laser.Orchard.PaymentGestPay.StateCodeTable.txt";
        private static Dictionary<string, string> ReadStateCodesFromFile() {
            //Read the StateCodeTable.txt file that stores the provinces documented in the GestPay pdf
            //Each line is like "X,Y", where X is the code to use as key, Y is the value.
            //Fill a Dictionary<string, string> with the information and return it
            Dictionary<string, string> allCodes = new Dictionary<string, string>();
            using (StreamReader reader = GetReaderForResource(stateCodesFileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
                    allCodes.Add(parts[0], parts[1]);
                }
            }
            return allCodes;
        }
        public static Dictionary<string, string> StateCodes {
            get { return _stateCodes.Value; }
        }

        //PayPal country codes
        private static Lazy<Dictionary<string, string>> _paypalCountryCodes =
            new Lazy<Dictionary<string, string>>(() => ReadPayPalCountryCodesFromFile());
        private const string paypalCountryCodesFileName = "Laser.Orchard.PaymentGestPay.PayPalCountryCodeTable.txt";
        private static Dictionary<string, string> ReadPayPalCountryCodesFromFile() {
            //Read the PayPalCountryCodeTable.txt file storing country codes for PayPal
            //Each line is like "XX,YYY" where XX is the country code, 2 characters long, and
            //YYY is the country namespace
            //Fill a Dictionary<string, string> with the information and return it
            Dictionary<string, string> allCodes = new Dictionary<string, string>();
            using (StreamReader reader = GetReaderForResource(paypalCountryCodesFileName)) {
                string line;
                while((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
                    allCodes.Add(parts[0], parts[1]);
                }
            }
            return allCodes;
        }
        public static Dictionary<string, string> PaypalCountryCodes {
            get { return _paypalCountryCodes.Value; }
        }

        //ISO country codes
        private static Lazy<List<ISOCountry>> _isoCountryCodes = 
            new Lazy<List<ISOCountry>>(() => ReadIsoCountryCodesFromFile());
        private const string isoCountryCodesFileName = "Laser.Orchard.PaymentGestPay.ISOCountrycodeTable.txt";
        private static List<ISOCountry> ReadIsoCountryCodesFromFile() {
            //Read the ISOCountrycodeTable.txt file that stores the ISO country codes
            //Each line is like "AA,BBB,NNN,X" where AA is the alpha2 code BBB is the 
            //alpha3 code, NNN is the numeric country code
            //Fill a List<ISOCountry> with the information and return it
            List<ISOCountry> allCodes = new List<ISOCountry>();
            using (StreamReader reader = GetReaderForResource(isoCountryCodesFileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 4, StringSplitOptions.RemoveEmptyEntries);
                    int tmpCode = 0;
                    if (int.TryParse(parts[2], out tmpCode)) {
                        allCodes.Add(new ISOCountry(parts[0], parts[1], tmpCode, parts[3]));
                    }
                }
            }
            return allCodes;
        }
        public static List<ISOCountry> ISOCountryCodes {
            get { return _isoCountryCodes.Value; }
        }

        //Payment type codes
        private static Lazy<Dictionary<string, string>> _paymentTypeCodes =
            new Lazy<Dictionary<string, string>>(() => ReadPaymentTypesFromFile());
        private const string paymentTypeCodesFileName = "Laser.Orchard.PaymentGestPay.PaymentTypeCodeTable.txt";
        private static Dictionary<string, string> ReadPaymentTypesFromFile() {
            //Read the PaymentTypeCodeTable.txt file that stores the codes for the payment types
            //Each line is like "X,Y", where X is a string for the code, and Y is a string describing
            //the payment type.
            //Fill a Dictionary<string, string> with the information and return it
            Dictionary<string, string> allCodes = new Dictionary<string, string>();
            using (StreamReader reader = GetReaderForResource(paymentTypeCodesFileName)) {
                string line;
                while((line = reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
                    allCodes.Add(parts[0], parts[1]);
                }
            }
            return allCodes;
        }
        public static Dictionary<string, string> PaymentTypeCodes {
            get { return _paymentTypeCodes.Value; }
        }
    }
    ///<summary>
    ///This class is used to represent a single currency code
    ///</summary>
    public sealed class CurrencyCode {
        public int codeUIC { get; set; }
        [StringLength(3)]
        public string isoCode { get; set; }
        public string description { get; set; }

        public CurrencyCode (int uic, string iso, string desc) {
            codeUIC = uic;
            isoCode = iso;
            description = desc;
        }

        public SelectListItem GetSelectListItem(bool sel = false) {
            return new SelectListItem { Selected = sel, Text = description, Value = isoCode.ToString() };
        }
    }
    ///<summary>
    ///This class is used to represent a single country, with its ISO codes
    ///</summary>
    public sealed class ISOCountry {
        [StringLength(2)]
        public string Alpha2 { get; set; }
        [StringLength(3)]
        public string Alpha3 { get; set; }
        public int NumericalCode { get; set; }
        public string CountryName { get; set; }

        public enum ISOCountrySelectCriteria { ALPHA2, ALPHA3, NUMERICAL };

        public ISOCountry (string a2, string a3,int nc, string cn) {
            Alpha2 = a2;
            Alpha3 = a3;
            NumericalCode = nc;
            CountryName = cn;
        }

        public SelectListItem GetSelectListItem(ISOCountrySelectCriteria crit = ISOCountrySelectCriteria.ALPHA2, bool sel = false) {
            switch (crit) {
                case ISOCountrySelectCriteria.ALPHA2:
                    return new SelectListItem { Selected = sel, Text = CountryName, Value = Alpha2 };
                case ISOCountrySelectCriteria.ALPHA3:
                    return new SelectListItem { Selected = sel, Text = CountryName, Value = Alpha3 };
                case ISOCountrySelectCriteria.NUMERICAL:
                    return new SelectListItem { Selected = sel, Text = CountryName, Value = NumericalCode.ToString() };
                default:
                    return null;
            }
        }
    }
}