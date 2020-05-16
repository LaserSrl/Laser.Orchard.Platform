using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Roles.Services;
using System;
using System.Linq;
using Orchard.Security.Permissions;

namespace Laser.Orchard.Questionnaires {

    public class Migrations : DataMigrationImpl {
        private readonly IUtilsServices _utilsServices;

        public Migrations(IUtilsServices utilsServices,
            IRoleService roleService) {
            _utilsServices = utilsServices;
        }

        public int Create() {
            SchemaBuilder.CreateTable("QuestionnairePartRecord", table => table
                .ContentPartRecord());

            SchemaBuilder.CreateTable("QuestionRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("Question", col => col.WithLength(200))
                .Column<string>("QuestionType", col => col.WithLength(20))
                .Column<bool>("Published", col => col.WithDefault(true))
                .Column<int>("Position")
                .Column<int>("QuestionnairePartRecord_Id"));

            SchemaBuilder.CreateTable("AnswerRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("Answer", col => col.WithLength(200))
                .Column<bool>("Published", col => col.WithDefault(true))
                .Column<int>("Position")
                .Column<int>("QuestionRecord_Id"));

            SchemaBuilder.CreateTable("UserAnswersRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<int>("User_Id")
                .Column<int>("QuestionRecord_Id")
                .Column<int>("AnswerRecord_Id")
                .Column<string>("AnswerText", col => col.WithLength(200))
                .Column<DateTime>("AnswerDate"));

            SchemaBuilder.CreateForeignKey("UserAnswersAnswer_Answer", "UserAnswersRecord", new string[] { "AnswerRecord_Id" }, "AnswerRecord", new string[] { "Id" });
            SchemaBuilder.CreateForeignKey("UserAnswersQuestion_Question", "UserAnswersRecord", new string[] { "QuestionRecord_Id" }, "QuestionRecord", new string[] { "Id" });

            ContentDefinitionManager.AlterPartDefinition(typeof(QuestionnairePart).Name, cfg => cfg
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("Questionnaire", cfg => cfg
                .WithPart(typeof(QuestionnairePart).Name)
                .WithPart("CommonPart")
                .WithPart("LocalizationPart")
                .WithPart("TitlePart")
                .WithPart("BodyPart")
                .WithPart("AutoroutePart")
                .Creatable()
                .Draftable());

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AddColumn<string>("QuestionText", col => col.WithLength(200)));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("QuestionnairePartRecord", t => t.AddColumn<bool>("MustAcceptTerms",
                col => col.WithDefault(false)));
            SchemaBuilder.AlterTable("QuestionnairePartRecord", t => t.AddColumn<string>("TermsText",
                col => col.Unlimited()));
            SchemaBuilder.AlterTable("QuestionnairePartRecord", t => t.AddColumn<bool>("UseRecaptcha",
                col => col.WithDefault(false)));
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("Section",
                col => col.WithLength(200)));
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("Condition",
                col => col.Unlimited()));
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("AnswerType",
                col => col.WithLength(50)));
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<bool>("IsRequired",
                col => col.WithDefault(true)));
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("ConditionType",
                col => col.WithLength(50)));
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AddColumn<int>("QuestionnairePartRecord_Id"));
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AddColumn<string>("SessionID",
                col => col.WithLength(24)));

            _utilsServices.EnableFeature("Orchard.Captcha");
            return 3;
        }

        public int UpdateFrom3() {
            // SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AddColumn<bool>("CorrectResponse", col => col.WithDefault(false)));
            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("AnswerRecord", t => t.AddColumn<bool>("CorrectResponse", col => col.WithDefault(false)));
            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.CreateTable("GamePartRecord", table => table
            .ContentPartRecord());
            _utilsServices.EnableFeature("Laser.Orchard.Events");
            return 6;
        }

        public int UpdateFrom6() {
            ContentDefinitionManager.AlterPartDefinition(typeof(GamePart).Name, cfg => cfg
              .Attachable());
            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.CreateTable("RankingPartRecord", table => table
            .ContentPartRecord()
            .Column<int>("Point")
            .Column<string>("Identifier")
            .Column<string>("UsernameGameCenter")
            .Column<string>("Device")
            .Column<int>("ContentIdentifier")
            .Column<DateTime>("RegistrationDate"));

            // ContentDefinitionManager.AlterPartDefinition(typeof(RankingPart).Name, cfg => cfg);
            ContentDefinitionManager.AlterTypeDefinition("Ranking", cfg => cfg
             .WithPart(typeof(RankingPart).Name)
             .WithPart("CommonPart")
             );
            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.AlterTable("RankingPartRecord", t => t.AddColumn<bool>("AccessSecured", col => col.WithDefault(false)));
            SchemaBuilder.AlterTable("RankingPartRecord", t => t.AddColumn<int>("User_Id"));
            return 9;
        }

        public int UpdateFrom9() {
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<string>("AbstractText"));
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<DateTime>("GameDate"));
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<string>("RankingIOSIdentifier"));
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<string>("RankingAndroidIdentifier"));
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<int>("MyOrder"));
            //      SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<string>("EmailToSendTemplatedRanking"));
            return 10;
        }
        public int UpdateFrom10() {
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<bool>("workflowfired"));
            return 11;
        }
        public int UpdateFrom11() {
            _utilsServices.EnableFeature("Laser.Orchard.StartUpConfig");
            _utilsServices.EnableFeature("Laser.Orchard.Events");
            _utilsServices.EnableFeature("Laser.Orchard.TemplateManagement");
            _utilsServices.EnableFeature("Orchard.Messaging");
            return 12;
        }
        public int UpdateFrom12() {
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<Int32>("QuestionsSortedRandomlyNumber"));
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<bool>("RandomResponse"));

            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<Decimal>("AnswerPoint"));
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<Decimal>("AnswerTime"));
            return 13;
        }
        public int UpdateFrom13() {
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<int>("State"));
            return 14;
        }
        public int UpdateFrom14() {
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("AllFiles",
                      column => column.Unlimited()));
            SchemaBuilder.AlterTable("AnswerRecord", t => t.AddColumn<string>("AllFiles",
                      column => column.Unlimited()));
            return 15;
        }
        public int UpdateFrom15() {
            _utilsServices.EnableFeature("Laser.Orchard.ContentExtension");

            return 16;
        }
        public int UpdateFrom16() {
            SchemaBuilder.AlterTable("GamePartRecord", t => t.AddColumn<String>("GameType"));
            return 17;
        }
        public int UpdateFrom17() {
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AlterColumn("Question", col => col.WithType(System.Data.DbType.String).WithLength(500)));
            return 18;
        }

        public int UpdateFrom18() {
            //update permissions based on the new stereotypes we created
            var stereotypes = new Permissions().GetDefaultStereotypes();
            _utilsServices.UpdateStereotypesPermissions(stereotypes);
            return 19;
        }
        public int UpdateFrom19() {
            ContentDefinitionManager.AlterTypeDefinition("QuestionnaireStatsExport", type => type.WithPart("TitlePart")
                .Draftable(false)
                .Creatable(false)); 
            return 20;
        }
        public int UpdateFrom20() {
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AddColumn<string>("Context", col => col.WithLength(255)));
            return 21;
        }
        public int UpdateFrom21() {
            return 22;
        }
        public int UpdateFrom22() {
             ContentDefinitionManager.AlterTypeDefinition("Questionnaire", cfg => cfg
            .Listable());
            _utilsServices.EnableFeature("Orchard.Email");
            return 23;
        }
        public int UpdateFrom23() {
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AlterColumn("QuestionText", col => col.WithType(System.Data.DbType.String).WithLength(500)));
            return 24;        }

        /// <summary>
        /// Updates some Columns Length in order to fit more cases
        /// </summary>
        /// <returns></returns>
        public int UpdateFrom24() {
            SchemaBuilder.AlterTable("AnswerRecord", t => t.AlterColumn("Answer", col => col.WithType(System.Data.DbType.String).WithLength(1200)));
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AlterColumn("AnswerText", col => col.WithType(System.Data.DbType.String).WithLength(1200)));
            return 25;
        }

        public int UpdateFrom25() {
            SchemaBuilder.AlterTable("UserAnswersRecord", t => t.AlterColumn("SessionID", col => col.WithType(System.Data.DbType.String).WithLength(400)));
            return 26;
        }
        public int UpdateFrom26() {
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("Identifier",
                      column => column.Unlimited()));
            SchemaBuilder.AlterTable("AnswerRecord", t => t.AddColumn<string>("Identifier",
                      column => column.Unlimited()));
            return 27;
        }

        public int UpdateFrom27() {
            SchemaBuilder.AlterTable("UserAnswersRecord", t => 
                t.AddColumn<string>("AnswerInstance",
                    //since these strings are generated programmatically, we know how long they will be
                    column => column.WithLength(64)));
            return 28;
        }

        public int UpdateFrom28() {
            SchemaBuilder.AlterTable("UserAnswersRecord", t =>
                t.AddColumn<string>("QuestionType",
                    col => col.WithLength(20)));

            // add indexes over the stuff we use to filter select queries
            SchemaBuilder.AlterTable("UserAnswersRecord", table => table
                .CreateIndex("IX_User_Id", "User_Id"));
            SchemaBuilder.AlterTable("UserAnswersRecord", table => table
                .CreateIndex("IX_QuestionnairePartRecord_Id", "QuestionnairePartRecord_Id"));
            SchemaBuilder.AlterTable("UserAnswersRecord", table => table
                .CreateIndex("IX_AnswerInstance", "AnswerInstance"));

            return 29;
        }

        public int UpdateFrom29() {
            SchemaBuilder
                .CreateTable("UserAnswerInstanceRecord", table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<int>("QuestionnairePartRecord_Id")
                    .Column<int>("User_Id")
                    // string properties are the same length as those in UserAnswersRecord
                    .Column<string>("SessionID", col => col.WithLength(24))
                    .Column<string>("Context", col => col.WithLength(255))
                    .Column<DateTime>("AnswerDate")
                    .Column<string>("AnswerInstance", col => col.WithLength(64)));

            return 30;
        }

        public int UpdateFrom30() {
            SchemaBuilder
                // add indexes for the columns we'll most often query on
                .AlterTable("UserAnswerInstanceRecord", table => table
                    .CreateIndex("IX_LatestAnswersQuery", 
                        "QuestionnairePartRecord_Id", "User_Id", "AnswerDate"))
                .AlterTable("UserAnswerInstanceRecord", table => table
                    .CreateIndex("IX_LatestAnswersQuery_WithContext",
                        "QuestionnairePartRecord_Id", "User_Id", "AnswerDate", "Context"));
            return 31;
        }

        // We must rename the Identifier property of QuestionRecord and AnswerRecord
        // because some mobile libraries have issues mapping that.
        public int UpdateFrom31() {
            // 1. add a new column for the renamed property
            SchemaBuilder.AlterTable("QuestionRecord", t => t.AddColumn<string>("GUIdentifier",
                      column => column.Unlimited()));
            SchemaBuilder.AlterTable("AnswerRecord", t => t.AddColumn<string>("GUIdentifier",
                      column => column.Unlimited()));
            // 2. Copy values from old column to new column
            // This is done in sql because the class does not have the property anymore
            // UPDATE table SET columnB = columnA
            var baseTableName = SchemaBuilder.FormatPrefix(SchemaBuilder.FeaturePrefix);
            var questionTableName = string.Concat(baseTableName, "QuestionRecord");
            var answerTableName = string.Concat(baseTableName, "AnswerRecord");
            SchemaBuilder.ExecuteSql(
                $"UPDATE {questionTableName} SET GUIdentifier = Identifier WHERE Identifier IS NOT NULL");
            SchemaBuilder.ExecuteSql(
                $"UPDATE {answerTableName} SET GUIdentifier = Identifier WHERE Identifier IS NOT NULL");
            // 3. Drop old column
            SchemaBuilder.AlterTable("QuestionRecord", t => t.DropColumn("Identifier"));
            SchemaBuilder.AlterTable("AnswerRecord", t => t.DropColumn("Identifier"));
            return 32;
        }
    }
}