using Laser.Orchard.UserProfiler.Models;
using Laser.Orchard.UserProfiler.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Tags.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace Laser.Orchard.UserProfiler.Service {

    public class UserProfilingService : IUserProfilingService {
        private readonly IRepository<UserProfilingSummaryRecord> _userProfilingSummaryRecord;
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly IOrchardServices _orchardServices;

        public UserProfilingService(
            IRepository<UserProfilingSummaryRecord> userProfilingSummaryRecord,
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices) {
            _userProfilingSummaryRecord = userProfilingSummaryRecord;
            _contentManager = contentManager;
            _workflowManager = workflowManager;
            _orchardServices = orchardServices;
        }

        public Dictionary<string, int> UpdateUserProfile(int UserId, List<ProfileVM> update) {
            var dicSUM = new Dictionary<string, int>();
            foreach (var el in update) {
                TextSourceTypeOptions sourcetype = (TextSourceTypeOptions)Enum.Parse(typeof(TextSourceTypeOptions), el.Type);
                var dicout = UpdateUserProfile(UserId, el.Text, sourcetype, el.Count);
                if (dicSUM.ContainsKey(dicout.Keys.First()))
                    dicSUM[dicout.Keys.First()] = dicout[dicout.Keys.First()];
                else
                    dicSUM.Add(dicout.Keys.First(), dicout[dicout.Keys.First()]);
            }
            return dicSUM;
        }

        private Dictionary<string, int> ProfileTagPart(int UserId, TagsPart tagPart) {
            var dicSUM = new Dictionary<string, int>();
            if (tagPart != null) {
                var listTags = tagPart.CurrentTags;
                foreach (var tag in listTags) {
                    var dicout = UpdateUserProfile(UserId, tag, TextSourceTypeOptions.Tag, 1);
                    if (dicSUM.ContainsKey(tag))
                        dicSUM[tag] = dicout[tag];
                    else
                        dicSUM.Add(tag, dicout[tag]);
                }
            }
            return dicSUM;
        }

        public Dictionary<string, int> UpdateUserProfile(int UserId, int id) {
            return FlowUpdateUserProfile(UserId, id, true);
        }

        public Dictionary<string, int> UpdateUserProfile(int UserId, List<int> ids) {
            var totaloutput = new Dictionary<string, int>();
            foreach (var id in ids) {
                var partial = FlowUpdateUserProfile(UserId, id, false);
                foreach (var key in partial.Keys) {
                    if (totaloutput.ContainsKey(key))
                        totaloutput[key] = partial[key];
                    else
                        totaloutput.Add(key, partial[key]);
                }
            }
            RefreshTotal(UserId);
            return totaloutput;
        }

        private Dictionary<string, int> FlowUpdateUserProfile(int UserId, int id, bool refreshTotal = true) {
            var dicSUM = new Dictionary<string, int>();
            if (id > 0) {
                var content = _contentManager.Get(id);
                if (content != null && content.As<TrackingPart>() != null) {
                    dicSUM = ProfileTagPart(UserId, content.As<TagsPart>());
                    var responseDictionary = UpdateUserProfile(UserId, id.ToString(), TextSourceTypeOptions.ContentItem, 1);
                    dicSUM.Add(responseDictionary.Keys.FirstOrDefault(), responseDictionary[responseDictionary.Keys.FirstOrDefault()]);
                }
            }
            if (refreshTotal)
                RefreshTotal(UserId);
            return dicSUM;
        }

        public Dictionary<string, int> UpdateUserProfile(int UserId, string text, TextSourceTypeOptions sourceType, int count) {
            var item = _userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id.Equals(UserId) && x.Text.Equals(text) && x.SourceType == sourceType).FirstOrDefault();

            if (item == null) {
                var userProfilingPartRecord = ((dynamic)_contentManager.Get(UserId)).UserProfilingPart.Record;
                item = new UserProfilingSummaryRecord() {
                    SourceType = sourceType,
                    Text = text,
                    Count = count,
                    UserProfilingPartRecord = userProfilingPartRecord
                };
                if (sourceType == TextSourceTypeOptions.ContentItem) {
                    try {
                        var ci = _contentManager.Get(Convert.ToInt32(text));
                        item.Data = string.Format("{{'ContentType':'{0}','Alias':'{1}'}}", ci.ContentType.Replace("'", "''"), ((dynamic)ci).AutoroutePart.DisplayAlias.Replace("'", "''"));
                    }
                    catch { }
                }
                _userProfilingSummaryRecord.Create(item);
            }
            else {
                item.Count += count;
                _userProfilingSummaryRecord.Update(item);
            }
            _userProfilingSummaryRecord.Flush();
            StartWorkflow(UserId, text, sourceType, item.Count);
            var data = new Dictionary<string, int>();
            data.Add(text, item.Count);
            return data;
        }

        private void StartWorkflow(int UserId, string text, TextSourceTypeOptions sourceType, int count) {
            var contentItem = _contentManager.Get(UserId);
            var contact = _contentManager.Query().ForType("CommunicationContact").List().Where(x => ((dynamic)x).CommunicationContactPart.UserIdentifier == UserId).FirstOrDefault();            //("CommunicationContact").Where(x => x.UserIdentifier_Id == UserId).List().FirstOrDefault();
            _workflowManager.TriggerEvent("UserTracking", contact, () => new Dictionary<string, object> {
                        { "contact", contact },
                        { "text", text },
                        { "sourceType", sourceType },
                        { "count", count},
                         { "UserId", UserId}
                    });
        }

        public void RefreshTotal(int UserId) {
            var contentItem = _contentManager.Get(UserId);
            contentItem.As<UserProfilingPart>().ListJson = GetJson(UserId);
        }

        private string GetJson(int UserId) {
            var ModuleSettings = _orchardServices.WorkContext.CurrentSite.As<UserProfilingSettingPart>();
            int count = ModuleSettings.Range;
            int countContentItem = ModuleSettings.RangeContentItem;
            StringBuilder builder = new StringBuilder();
            var types = Enum.GetValues(typeof(TextSourceTypeOptions)).Cast<TextSourceTypeOptions>();
            var tot = _userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id == -1);
            int usedcount = 0;
            foreach (var type in types) {
                if (type == TextSourceTypeOptions.ContentItem)
                    usedcount = countContentItem;
                else
                    usedcount = count;
                var pp = _userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id.Equals(UserId) && x.SourceType == type).OrderByDescending(y => y.Count).Take(usedcount).ToList();
                tot = tot.Union(_userProfilingSummaryRecord.Fetch(x => x.UserProfilingPartRecord.Id.Equals(UserId) && x.SourceType == type).OrderByDescending(y => y.Count).Take(usedcount));
            }
            var list = tot.ToList();
            builder.Append("{");
            // string jsonTotale = "{";
            foreach (var type in types) {
                var partiallist = tot.Where(x => x.SourceType == type);
                var vm = new List<dynamic>();
                string jsonvalue = "";
                foreach (var tag in partiallist) {
                    jsonvalue += "{'Count':" + tag.Count + ",'Text':'" + tag.Text.Replace("'", "''") + "'";
                    if (tag.Data != "")
                        jsonvalue += ",'Profile" + type.ToString() + "Data':" + tag.Data + "},";
                    else
                        jsonvalue += "},";
                }
                if (jsonvalue != "")
                    builder.Append("'Profil" + type.ToString() + "':{ DetProfil" + type.ToString() + ":[" + jsonvalue.Substring(0, jsonvalue.Length - 1) + "]},");
                //jsonTotale += "'Profil_" + type.ToString() + "':{ Det_Profil_" + type.ToString() + ":[" + jsonvalue.Substring(0, jsonvalue.Length - 1) + "]},";
            }

            string jsonTotale = builder.ToString();

            if (jsonTotale != "{")
                jsonTotale = jsonTotale.Substring(0, jsonTotale.Length - 1);
            jsonTotale += "}";
            return jsonTotale;
        }

        public object GetList(int UserId) {
            var contentItem = _contentManager.Get(UserId);
            // return Json.Decode(GetJson(UserId));
            if (contentItem == null)
                return null;
            var output = contentItem.As<UserProfilingPart>().ListJson;
            if (string.IsNullOrEmpty(output))
                return null;
            return Json.Decode(output);// = GetJson(UserId);
        }
    }
}