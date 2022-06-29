using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Security;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPartDriver
        : ContentPartCloningDriver<PickupPointPart> {
        private readonly IAuthorizer _authorizer;
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly IContentManager _contentManager;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public PickupPointPartDriver(
            IAuthorizer authorizer,
            IAddressConfigurationService addressConfigurationService,
            IContentManager contentManager,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _authorizer = authorizer;
            _addressConfigurationService = addressConfigurationService;
            _contentManager = contentManager;
            _territoriesRepositoryService = territoriesRepositoryService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "PickupPointPart"; }
        }

        protected override DriverResult Display(PickupPointPart part, string displayType, dynamic shapeHelper) {

            return ContentShape("Parts_PickupPoint_Summary",
                () => shapeHelper.Parts_PickupPoint_Summary());
        }


        protected override DriverResult Editor(PickupPointPart part, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            var shapes = new List<DriverResult>();
            // add a shape with a "return to list" button
            shapes.Add(ContentShape("Parts_PickupPointPart_BackToList", () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/PickupPoint.BackToList",
                    Prefix: Prefix);
            }));
            // add the actuall editor shape
            shapes.Add(ContentShape("Parts_PickupPointPart_Edit", () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/PickupPoint",
                    Model: CreateVM(part),
                    Prefix: Prefix);
            }));

            return Combined(shapes.ToArray());
        }

        protected override DriverResult Editor(PickupPointPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            var updatedModel = new PickupPointPartEditViewModel();

            var updateIsValid = updater.TryUpdateModel(updatedModel, Prefix, null, null);
            updateIsValid &= ValidateVM(updatedModel.AddressVM);
            if (updateIsValid) {
                part.AddressLine1 = updatedModel.AddressVM.Address1;
                part.AddressLine2 = updatedModel.AddressVM.Address2;
                part.CountryId = updatedModel.AddressVM.CountryId;
                part.CountryName = updatedModel.AddressVM.Country;
                part.ProvinceId = updatedModel.AddressVM.ProvinceId;
                part.ProvinceName = updatedModel.AddressVM.Province;
                part.CityId = updatedModel.AddressVM.CityId;
                part.CityName = updatedModel.AddressVM.City;
                part.PostalCode = updatedModel.AddressVM.PostalCode;
            }

            return Editor(part, shapeHelper);
        }
        
        protected override void Exporting(PickupPointPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            // Most properties can be exported as they are.
            element.SetAttributeValue("CountryName", part.CountryName);
            element.SetAttributeValue("ProvinceName", part.ProvinceName);
            element.SetAttributeValue("CityName", part.CityName);
            element.SetAttributeValue("AddressLine1", part.AddressLine1);
            element.SetAttributeValue("AddressLine2", part.AddressLine2);
            element.SetAttributeValue("PostalCode", part.PostalCode);
            // For territories, we should be exporting identities. Note however that the
            // Ids we are saving here are for the TerritoryInternalRecords, so we'll export
            // their Name.
            element.SetAttributeValue("CountryId-InternalName", GetInternalTerritoryName(part.CountryId));
            element.SetAttributeValue("ProvinceId-InternalName", GetInternalTerritoryName(part.ProvinceId));
            element.SetAttributeValue("CityId-InternalName", GetInternalTerritoryName(part.CityId));
        }

        private string GetInternalTerritoryName(int id) {
            var record = _territoriesRepositoryService.GetTerritoryInternal(id);
            if (record == null) {
                return string.Empty;
            }
            return record.Name;
        }

        protected override void Importing(PickupPointPart part, ImportContentContext context) {
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }
            // Some properties are simply strings
            context.ImportAttribute(part.PartDefinition.Name, "CountryName", s => part.CountryName = s);
            context.ImportAttribute(part.PartDefinition.Name, "ProvinceName", s => part.ProvinceName = s);
            context.ImportAttribute(part.PartDefinition.Name, "CityName", s => part.CityName = s);
            context.ImportAttribute(part.PartDefinition.Name, "AddressLine1", s => part.AddressLine1 = s);
            context.ImportAttribute(part.PartDefinition.Name, "AddressLine2", s => part.AddressLine2 = s);
            context.ImportAttribute(part.PartDefinition.Name, "PostalCode", s => part.PostalCode = s);
            // The identities of territories may be more complex
            part.CountryId = AssignTerritoryId(context, 
                context.Attribute(part.PartDefinition.Name, "CountryId-InternalName"));
            part.ProvinceId = AssignTerritoryId(context,
                context.Attribute(part.PartDefinition.Name, "ProvinceId-InternalName"));
            part.CityId = AssignTerritoryId(context,
                context.Attribute(part.PartDefinition.Name, "CityId-InternalName"));

        }

        private int AssignTerritoryId(ImportContentContext context, string territoryIdName) {
            if (string.IsNullOrWhiteSpace(territoryIdName)) {
                return 0;
            } else {
                var country = _territoriesRepositoryService.GetTerritoryInternal(territoryIdName);
                if (country != null) {
                    return country.Id;
                } else {
                    return 0;
                }
            }
        }

        protected override void Cloning(PickupPointPart originalPart, PickupPointPart clonePart, CloneContentContext context) {
            // Cloning's easier because we don't have to worry too much about identities
            clonePart.CountryName = originalPart.CountryName;
            clonePart.CountryId = originalPart.CountryId;
            clonePart.ProvinceName = originalPart.ProvinceName;
            clonePart.ProvinceId = originalPart.ProvinceId;
            clonePart.CityName = originalPart.CityName;
            clonePart.CityId = originalPart.CityId;
            clonePart.AddressLine1 = originalPart.AddressLine1;
            clonePart.AddressLine2 = originalPart.AddressLine2;
            clonePart.PostalCode = originalPart.PostalCode;
        }

        private bool ValidateVM(PickupPointAddressEditViewModel vm) {
            int id = -1;
            if (vm.CityId > 0) {
                if (int.TryParse(vm.City, out id)) {
                    // the form sent the city's id instead of its name
                    vm.City = _addressConfigurationService
                        .GetCity(vm.CityId)
                        ?.As<TitlePart>()
                        ?.Title
                        ?? string.Empty;
                }
            }
            if (vm.ProvinceId > 0) {
                if (int.TryParse(vm.Province, out id)) {
                    // the form sent the city's id instead of its name
                    vm.Province = _addressConfigurationService
                        .GetProvince(vm.ProvinceId)
                        ?.As<TitlePart>()
                        ?.Title
                        ?? string.Empty;
                }
            }
            if (vm.CountryId > 0) {
                if (int.TryParse(vm.Country, out id)) {
                    // the form sent the city's id instead of its name
                    vm.Country = _addressConfigurationService
                        .GetCountry(vm.CountryId)
                        ?.As<TitlePart>()
                        ?.Title
                        ?? string.Empty;
                }
            }
            return true;
        }

        private bool Authorized(PickupPointPart part) {
            return _authorizer.Authorize(
                PickupPointPermissions.MayConfigurePickupPoints, 
                part, 
                T("Cannot manage pickup points"));
        }

        private PickupPointPartEditViewModel CreateVM(PickupPointPart part) {

            return new PickupPointPartEditViewModel {
                PickupPointPart = part,
                AddressVM = CreateAddressVM(part)
            };
        }

        private PickupPointAddressEditViewModel CreateAddressVM(PickupPointPart part) {
            
            return new PickupPointAddressEditViewModel(part) {
                Countries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.ShippingAddress, part.CountryId)
            };
        }
    }
}