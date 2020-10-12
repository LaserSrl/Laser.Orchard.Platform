using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.Localization;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IValidationProvider : IDependency {
        /// <summary>
        /// add the validations that are needed based on the module in which you are located
        /// e.g. for emotion add email and phone number that are not needed for ecommerce
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        bool Validate(AddressEditViewModel vm);

        List<LocalizedString> Validate(AddressesVM vm);
    }
}