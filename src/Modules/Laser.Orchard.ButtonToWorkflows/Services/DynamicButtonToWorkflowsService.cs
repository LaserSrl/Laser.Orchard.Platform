using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.ButtonToWorkflows.Services {

    public interface IDynamicButtonToWorkflowsService : IDependency {
        IList<DynamicButtonToWorkflowsRecord> GetButtons();
        Permission GetButtonPermission(string buttonName);
        void UpdateButtons(IEnumerable<DynamicButtonToWorkflowsEdit> buttons);
    }

    public class DynamicButtonToWorkflowsService : IDynamicButtonToWorkflowsService {

        private readonly IRepository<DynamicButtonToWorkflowsRecord> _dynButtonRecordRepository;

        private readonly string Prefix = "Laser.DynamicButtonToWorkflows.Permissions";

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsService(IRepository<DynamicButtonToWorkflowsRecord> dynButtonRecordRepository) {
            _dynButtonRecordRepository = dynButtonRecordRepository;
            T = NullLocalizer.Instance;
        }

        public IList<DynamicButtonToWorkflowsRecord> GetButtons() {
            return _dynButtonRecordRepository.Table.ToList();
        }

        public Permission GetButtonPermission(string buttonName) {
            return new Permission {
                Description = T("{0} Dynamic Button Permission", buttonName).ToString(),
                Name = Prefix + "/" + buttonName,
                Category = T("Dynamic Button to Workflow Feature").ToString()
            };
        }

        public void UpdateButtons(IEnumerable<DynamicButtonToWorkflowsEdit> buttons) {
            foreach (var buttonData in buttons) {
                DynamicButtonToWorkflowsRecord button = _dynButtonRecordRepository.Get(buttonData.Id);

                if (buttonData.Delete) {
                    if (button != null)
                        _dynButtonRecordRepository.Delete(_dynButtonRecordRepository.Get(buttonData.Id));
                }
                else {
                    if (button == null)
                        _dynButtonRecordRepository.Create(new DynamicButtonToWorkflowsRecord {
                            ButtonName = buttonData.ButtonName,
                            ButtonText = buttonData.ButtonText,
                            ButtonDescription = buttonData.ButtonDescription,
                            ButtonMessage = buttonData.ButtonMessage,
                            ButtonAsync = buttonData.ButtonAsync,
                            GlobalIdentifier = string.IsNullOrWhiteSpace(buttonData.GlobalIdentifier) ? Guid.NewGuid().ToString() : buttonData.GlobalIdentifier
                        });
                    else {
                        button.ButtonName = buttonData.ButtonName;
                        button.ButtonText = buttonData.ButtonText;
                        button.ButtonDescription = buttonData.ButtonDescription;
                        button.ButtonMessage = buttonData.ButtonMessage;
                        button.ButtonAsync = buttonData.ButtonAsync;

                        if (string.IsNullOrWhiteSpace(button.GlobalIdentifier) && !string.IsNullOrWhiteSpace(buttonData.GlobalIdentifier))
                            button.GlobalIdentifier = buttonData.GlobalIdentifier;

                        _dynButtonRecordRepository.Update(button);
                    }
                }
            }

            _dynButtonRecordRepository.Flush();
        }
    }
}