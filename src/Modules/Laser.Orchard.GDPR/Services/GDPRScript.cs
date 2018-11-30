using System;
using System.Collections.Generic;
using Laser.Orchard.GDPR.Extensions;

namespace Laser.Orchard.GDPR.Services {
    public class GDPRScript : IGDPRScript {
        private IEnumerable<ICookieGDPR> _cookies;
        public GDPRScript(IEnumerable<ICookieGDPR> cookies) {
            _cookies = cookies;
        }
        /// <summary>
        /// Get a script to create GDPR choice banner for the user.
        /// </summary>
        /// <returns></returns>
        public string GetBannerChoice() {
            // cicla sui tipi di cookie e crea il banner in modo da visualizzare solo le checkbox necessarie
            foreach(var cookie in _cookies) {

            }
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get choices of the user about cookies according to GDPR.
        /// </summary>
        /// <returns></returns>
        public IList<CookieType> GetCurrentGDPR() {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Set choices of the user about cookies according to GDPR.
        /// </summary>
        /// <param name="cookieTypes"></param>
        public void SetCurrentGDPR(IList<CookieType> cookieTypes) {
            throw new NotImplementedException();
        }
    }
}