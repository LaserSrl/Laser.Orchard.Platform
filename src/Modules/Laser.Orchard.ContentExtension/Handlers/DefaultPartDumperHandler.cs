using Laser.Orchard.StartupConfig.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.ContentExtension.Handlers {
    public class DefaultPartDumperHandler : IDumperHandler {
        private readonly IUtilsServices _utilsServices;
        public DefaultPartDumperHandler(IUtilsServices utilsServices) {
            _utilsServices = utilsServices;
        }
        public void StoreLikeDynamic(ContentItem item, string[] listProperty, object value) {
            dynamic subobject = item.Parts.Where(x => x.PartDefinition.Name == listProperty[0]).FirstOrDefault();
            if (subobject == null) {
                throw new Exception("Part " + listProperty[0] + " not exist");
            }
            Int32 numparole = listProperty.Count();
            for (int i = 1; i < numparole; i++) {
                string property = listProperty[i];
                if (i != numparole - 1) {
                    subobject = subobject.GetType().GetProperty(property);
                } else {
                    // prova a salvarlo come proprietà
                    try { 
                        subobject.GetType().GetProperty(property).SetValue(subobject, Convert.ChangeType(value, subobject.GetType().GetProperty(property).PropertyType), null);
                        // potrei ancora tentare di scrivere direttamente con
                        // subobject.GetType().GetProperty(property).SetValue(subobject, value, null);
                    } catch {
                        // ignora volutamente eventuali errori
                    }
                    // prova a salvarlo come field
                    try {
                        List<ContentPart> lcp = new List<ContentPart>();
                        lcp.Add((ContentPart)subobject);
                        _utilsServices.StoreInspectExpandoFields(lcp, property, value);
                    } catch {
                        // ignora volutamente eventuali errori
                    }
                }
            }
        }
    }
}