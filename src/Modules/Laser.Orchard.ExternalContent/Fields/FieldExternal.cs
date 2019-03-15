using Laser.Orchard.ExternalContent.Settings;
using Orchard.ContentManagement;

namespace Laser.Orchard.ExternalContent.Fields {
    public class FieldExternal : ContentField {
        private dynamic _contentObject;
        private dynamic _ContentUrl;
        public string ExternalUrl {
            get { return Storage.Get<string>("ExternalUrl"); }
            set { Storage.Set("ExternalUrl", value); }
        }
        public string HttpVerbCode
        {
            get 
            {
                return Storage.Get<string>("HttpVerbCode") ?? "GET";
            }
            set 
            { 
                Storage.Set("HttpVerbCode", value);
            }
        }
        public HttpVerbOptions HttpVerb
        {
            get {
                return (HttpVerbOptions)System.Enum.Parse(typeof(HttpVerbOptions), HttpVerbCode); 
            }
        }
        public string HttpDataTypeCode
        {
            get { return Storage.Get<string>("HttpDataType") ?? "JSON"; }
            set { Storage.Set("HttpDataType", value); }
        }
        public HttpDataTypeOptions HttpDataType
        {
            get { return (HttpDataTypeOptions)System.Enum.Parse(typeof(HttpDataTypeOptions), HttpDataTypeCode); }
        }
        public string BodyRequest
        {
            get { return Storage.Get<string>("BodyRequest"); }
            set { Storage.Set("BodyRequest", value); }
        }

        public dynamic ContentObject {
            get { return _contentObject; }
            set { _contentObject = value; }
        }
        public FieldExternalSetting Setting {
            get {return this.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>(); }
            
        }
        public dynamic ContentUrl{
            get { return _ContentUrl; }
            set { _ContentUrl = value; }
        }
        public string AdditionalHeadersText {
            get { return Storage.Get<string>("AdditionalHeadersText"); }
            set { Storage.Set("AdditionalHeadersText", value); }
        }
    }
}