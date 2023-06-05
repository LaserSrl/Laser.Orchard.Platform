
using Laser.Orchard.CulturePicker.Models;
using Laser.Orchard.CulturePicker.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Configuration;
using Orchard.Localization.Services;
using System;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CulturePicker.Drivers {

    public class CulturePickerPartDriver : ContentPartDriver<CulturePickerPart> {
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICulturePickerSettingsService _extendedCultureService;
        private readonly ILocalizableContentService _localizableContentService;

        public CulturePickerPartDriver(
            ICultureManager cultureManager,
            IWorkContextAccessor workContextAccessor,
            ICulturePickerSettingsService extendedCultureService,
            ILocalizableContentService localizableContentService) {

            _cultureManager = cultureManager;
            _workContextAccessor = workContextAccessor;
            _extendedCultureService = extendedCultureService;
            _localizableContentService = localizableContentService;
        }

        protected override DriverResult Display(CulturePickerPart part, string displayType, dynamic shapeHelper) {
            var siteAvailableCultures = _cultureManager.ListCultures().AsQueryable();
            var context = _workContextAccessor.GetContext();

            // Here we want to compute a segment of the URL of the Request such that it is
            // basically hte same as the alias of a content. For example:
            // URL: https://www.mysite.com/lorem/ipsum => lorem/ispum
            // URL: http://www.mysite.com/ => (empty string)
            // URL: https://www.mysite.com/Users/Account/AccessDenied => Users/Account/AccessDenied
            // On principle, the resulting segment would be the slug of a content, or the
            // path of an action.
            // In the simplest case, the URL is obtained as {BaseUrl} + {slug}, eventually
            // with interposing '/' characters and the tenant's prefix (for simplicity we'll
            // consider the prefix part of the slug in this discussion).
            // However, in may cases that isn't true. Often, the BaseUrl will be set to something like
            // "https://www.mysite.com", but the website will naturally also have to respond to requests
            // to https://mysite.com.
            // To manage such cases we can use URI objects. Here are some examples.
            /*
             * var mySiteUri = new Uri("https://www.mysite.com");
             * mySiteUri.Segments = string[1] { "/" }
             * var myReq1 = new Uri("https://www.mysite.com");
             * var myReq2 = new Uri("https://www.mysite.com/");
             * var myReq3 = new Uri("https://www.mysite.com/asd");
             * mySiteUri.MakeRelative(myReq1) = ""
             * mySiteUri.MakeRelative(myReq2) = ""
             * mySiteUri.IsBaseOf(myReq3) = true
             * mySiteUri.MakeRelative(myReq3) = "asd"
             * myReq3.Segments string[2] { "/", "asd" }
             * var myReq4 = new Uri("https://mysite.com/asd");
             * string[2] { "/", "asd" }
             * mySiteUri.IsBaseOf(myReq4) = false
             * var myLocalUri = new Uri("https://localhost/IisApp/");
             * myLocalUri.Segments = string[2] { "/", "IisApp/" }
             * var localReq = new Uri("https://localhost/iisapp/prefix/lorem/ipsum");
             * localReq.Segments = string[5] { "/", "iisapp/", "prefix/", "lorem/", "ipsum" }
             * myLocalUri.IsBaseOf(localReq) = false
             * */
            // The Segments array contains all the segments of the URI that follow the Domain. That would
            // also include the application and the tenant's prefix, as is often the case locally. 
            // NOTE that, critically, the comparisons in the URI class are case sensitive.
            var cleanUrl = (string)null;
            var isHomePage = false;

            var baseUrl = context.CurrentSite.BaseUrl;
            if (!baseUrl.EndsWith("/")) {
                baseUrl = baseUrl + "/";
            }
            var baseUri = new Uri(baseUrl);
            var requestUri = context.HttpContext.Request.Url;
            if (baseUri.IsBaseOf(requestUri)) {
                // simple case: configuration matches the request
                cleanUrl = baseUri.MakeRelativeUri(requestUri).ToString();
            } else {
                // complex case.
                // For some reason the configuration doesn't exactly match the request. For example,
                // the baseUrl has www and the request doesn't (or viceversa), or there's different
                // cpaitalizations.
                // From requestUri.Segments we remove baseUri.Segments. The remaining strings we'll
                // use to build the clean url.
                // Sanity check: all elements from baseUri.Segments must be in requestUri.Segments, 
                // in the same order:
                var sanityCheck = baseUri.Segments.Length <= requestUri.Segments.Length;
                if (sanityCheck) {
                    for (int i = 0; i < baseUri.Segments.Length && sanityCheck; i++) {
                        // segments must be the same EXCEPT that their comparison should be agnostic to
                        // an ending '/'.
                        var baseSegment = baseUri.Segments[i].TrimEnd('/');
                        var requestSegment = requestUri.Segments[i].TrimEnd('/');
                        sanityCheck &= string.Equals(baseSegment, requestSegment, StringComparison.OrdinalIgnoreCase);
                    }
                }
                if (sanityCheck) {
                    cleanUrl = string.Join("", requestUri.Segments.Skip(baseUri.Segments.Length));
                }
            }
            if (cleanUrl != null) {
                // we assigned a value to cleanurl
                cleanUrl = context.HttpContext.Server.UrlDecode(cleanUrl)
                    .Trim().Trim('/');
                var urlPrefix = context.Resolve<ShellSettings>().RequestUrlPrefix;
                if (!string.IsNullOrWhiteSpace(urlPrefix)) {
                    cleanUrl = cleanUrl
                        .StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase)
                            ? cleanUrl.Substring(urlPrefix.Length) : cleanUrl;
                }
                cleanUrl = HttpUtility.UrlDecode(cleanUrl);
                cleanUrl = cleanUrl
                    .Trim().Trim('/');
                isHomePage = string.IsNullOrWhiteSpace(cleanUrl);
            }

            // reading settings
            var settings = _extendedCultureService.ReadSettings();
            part.AvailableCultures = settings.ExtendedCulturesList;
            part.ShowOnlyPertinentCultures = settings.Settings.ShowOnlyPertinentCultures;
            part.ShowLabel = settings.Settings.ShowLabel;

            part.TranslatedCultures = _localizableContentService.AvailableTranslations(cleanUrl, isHomePage);

            part.UserCulture = _extendedCultureService
                .GetExtendedCulture(_cultureManager.GetCurrentCulture(_workContextAccessor.GetContext().HttpContext));

            // if the UserCulture is null set the default culture of the site
            if (part.UserCulture == null) {
                part.UserCulture = _extendedCultureService
                    .GetExtendedCulture(_workContextAccessor.GetContext().CurrentSite.SiteCulture);
            }

            return ContentShape("Parts_CulturePicker", 
                () => shapeHelper.Parts_CulturePicker(
                    AvailableCultures: part.AvailableCultures,
                    TranslatedCultures: part.TranslatedCultures,
                    UserCulture: part.UserCulture,
                    ShowOnlyPertinentCultures: part.ShowOnlyPertinentCultures,
                    ShowLabel: part.ShowLabel));
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