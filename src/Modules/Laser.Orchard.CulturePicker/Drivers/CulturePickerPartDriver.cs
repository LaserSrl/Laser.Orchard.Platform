
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Localization.Services;
using Laser.Orchard.CulturePicker.Services;
using Orchard.Environment.Configuration;
using System;
using System.Web;
using Orchard;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;

namespace Laser.Orchard.CulturePicker.Drivers {
    
    public class CulturePickerPartDriver : ContentPartDriver<CulturePickerPart> {
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Services.ICulturePickerSettingsService _extendedCultureService;
        private readonly Services.ILocalizableContentService _localizableContentService;

        public CulturePickerPartDriver(ICultureManager cultureManager, IWorkContextAccessor workContextAccessor, Services.ICulturePickerSettingsService extendedCultureService,ILocalizableContentService localizableContentService)
        {
            _cultureManager = cultureManager;
            _workContextAccessor = workContextAccessor;
            _extendedCultureService = extendedCultureService;
            _localizableContentService = localizableContentService;
        }

        protected override DriverResult Display(CulturePickerPart part, string displayType, dynamic shapeHelper) {
            var siteAvailableCultures = _cultureManager.ListCultures().AsQueryable();
            var context = _workContextAccessor.GetContext();
            var baseUrl = context.CurrentSite.BaseUrl;
            baseUrl = baseUrl.Replace("http://", "").Replace("https://", "");
            var cleanUrl = context.HttpContext.Request.Url.AbsoluteUri.Replace(baseUrl, "");
            cleanUrl = cleanUrl.Replace("http://", "").Replace("https://", "");
            cleanUrl = context.HttpContext.Server.UrlDecode(cleanUrl);
            cleanUrl = cleanUrl.StartsWith("/") ? cleanUrl.Substring(1) : cleanUrl;
            // reading settings
            var settings = _extendedCultureService.ReadSettings();
            part.AvailableCultures = settings.ExtendedCulturesList;
            part.ShowOnlyPertinentCultures = settings.Settings.ShowOnlyPertinentCultures;
            part.ShowLabel = settings.Settings.ShowLabel;
            settings = null;
            var urlPrefix = _workContextAccessor.GetContext().Resolve<ShellSettings>().RequestUrlPrefix;
            if (!String.IsNullOrWhiteSpace(urlPrefix)) {
                cleanUrl = cleanUrl.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase) ? cleanUrl.Substring(urlPrefix.Length) : cleanUrl;
            }
            cleanUrl = HttpUtility.UrlDecode(cleanUrl);
            cleanUrl = cleanUrl.StartsWith("/") ? cleanUrl.Substring(1) : cleanUrl;
            var isHomePage = String.IsNullOrWhiteSpace(cleanUrl);
            part.TranslatedCultures = _localizableContentService.AvailableTranslations(cleanUrl, isHomePage);


            part.UserCulture = _extendedCultureService.GetExtendedCulture(_cultureManager.GetCurrentCulture(_workContextAccessor.GetContext().HttpContext));

            return ContentShape("Parts_CulturePicker", () => shapeHelper.Parts_CulturePicker(AvailableCultures: part.AvailableCultures, TranslatedCultures: part.TranslatedCultures, UserCulture: part.UserCulture, ShowOnlyPertinentCultures: part.ShowOnlyPertinentCultures, ShowLabel: part.ShowLabel));
        }


    //    //mod 30-11-2016
    //    protected override void Exporting(CulturePickerPart part, ExportContentContext context) {

    //       var root = context.Element(part.PartDefinition.Name);
    //       if (part.AvailableCultures != null) 
    //        {
                                
    //            foreach (ExtendedCultureRecord recAvCulture in part.AvailableCultures) 
    //            {
    //                XElement avCult = new XElement("AvailableCultures");
    //                avCult.SetAttributeValue("CultureCode", recAvCulture.CultureCode);
    //                avCult.SetAttributeValue("DisplayName", recAvCulture.DisplayName);
    //                avCult.SetAttributeValue("Priority", recAvCulture.Priority);
    //                root.Add(avCult);
    //            }
    //        }

