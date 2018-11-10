using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class ExportContactTaskHandler : IScheduledTaskHandler {

        private readonly IExportContactService _exportContactService;
        private readonly ShellSettings _shellSettings;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        private const string TaskType = "Laser.Orchard.CommunicationGateway.ExportContact.Task";

        public ExportContactTaskHandler(IExportContactService exportContactService, ShellSettings shellSettings) {
            _exportContactService = exportContactService;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }

            try {
                // Parametri search
                string[] parametri = (((dynamic)context.Task.ContentItem).ExportTaskParametersPart.Parameters.Value).Split('&');
                string[] parExpression = parametri[0].Split('=');
                string[] parField = parametri[1].Split('=');
                string[] parCommUse = parametri[2].Split('=');
                string[] parThirdParty = parametri[3].Split('=');

                SearchVM search = new SearchVM();
                search.Expression = HttpUtility.UrlDecode(parExpression[1]);
                switch (HttpUtility.UrlDecode(parField[1])) {
                    case "Name":
                        search.Field = ViewModels.SearchFieldEnum.Name;
                        break;
                    case "Mail":
                        search.Field = ViewModels.SearchFieldEnum.Mail;
                        break;
                    case "Phone":
                        search.Field = ViewModels.SearchFieldEnum.Phone;
                        break;
                }

                if (!String.IsNullOrWhiteSpace(parCommUse[1]))
                    search.CommercialUseAuthorization = Convert.ToBoolean(parCommUse[1]);
                else
                    search.CommercialUseAuthorization = null;

                if (!String.IsNullOrWhiteSpace(parThirdParty[1]))
                    search.ThirdPartyAuthorization = Convert.ToBoolean(parThirdParty[1]);
                else
                    search.ThirdPartyAuthorization = null;

                IEnumerable<ContentItem> contentItems = _exportContactService.GetContactList(search);
                List<ContactExport> listaContatti = new List<ContactExport>();

                foreach (ContentItem contenuto in contentItems) {
                    // Contact Master non viene esportato
                    if (!contenuto.As<CommunicationContactPart>().Master) {
                        listaContatti.Add(_exportContactService.GetInfoContactExport(contenuto));
                    }
                }

                // Export CSV
                StringBuilder strBuilder = new StringBuilder();
                string Separator = ";";
                bool isColumnExist = false;

                foreach (ContactExport contatto in listaContatti) {

                    if (!isColumnExist) {
                        #region column
                        strBuilder.Append("Id" + Separator);
                        strBuilder.Append("TitlePart.Title" + Separator);
                        foreach (Hashtable fieldColumn in contatto.Fields) {
                            foreach (DictionaryEntry nameCol in fieldColumn) {
                                strBuilder.Append(nameCol.Key + Separator);
                            }
                        }
                        strBuilder.Append("ContactPart.Sms" + Separator);
                        strBuilder.Append("ContactPart.Email" + Separator);
                        strBuilder.Append(Environment.NewLine);
                        #endregion

                        isColumnExist = true;
                    }

                    #region row
                    strBuilder.Append(contatto.Id.ToString() + Separator);
                    strBuilder.Append(NormalizeValueForCsv(contatto.Title) + Separator);
                    foreach (Hashtable fieldRow in contatto.Fields) {
                        foreach (DictionaryEntry valueRow in fieldRow) {
                            strBuilder.Append(NormalizeValueForCsv(valueRow.Value.ToString()) + Separator);
                        }
                    }
                    strBuilder.Append(NormalizeValueForCsv(string.Join(";", contatto.Sms)) + Separator);
                    strBuilder.Append(NormalizeValueForCsv(string.Join(";", contatto.Mail)) + Separator);
                    strBuilder.Append(Environment.NewLine);
                    #endregion
                }

                // Save File
                string fileName = String.Format("contacts_{0}_{1:yyyyMMddHHmmss}.csv", _shellSettings.Name, context.Task.ScheduledUtc.Value.ToLocalTime());
                string filePath = HostingEnvironment.MapPath(string.Format("~/App_Data/Sites/{0}/Export/Contacts/{1}", _shellSettings.Name, fileName));
                string tempFileName = String.Format("__contacts_{0}_{1:yyyyMMddHHmmss}.tmp", _shellSettings.Name, context.Task.ScheduledUtc.Value.ToLocalTime());
                string tempFilePath = HostingEnvironment.MapPath(string.Format("~/App_Data/Sites/{0}/Export/Contacts/{1}", _shellSettings.Name, tempFileName));

                if (!File.Exists(filePath)) {

                    // Creo la directory
                    FileInfo fi = new FileInfo(filePath);
                    if (!fi.Directory.Exists) {
                        System.IO.Directory.CreateDirectory(fi.DirectoryName);
                    }

                    // Write File
                    byte[] buffer = Encoding.Unicode.GetBytes(strBuilder.ToString());
                    File.WriteAllBytes(filePath, buffer);
                }
                else {
                    Logger.Debug(T("File {0} already exist").Text, fileName);
                }
                if (File.Exists(tempFilePath)) {
                    File.Delete(tempFilePath);
                }
            } 
            catch (Exception ex) {
                Logger.Error(T("Export Contacts - Error Message: " + ex.Message).Text);
            }
        }

        /// <summary>
        /// Converte i valori contennti punto e virgole (;) in modo che 
        /// siano interpretati correttamente come un singolo valore nel formato CSV.
        /// In pratica racchiude questi valori tra doppi apici e raddoppia eventuali doppi apici 
        /// già presenti all'interno del valore.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string NormalizeValueForCsv(string value){
            if (value.Contains(';')) {
                return string.Format("\"{0}\"", value.Replace("\"", "\"\""));
            } else {
                return value;
            }
        }
    }
}