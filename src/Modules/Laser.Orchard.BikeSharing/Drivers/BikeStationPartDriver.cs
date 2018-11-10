using System;
using System.Xml.Linq;
using Laser.Orchard.BikeSharing.Models;
using Laser.Orchard.BikeSharing.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;

namespace Laser.Orchard.BikeSharing.Drivers {
    public class BikeStationPartDriver : ContentPartCloningDriver<BikeStationPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IBikeServices _bikeServices;

        protected override string Prefix {
            get { return "Laser.BikeSharing"; }
        }
        public BikeStationPartDriver(IOrchardServices orchardServices, IBikeServices bikeServices) {
            _orchardServices = orchardServices;
            _bikeServices = bikeServices;
            Logger = NullLogger.Instance;
        }
        public ILogger Logger { get; set; }

        protected override DriverResult Display(BikeStationPart part, string displayType, dynamic shapeHelper) {

            if (displayType == "Summary")
                return ContentShape("Parts_BikeStation_Summary",
                    () => shapeHelper.Parts_BikeStation_Summary(BikeStation: part));
            if (displayType == "SummaryAdmin")
                return ContentShape("Parts_BikeStation_SummaryAdmin",
                    () => shapeHelper.Parts_BikeStation_Summary(BikeStation: part));

                return ContentShape("Parts_BikeStation",
                    () => shapeHelper.Parts_BikeStation(BikeStation: part));
        }

        //GET
        protected override DriverResult Editor(BikeStationPart part, dynamic shapeHelper) {
            return ContentShape("Parts_BikeStation_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/BikeStation",
                                    Model: part,
                                    Prefix: Prefix));
        }
        //POST
        protected override DriverResult Editor(BikeStationPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(BikeStationPart part, ImportContentContext context) {
            var importedBikeStationUName = context.Attribute(part.PartDefinition.Name, "BikeStationUName");
            if (importedBikeStationUName != null) {
                part.BikeStationUName = importedBikeStationUName;
            }
        }

        protected override void Exporting(BikeStationPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("BikeStationUName", part.BikeStationUName);
        }

        protected override void Cloning(BikeStationPart originalPart, BikeStationPart clonePart, CloneContentContext context) {
            clonePart.BikeStationUName = originalPart.BikeStationUName;
        }
    }
}