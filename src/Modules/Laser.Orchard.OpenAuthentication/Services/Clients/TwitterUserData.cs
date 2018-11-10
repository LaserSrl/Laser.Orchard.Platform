using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    [DataContract]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TwitterUserData {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value> The name. </value>
        [DataMember(Name = "screen_name", IsRequired = true)]
        public string Screen_Name { get; set; }

        [DataMember(Name = "email")]
        public String Email { get; set; }
    }

}