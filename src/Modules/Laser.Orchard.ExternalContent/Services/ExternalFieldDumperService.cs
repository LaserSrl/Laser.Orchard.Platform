using Laser.Orchard.Commons.Services;
using Laser.Orchard.ExternalContent.Fields;
using Laser.Orchard.ExternalContent.Settings;
using Laser.Orchard.StartupConfig.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser.Orchard.ExternalContent.Services {
    public class ExternalFieldDumperService : IDumperService {

        private readonly IFieldExternalService _fieldExternalService;

        public ExternalFieldDumperService(
            IFieldExternalService fieldExternalService) {

            _fieldExternalService = fieldExternalService;
        }

        public void DumpList(DumperServiceContext context) {
            var fields = context.Content
                .ContentItem
                .Parts
                .SelectMany(pa => pa.Fields)
                .Where(fi => fi is FieldExternal)
                .Select(fi => fi as FieldExternal)
                .Where(fi => fi.Setting.GenerateL);
            if (fields != null && fields.Any()) {
                var Myobject = new Dictionary<string, object> { { "Content", context.Content.ContentItem } };
                foreach (var externalField in fields) {
                    var settings = externalField.PartFieldDefinition
                        .Settings.GetModel<FieldExternalSetting>();
                    if (!settings.NoFollow) {

                        var mainSb = new StringBuilder();
                        mainSb.Append("{");

                        try {
                            if (string.IsNullOrEmpty(settings.ExternalURL)) {
                                externalField.ContentObject = _fieldExternalService
                                    .GetContentfromField(Myobject, externalField.ExternalUrl, externalField.Name, 
                                        settings, context.Content.ContentItem.ContentType, externalField.HttpVerb,
                                        externalField.HttpDataType, externalField.AdditionalHeadersText, externalField.BodyRequest);
                            } else {
                                externalField.ContentObject = _fieldExternalService
                                    .GetContentfromField(Myobject, settings.ExternalURL, externalField.Name, 
                                        settings, context.Content.ContentItem.ContentType, settings.HttpVerb,
                                        settings.HttpDataType, settings.AdditionalHeadersText, settings.BodyRequest);
                            }
                        } catch (ExternalFieldRemoteException ex) {
                            externalField.ContentObject = ex;
                        }

                        if (externalField.ContentObject != null) {
                            var dump = context.GetDumper()
                                .Dump(cleanobj(externalField.ContentObject), externalField.Name, "List<generic>");
                            context.ConvertToJson(dump, mainSb);
                        }

                        mainSb.Append("}");

                        // Add the serialization to the results
                        context.ContentLists.Add(mainSb.ToString());
                    }
                }
            }
        }
        private dynamic cleanobj(dynamic objec) {
            if (objec != null)
                if (objec.ToRemove != null) {
                    return cleanobj(objec.ToRemove);
                }
            return objec;
        }
    }
}