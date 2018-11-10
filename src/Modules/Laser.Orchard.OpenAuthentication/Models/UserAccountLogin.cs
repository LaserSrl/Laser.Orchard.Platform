using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Models {
    
    public class UserAccountLogin  {
        public IUser IUserParz { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Sesso { get; set; }
        public string UserNameOrchard { get; set; }
    }
}