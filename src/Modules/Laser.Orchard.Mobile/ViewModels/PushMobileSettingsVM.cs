using System.Linq;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.ViewModels {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushMobileSettingsVM {
        public string ApplePathCertificateFile { get; set; }
        public string AppleCertificatePassword { get; set; }
        public string ApplePathCertificateFileDevelopment { get; set; }
        public string AppleCertificatePasswordDevelopment { get; set; }
        public string ApplePushSound { get; set; }
        public string AndroidApiKey { get; set; }
        public string AndroidApiKeyDevelopment { get; set; }
        public string AndroidPushServiceUrl { get; set; }
        public string AndroidPushNotificationIcon { get; set; }
        public string WindowsEndPoint { get; set; }
        public string WindowsAppPackageName { get; set; }
        public string WindowsAppSecurityIdentifier { get; set; }
        public string AppleCertificateTenant { get; set; }
        public bool ShowTestOptions { get; set; }
        public string TaxonomyName { get; set; }
        public int PushSendBufferSize { get; set; }
        public bool CommitSentOnly { get; set; }
        public int DelayMinutesBeforeRetry { get; set; }
        public int MaxNumRetry { get; set; }
        public int MaxPushPerIteration { get; set; }
        public SelectList ListOfTaxonomies { get; set; }

        public SelectList ListOfCertificates {
            get {
                string[] filelist = Directory.GetFiles(HostingEnvironment.MapPath(@"~/App_Data/Sites/" + AppleCertificateTenant + @"/Mobile"));
                return new SelectList(filelist.Select(x => new SelectListItem { Text = Path.GetFileName(x), Value = Path.GetFileName(x) }), "Value", "Text", ApplePathCertificateFile);
            }
        }
        public SelectList ListOfCertificatesDevelopment {
            get {
                string[] filelistdev = Directory.GetFiles(HostingEnvironment.MapPath(@"~/App_Data/Sites/" + AppleCertificateTenant + @"/Mobile"));
                return new SelectList(filelistdev.Select(x => new SelectListItem { Text = Path.GetFileName(x), Value = Path.GetFileName(x) }), "Value", "Text", ApplePathCertificateFileDevelopment);
            }
        }
        private void VerificaPath(string stringpath) {
            if (!Directory.Exists(stringpath))
                Directory.CreateDirectory(stringpath);
        }
    }
}
