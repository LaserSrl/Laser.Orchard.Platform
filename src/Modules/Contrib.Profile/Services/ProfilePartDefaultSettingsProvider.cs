using Contrib.Profile.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using System.Collections.Generic;
using System.Linq;

namespace Contrib.Profile.Services {
    /// <summary>
    /// The front end settings are all true for profile part and any field in it.
    /// </summary>
    public class ProfilePartDefaultSettingsProvider : IDefaultFrontEndSettingsProvider {

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ProfilePartDefaultSettingsProvider(
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;
        }

        public void ConfigureDefaultValues(ContentTypeDefinition definition, params string[] options) {
            //The options parameter here is used as an array of the names of the fields that need processing.
            //If empty, all fields will be processed.
            var typePartDefinition = definition.Parts
                .FirstOrDefault(ctpd => ctpd.PartDefinition.Name == "ProfilePart");
            if (typePartDefinition != null) { //sanity check

                var fieldDefinitions = typePartDefinition.PartDefinition.Fields;
                if (options != null && options.Any()) { //only desired fields
                    //In this case, we are not going to update the settings for the part
                    fieldDefinitions = fieldDefinitions
                        .Where(cpfd => {
                            var longName = cpfd.FieldDefinition.Name + "." + cpfd.Name;
                            return options.Contains(longName);
                        });
                }
                //time to update field settings
                if (fieldDefinitions.Any()) {
                    _contentDefinitionManager.AlterPartDefinition("ProfilePart",
                        partBuilder => {
                            foreach (var fieldDefinition in fieldDefinitions) {
                                partBuilder.WithField(fieldDefinition.Name,
                                    partFieldDefinitionBuilder => {
                                        ProfileFrontEndSettings.SetValues(partFieldDefinitionBuilder, true, true);
                                    });
                            }
                        });
                }
                //we'll update the type definition, since we update the part definition
                if (options == null || !options.Any()) {
                    //in this case we update the settings for the part and all contained fields
                    _contentDefinitionManager.AlterTypeDefinition(definition.Name,
                        typeBuilder =>
                            typeBuilder.WithPart("ProfilePart",
                                partBuilder => ProfileFrontEndSettings.SetValues(partBuilder, true, true)));
                }
            }
        }

        public IEnumerable<string> ForParts() {
            return new string[] { "ProfilePart" };
        }
    }
}