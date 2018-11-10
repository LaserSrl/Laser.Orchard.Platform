using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Queries {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
               "MyCustomQueryPart",
                b => b
                   .Attachable(false)
                   .WithField("QueryString", cfg => cfg.OfType("TextField")
                       .WithDisplayName("Text Query")
                         .WithSetting("TextFieldSettings.Flavor", "TextArea")
                        .WithSetting("TextFieldSettings.Required", "true")
                        .WithSetting("TextFieldSettings.Hint", "Insert a query")
                        )
                   .WithField("Options", cfg => cfg.OfType("TextField").WithDisplayName("Options"))
            );
            ContentDefinitionManager.AlterTypeDefinition(
              "MyCustomQuery",
              type => type
                  .WithPart("TitlePart")
                  //.WithPart("AutoroutePart", p => p
                  //  .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                  //  .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                  //  .WithSetting("AutorouteSettings.PatternDefinitions", @"[{Name:'Title', Pattern:'{Content.Slug}',Description:'Title of campaign'}]")
                  //  .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                  //     )
                .WithPart("IdentityPart")

                .WithPart("MyCustomQueryPart")

                .WithPart("CommonPart")
                .Creatable(false)
                .DisplayedAs("CustomQuery")
              );
            ContentDefinitionManager.AlterPartDefinition(
                 "QueryUserFilterExtensionPart",
                  b => b
                     .Attachable(false)
                     .WithField("UserQuery", cfg => cfg.OfType("BooleanField")
                         .WithSetting("BooleanFieldSettings.Hint", "If checked, the query will modified by users")
                         .WithSetting("BooleanFieldSettings.DefaultValue", "false")
                     )
               );
            ContentDefinitionManager.AlterTypeDefinition(
                    "Query",
                    type => type
                        .WithPart("CommonPart")
                          .WithPart("QueryUserFilterExtensionPart", p => p.WithSetting("QueryUserFilterExtensionPartSettingVM.QueryUserFilter", "ProfilePartContentFields"))
                    );
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(
         "QueryPickerPart",
         p => p.Attachable(false));
            return 2;
        }
        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition(
                "QueryUserFilterExtensionPart",
                 b => b
                    .Attachable(false)
                    .WithField("OneShotQuery", cfg => cfg.OfType("BooleanField")
                        .WithSetting("BooleanFieldSettings.Hint", "If checked, the query will be shown in a separate group")
                        .WithSetting("BooleanFieldSettings.DefaultValue", "false")
                    )
            );
            return 3;
        }
        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition(
              "MyCustomQueryPart",
               b => b
                  .Attachable(false)
                  .WithField("QueryString", cfg => cfg
                    .OfType("TextField")
                    .WithDisplayName("Text of HQL Query")
                    .WithSetting("TextFieldSettings.Flavor", "TextArea")
                    .WithSetting("TextFieldSettings.Required", "true")
                    .WithSetting("TextFieldSettings.Hint", "Insert a HQL query")
                    )
                  .WithField("Options", cfg => cfg.OfType("TextField").WithDisplayName("Options"))
           );
            return 4;
        }
    }
}