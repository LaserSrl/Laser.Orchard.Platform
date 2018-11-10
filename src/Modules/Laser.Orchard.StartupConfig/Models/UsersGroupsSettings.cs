using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Orchard;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsSettings {
        //   public virtual SettingsRecord Settings { get; set; }
        public virtual List<ExtendedUsersGroupsRecord> ExtendedUsersGroupsList { get; set; }
    }
     [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class ExtendedUsersGroupsRecord {
        public virtual int Id { get; set; }
        public virtual string GroupName { get; set; }
    }

    //public class SettingsRecord {
    //    public SettingsRecord() {
    //        Id = 0;
    //        ShowLabel = true;
    //        ShowOnlyPertinentCultures = false;
    //    }
    //    public virtual int Id { get; set; }
    //    public virtual bool ShowLabel { get; set; }
    //    public virtual bool ShowOnlyPertinentCultures { get; set; }

    //   }

    //    public List<Groupdata> GroupSerialized {
    //        get {
    //            List<Groupdata> _thelist = new List<Groupdata>();
    //            try {
    //                string _transform = Record.GroupSerialized; // this.Retrieve(r => r.GroupSerialized);
    //                _thelist = (List<Groupdata>)JsonConvert.DeserializeObject(_transform, _thelist.GetType());
    //            }
    //            catch { }
    //            return _thelist;

    //        }
    //        set {
    //            List<Groupdata> _thelist = value;

    //           _thelist = _thelist.Where(m => m.Delete != true).ToList();

    //            for (Int32 i = 0; i < _thelist.Count(); i++) {
    //                 if (_thelist[i].Number == 0  ){
    //                    if (string.IsNullOrEmpty(_thelist[i].GroupName))
    //                        _thelist[i].Delete = true;
    //                    else
    //                        _thelist[i].Number = _thelist.Max(r => r.Number) + 1;
    //                }
    //            }
    //            _thelist = _thelist.Where(m => m.Delete != true).ToList();
    //            string _transform = JsonConvert.SerializeObject(_thelist);
    //            Record.GroupSerialized = _transform;
    //            // this.Store(r => r.GroupSerialized, _transform); 
    //        }
    //    }
    //}
    // [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    //public class UsersGroupsSettingsPartRecord : ContentPartRecord {
    //    public virtual string GroupSerialized { get; set; }
    //}
    // [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    //public class Groupdata {
    //    public int Number { get; set; }
    //    public string GroupName { get; set; }
    //    public bool Delete { get; set; }
    //}
}