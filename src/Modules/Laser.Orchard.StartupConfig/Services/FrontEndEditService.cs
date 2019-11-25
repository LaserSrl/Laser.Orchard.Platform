using System;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Laser.Orchard.StartupConfig.Services {
    public class FrontEndEditService : IFrontEndEditService {
        public dynamic BuildFrontEndShape(dynamic shape, Func<ContentTypePartDefinition, string, bool> partTest, Func<ContentPartFieldDefinition, bool> fieldTest) {
            //shape.Content.Items contains the List<object> of the things we will display
            //we can do a ((List<dynamic>)(shape.Content.Items)).RemoveAll(condition) to get rid 
            //of the stuff we do not want to see.

            //remove parts. This also removes all parts that are dynamically attached and hence
            //cannot have the setting to control their visibility
            ((List<dynamic>)(shape.Content.Items))
                .RemoveAll(it =>
                    it.ContentPart != null &&
                    !partTest(it.ContentPart.TypePartDefinition, it.ContentPart.TypeDefinition.Name)
                );
            //remove fields
            ((List<dynamic>)(shape.Content.Items))
                .RemoveAll(it =>
                    it.ContentPart != null &&
                    it.ContentField != null &&
                    !fieldTest(it.ContentField.PartFieldDefinition)
                );

            return shape;
        }

    }
}