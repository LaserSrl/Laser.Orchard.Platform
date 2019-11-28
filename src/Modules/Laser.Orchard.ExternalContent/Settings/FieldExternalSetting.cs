using System;

namespace Laser.Orchard.ExternalContent.Settings {
    public class FieldExternalSetting {
        public FieldExternalSetting() {
            CacheMinute = 5;
            CacheToFileSystem = false;
            ScheduledMinute = 0;
            DataType = OriginData.RestWebService;
        }
        public bool Required { get; set; }
        public string ExternalURL { get; set; }
        public bool NoFollow { get; set; }
        public bool GenerateL { get; set; }
        public HttpVerbOptions HttpVerb { get; set; }
        public HttpDataTypeOptions HttpDataType { get; set; }
        public string BodyRequest { get; set; }
        public bool CertificateRequired { get; set; }
        public string CerticateFileName { get; set; }
        public string CertificatePrivateKey { get; set; }
        public Int32 CacheMinute { get; set; }
        public string CacheInput { get; set; }
        public bool CacheToFileSystem { get; set; }
        public Int32 ScheduledMinute { get; set; } 
        public OriginData DataType { get; set; }
        public string AdditionalHeadersText { get; set; }
        public string InternalHostNameForScheduledTask { get; set; }
    }
}