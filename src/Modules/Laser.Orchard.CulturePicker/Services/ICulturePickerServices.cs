using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Orchard;

namespace Laser.Orchard.CulturePicker.Services {
    public interface ICulturePickerServices : IDependency{
        void SaveCultureCookie(string cultureName, HttpContextBase context);

    }
}
