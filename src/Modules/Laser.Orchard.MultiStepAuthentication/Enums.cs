using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication {
    /// <summary>
    /// Enumerates the possible types of channels used to deliver information (e.g. a nonce) to a user
    /// </summary>
    public enum DeliveryChannelType { Email, Sms, Push }
    
    /// <summary>
    /// Types of password for OTPRecord
    /// </summary>
    public enum PasswordType { Password, Nonce }

    /// <summary>
    /// Types of ux to use  
    /// </summary>
    public enum FlowType { App , Website}

    public enum PriorityDelivery {Defaultmail=0, TempleteEmail = 1 }
}