    //        if (part.TranslatedCultures != null) 
    //        {
                
    //            foreach (ExtendedCultureRecord recTranslCulture in part.TranslatedCultures) 
    //            {
    //                XElement transCult = new XElement("TranslatedCultures");
    //                transCult.SetAttributeValue("CultureCode", recTranslCulture.CultureCode);
    //                transCult.SetAttributeValue("DisplayName", recTranslCulture.DisplayName);
    //                transCult.SetAttributeValue("Priority", recTranslCulture.Priority);
    //                root.Add(transCult);
    //            }
    //        }

                                     
    //        if (part.UserCulture !=null)
    //        {
    //            ExtendedCultureRecord userCulture= part.UserCulture;
    //            context.Element(part.PartDefinition.Name).SetAttributeValue("UserCulture", part.UserCulture);
                
    //            var userCult = context.Element(part.PartDefinition.Name).Element("UserCulture");
                
    //            userCult.SetAttributeValue("CultureCode", userCulture.CultureCode);
    //            userCult.SetAttributeValue("DisplayName", userCulture.DisplayName);
    //            userCult.SetAttributeValue("Priority", userCulture.Priority);
    //            root.Add(userCult);
    //        }

    //   }




    //    protected override void Importing(CulturePickerPart part, ImportContentContext context) 
    //    {
    //        var root = context.Data.Element(part.PartDefinition.Name);
    //        var importedAvailableCultures = context.Attribute("AvailableCultures", "AvailableCultures");
           
    //        if (importedAvailableCultures != null) 
    //        {
    //            foreach (ExtendedCultureRecord rec in part.AvailableCultures) 
    //            {
    //                rec.CultureCode =root.Attribute("AvailableCultures").Parent.Element("CultureCode").Value;
    //                rec.DisplayName = root.Attribute("AvailableCultures").Parent.Element("DisplayName").Value;
    //                rec.Priority = int.Parse(root.Attribute("AvailableCultures").Parent.Element("Priority").Value);
    //                part.AvailableCultures.Add(rec);
    //            }
    //        }

    //        var importedTranslatedCultures = context.Attribute("TranslatedCultures", "TranslatedCultures");
    //        if (importedTranslatedCultures != null) {
    //            foreach (ExtendedCultureRecord rec in part.TranslatedCultures) {
                  
    //                rec.CultureCode = root.Attribute("TranslatedCultures").Parent.Element("CultureCode").Value;
    //                rec.DisplayName = root.Attribute("TranslatedCultures").Parent.Element("DisplayName").Value;
    //                rec.Priority = int.Parse(root.Attribute("TranslatedCultures").Parent.Element("Priority").Value);
    //                part.TranslatedCultures.Add(rec);
    //            }
    //        }

    //        var importedShowOnlyPertinentCultures = context.Attribute("ShowOnlyPertinentCultures", "ShowOnlyPertinentCultures");
    //        if (importedShowOnlyPertinentCultures != null)
    //        {
    //            part.ShowOnlyPertinentCultures = bool.Parse(root.Attribute("ShowOnlyPertinentCultures").Value);                
    //        }

    //        var importedShowLabel = context.Attribute("ShowLabel", "ShowLabel");
    //        if (importedShowLabel != null) {
    //            part.ShowLabel = bool.Parse(root.Attribute("ShowLabel").Value);
    //        }
     
    //        var importedUserCulture = context.Attribute("UserCulture", "UserCulture");            
    //        if (importedUserCulture != null) 
    //        {
    //                part.UserCulture.CultureCode = root.Attribute("UserCulture").Parent.Element("CultureCode").Value;
    //                part.UserCulture.DisplayName = root.Attribute("UserCulture").Parent.Element("DisplayName").Value;
    //                part.UserCulture.Priority = int.Parse(root.Attribute("UserCulture").Parent.Element("Priority").Value);  
    //            }
    //        }

    }
}