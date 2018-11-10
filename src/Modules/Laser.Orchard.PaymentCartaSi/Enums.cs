using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentCartaSi {
    //TODO: implement CodeTables class with lazy dictionaries like it was done for GestPay
    public enum LanguageIdCode { ITA, ENG, SPA, FRA, GER, JPG, CHI, ARA, RUS}
    public enum CardType { VISA, MasterCard, Amex, Diners, Jcb, Maestro, MYBANK, SCT, SDD, MYSI }

    public static class CodeTables {
        //lazy collections to store the tables from the Cartasì documentation

        private static StreamReader GetReaderForResource(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            return new StreamReader(
                assembly.GetManifestResourceStream(resourceName)
            );
        }

        private static readonly string[] SplitSeparator = new string[] { "," };

        private static Lazy<Dictionary<string, string>> _languageIds =
            new Lazy<Dictionary<string, string>>(() => ReadLanguageIdsFromFile());
        private const string languageIdsFileName = "Laser.Orchard.PaymentCartaSi.LanguageIdTable.txt";
        private static Dictionary<string, string> ReadLanguageIdsFromFile() {
            Dictionary<string, string> lIds = new Dictionary<string, string>();
            using (StreamReader reader = GetReaderForResource(languageIdsFileName)) {
                string line;
                while ((line=reader.ReadLine()) != null) {
                    string[] parts = line.Split(SplitSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
                    lIds.Add(parts[0], parts[1]);
                }
            }
            return lIds;
        }
        public static Dictionary<string, string> LanguageIds { get { return _languageIds.Value; } }

        private static Lazy<List<CurrencyCode>> _currencyCodes =
            new Lazy<List<CurrencyCode>>(() => ReadCurrencyCodesFromFile());
        private const string currencyCodesFileName = "Laser.Orchard.PaymentCartaSi.CurrencyCodeTable.txt";
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
        public static List<CurrencyCode> CurrencyCodes { get { return _currencyCodes.Value; } }

        //Error codes
        private static Lazy<Dictionary<int, string>> _errorCodes =
            new Lazy<Dictionary<int, string>>(() => ReadErrorCodesFromFile());
        private const string errorCodesFileName = "Laser.Orchard.PaymentCartaSi.ErrorCodeTable.txt";
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
        public static Dictionary<int, string> ErrorCodes { get { return _errorCodes.Value; } }

        private static Lazy<List<string>> _cardTypes =
            new Lazy<List<string>>(() => ReadCardTypesFromFile());
        private const string cardTypesFileName = "Laser.Orchard.PaymentCartaSi.CardTypesTable.txt";
        private static List<string> ReadCardTypesFromFile() {
            List<string> cTypes = new List<string>();
            using (StreamReader reader = GetReaderForResource(cardTypesFileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    cTypes.Add(line);
                }
            }
            return cTypes;
        }
        public static List<string> CardTypes { get { return _cardTypes.Value; } }
    }
    ///<summary>
    ///This class is used to represent a single currency code
    ///</summary>
    public sealed class CurrencyCode {
        public int codeUIC { get; set; }
        [StringLength(3)]
        public string isoCode { get; set; }
        public string description { get; set; }

        public CurrencyCode(int uic, string iso, string desc) {
            codeUIC = uic;
            isoCode = iso;
            description = desc;
        }

        public SelectListItem GetSelectListItem(bool sel = false) {
            return new SelectListItem { Selected = sel, Text = description, Value = isoCode.ToString() };
        }
    }
}