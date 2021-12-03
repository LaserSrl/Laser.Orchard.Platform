using Laser.Orchard.StartupConfig.AuditTrail.Extensions;
using Orchard;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.AuditTrail.Services {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class TrailLogTrimmingBackgroundTask : Component, IBackgroundTask {
        private readonly ISiteService _siteService;
        private readonly IClock _clock;
        private readonly IAppDataFolder _appDataFolder;

        public TrailLogTrimmingBackgroundTask(
            ISiteService siteService,
            IClock clock,
            IAppDataFolder appDataFolder) {

            _siteService = siteService;
            _clock = clock;
            _appDataFolder = appDataFolder;
        }

        public void Sweep() {
            Logger.Debug("Beginning sweep for TrailLogTrimmingBackgroundTask.");
            try {
                // This task should look for audit trail log files older than a timespan
                // and delete them if any are found.
                if (GetIsTimeToTrim()) {
                    var logFilePath = LaserAuditTrailHelper.GetLogsFilePath();
                    var logFileExtension = LaserAuditTrailHelper.GetAppenderFileExtension();
                    var fileNameBeginning = Path.Combine(
                        logFilePath,
                        LaserAuditTrailHelper.GetAppenderFileName(
                                GetSiteSettings().SiteName))
                        .Replace(Path.DirectorySeparatorChar, '/');
                    var myLogs = _appDataFolder
                        .ListFiles(logFilePath)
                        .Where(fn => fn.StartsWith(fileNameBeginning));
                    // TODO: get the logs older than the specific timespan and delete them
                }
            } catch (Exception ex) {
                Logger.Error(ex, "Error during sweep for TrailLogTrimmingBackgroundTask.");
            } finally {
                Logger.Debug("Ending sweep for TrailLogTrimmingBackgroundTask.");
            }
        }
        
        private bool GetIsTimeToTrim() {
            return true;
            var lastRun = GetAuditTrailSettings().LastRunUtc ?? DateTime.MinValue;
            var now = _clock.UtcNow;
            var interval = TimeSpan.FromHours(GetAuditTrailSettings().MinimumRunInterval);
            return now - lastRun >= interval;
        }

        #region Memorize settings

        private ISite _siteSettings;
        private ISite GetSiteSettings() {
            if (_siteSettings == null) {
                _siteSettings = _siteService.GetSiteSettings();
            }
            return _siteSettings;
        }

        private AuditTrailTrimmingSettingsPart _auditTrailSettings;
        private AuditTrailTrimmingSettingsPart GetAuditTrailSettings() {
            if (_auditTrailSettings == null) {
                _auditTrailSettings = GetSiteSettings().As<AuditTrailTrimmingSettingsPart>();
            }
            return _auditTrailSettings;
        }
        #endregion
    }
}