using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.BikeSharing.Models;
using Laser.Orchard.BikeSharing.Services;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.BikeSharing.ViewModels;

namespace Laser.Orchard.BikeSharing.Handlers {
    public class BikeStationPartHandler : ContentHandler {
        private readonly IBikeServices _bikeServices;

        public BikeStationPartHandler(
            IBikeServices bikeServices) {
            _bikeServices = bikeServices;
        }

        protected override void Loading(LoadContentContext context) {
            base.Loading(context);

            var bikeSharingPart = (BikeStationPart)context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "BikeStationPart");

            if (bikeSharingPart == null) {
                return;
            }
            //Loader(Func<T, T>) is obsolete as of Orchard 1.10
            //bikeSharingPart._bikeStation.Loader(x => {
            //    return _bikeServices.GetStationInfo(bikeSharingPart.BikeStationUName);
            //});
            
            bikeSharingPart._bikeStation.Loader(() => _bikeServices.GetStationInfo(bikeSharingPart.BikeStationUName));
        }

    }
}