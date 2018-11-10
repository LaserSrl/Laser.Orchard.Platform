using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Extensions {
    public static class Constants {
        public const string PosName = "GestPay";
        public const string LocalArea = "Laser.Orchard.PaymentGestPay";
    }

    public static class Endpoints {
        //we call these API points to encrypt/decrypt stuff
        public const string TestWSEntry = @"https://testecomm.sella.it/{0}";
        public const string ProdWSEntry = @"https://ecommS2S.sella.it/{0}";
        public const string CryptDecryptEndPoint = @"gestpay/GestPayWS/WsCryptDecrypt.asmx?{0}"; //verbatim string to be used in string.Format()
        public const string CryptDecryptWSDL = "WSDL"; //
        public const string CryptDecryptDecrypt = "op=Decrypt";
        public const string CryptDecryptEncrypt = "op=Encrypt";
        //we redirect the client to these pages
        public const string TestPayEntry = @"https://testecomm.sella.it/{0}";
        public const string ProdPayEntry = @"https://ecomm.sella.it/{0}";
        public const string PaymentPage = @"pagam/pagam.aspx?a={0}&b={1}"; //verbatim string to be used in string.Format()

        ///
        ///Example use of the strings, to get the url for the wsdl representation of an endpoint:
        /// string urlToCall = string.Format(
        ///     string.Format(TestWSEntry, CryptDecryptEndPoint),
        ///     CryptDecryptWSDL
        /// );
        ///
    }
}