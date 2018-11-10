using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Features {
    /// <summary>
    /// This allows cross enabling features. For example, if Laser.Orchard.GDPR is being enabled,
    /// and Orchard.ContentPicker is enabled already, we enable Laser.Orchard.GDPR.ContentPickerFieldExtension
    /// automatically.
    /// </summary>
    public class GDPRFeaturesEventHandler : IFeatureEventHandler {
        private readonly IEnumerable<IKeyFeaturesProvider> _keyFeaturesProviders;
        private readonly IFeatureManager _featureManager;
        private readonly INotifier _notifier;

        /*
         * The idea is to have some code here that is generic enough that it can be reused as is
         * for all the combinations of features for GDPR processing.
         * The most general way I can think of this is by having, for each feature we want to enable,
         * a list of its dependencies (i.e. in Orcahrd terms that is the list of features it depdends
         * on). Whenever a feature is being enabled, I see whether it belongs to any of those lists. 
         * If it does, I check whether the other features there are enabled already. If all features 
         * in a list are enabled, I will also enable the feature that corresponds there.
         * For the discussion in this file, I call KeyFeature each of the features we want to enable,
         * and that have a list of dependencies we monitor. We will need a list of the KeyFeatures we
         * have enabled already.
         */

        public GDPRFeaturesEventHandler(
            IEnumerable<IKeyFeaturesProvider> keyFeaturesProviders,
            IFeatureManager featureManager,
            INotifier notifier) {

            _keyFeaturesProviders = keyFeaturesProviders;
            _featureManager = featureManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private Dictionary<string, string[]> KeyFeaturesLists { get; set; }

        public void Enabled(Feature feature) {
            var keyFeaturesToEnable = new List<string>();
            // we check in the lists of dependencies for those KeyFeatures that are not currently enabled
            foreach (var keyFeatureDescriptor in _keyFeaturesProviders.SelectMany(kfp => kfp
                .DisabledKeyFeatures(feature))) {
                // the feature we just enabled is a dependency of the keyFeature represented by the
                // keyFeatureDescriptor. If all other dependencies are enabled already, we enable the
                // keyFeature too. In other words, if any of the dependencies is disabled we should do
                // nothing.
                if (_featureManager
                    .GetDisabledFeatures() // this is an IEnumerable<FeatureDescriptor>
                    .Any(dfd => keyFeatureDescriptor
                        .Dependencies // this is an IEnumerable<string> of the dependecies' names
                        .Contains(dfd.Id))) {
                    continue; // go to the next keyFeatureDescriptor
                }
                // All dependencies are now enabled, so we enable the KeyFeature too
                keyFeaturesToEnable.Add(keyFeatureDescriptor.Id);
            }

            // Actually enable the KeyFeatures
            _featureManager.EnableFeatures(keyFeaturesToEnable);
        }

        public void Disabled(Feature feature) { }

        public void Disabling(Feature feature) { }
        
        public void Enabling(Feature feature) { }

        public void Installed(Feature feature) { }

        public void Installing(Feature feature) { }

        public void Uninstalled(Feature feature) { }

        public void Uninstalling(Feature feature) { }
    }
}