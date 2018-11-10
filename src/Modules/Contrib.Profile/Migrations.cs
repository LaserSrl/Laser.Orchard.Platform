using Contrib.Profile.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Contrib.Profile {
    public class Migrations : DataMigrationImpl {

        private readonly IEnumerable<IDefaultFrontEndSettingsProvider> _frontEndSettingsProviders;

        public Migrations(
            IEnumerable<IDefaultFrontEndSettingsProvider> frontEndSettingsProviders) {

            _frontEndSettingsProviders = frontEndSettingsProviders;
        }

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart("ProfilePart")
                );

            return 2;
        }

        /// <summary>
        /// This update is required by the fact we added the setting to choose which Parts and Fields
        /// should be diplayed or edited on the front-end.
        /// </summary>
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("ProfilePart", builder => builder
                .Attachable(false));

            //All ContentTypes that contain a ProfilePart are affected by the changes
            var typeDefinitions = ContentDefinitionManager
                .ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart"));
            //By default, every part and field will be configured to not show on front-end, both 
            //for Display and Edit. This is a break with what was in place before, when everything
            //sould be shown in front-end. However, this is also safer.
            //We will only go and change the setting for those ContentParts that we know must be
            //available on the front-end.
            //For each type we will only run those providers that are defined to handle settings
            //for a ContentPart that is actually in the type's definition
            foreach (var typeDefinition in typeDefinitions) { 
                foreach (var provider in _frontEndSettingsProviders
                    .Where(provider => { 
                        return typeDefinition
                            .Parts
                            .Select(ctpd => ctpd.PartDefinition.Name)
                            .Any(name => provider.ForParts().Contains(name));
                    })) {
                    provider.ConfigureDefaultValues(typeDefinition);
                }
            }

            return 2;
        }
    }
}