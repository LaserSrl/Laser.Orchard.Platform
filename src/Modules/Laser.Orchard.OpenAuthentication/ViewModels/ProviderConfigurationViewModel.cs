﻿using Laser.Orchard.OpenAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class ProviderConfigurationViewModel {
        public ProviderConfigurationViewModel() {
            Attributes = new List<ProviderAttributeViewModel>();
        }
        public ProviderConfigurationViewModel(ProviderConfigurationViewModel pcvm) {
            Id = pcvm.Id;
            IsEnabled = pcvm.IsEnabled;
            DisplayName = pcvm.DisplayName;
            ProviderName = pcvm.ProviderName;
            ProviderIdKey = pcvm.ProviderIdKey;
            ProviderSecret = pcvm.ProviderSecret;
            ProviderIdentifier = pcvm.ProviderIdentifier;
            UserIdentifier = pcvm.UserIdentifier;
            Attributes = pcvm.Attributes != null
                ? pcvm.Attributes
                : new List<ProviderAttributeViewModel>();
        }
        public ProviderConfigurationViewModel(
            ProviderConfigurationViewModel pcvm,
            IEnumerable<ProviderAttributeViewModel> attributes) 
            : this(pcvm) {

            this.Attributes.AddRange(attributes);
        }
        public int Id { get; set; }

        /// <summary>
        /// This property shows/hides the login button within the LogOn page/shape. It does not mean that the provider is enabled or not.
        /// If IsEnabled == 1 a specific button will be shown in the LogOn page/shape otherwise not.
        /// If IsEnabled == 0 External Logons are still permitted; to completely disable the provider simply remove it from OpenAuthentication settings url: /Admin/OpenAuthentication
        /// </summary>
        public int IsEnabled { get; set; }
        public string DisplayName { get; set; }
        public string ProviderName { get; set; }
        public string ProviderIdKey { get; set; }
        public string ProviderSecret { get; set; }
        public string ProviderIdentifier { get; set; }
        public string UserIdentifier { get; set; }
        public List<ProviderAttributeViewModel> Attributes { get; set; }
    }
}
