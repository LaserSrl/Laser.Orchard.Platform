using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Models {

    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ProtectionSettingsPart : ContentPart {
        private ExternalApplicationList _extApplicationList;
        public ExternalApplicationList ExternalApplicationList {
            get {
                if (_extApplicationList == null) {
                    var json = Retrieve<string>("ExternalApplications");
                    if (json == null) return new ExternalApplicationList();
                    _extApplicationList = new JavaScriptSerializer().Deserialize<ExternalApplicationList>(json);
                }
                return _extApplicationList;
            }
            set {
                var json = new JavaScriptSerializer().Serialize(value);
                _extApplicationList = value;
                this.Store("ExternalApplications", json);
            }
        }

        public string ProtectedEntries {
            get { return this.Retrieve(x => x.ProtectedEntries); }
            set { this.Store(x => x.ProtectedEntries, value); }
        }

    }

    public class ExternalApplicationList {
        public ExternalApplicationList() {
            ExternalApplications = new List<ExternalApplication>();
        }

        public IEnumerable<ExternalApplication> ExternalApplications { get; set; }
    }

    public enum ApiValidationTypes {
        ApiKey = 0,
        Website,
        IpAddress
    }

    public class ExternalApplication {

        [ValidateExternalApplication]
        public string Name { get; set; }

        public ApiValidationTypes ValidationType { get; set; }

        [ValidateExternalApplication]
        public string ApiKey { get; set; }

        [ValidateExternalApplication]
        public bool EnableTimeStampVerification { get; set; }

        public int Validity { get; set; }

        public bool Delete { get; set; }

    }

    public class ValidateExternalApplication : ValidationAttribute {
        public ValidateExternalApplication() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var model = (ExternalApplication)validationContext.ObjectInstance;


            if (model.Delete)
                return ValidationResult.Success; // it will be deleted, so no validation 
            else {
                if (validationContext.DisplayName == "Name") {
                    if (String.IsNullOrWhiteSpace(model.Name)) {
                        return new ValidationResult(T("Name field is required.").Text);
                    }
                } else if (validationContext.DisplayName == "ApiKey") {
                    switch (model.ValidationType) {
                        case ApiValidationTypes.ApiKey:
                            if (String.IsNullOrWhiteSpace(model.ApiKey)) {
                                return new ValidationResult(T("ApiKey field is required.").Text);
                            } else if (model.ApiKey.Trim().Length < 22) {
                                return new ValidationResult(T("ApiKey field must be minimum 22 characters length.").Text);
                            }
                            break;

                        case ApiValidationTypes.Website:
                            // Website url formal validation
                            if (String.IsNullOrWhiteSpace(model.ApiKey)) {
                                return new ValidationResult(T("Website field is required.").Text);
                            }
                            if (!ValidateWebsite(model.ApiKey.Trim())) {
                                return new ValidationResult(T("One or more websites is not valid.").Text);
                            }
                            break;

                        case ApiValidationTypes.IpAddress:
                            // Ip address formal validation
                            if (String.IsNullOrWhiteSpace(model.ApiKey)) {
                                return new ValidationResult(T("Ip address field is required.").Text);
                            }
                            if (!ValidateIpAddresses(model.ApiKey.Trim())) {
                                return new ValidationResult(T("One or more ip addresses is not valid.").Text);
                            }
                            break;
                    }
                } else if (validationContext.DisplayName == "ApiKey" && validationContext.DisplayName == "EnableTimeStampVerification") {
                    if (model.EnableTimeStampVerification && model.Validity <= 0) {
                        return new ValidationResult(T("Validity should be more than 0 minutes.").Text);
                    }
                }
            }
            return ValidationResult.Success;
        }

        private bool ValidateWebsite (string input) {
            var websites = input.Split(',');
            // If no website is specified
            if (websites.Length == 0) { 
                return false; 
            } 

            foreach(var website in websites) {
                if (!Uri.IsWellFormedUriString(website, UriKind.Absolute)) {
                    return false;   
                }
            }
            return true;
        }

        private bool ValidateIpAddresses(string input) {
            var ips = input.Split(',');
            // If no ip is specified
            if (ips.Length == 0) {
                return false;
            }

            foreach (var ip in ips) {
                var mask = ip.Split('.'); 
                if (mask.Length != 4) {
                    return false;
                }

                int i = 0;
                foreach (var subnet in mask) {
                    i++;                    
                    // Last element of ip address can be the character '*'.
                    if (!(i == 4 && subnet == "*")) {
                        int subnetNr = -1;
                        var isInt = int.TryParse(subnet, out subnetNr);
                        if (!isInt || subnetNr < 0 || subnetNr > 255) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}