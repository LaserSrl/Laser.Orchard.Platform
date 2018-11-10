using Laser.Orchard.ButtonToWorkflows.Services;
using Laser.Orchard.ButtonToWorkflows.ViewModels;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Workflows.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.ButtonToWorkflows {
    public class Migrations : DataMigrationImpl {
        IContentDefinitionManager _contentDefinitionManager;
        IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;
        IRepository<ActivityRecord> _activityRepository;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService, IRepository<ActivityRecord> activityRepository) {
            _activityRepository = activityRepository;
            _contentDefinitionManager = contentDefinitionManager;
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
        }

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ButtonToWorkflowsPart", part => part.Attachable());
            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .ContentPartRecord()
                        .Column<string>("ButtonsText")
                        .Column<string>("ButtonsAction")
         );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .AddColumn<string>("ButtonsDescription")

         );
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .AddColumn<string>("ButtonsMessage")

         );
            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .AddColumn<string>("ButtonsAsync")
         );
            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.CreateTable("DynamicButtonToWorkflowsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ButtonName")
                .Column<string>("ButtonText")
                .Column<string>("ButtonDescription")
                .Column<string>("ButtonMessage")
                .Column<bool>("ButtonAsync")
            );

            ContentDefinitionManager.AlterPartDefinition("DynamicButtonToWorkflowsPart",
                part => part.Attachable()
                            .WithDescription("Allows to associate dynamically created buttons that are able to trigger workflows."));

            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("DynamicButtonToWorkflowsRecord", table => table
                .AddColumn<string>("GlobalIdentifier")
            );

            var buttonsWithoutGuid = _dynamicButtonToWorkflowsService.GetButtons().Where(w => String.IsNullOrWhiteSpace(w.GlobalIdentifier));

            if (buttonsWithoutGuid.Count() > 0) {
                List<DynamicButtonToWorkflowsEdit> buttonList = new List<DynamicButtonToWorkflowsEdit>();
                var activities = _activityRepository.Table.Where(w => w.Name.Equals("DynamicButtonEvent"));
                var typesWithButtons = _contentDefinitionManager.ListTypeDefinitions().Where(w => w.Parts.Any(p => p.PartDefinition.Name == "DynamicButtonToWorkflowsPart")).ToList();

                foreach (var button in buttonsWithoutGuid) {
                    // Associo un Guid a tutti i bottoni già esistenti che non ne hanno uno
                    DynamicButtonToWorkflowsEdit buttonData = new DynamicButtonToWorkflowsEdit();
                    buttonData.Id = button.Id;
                    buttonData.ButtonText = button.ButtonText;
                    buttonData.ButtonAsync = button.ButtonAsync;
                    buttonData.ButtonDescription = button.ButtonDescription;
                    buttonData.ButtonMessage = button.ButtonMessage;
                    buttonData.ButtonName = button.ButtonName;
                    buttonData.GlobalIdentifier = Guid.NewGuid().ToString();
                    buttonData.Delete = false;

                    buttonList.Add(buttonData);

                    // Correggo i riferimenti ai bottoni nelle parti
                    string partSettingToEdit = string.Format("{{{0}}}", button.Id);

                    foreach (var type in typesWithButtons) {
                        var part = type.Parts.Where(w => w.PartDefinition.Name == "DynamicButtonToWorkflowsPart").FirstOrDefault();
                        if (part != null) {
                            part.Settings["DynamicButtonsSetting.Buttons"] = part.Settings["DynamicButtonsSetting.Buttons"].Replace(partSettingToEdit, string.Format("{{{0}}}", buttonData.GlobalIdentifier));
                            _contentDefinitionManager.StoreTypeDefinition(type);
                        }
                    }

                    // Correggo i riferimenti ai bottoni nelle activities dei workflows
                    string activitySettingToEdit = string.Format("\"DynamicButton\":\"{0}\"", button.Id);
                    var activitiesWithButton = activities.Where(w => w.State.Contains(activitySettingToEdit));

                    foreach (var activity in activitiesWithButton) {
                        activity.State = activity.State.Replace(activitySettingToEdit, string.Format("\"DynamicButton\":\"{0}\"", buttonData.GlobalIdentifier));
                        _activityRepository.Update(activity);
                    }
                }

                _dynamicButtonToWorkflowsService.UpdateButtons(buttonList);
                _activityRepository.Flush();
            }

            return 7;
        }
    }
}