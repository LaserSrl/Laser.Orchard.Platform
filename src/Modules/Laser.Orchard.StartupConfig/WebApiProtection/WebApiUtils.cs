using System;
using System.Linq;

namespace Laser.Orchard.StartupConfig.WebApiProtection {
    public class WebApiUtils {
        private Random _rnd;
        public WebApiUtils() {
            _rnd = new Random(DateTime.Now.Millisecond);
        }
        public string RandomString(int length) {
            const string chars = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_rnd.Next(s.Length)]).ToArray());
        }
    }
}