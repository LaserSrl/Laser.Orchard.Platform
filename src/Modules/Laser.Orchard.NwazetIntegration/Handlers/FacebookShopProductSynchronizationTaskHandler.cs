using Laser.Orchard.NwazetIntegration.Services.FacebookShop;
using Orchard.Environment.Extensions;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductSynchronizationTaskHandler : IScheduledTaskHandler {
        public static string TASK_NAME = "FacebookShopProductSynchronization";

        private readonly FacebookShopService _facebookShopService;

        public FacebookShopProductSynchronizationTaskHandler(FacebookShopService facebookShopService) {
            _facebookShopService = facebookShopService;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType.Equals(TASK_NAME, StringComparison.InvariantCultureIgnoreCase)) {
                _facebookShopService.SyncProducts();
            }
        }
    }
}