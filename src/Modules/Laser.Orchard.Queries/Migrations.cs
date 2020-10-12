using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Queries {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
               "MyCustomQueryPart",
                part => part
                   .Attachable(false)
                   .WithField("QueryString", cfg => cfg
                        .OfType("TextField")
                        .WithDisplayName("Text of HQL Query")
                        .WithSetting("TextFieldSettings.Flavor", "TextArea")
                        .WithSetting("TextFieldSettings.Required", "true")
                        .WithSetting("TextFieldSettings.Hint", "Enter the HQL query. You may use parameters with the following syntax: ( :paramN ). N is the line number (0-based) for the parameter configuration above. The parentheses and whitespace around :paramN are mandatory."))
                   .WithField("Options", cfg => cfg.OfType("TextField").WithDisplayName("Options"))
                   .WithField("QueryParameterValues", cfg => cfg
                        .OfType("TextField")
                        .WithDisplayName("Parameters for HQL Query")
                        .WithSetting("TextFieldSettings.Flavor", "TextArea")
                        .WithSetting("TextFieldSettings.Required", "false")
                        .WithSetting("TextFieldSettings.Hint", "Enter the values for the parameters. You may use tokens. Enter one value per line, and end each line with a comma: they will each map to a different parameter. This does not support multiline string parameters. All parameters are parsed as strings."))

            );
            ContentDefinitionManager.AlterTypeDefinition(
              "MyCustomQuery",
              type => type
                .WithPart("TitlePart")
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
                    .WithField("OneShotQuery", cfg => cfg.OfType("BooleanField")
                        .WithSetting("BooleanFieldSettings.Hint", "If checked, the query will be shown in a separate group")
                        .WithSetting("BooleanFieldSettings.DefaultValue", "false")
                    )

               );
            ContentDefinitionManager.AlterTypeDefinition(
                    "Query",
                    type => type
                        .WithPart("CommonPart")
                          .WithPart("QueryUserFilterExtensionPart", p => p.WithSetting("QueryUserFilterExtensionPartSettingVM.QueryUserFilter", "ProfilePartContentFields"))
                    );
            ContentDefinitionManager.AlterPartDefinition(
                "QueryPickerPart", p => p.
                    Attachable(false));

            return 5;
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
                        .WithSetting("BooleanFieldSettings.DefaultValue", "false"))
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
                        .WithSetting("TextFieldSettings.Hint", "Enter the HQL query. You may use parameters with the following syntax: ( :paramN ). N is the line number (0-based) for the parameter configuration above. The parentheses and whitespace around :paramN are mandatory."))
                  .WithField("Options", cfg => cfg.OfType("TextField").WithDisplayName("Options"))
           );
            return 4;
        }
        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition(
              "MyCustomQueryPart",
               b => b
                  .WithField("QueryParameterValues", cfg => cfg
                        .OfType("TextField")
                        .WithDisplayName("Parameters' values for HQL Query")
                        .WithSetting("TextFieldSettings.Flavor", "TextArea")
                        .WithSetting("TextFieldSettings.Required", "false")
                        .WithSetting("TextFieldSettings.Hint", "Enter the values for the parameters. You may use tokens. Enter one value per line, and end each line with a comma: they will each map to a different parameter. This does not support multiline string parameters. All parameters are parsed as strings."))
           );
            return 5;
        }

        public int UpdateFrom5() {
            ContentDefinitionManager.AlterPartDefinition(
               "MyCustomQueryPart",
                part => part
                   .WithField("IsSQL", cfg => cfg
                        .OfType("BooleanField")
                        .WithDisplayName("Is the Query language SQL?")
                        .WithSetting("BooleanFieldSettings.Hint", "Select this if the language you are using is SQL rather than HQL. When using SQL, you must pay special care to table names, especially when you are importing a query from a different tenant."))

            );

            return 6;
        }
    }
}