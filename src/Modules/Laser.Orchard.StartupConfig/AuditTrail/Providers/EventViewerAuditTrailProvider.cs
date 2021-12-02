using Laser.Orchard.StartupConfig.AuditTrail.Models;
using Orchard;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Roles.Models;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace Laser.Orchard.StartupConfig.AuditTrail.Providers {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class EventViewerAuditTrailProvider : AuditTrailEventHandlerBase {

        private readonly IEventDataSerializer _serializer;
        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _workContextAccessor;
        
        public EventViewerAuditTrailProvider(
            IEventDataSerializer serializer,
            ISiteService siteService,
            IWorkContextAccessor workContextAccessor) {

            _serializer = serializer;
            _siteService = siteService;
            _workContextAccessor = workContextAccessor;

            var settingsPart = GetOutputSettingsPart();

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
            _isSourceEnabled = settingsPart.IsEventViewerEnabled && EventLog.SourceExists(_sourceName);
        }

        private string _sourceName;
        private bool _isSourceEnabled;

        public override void Create(AuditTrailCreateContext context) {
            if (_isSourceEnabled) {
                // prep log string
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

                using (EventLog eventLog = new EventLog("Application")) {
                    eventLog.Source = _sourceName;

                    eventLog.WriteEntry(
                        eventMessageBuilder.ToString(), // Event Message
                        EventLogEntryType.Information//, // Event Type
                        //4738, // Event ID
                        //1 // Event Category
                        );
                }
            }
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
        private AuditTrailSettingsPart _auditTrailSettings;
        private AuditTrailSettingsPart GetAuditTrailSettings() {
            if (_auditTrailSettings == null) {
                _auditTrailSettings = _siteService.GetSiteSettings().As<AuditTrailSettingsPart>();
            }
            return _auditTrailSettings;
        }
        private AuditTrailOutputSettingsPart _auditTrailOutputSettingsPart;
        private AuditTrailOutputSettingsPart GetOutputSettingsPart() {
            if (_auditTrailOutputSettingsPart == null) {
                _auditTrailOutputSettingsPart = _siteService.GetSiteSettings().As<AuditTrailOutputSettingsPart>();
            }
            return _auditTrailOutputSettingsPart;
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
    }
}