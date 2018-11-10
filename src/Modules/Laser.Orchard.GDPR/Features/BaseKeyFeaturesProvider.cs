using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Features {
    /// <summary>
    /// Base class for providers telling which features should auto-enable when all their
    /// dependencies are met. To implement, the minimum is to provide a constructor for the 
    /// implementation passind an IFeatureManager to the base constructor, and initializing 
    /// the KeyFeatureNames array to some value. See for example GDPRKeyFeaturesProvider.
    /// Note that implementatios of this should not be "hidden" behind an OrchardFeatureAttribute
    /// so that they may be found and used even when the feature they actually belong to is
    /// not enabled. The reason for this is that they are often involved in enabling that very
    /// same feature.
    /// </summary>
    public abstract class BaseKeyFeaturesProvider : IKeyFeaturesProvider {
        private readonly IFeatureManager _featureManager;

        public BaseKeyFeaturesProvider(
            IFeatureManager featureManager) {

            _featureManager = featureManager;
        }

        protected string[] KeyFeatureNames { get; set; }

        public virtual IEnumerable<FeatureDescriptor> DisabledKeyFeatures() {
            return _featureManager
                .GetDisabledFeatures()
                .Where(fd => KeyFeatureNames.Contains(fd.Id));
        }

        public virtual IEnumerable<FeatureDescriptor> DisabledKeyFeatures(Feature dependency) {
            return DisabledKeyFeatures(dependency.Descriptor.Id);
        }

        public virtual IEnumerable<FeatureDescriptor> DisabledKeyFeatures(string dependencyName) {
            return DisabledKeyFeatures()
                .Where(fd => fd.Dependencies.Contains(dependencyName));
        }
    }
}