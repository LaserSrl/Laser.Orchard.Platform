using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Services {

    public interface IUsersGroupsSettingsService : IDependency {
        ViewModels.UsersGroupsSettingsVM ReadSettings();
        IList<ExtendedUsersGroupsRecord> GetGroups(int[] groupIds);
        List<UserPart> GetUsersByGroup(int id);
        void WriteSettings(ViewModels.UsersGroupsSettingsVM settings);
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsSettingsService : IUsersGroupsSettingsService {

        private readonly IContentManager _contentManager;
        //     private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _work;
        //   private readonly ILocalizationService _localizationService;
        private readonly IRepository<ExtendedUsersGroupsRecord> _extendedUsersGroupsRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly INotifier _notifier;

        public Localizer T { get; set; }

        public UsersGroupsSettingsService(
               IContentManager contentManager,
            IRepository<ExtendedUsersGroupsRecord> repository,
            IWorkContextAccessor work,
            IOrchardServices orchardServices,
            INotifier notifier
            ) {

            _contentManager = contentManager;
            _extendedUsersGroupsRepository = repository;
            _orchardServices = orchardServices;
            _work = work;
            T = NullLocalizer.Instance;
            _notifier = notifier;
        }
        public ViewModels.UsersGroupsSettingsVM ReadSettings() {
            var settings = new ViewModels.UsersGroupsSettingsVM();
            settings.ExtendedUsersGroupsListVM = new List<ViewModels.ExtendedUsersGroupsRecordVM>();
            var tab = _extendedUsersGroupsRepository.Table.OrderBy(o => o.GroupName).ToList();
            foreach (var record in tab) {
                ViewModels.ExtendedUsersGroupsRecordVM evm = new ViewModels.ExtendedUsersGroupsRecordVM();
                evm.Id = record.Id;
                evm.GroupName = record.GroupName;
                settings.ExtendedUsersGroupsListVM.Add(evm);
            }

            return settings;
        }

        public IList<ExtendedUsersGroupsRecord> GetGroups(int[] groupIds) {
            return _extendedUsersGroupsRepository.Table.Where(w => groupIds.Contains(w.Id)).ToList();
        }

        public List<UserPart> GetUsersByGroup(int id) {

            var usersgroups = _contentManager
                               .Query<UsersGroupsPart, UsersGroupsPartRecord>()
                        .Where(x => (!(x.theUserGroup == null || x.theUserGroup == "")))
                                    .List();
            List<UserPart> lu = new List<UserPart>();
            foreach (UsersGroupsPart up in usersgroups) {
                if (up.UserGroup.Split(',').Select(int.Parse).ToList().Contains(id)) {
                    lu.Add(up.ContentItem.As<UserPart>());
                }
            }
            return lu;

        }

        private bool TryDelete(int id) {
            if (GetUsersByGroup(id).Count() == 0)
                return true;
            else
                return false;

        }
        public void WriteSettings(ViewModels.UsersGroupsSettingsVM settings) {

            foreach (var singlerecord in settings.ExtendedUsersGroupsListVM) {
                if (singlerecord.Delete) {
                    if (!TryDelete(singlerecord.Id)) {
                        _orchardServices.Notifier.Add(NotifyType.Error, T("Group user is in use and can't be deleted"));

                    } else {
                        ExtendedUsersGroupsRecord ex = new ExtendedUsersGroupsRecord();
                        ex.Id = singlerecord.Id;
                        ex.GroupName = singlerecord.GroupName;
                        this.DeleteGroup(ex);

                    }

                } else {
                    ExtendedUsersGroupsRecord ex = new ExtendedUsersGroupsRecord();
                    ex.Id = singlerecord.Id;
                    ex.GroupName = singlerecord.GroupName;

                    this.UpdateGroup(ex);
                }
            }
        }

        private void UpdateGroup(ExtendedUsersGroupsRecord singleRecord) {

            var record = _extendedUsersGroupsRepository.Get(o => o.Id.Equals(singleRecord.Id));
            var inErrorGroups = new List<string>();
            if (record != null)
                _extendedUsersGroupsRepository.Update(singleRecord);
            else {
                if (!string.IsNullOrEmpty(singleRecord.GroupName)) {
                    var groupsToCreate = singleRecord.GroupName.Split(new string[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var groupToCreate in groupsToCreate) {
                        var groupRecordToCreate = new ExtendedUsersGroupsRecord {
                            GroupName = groupToCreate,
                        };
                        try {
                            _extendedUsersGroupsRepository.Create(groupRecordToCreate);
                            _extendedUsersGroupsRepository.Flush();
                        } catch {
                            inErrorGroups.Add(groupRecordToCreate.GroupName);
                        }
                    }
                }
            }

            if (inErrorGroups.Count() > 0) {
                _notifier.Error(T("Some groups cannot be inserted. {0}", String.Join(",", inErrorGroups)));
            }
        }
        private void DeleteGroup(ExtendedUsersGroupsRecord singleRecord) {

            var record = _extendedUsersGroupsRepository.Get(o => o.Id.Equals(singleRecord.Id));
            if (record != null) {
                _extendedUsersGroupsRepository.Delete(record);
            }
            _extendedUsersGroupsRepository.Flush();
        }


    }
}