using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Security;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface INwazetCommunicationService : IDependency {
        void OrderToContact(OrderPart order);
        List<AddressRecord> GetShippingByUser(IUser user);
        List<AddressRecord> GetBillingByUser(IUser user);
        string[] GetPhone(IUser user);

        // In the following CRUD methods, the ones that do not have a IUser parameter
        // are generally to be used for admin functionalities. The other methods are 
        // generally geared towards management of personal information.
        AddressRecord GetAddress(int id);
        AddressRecord GetAddress(int id, IUser user);

        void DeleteAddress(int id);
        void DeleteAddress(int id, IUser user);

        void AddAddress(AddressRecord newAddress, IUser user);
    }
}