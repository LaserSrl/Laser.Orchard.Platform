using Laser.Orchard.ExternalContent.Fields;
using Laser.Orchard.ExternalContent.Services;
using Laser.Orchard.ExternalContent.Settings;
using Laser.Orchard.StartupConfig.Exceptions;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.ExternalContent.Handlers {
    public class FieldExternalHandler : ContentHandler {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IFieldExternalService _fieldExternalService;
        private readonly IOrchardServices _orchardServices;
        //public ILogger Logger { get; set; } //no need to declare it here, because ContentHandler already declares it
        public FieldExternalHandler(
             IContentDefinitionManager contentDefinitionManager
            , IFieldExternalService fieldExternalService
            , IOrchardServices orchardServices) {
            _contentDefinitionManager = contentDefinitionManager;
            _fieldExternalService = fieldExternalService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            
        }


        protected override void BuildDisplayShape(BuildDisplayContext context) {

            //    _cacheManager.Get(
            //    "Vandelay.Favicon.Url",
            //    ctx => {
            //        ctx.Monitor(_signals.When("Vandelay.Favicon.Changed"));
            //        var faviconSettings = ...;
            //        return faviconSettings.FaviconUrl;
            //    });
            //_signals.Trigger("Vandelay.Favicon.Changed");


            base.BuildDisplayShape(context);
            if (context.DisplayType == "Detail" || context.DisplayType == "Summary") {

                var fields = context.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(FieldExternal).Name)).Cast<FieldExternal>();
                if (fields.Count() > 0) {
                    var Myobject = new Dictionary<string, object> { { "Content", context.ContentItem } };

                    foreach (var field in fields) {
                        //       Logger.Error("Field get inizio:"+DateTime.Now.ToString());
                        if (field.GetType().Name == "FieldExternal") {
                            var settings = field.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>();
                            if (settings.NoFollow) {
                                field.ContentUrl = _fieldExternalService.GetUrl(Myobject, settings.ExternalURL);
                            }
                            else if (context.DisplayType == "Detail") {
                                try {
                                    if (string.IsNullOrEmpty(settings.ExternalURL))
                                        field.ContentObject = _fieldExternalService.GetContentfromField(Myobject, field.ExternalUrl, field.Name, settings, context.ContentItem.ContentType, field.HttpVerb, field.HttpDataType, field.AdditionalHeadersText, field.BodyRequest);
                                    else
                                        field.ContentObject = _fieldExternalService.GetContentfromField(Myobject, settings.ExternalURL, field.Name, settings, context.ContentItem.ContentType, settings.HttpVerb, settings.HttpDataType, settings.AdditionalHeadersText, settings.BodyRequest);
                                    //               Logger.Error("Field get fine:" + DateTime.Now.ToString());
                                }
                                catch (ExternalFieldRemoteException ex) {
                                    field.ContentObject = ex;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}