using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class CheckoutPolicySettingsPartEditViewModel {

        public CheckoutPolicySettingsPartEditViewModel() {
            PolicyTextReferences = new string[0]; // empty array
            PolicyTitles = new List<TitlePart>();
        }
        
        public CheckoutPolicySettingsPartEditViewModel(CheckoutPolicySettingsPart part)
            : this() {
            if (part != null && part.PolicyTextReferences != null) {
                PolicyTextReferences = part.PolicyTextReferences;
            }
        }

        public string[] PolicyTextReferences { get; set; }

        public List<TitlePart> PolicyTitles { get; set; }
    }
}