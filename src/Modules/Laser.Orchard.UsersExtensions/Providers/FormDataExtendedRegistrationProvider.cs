using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Services;
using System;
using System.Linq;
using System.Web;
using System.Web.Http.Metadata;
using System.Web.Mvc;

namespace Laser.Orchard.UsersExtensions.Providers {
    public class FormDataExtendedRegistrationProvider : BaseExtendedRegistrationProvider {
        private readonly IContentManager _contentManager;

        public FormDataExtendedRegistrationProvider(IContentManager contentManager,
            ITaxonomyService taxonomyService) : base(contentManager, taxonomyService) {

            _contentManager = contentManager;
        }

        public override void FillCustomFields(HttpRequestBase request, JObject registeredData) {
            if (request.Form != null && request.Form.Keys.Count > 0) {
                var user = _contentManager.Get(GetUserIdFromRegisteredData(registeredData));
                if (user != null) {
                    //foreach (var key in request.Form.Keys) {
                    //}
                    //var type = user.GetType();
                    //var binder = new DefaultModelBinder();
                    //var context = new ModelBindingContext {
                    //    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, type)
                    //};

                    //var controllerContext = new ControllerContext();

                    var allFields = user.Parts.SelectMany(pa => pa.Fields);

                    foreach (var key in request.Form.Keys) {
                        var field = allFields.FirstOrDefault(fi => fi.PartFieldDefinition.Name.Equals(key.ToString(), StringComparison.OrdinalIgnoreCase));
                        FillCustomField(user, field, request.Form[key.ToString()].ToString());
                    }
                }
            }
        }
    }
}