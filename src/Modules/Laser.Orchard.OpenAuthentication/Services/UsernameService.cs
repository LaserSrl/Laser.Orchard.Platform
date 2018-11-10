using Laser.Orchard.OpenAuthentication.Extensions;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.Users.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IUsernameService : IDependency {
        string Calculate(string currentValue);
        /// <summary>
        /// Normalizza lo username in modo che sia composto solo da lettere, numeri e i caratteri ".-_".
        /// </summary>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        string Normalize(string currentValue);
    }

    public class UsernameService : IUsernameService {
        private readonly ISessionLocator _session;
        private readonly IOrchardServices _orchardServices;

        public UsernameService(ISessionLocator session, IOrchardServices orchardServices) {
            _session = session;
            _orchardServices = orchardServices;
        }

        public string Calculate(string currentValue) {
            /* I Dont want to user an email address as a Username...*/
            string userName = currentValue.IsEmailAddress() 
                ? currentValue.Substring(0, currentValue.IndexOf('@')) 
                : currentValue; //.Replace(" ", "");
            int uniqueValue = 0;
            string newUniqueUserName = userName;
            // this is only as unique as the same process in generating an alias/autoroute:
            // there is no guarantee that two calls happening at the same time will generate
            // different usernames, given the same starting string. Actually, in that condition
            // they will be likely to generate the same username.
            while (true) {
                var numExistingUsers = _orchardServices
                    .ContentManager.HqlQuery()
                    .ForPart<UserPart>()
                    .Where(a => a.ContentPartRecord<UserPartRecord>(), x => x.Eq("UserName", newUniqueUserName))
                    .Count();
                if (numExistingUsers == 0) {
                    break;
                }
                newUniqueUserName = string.Format("{0}{1}", userName, uniqueValue);
                uniqueValue++;
            }

            return newUniqueUserName;
        }

        public string Normalize(string currentValue) {
            string username = currentValue.IsEmailAddress() ? currentValue.Substring(0, currentValue.IndexOf('@')) : currentValue;

            if (Regex.IsMatch(username, "^[-_.a-zA-Z0-9]+$") == false) {
                StringBuilder sb = new StringBuilder();
                foreach (var aChar in currentValue) {
                    if(Regex.IsMatch(aChar.ToString(), "[-_.a-zA-Z0-9]")) {
                        sb.Append(aChar);
                    }
                }
                if (sb.Length == 0) {
                    username = "user";
                }
                else {
                    username = sb.ToString();
                }
            }
            return username;
        }
    }
}