using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using OrchardFields = Orchard.Fields;

namespace Laser.Orchard.Policy {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PolicyTextInfoPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("UserHaveToAccept")
                .Column<int>("Priority")
                .Column<string>("PolicyType", col => col.WithLength(25))
                .Column<bool>("AddPolicyToRegistration")
                );

            SchemaBuilder.CreateTable("UserPolicyPartRecord", table => table
                .ContentPartRecord());

            SchemaBuilder.CreateTable("UserPolicyAnswersRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("UserPolicyPartRecord_Id")
                .Column<int>("PolicyTextInfoPartRecord_Id")
                .Column<DateTime>("AnswerDate")
                .Column<bool>("Accepted")
                .Column<int>("UserPartRecord_Id", col => col.Nullable())
                );

            SchemaBuilder.CreateTable("UserPolicyAnswersHistoryRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("UserPolicyPartRecord_Id")
                .Column<int>("PolicyTextInfoPartRecord_Id")
                .Column<DateTime>("AnswerDate")
                .Column<DateTime>("EndValidity")
                .Column<bool>("Accepted")
                .Column<int>("UserPartRecord_Id", col => col.Nullable())
                );

            // Creating Policy ContentPart
            ContentDefinitionManager.AlterPartDefinition("PolicyPart", part => part
                .Attachable(true)
                .WithDescription("Adds the ability to define from which policies the content dependes on.")
                );
            // Creating PolicyText ContentPart
            ContentDefinitionManager.AlterPartDefinition("PolicyTextInfoPart", part => part
                .Attachable(false)
                .WithDescription("Adds the ability to define a content as a policy.")
                );

            // Creating PolicyText ContentItem
            ContentDefinitionManager.AlterTypeDefinition("PolicyText", content => content
                .Draftable(false)
                .Creatable()
                .Listable()
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", part => part.WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Policy', Pattern: 'policy/{Content.Slug}', Description: 'policy/my-page'}]"))
                .WithPart("BodyPart")
                .WithPart("LocalizationPart")
                .WithPart("PolicyTextInfoPart")
                );

            ContentDefinitionManager.AlterPartDefinition("UserPolicyPart", part => part
            .Attachable(true)
            .WithDescription("Summarizes the choices of the policies for a content and ties policy functionalities around a user."));

            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("UserPolicyPart"));

            return 8;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("PolicyTextInfoPartRecord", table => table
                .AddColumn<string>("PolicyType", col => col.WithLength(25)));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("UserPolicyAnswersHistoryRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("UserPolicyPartRecord_Id")
                .Column<int>("PolicyTextInfoPartRecord_Id")
                .Column<DateTime>("AnswerDate")
                .Column<DateTime>("EndValidity")
                .Column<bool>("Accepted")
                );
            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterTypeDefinition("PolicyText", content => content
               .Listable()
               );

            return 4;
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("UserPolicyAnswersRecord", t =>
                t.AddColumn<int>("UserPartRecord_Id", col => col.Nullable())
            );
            SchemaBuilder.AlterTable("UserPolicyAnswersHistoryRecord", t =>
                t.AddColumn<int>("UserPartRecord_Id", col => col.Nullable())
            );
            return 5;
        }
        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("PolicyTextInfoPartRecord", table => table
                .AddColumn<bool>("AddPolicyToRegistration"));
            return 6;
        }

        public int UpdateFrom6() {
            ContentDefinitionManager.AlterPartDefinition("PolicyPart", part => part
                .WithDescription("Adds the ability to define from which policies the content dependes on.")
                );

            ContentDefinitionManager.AlterPartDefinition("PolicyTextInfoPart", part => part
                .WithDescription("Adds the ability to define a content as a policy.")
                );

            ContentDefinitionManager.AlterPartDefinition("UserPolicyPart", part => part
            .Attachable(true)
            .WithDescription("Resume the choices of the policies for the content."));


            return 7;
        }

        public int UpdateFrom7() {

            ContentDefinitionManager.AlterPartDefinition("UserPolicyPart", part => part
                .WithDescription("Summarizes the choices of the policies for a content and ties policy functionalities around a user."));

            return 8;
        }
    }
}
