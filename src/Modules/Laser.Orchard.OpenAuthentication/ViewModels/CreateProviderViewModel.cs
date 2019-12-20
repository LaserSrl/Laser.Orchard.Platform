using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class CreateProviderViewModel {
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string ProviderName { get; set; }
        public string ProviderIdentifier { get; set; }
        public string UserIdentifier { get; set; }
        public string ProviderIdKey { get; set; }
        public string ProviderSecret { get; set; }
        public bool IsEnabled { get; set; }
        public Int32 Id {get;set;}
        public SelectList ProviderNameList { get; set; }
        public List<ProviderAttributeViewModel> Attributes { get; set; }
        public CreateProviderViewModel() {
            IsEnabled = true;
            Attributes = new List<ProviderAttributeViewModel>();
        }
   }
}