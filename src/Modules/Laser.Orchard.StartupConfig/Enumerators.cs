using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig {
    public enum InspectionType {
        Device, DeviceBrand
    }

    public enum DevicesBrands {
        Apple, Google, Windows, Blackberry, Unknown
    }

    public enum OrderType { 
        Default, PublishedUtc, Title
    }

    /// <summary>
    /// How should we alter the SameSite attribute of the cookies we are
    /// trying to set?
    /// <see cref="System.Web.SameSiteMode"/>
    /// <see cref="Laser.Orchard.StartupConfig.Filters.AllowCrossOriginFilter"/>
    /// </summary>
    public enum CookieSameSiteModeSetting {
        /// <summary>
        /// Don't alter the attribute for the cookie.
        /// </summary>
        DontAlter = -2,
        /// <summary>
        /// Set the attribute to -1, telling the server to not send
        /// the attribute along with the cookie and let the client use
        /// its own default interpretation of it.
        /// </summary>
        Unspecified = -1,
        /// <summary>
        /// Set the attribute to SameSiteMode.None
        /// </summary>
        None = 0,
        /// <summary>
        /// Set the attribute to SameSiteMode.Lax
        /// </summary>
        Lax = 1,
        /// <summary>
        /// Set the attribute to SameSiteMode.Strict
        /// </summary>
        Strict = 2
    }
}