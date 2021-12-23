using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services.FacebookShop;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductSynchronizationTaskHandler : IScheduledTaskHandler {
        public static string SYNCPRODUCTS_TASK = "FacebookShopProductSynchronization";
        public static string CHECKHANDLE_TASK = "FacebookShopCheckHandle";

        private readonly FacebookShopService _facebookShopService;
        private readonly IRepository<FacebookShopHandleRecord> _handles;

        public FacebookShopProductSynchronizationTaskHandler(FacebookShopService facebookShopService,
            IRepository<FacebookShopHandleRecord> handles) {
            _facebookShopService = facebookShopService;
            _handles = handles;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType.Equals(SYNCPRODUCTS_TASK, StringComparison.InvariantCultureIgnoreCase)) {
                _facebookShopService.SyncProducts();
            } else if (context.Task.TaskType.StartsWith(CHECKHANDLE_TASK + "_")) {
                // Task name is something like "FacebookShopCheckHandle_RecordId"
                var recordId = context.Task.TaskType.Substring((CHECKHANDLE_TASK + "_").Length);
                int numericRecordId = 0;
                int.TryParse(recordId, out numericRecordId);
                if (numericRecordId > 0) {
                    var handleRecord = _handles.Get((int)numericRecordId);
                    _facebookShopService.CheckFacebookHandles(handleRecord.Handle, handleRecord.RequestJson);
                    handleRecord.Processed = true;
                    _handles.Update(handleRecord);
                }
            }
        }
    }
}