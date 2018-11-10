using Contrib.Profile.Settings;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Settings;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Contrib.Profile.Services {
    public class FrontEndProfileService : IFrontEndProfileService {
        Func<ContentTypePartDefinition, string, bool> IFrontEndProfileService.MayAllowPartDisplay {
            get {
                return new Func<ContentTypePartDefinition, string, bool>(MayAllowPartDisplay);
            }
        }

        Func<ContentTypePartDefinition, string, bool> IFrontEndProfileService.MayAllowPartEdit {
            get {
                return new Func<ContentTypePartDefinition, string, bool>(MayAllowPartEdit);
            }
        }

        Func<ContentPartFieldDefinition, bool> IFrontEndProfileService.MayAllowFieldDisplay {
            get {
                return new Func<ContentPartFieldDefinition, bool>(MayAllowFieldDisplay);
            }
        }

        Func<ContentPartFieldDefinition, bool> IFrontEndProfileService.MayAllowFieldEdit {
            get {
                return new Func<ContentPartFieldDefinition, bool>(MayAllowFieldEdit);
            }
        }

        public bool UserHasNoProfilePart(IUser user) {
            return !user.ContentItem.Parts.Any(pa => pa.PartDefinition.Name == "ProfilePart");
        }

        private bool MayAllowPartDisplay(ContentTypePartDefinition definition, string typeName) {
            return definition.PartDefinition.Name == typeName || //this is to account for fields added to the type
                definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndDisplay;
        }

        private bool MayAllowPartEdit(ContentTypePartDefinition definition, string typeName) {
            return definition.PartDefinition.Name == typeName || //this is to account for fields added to the type
                definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndEdit;
        }

        private bool MayAllowFieldDisplay(ContentPartFieldDefinition definition) {
            return definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndDisplay;
        }

        private bool MayAllowFieldEdit(ContentPartFieldDefinition definition) {
            return definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndEdit;
        }

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

        public PlacementSettings[] GetFrontEndPlacement(ContentTypeDefinition contentTypeDefinition) {
            var currentSettings = contentTypeDefinition.Settings;
            string placement;
            var serializer = new JavaScriptSerializer();

            currentSettings.TryGetValue("ContentTypeSettings.Placement.ProfileFrontEndEditor", out placement);

            return String.IsNullOrEmpty(placement) ? new PlacementSettings[0] : serializer.Deserialize<PlacementSettings[]>(placement);
        }
        
    }
}