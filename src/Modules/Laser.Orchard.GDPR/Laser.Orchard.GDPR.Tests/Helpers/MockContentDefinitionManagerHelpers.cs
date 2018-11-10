using Moq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.GDPR.Tests.Helpers {
    public static class MockContentDefinitionManagerHelpers {

        public static IEnumerable<ContentPartDefinition> MockPartDefinitions() {
            var partDefinitions = new List<ContentPartDefinition>();

            // definition of the part for the type with no GDPRPart
            partDefinitions.Add(new ContentPartDefinitionBuilder()
                .Named("NoGDPRType")
                .WithField("AlphaField", cpfdb => cpfdb
                    .OfType(new ContentFieldDefinition("AlphaField"))
                    .WithSetting("Storage", "Infoset"))
                .Build());
            // definition of the AlphaPart, that will have an AlphaField but no GDPR setting
            partDefinitions.Add(new ContentPartDefinitionBuilder()
                .Named("AlphaPart")
                .WithField("AlphaField", cpfdb => cpfdb
                    .OfType(new ContentFieldDefinition("AlphaField"))
                    .WithSetting("Storage", "Infoset"))
                .Build());
            // definition of the part for the type with GDPRPart
            partDefinitions.Add(new ContentPartDefinitionBuilder()
                .Named("GDPRType")
                // a field for anonymization
                .WithField("AlphaAnonymize", cpfdb => cpfdb
                    .OfType(new ContentFieldDefinition("AlphaField"))
                    .WithSetting("Storage", "Infoset")
                    .WithSetting("GDPRPartFieldSettings.ShouldAnonymize", (true).ToString(CultureInfo.InvariantCulture))
                    .WithSetting("GDPRPartFieldSettings.ShouldErase", (false).ToString(CultureInfo.InvariantCulture)))
                // a field for erasure
                .WithField("AlphaErase", cpfdb => cpfdb
                    .OfType(new ContentFieldDefinition("AlphaField"))
                    .WithSetting("Storage", "Infoset")
                    .WithSetting("GDPRPartFieldSettings.ShouldAnonymize", (false).ToString(CultureInfo.InvariantCulture))
                    .WithSetting("GDPRPartFieldSettings.ShouldErase", (true).ToString(CultureInfo.InvariantCulture)))
                .Build());
            // definition of the part for the Profile Item Type
            partDefinitions.Add(new ContentPartDefinitionBuilder()
                .Named("ProfileItemType")
                // a field for anonymization
                .WithField("AlphaAnonymize", cpfdb => cpfdb
                    .OfType(new ContentFieldDefinition("AlphaField"))
                    .WithSetting("Storage", "Infoset")
                    .WithSetting("GDPRPartFieldSettings.ShouldAnonymize", (true).ToString(CultureInfo.InvariantCulture))
                    .WithSetting("GDPRPartFieldSettings.ShouldErase", (false).ToString(CultureInfo.InvariantCulture)))
                // a field for erasure
                .WithField("AlphaErase", cpfdb => cpfdb
                    .OfType(new ContentFieldDefinition("AlphaField"))
                    .WithSetting("Storage", "Infoset")
                    .WithSetting("GDPRPartFieldSettings.ShouldAnonymize", (false).ToString(CultureInfo.InvariantCulture))
                    .WithSetting("GDPRPartFieldSettings.ShouldErase", (true).ToString(CultureInfo.InvariantCulture)))
                .Build());
            // a part for anonymization
            partDefinitions.Add(new ContentPartDefinitionBuilder()
                .Named("AnonymizeMePart")
                .WithSetting("GDPRPartPartSettings.ShouldAnonymize", (true).ToString(CultureInfo.InvariantCulture))
                .WithSetting("GDPRPartPartSettings.ShouldErase", (false).ToString(CultureInfo.InvariantCulture))
                .Build());
            // a part for erasure
            partDefinitions.Add(new ContentPartDefinitionBuilder()
                .Named("EraseMePart")
                .WithSetting("GDPRPartPartSettings.ShouldAnonymize", (false).ToString(CultureInfo.InvariantCulture))
                .WithSetting("GDPRPartPartSettings.ShouldErase", (true).ToString(CultureInfo.InvariantCulture))
                .Build());

            return partDefinitions;
        }

        public static IEnumerable<ContentTypeDefinition> MockTypeDefinitions() {
            var typeDefinitions = new List<ContentTypeDefinition>();
            // I need a type without GDPRPart
            typeDefinitions.Add(new ContentTypeDefinitionBuilder()
                .Named("NoGDPRType")
                .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "NoGDPRType"), configuration => { })
                .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "AlphaPart"), configuration => { })
                .Build());
            // I need a type with a GDPRPart
            typeDefinitions.Add(new ContentTypeDefinitionBuilder()
                .Named("GDPRType")
                .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "GDPRType"), configuration => { })
                .WithPart("GDPRPart")
                .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "AnonymizeMePart"), ctpdb => ctpdb
                    .WithSetting("GDPRPartPartSettings.ShouldAnonymize", (true).ToString(CultureInfo.InvariantCulture))
                    .WithSetting("GDPRPartPartSettings.ShouldErase", (false).ToString(CultureInfo.InvariantCulture)))
                .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "EraseMePart"), ctpdb => ctpdb
                    .WithSetting("GDPRPartPartSettings.ShouldAnonymize", (false).ToString(CultureInfo.InvariantCulture))
                    .WithSetting("GDPRPartPartSettings.ShouldErase", (true).ToString(CultureInfo.InvariantCulture)))
                .Build());
            // I need a ProfileItem
            typeDefinitions.Add(new ContentTypeDefinitionBuilder()
               .Named("ProfileItemType")
               .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "GDPRType"), configuration => { })
               .WithPart("GDPRPart", ctpdb => ctpdb
                    .WithSetting("GDPRPartTypeSettings.IsProfileItemType", (true).ToString(CultureInfo.InvariantCulture)))
               .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "AnonymizeMePart"), ctpdb => ctpdb
                   .WithSetting("GDPRPartPartSettings.ShouldAnonymize", (true).ToString(CultureInfo.InvariantCulture))
                   .WithSetting("GDPRPartPartSettings.ShouldErase", (false).ToString(CultureInfo.InvariantCulture)))
               .WithPart(MockPartDefinitions().FirstOrDefault(cpd => cpd.Name == "EraseMePart"), ctpdb => ctpdb
                   .WithSetting("GDPRPartPartSettings.ShouldAnonymize", (false).ToString(CultureInfo.InvariantCulture))
                   .WithSetting("GDPRPartPartSettings.ShouldErase", (true).ToString(CultureInfo.InvariantCulture)))
               .Build());

            return typeDefinitions;
        }

        public static Mock<IContentDefinitionManager> MockManager() {
            var mockDefinitionManager = new Mock<IContentDefinitionManager>();
            mockDefinitionManager
                .Setup(mdm => mdm.ListTypeDefinitions())
                .Returns(MockTypeDefinitions);
            mockDefinitionManager
                .Setup(mdm => mdm.ListPartDefinitions())
                .Returns(MockPartDefinitions);
            mockDefinitionManager // this is required to create the test items
                .Setup(mdm => mdm.GetTypeDefinition(It.IsAny<string>()))
                .Returns<string>(name => MockTypeDefinitions()
                    .FirstOrDefault(ctd => ctd.Name == name));
            mockDefinitionManager
                .Setup(mdm => mdm.GetPartDefinition(It.IsAny<string>()))
                .Returns<string>(name => MockPartDefinitions()
                    .FirstOrDefault(ctd => ctd.Name == name));
            return mockDefinitionManager;
        }
    }
}
