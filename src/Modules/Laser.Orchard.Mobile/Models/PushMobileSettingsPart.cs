using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Models {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushMobileSettingsPart : ContentPart<PushMobileSettingsPartRecord> {
        public string ApplePathCertificateFile {
            get { return Record.ApplePathCertificateFile; }
            set { Record.ApplePathCertificateFile = value; }
        }
        public string AppleCertificatePassword {
            get { return Record.AppleCertificatePassword; }
            set { Record.AppleCertificatePassword = value; }
        }
        public string ApplePathCertificateFileDevelopment {
            get { return Record.ApplePathCertificateFileDevelopment; }
            set { Record.ApplePathCertificateFileDevelopment = value; }
        }
        public string AppleCertificatePasswordDevelopment {
            get { return Record.AppleCertificatePasswordDevelopment; }
            set { Record.AppleCertificatePasswordDevelopment = value; }
        }
        public string ApplePushSound {
            get { return Record.ApplePushSound; }
            set { Record.ApplePushSound = value; }
        }

        public string TaxonomyName {
            get { return Record.TaxonomyName; }
            set { Record.TaxonomyName = value; }
        }

        public string AndroidApiKey {
            get { return Record.AndroidApiKey; }
            set { Record.AndroidApiKey = value; }
        }
        public string WindowsEndPoint {
            get { return Record.WindowsEndPoint; }
            set { Record.WindowsEndPoint = value; }
        }
        public string WindowsAppPackageName {
            get { return Record.WindowsAppPackageName; }
            set { Record.WindowsAppPackageName = value; }
        }
        public string WindowsAppSecurityIdentifier {
            get { return Record.WindowsAppSecurityIdentifier; }
            set { Record.WindowsAppSecurityIdentifier = value; }
        }
        public bool ShowTestOptions {
            get { return Record.ShowTestOptions; }
            set { Record.ShowTestOptions = value; }
        }
        public string AndroidApiKeyDevelopment {
            get { return Record.AndroidApiKeyDevelopment; }
            set { Record.AndroidApiKeyDevelopment = value; }
        }
        public string AndroidPushServiceUrl {
            get { return Record.AndroidPushServiceUrl; }
            set { Record.AndroidPushServiceUrl = value; }
        }
        public string AndroidPushNotificationIcon {
            get { return Record.AndroidPushNotificationIcon; }
            set { Record.AndroidPushNotificationIcon = value; }
        }
        public int PushSendBufferSize {
            get { return Record.PushSendBufferSize; }
            set { Record.PushSendBufferSize = value; }
        }
        public bool CommitSentOnly {
            get { return Record.CommitSentOnly; }
            set { Record.CommitSentOnly = value; }
        }
        public int DelayMinutesBeforeRetry {
            get { return Record.DelayMinutesBeforeRetry; }
            set { Record.DelayMinutesBeforeRetry = value; }
        }
        public int MaxNumRetry {
            get { return Record.MaxNumRetry; }
            set { Record.MaxNumRetry = value; }
        }
        public int MaxPushPerIteration {
            get { return Record.MaxPushPerIteration; }
            set { Record.MaxPushPerIteration = value; }
        }
    }

    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushMobileSettingsPartRecord : ContentPartRecord {
        public virtual string ApplePathCertificateFile { get; set; }
        public virtual string AppleCertificatePassword { get; set; }
        public virtual string ApplePathCertificateFileDevelopment { get; set; }
        public virtual string AppleCertificatePasswordDevelopment { get; set; }
        public virtual string ApplePushSound { get; set; }
        public virtual string AndroidApiKey { get; set; }
        public virtual string WindowsEndPoint { get; set; }
        public virtual string WindowsAppPackageName { get; set; }
        public virtual string WindowsAppSecurityIdentifier { get; set; }
        public virtual bool ShowTestOptions { get; set; }
        public virtual string AndroidApiKeyDevelopment { get; set; }
        public virtual string AndroidPushServiceUrl { get; set; }
        public virtual string AndroidPushNotificationIcon { get; set; }
        public virtual string TaxonomyName { get; set; }
        public virtual int PushSendBufferSize { get; set; }
        public virtual bool CommitSentOnly { get; set; }
        public virtual int DelayMinutesBeforeRetry { get; set; }
        public virtual int MaxNumRetry { get; set; }
        public virtual int MaxPushPerIteration { get; set; }
    }
}



