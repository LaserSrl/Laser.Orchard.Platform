using Laser.Orchard.StartupConfig.AuditTrail.Extensions;
using Laser.Orchard.StartupConfig.AuditTrail.Models;
using log4net.Appender;
using Orchard;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.StartupConfig.AuditTrail.Providers {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class ChannelsAuditTrailProvider : AuditTrailEventHandlerBase {

        private readonly IEventDataSerializer _serializer;
        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _workContextAccessor;

        private static string _logMatchString = "##AUDIT TRAIL##";

        public ChannelsAuditTrailProvider(
            IEventDataSerializer serializer,
            ISiteService siteService,
            IWorkContextAccessor workContextAccessor) {

            _serializer = serializer;
            _siteService = siteService;
            _workContextAccessor = workContextAccessor;
            
            var settingsPart = GetOutputSettingsPart();

            // System Event logs:
            _sourceName = settingsPart.EventViewerSourceName;
            if (string.IsNullOrWhiteSpace(_sourceName)) {
                // Test the settings to see whether we should use the tenant name
                // or the default "Application" source. In case we should use the
                // tenant name, a system admin should take care to create it.
                // Creation of a new Log source for an application, from Powershell
                // >> New-EventLog -Source {sourceName} -LogName Application
                _sourceName = "Application";
            }
            // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.eventlog?view=netframework-4.8
            // Make sure the EventLog source exists.
            _isEventLogEnabled = settingsPart.IsEventViewerEnabled && EventLog.SourceExists(_sourceName);

            // File system logs:
            // Besides being enabled here from the setting, we should ensure that
            // there is the correctly configured logger and appender. We are using 
            // a "custom" logger rather than those injected normally because we need
            // have a file specific for the tenant. This in turn requires a specific
            // unique appender for each tenant, which would be hard to do from the
            // xml configuration files.
            _isFileSystemLogEnabled = settingsPart.IsFileSystemEnabled;
            if (_isFileSystemLogEnabled) {
                // Ensure the logger for audit trail exists.
                EnsureLogger();
                // Ensure the logger for audit trail is associated with the correct appender.
                EnsureAppender();
            }
        }
        
        private string _sourceName;
        private bool _isEventLogEnabled;
        private bool _isFileSystemLogEnabled;
        private log4net.ILog _logger;

        public override void Create(AuditTrailCreateContext context) {
            if (_isEventLogEnabled || _isFileSystemLogEnabled) {
                // prep log string
                var message = ComputeMessage(context);

                if (_isEventLogEnabled) {
                    using (EventLog eventLog = new EventLog("Application")) {
                        eventLog.Source = _sourceName;

                        eventLog.WriteEntry(
                            message, // Event Message
                            EventLogEntryType.Information//, // Event Type
                            //4738, // Event ID
                            //1 // Event Category
                            );
                    }
                }
                if (_isFileSystemLogEnabled) {
                    _logger.Info(_logMatchString + Environment.NewLine + message);
                }
            }
        }

        #region Stuff for the message
        private string ComputeMessage(AuditTrailCreateContext context) {
            var eventMessageBuilder = new StringBuilder();
            eventMessageBuilder.AppendLine(
                $"Event: {context.Event}");
            eventMessageBuilder.AppendLine(
                $"Category: {context.EventDescriptor.CategoryDescriptor.Category}");
            if (context.User != null) {
                eventMessageBuilder.AppendLine(
                    $"UserName: {context.User.UserName}");
                var userRoles = context.User.As<IUserRoles>()?.Roles.ToList();
                if (userRoles != null) {
                    eventMessageBuilder.AppendLine(
                        $"UserRoles: {string.Join(", ", userRoles)}");
                }
            }
            eventMessageBuilder.AppendLine(
                $"Description: {context.EventDescriptor.Description.Text}");
            var clientAddress = GetClientAddress();
            if (!string.IsNullOrWhiteSpace(clientAddress)) {
                eventMessageBuilder.AppendLine(
                        $"ClientAddress: {clientAddress}");
            }
            var userAgent = GetUserAgent();
            if (!string.IsNullOrWhiteSpace(userAgent)) {
                eventMessageBuilder.AppendLine(
                        $"UserAgent:");
                eventMessageBuilder.AppendLine(
                        $"{userAgent}");
            }
            eventMessageBuilder.AppendLine(
                    $"EventData:");
            eventMessageBuilder.AppendLine(
                    $"{_serializer.Serialize(context.EventData)}");

            return eventMessageBuilder.ToString();
        }

        private HttpRequestBase _httpRequest;
        private HttpRequestBase GetHttpRequest() {
            if (_httpRequest == null) {
                var workContext = _workContextAccessor.GetContext();
                if (workContext == null || workContext.HttpContext == null)
                    return null;
                _httpRequest = workContext.HttpContext.Request;
            }
            return _httpRequest;
        }

        private string GetClientAddress() {

            var settings = GetAuditTrailSettings();

            if (!settings.EnableClientIpAddressLogging)
                return null;
            
            var request = GetHttpRequest();

            return $"Host: {request.UserHostName} IP: {request.UserHostAddress} Port: {request.ServerVariables["REMOTE_PORT"]}";
        }

        private string GetUserAgent() {
            var request = GetHttpRequest();
            return request.UserAgent;
        }
        #endregion

        #region stuff for log4net
        private log4net.ILog EnsureLogger() {
            // LogManager.GetLogger will create a new logger if it doesn't find
            // one, so it always returns something. For this reason, we always have
            // to do the settings below.
            _logger = log4net.LogManager.GetLogger(GetLoggerName());
            var logger = (log4net.Repository.Hierarchy.Logger)_logger.Logger;
            // This enables the INFO log level. This matches the fact that we'll
            // call _logger.Info(string).
            logger.Level = logger.Hierarchy.LevelMap["INFO"];
            // Prevent stuff added to this logger from bubbling up to parent loggers.
            // This way, the audit trail information will not appear in orchard's
            // default debug log.
            logger.Additivity = false;
            return _logger;
        }

        private IAppender EnsureAppender() {
            var logger = (log4net.Repository.Hierarchy.Logger)_logger.Logger;
            var appender = logger.GetAppender(GetAppenderName());
            if (appender == null) {
                appender = CreateSiteAppender();
                logger.AddAppender(appender);
            }
            return appender;
        }

        private IAppender CreateSiteAppender() {
            OrchardFileAppender appender = new OrchardFileAppender();
            appender.Name = GetAppenderName();
            appender.File = GetAppenderFileName();
            appender.AppendToFile = true;
            // Allow extended character sets
            appender.Encoding = Encoding.UTF8;
            // Immediately flush on error to avoid data loss
            appender.ImmediateFlush = true;
            // Filename will also depend on date
            appender.StaticLogFileName = false;
            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
            appender.DatePattern = $"{LaserAuditTrailHelper.GetAppenderDatePattern()}'.{LaserAuditTrailHelper.GetAppenderFileExtension()}'";
            // Prevent Orchard from displaying locking debug messages
            appender.LockingModel = new FileAppender.MinimalLock();
            // Filters
            var stringMatchFilter = new log4net.Filter.StringMatchFilter();
            stringMatchFilter.StringToMatch = _logMatchString;
            appender.AddFilter(stringMatchFilter);
            appender.AddFilter(new log4net.Filter.DenyAllFilter());
            // Log Layout
            var layout = new log4net.Layout.PatternLayout(
                @"%date %logger - %P{Tenant} - %level% [ExecutionId=%P{ExecutionId}]%newline[%P{Url}]%newline%message%newline "
                );
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }

        private string GetLoggerName() {
            return LaserAuditTrailHelper.GetLoggerName(GetSiteSettings().SiteName);
        }

        private string GetAppenderName() {
            return LaserAuditTrailHelper.GetAppenderName(GetSiteSettings().SiteName);
        }

        private string GetAppenderFileName() {
            return Path.Combine(
                LaserAuditTrailHelper.GetAppenderFilePath(),
                LaserAuditTrailHelper.GetAppenderFileName(GetSiteSettings().SiteName));
        }
        #endregion

        #region Settings memorization
        private ISite _siteSettings;
        private ISite GetSiteSettings() {
            if (_siteSettings == null) {
                _siteSettings = _siteService.GetSiteSettings();
            }
            return _siteSettings;
        }

        private AuditTrailSettingsPart _auditTrailSettings;
        private AuditTrailSettingsPart GetAuditTrailSettings() {
            if (_auditTrailSettings == null) {
                _auditTrailSettings = GetSiteSettings().As<AuditTrailSettingsPart>();
            }
            return _auditTrailSettings;
        }

        private AuditTrailOutputSettingsPart _auditTrailOutputSettingsPart;
        private AuditTrailOutputSettingsPart GetOutputSettingsPart() {
            if (_auditTrailOutputSettingsPart == null) {
                _auditTrailOutputSettingsPart = GetSiteSettings().As<AuditTrailOutputSettingsPart>();
            }
            return _auditTrailOutputSettingsPart;
        }
        #endregion
    }
}