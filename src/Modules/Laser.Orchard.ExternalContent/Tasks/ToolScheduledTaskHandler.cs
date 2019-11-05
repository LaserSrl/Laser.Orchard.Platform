using Orchard.Mvc.Html;
using Laser.Orchard.ExternalContent.Fields;
using Laser.Orchard.ExternalContent.Services;
using Laser.Orchard.ExternalContent.Settings;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using Orchard.Mvc;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.ExternalContent.Tasks {
    public class ToolScheduledTaskHandler : IScheduledTaskHandler {

        private const string TaskType = "FieldExternalTask";
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly ShellSettings _shellSettings;
        private readonly IFieldExternalService _fieldExternalService;
        private readonly IWorkContextAccessor _workContextAccessor;


        public ILogger Logger { get; set; }

        public ToolScheduledTaskHandler(
            IOrchardServices orchardServices,
            IScheduledTaskManager scheduledTaskManager,
            ShellSettings shellSettings,
            IFieldExternalService fieldExternalService,
            IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            _orchardServices = orchardServices;
            _scheduledTaskManager = scheduledTaskManager;
            _fieldExternalService = fieldExternalService;
            Logger = NullLogger.Instance;
            _shellSettings = shellSettings;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == TaskType) {
                string CallUrl = "";
                try {
                    Logger.Information("ExternalContent task item #{0} version {1} scheduled at {2} utc",
                         context.Task.ContentItem.Id,
                         context.Task.ContentItem.Version,
                         context.Task.ScheduledUtc);
                    var displayalias = context.Task.ContentItem.As<AutoroutePart>().DisplayAlias;

                    // Calculate the hostname to call based on settings of fields (if defined an InternalHostNameForScheduledTask) or the base url of the tenant
                    var baseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                    var baseUrlFromSettings = context.Task.ContentItem.Parts.SelectMany(x => x.Fields.Where(f =>
                        f.FieldDefinition.Name == typeof(FieldExternal).Name &&
                        f.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>() != null &&
                        !string.IsNullOrWhiteSpace(f.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>().InternalHostNameForScheduledTask)))
                        .Select(p => p.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>().InternalHostNameForScheduledTask)
                        .FirstOrDefault();
                    CallUrl = baseUrl;
                    if (baseUrlFromSettings != null) {
                        CallUrl = baseUrlFromSettings;
                    }
                    //var host = _shellSettings.RequestUrlHost;
                    var prefix = _shellSettings.RequestUrlPrefix;
                    //if (!string.IsNullOrEmpty(host))
                    //    CallUrl += host;
                    if (!string.IsNullOrEmpty(prefix) && prefix.ToLower() != "default")
                        CallUrl += "/" + prefix;
                    CallUrl += @"/Webservices/Alias?displayalias=" + displayalias;


                    // var urlHelper = new UrlHelper(wc.HttpContext.Request.RequestContext);
                    //      var CallUrl = urlHelper.ItemDisplayUrl(context.Task.ContentItem);
                    // HostingEnvironment.MapPath("~/") + _shellSettings.Name + "\\Webservices\\Alias?displayalias=" + displayalias;
                    WebClient myClient = new WebClient();

                    IApiKeyService apiKeyService = null;
                    if (_orchardServices.WorkContext.TryResolve<IApiKeyService>(out apiKeyService)) {
                        var iv = GetRandomIV();
                        var key = apiKeyService.GetValidApiKey(iv, true);
                        // protezione attiva inserisco header
                        myClient.Headers.Set("ApiKey", key);
                        myClient.Headers.Set("AKIV", iv);
                    }
                    Stream response = myClient.OpenRead(CallUrl);
                    response.Close();
                }
                catch (Exception e) {
                    Logger.Error(e, "Error during webclient call to this URL: "+ CallUrl+ "\r\nMessage: " + e.Message);
                }
                finally {
                    try {
                        var fields = context.Task.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(FieldExternal).Name)).Cast<FieldExternal>();
                        Int32 minuti = 0;
                        foreach (var field in fields) {
                            var settings = field.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>();
                            if (settings.ScheduledMinute > 0) {
                                minuti = settings.ScheduledMinute;
                            }
                        }
                        if (minuti > 0)
                            _fieldExternalService.ScheduleNextTask(minuti, context.Task.ContentItem);
                    }
                    catch (Exception e) {
                        Logger.Error(e, e.Message);
                    }
                }
            }
        }
        private string GetRandomIV() {
            string iv = string.Format("{0}{0}", DateTime.UtcNow.ToString("ddMMyyyy").Substring(0, 8));
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(iv);
            return Convert.ToBase64String(arr);
        }
        //private void ScheduleNextTask(Int32 minute, ContentItem ci) {
        //    if (minute > 0) {
        //        DateTime date = DateTime.UtcNow.AddMinutes(minute);
        //        _scheduledTaskManager.CreateTask(TaskType, date, ci);
        //    }
        //}
    }
}