using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.CommunicationGateway.Utils;
using Orchard;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Laser.Orchard.CommunicationGateway.Handlers {
    public class ImportContactTaskHandler : IScheduledTaskHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly ICommunicationService _communicationService;
        private readonly ITransactionManager _transactionManager;
        private string _contactsImportRelativePath;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public const string TaskType = "Laser.Orchard.CommunicationGateway.ImportContact.Task";
        public ImportContactTaskHandler(IOrchardServices orchardServices, ShellSettings shellSettings, ICommunicationService communicationService, ITransactionManager transactionManager) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _communicationService = communicationService;
            _transactionManager = transactionManager;
            Logger = NullLogger.Instance;
            _contactsImportRelativePath = string.Format("~/App_Data/Sites/{0}/Import/Contacts", _shellSettings.Name);
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }
            try {
                string path = HostingEnvironment.MapPath(_contactsImportRelativePath);
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists == false) {
                    dir.Create();
                }
                FileInfo logFile = null;
                foreach (var file in dir.GetFiles("*.csv", SearchOption.TopDirectoryOnly)) {
                    try {
                        // avvia una nuova transazione per ogni file da elaborare
                        _transactionManager.RequireNew();

                        // importa solo i csv che non hanno già un log corrispondente
                        logFile = new FileInfo(file.FullName + ".log");
                        if (logFile.Exists == false) {
                            byte[] bufferContent = File.ReadAllBytes(file.FullName);
                            string fileContent = Encoding.Unicode.GetString(bufferContent);
                            ImportUtil import = new ImportUtil(_orchardServices);
                            import.ImportCsv(fileContent);
                            string result = string.Format("Import result: Errors: {0}, Mails: {1}, Sms: {2}.", import.Errors.Count, import.TotMail, import.TotSms);
                            string strErrors = FormatErrors(import.Errors);
                            byte[] buffer = Encoding.Unicode.GetBytes(string.Format("{0}{1}{2}", result, Environment.NewLine, strErrors));
                            File.WriteAllBytes(string.Format("{0}{1}{2}.log", dir.FullName, Path.DirectorySeparatorChar, file.Name),
                                buffer);
                        }
                    } finally {
                        try {
                            // sposta il file in una sottocartella
                            DirectoryInfo archiveDir = new DirectoryInfo(dir.FullName + Path.DirectorySeparatorChar + "history");
                            if (archiveDir.Exists == false) {
                                archiveDir.Create();
                            }
                            string archivePath = archiveDir.FullName + Path.DirectorySeparatorChar + file.Name;
                            file.MoveTo(archivePath);
                        } catch {
                            // se non riesce a spostare il file, lo elimina per non dare fastidio agli import successivi
                            file.Delete();
                        }
                    }
                }
            } catch (Exception ex) {
                string idcontenuto = "nessun id ";
                try {
                    idcontenuto = context.Task.ContentItem.Id.ToString();
                } catch (Exception ex2) { Logger.Error(ex2, ex2.Message); }
                Logger.Error(ex, "Error on " + TaskType + " for ContentItem id = " + idcontenuto + " : " + ex.Message);
            }
        }

        private string FormatErrors(List<string> errors) {
            StringBuilder sb = new StringBuilder();
            foreach (var line in errors) {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}