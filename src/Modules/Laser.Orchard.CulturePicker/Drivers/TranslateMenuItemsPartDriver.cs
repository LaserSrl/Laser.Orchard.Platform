using Laser.Orchard.CulturePicker.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;

namespace Laser.Orchard.CulturePicker.Drivers {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemsPartDriver : ContentPartCloningDriver<TranslateMenuItemsPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ILocalizationService _localizationService;
        private readonly ISessionLocator _sessionLocator;
        private readonly Services.ITranslateMenuItemsServices _translateMenuItemsService;

        public TranslateMenuItemsPartDriver(IOrchardServices orchardServices,
            ILocalizationService localizationService,
            ISessionLocator sessionLocator,
            Services.ITranslateMenuItemsServices translateMenuItemsService) {
             
            _orchardServices = orchardServices;
            _translateMenuItemsService = translateMenuItemsService;
            _localizationService = localizationService;
            _sessionLocator = sessionLocator;
        }

        //protected override DriverResult Display(TranslateMenuItemsPart part, string displayType, dynamic shapeHelper) {
        //    //TODO
        //    //Check permissions, because if the current user does not have ReplayMenuTranslation, then the checkbox
        //    //for Translated should be disabled.
        //    string shapeName = "Parts_TranslateMenuItem";
        //    bool authorizedTranslated = _orchardServices.Authorizer.Authorize(TranslateMenuItemsPermissions.ReplayMenuTranslation);
        //    return ContentShape(shapeName,
        //        () => shapeHelper.Parts_TranslateMenuItem(
        //            fromLocale: part.FromLocale,
        //            cbToBeTranslatedValue: part.ToBeTranslated,
        //            cbTranslatedValue: part.Translated,
        //            cbTranslatedEnabled: authorizedTranslated
        //            )
        //        );
        //}

        //GET
        protected override DriverResult Editor(TranslateMenuItemsPart part, dynamic shapeHelper) {
            //TODO
            string shapeName = "Parts_TranslateMenuItem";
            string templateName = "Parts/TranslateMenuItem";
            return ContentShape(shapeName,
                () => shapeHelper.EditorTemplate(TemplateName: templateName,
                    Model: part
                    )
                );
        }

        //POST
        protected override DriverResult Editor(TranslateMenuItemsPart part, IUpdateModel updater, dynamic shapeHelper) {
            //TODO
            //call the TryUpdateModel to update stuff from the form into the part
            bool ciao = updater.TryUpdateModel(part, "", null, null);

            //Take the menu for translation. Note that part.ContentItem is the target menu. The one we translate
            //from is its master.
            var menu = part.ContentItem;


            //Use the localization part to compute the FromLocale member of the part
            //var locPart = menu.As<LocalizationPart>();
            //apparently,here the localization part from the menu has not yet been updated.
            //we take the target culture information directly from the form
            string selectedCulture = _orchardServices.WorkContext.HttpContext.Request.Form.Get("SelectedCulture");            

            string shapeName = "Parts_TranslateMenuItem";
            string templateName = "Parts/TranslateMenuItem";
            return ContentShape(shapeName, 
                () => shapeHelper.EditorTemplate(TemplateName: templateName,
                    Model: part
                    )
                );
        }

        protected override void Cloning(TranslateMenuItemsPart originalPart, TranslateMenuItemsPart clonePart, CloneContentContext context) {
            //actualy clone nothing
        }


        //protected override void Exporting(TranslateMenuItemsPart part, ExportContentContext context) {
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("ToBeTranslated", part.ToBeTranslated);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("Translated", part.Translated);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("FromLocale", part.FromLocale);

        //}


        //protected override void Importing(TranslateMenuItemsPart part, ImportContentContext context) {
        //    var importedToBeTranslated = context.Attribute(part.PartDefinition.Name, "ToBeTranslated");
        //    if (importedToBeTranslated != null) {
        //        part.ToBeTranslated = bool.Parse(importedToBeTranslated);
        //    }

        //    var importedTranslated = context.Attribute(part.PartDefinition.Name, "Translated");
        //    if (importedTranslated != null) {
        //        part.Translated = bool.Parse(importedTranslated);
        //    }

        //    var importedFromLocale = context.Attribute(part.PartDefinition.Name, "FromLocale");
        //    if (importedFromLocale != null) {
        //        part.FromLocale =importedFromLocale;
        //    }

        //}
    }
}