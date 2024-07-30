using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laser.Orchard.Questionnaires {
    public class Permissions : IPermissionProvider {
        public static readonly Permission SubmitQuestionnaire = new Permission {
            Description = "Submit questionnaire",
            Name = "SubmitQuestionnaire"
        };
        public static readonly Permission AccessStatistics = new Permission {
            Description = "Access questionnaire statistics",
            Name = "AccessStatistics"
        };
        public static readonly Permission ExportStatistics = new Permission {
            Description = "Export questionnaire statistics",
            Name = "ExportStatistics",
            ImpliedBy = new[] { AccessStatistics }
        };
        public static readonly Permission GameRanking = new Permission {
            Description = "View game rankings",
            Name = "GameRanking"
        };
        public static readonly Permission AccessExportQuestionnairesStatistics = new Permission {
            Description = "Access questionnaire statistics export files",
            Name = "AccessExportQuestionnairesStatistics"
        };
        public static readonly Permission ManageAccessToSpecificQuestionnaireStatistics = new Permission {
            Description = "Manage access to the statistics of specific questionnaires",
            Name = "ManageAccessToSpecificQuestionnaireStatistics"
        };
        public static readonly Permission AccessSpecificQuestionnaireStatistics = new Permission {
            Description = "Access to the statistics of specific questionnaire",
            Name = "AccessSpecificQuestionnaireStatistics"
        };
        public static readonly Permission ExportSpecificQuestionnaireStatistics = new Permission {
            Description = "Export specific questionnaire statistics",
            Name = "ExportSpecificQuestionnaireStatistics"
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                SubmitQuestionnaire,
                AccessStatistics,
                ExportStatistics,
                GameRanking,
                AccessExportQuestionnairesStatistics,
                ManageAccessToSpecificQuestionnaireStatistics,
                AccessSpecificQuestionnaireStatistics,
                ExportSpecificQuestionnaireStatistics
            };
        }

        /// <summary>
        /// Property used to ensure a proper permission is used when requesting access to a specific questionnaire
        /// </summary>
        public static readonly IEnumerable<Permission> SpecificQuestionnairePermissions = new[] {
                AccessSpecificQuestionnaireStatistics,
                ExportSpecificQuestionnaireStatistics
            };


        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {SubmitQuestionnaire}
                },
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {AccessStatistics, GameRanking, ManageAccessToSpecificQuestionnaireStatistics } //
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {AccessStatistics, GameRanking }
                }
            };
        }
    }
}



