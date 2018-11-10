using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class APIResult<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T data { get; set; }

    }
}