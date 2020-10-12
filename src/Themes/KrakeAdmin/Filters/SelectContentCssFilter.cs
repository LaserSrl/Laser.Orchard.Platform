using Orchard;
using Orchard.Environment.Features;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace KrakeAdmin.Filters {
    public class SelectContentCssFilter : FilterProvider, IResultFilter {

        private readonly IResourceManager _resourceManager;
        private readonly IFeatureManager _featureManager;

        private TextWriter _originalWriter;
        private StringWriter _tempWriter;

        public SelectContentCssFilter(IResourceManager resourceManager, IOrchardServices orchardServices,
            IFeatureManager featureManager) {

            _resourceManager = resourceManager;
            _featureManager = featureManager;

        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            if (isAdminKrakePicker(filterContext.RouteData) || isAdminKrakeTranslator(filterContext.RouteData)) {
                CaptureResponse(filterContext);
            }
        }


        public void OnResultExecuting(ResultExecutingContext filterContext) {                 
            if (isAdminKrakePicker(filterContext.RouteData) || isAdminKrakeTranslator(filterContext.RouteData)) {
                _resourceManager.Require("stylesheet", ResourceManifest.BaseAdmin).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.Bootstrap).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.Site).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.KrakeAdmin).AtHead();
                _resourceManager.Require("stylesheet", ResourceManifest.KrakeNavigation).AtHead();

                _originalWriter = filterContext.HttpContext.Response.Output;
                _tempWriter = new StringWriterWithEncoding(_originalWriter.Encoding, _originalWriter.FormatProvider);
                filterContext.HttpContext.Response.Output = _tempWriter;

            }
        }

        public bool isAdminKrakePicker(RouteData route) {
            var featureTheme = _featureManager
                .GetAvailableFeatures()
                .FirstOrDefault(f => f.Id.Equals("KrakeAdmin", StringComparison.OrdinalIgnoreCase));
            var isKrakeTheme = featureTheme != null;
            bool isActionIndex = route.Values.ContainsKey("action") && String.Equals(route.Values["action"].ToString(), "index", StringComparison.OrdinalIgnoreCase);

            //advanced search
            bool isPickerContent = route.Values.ContainsKey("area") && route.Values["area"].Equals("Orchard.ContentPicker");
            bool isAdmin = route.Values.ContainsKey("controller") && route.Values["controller"].Equals("admin");
            
            //simple search
            bool isSearchContent = route.Values.ContainsKey("area") && route.Values["area"].Equals("Orchard.Search");
            bool isContentPicker = route.Values.ContainsKey("controller") && route.Values["controller"].Equals("ContentPicker");

            return isActionIndex && isKrakeTheme && (isAdmin && isPickerContent || isSearchContent && isContentPicker);
        }

        private bool isAdminKrakeTranslator(RouteData route) {
            var featureTheme = _featureManager
                .GetAvailableFeatures()
                .FirstOrDefault(f => f.Id.Equals("KrakeAdmin", StringComparison.OrdinalIgnoreCase));
            var isKrakeTheme = featureTheme != null;

            //translator
            bool isTranslator = route.Values.ContainsKey("area") && route.Values["area"].Equals("Laser.Orchard.Translator");
            bool isTranslatorController = route.Values.ContainsKey("controller") && route.Values["controller"].Equals("Translator");
            bool isTranslatorFrame = route.Values.ContainsKey("action") && (route.Values["action"].Equals("TranslatorForm") || route.Values["action"].Equals("TranslatorFolderSettings"));

            return isTranslator && isTranslatorController && isTranslatorFrame;
        }

        private void CaptureResponse(ControllerContext filterContext) {
            filterContext.HttpContext.Response.Output = _originalWriter;

            string capturedText = _tempWriter.ToString();

            var regex = new Regex("(<[^>]+site.css[^>]+>)");
            Match firstOcc = regex.Match(capturedText);
            var offset = firstOcc.Index + firstOcc.Length;
            capturedText = regex.Replace(capturedText, "", 1, offset);
       
            filterContext.HttpContext.Response.Write(capturedText);
        }
    }
}