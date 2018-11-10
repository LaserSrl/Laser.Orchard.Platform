using Autofac;
using Laser.Orchard.CommunicationGateway.Features;
using Laser.Orchard.GDPR.Features;
using Laser.Orchard.Mobile.Feature;
using Laser.Orchard.OpenAuthentication.Features;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.GDPR.Tests.Features {
    [TestFixture]
    public class GDPRFeaturesEventHandlerTests {

        protected IContainer _container;
        private IFeatureManager _featureManager;

        [SetUp]
        public void Init() {

            var builder = new ContainerBuilder();
            // register the stuff we'll need
            // we mock the INotifier: we don't really need to test that anyway
            builder.RegisterInstance(new Mock<INotifier>().Object).As<INotifier>();
            // register the deafault GDPR provider
            builder.RegisterType<GDPRKeyFeaturesProvider>().As<IKeyFeaturesProvider>();
            // register the provider from CommunicationGateway
            builder.RegisterType<ContactGDPRKeyFeaturesProvider>().As<IKeyFeaturesProvider>();
            // register the providers from the Mobile module
            builder.RegisterType<MobileGDPRKeyFeaturesProvider>().As<IKeyFeaturesProvider>();
            builder.RegisterType<SmsGatewayGDPRKeyFeaturesProvider>().As<IKeyFeaturesProvider>();
            builder.RegisterType<SmsExtensionGDPRKeyFeaturesProvider>().As<IKeyFeaturesProvider>();
            builder.RegisterType<PushGatewayGDPRKeyFeaturesProvider>().As<IKeyFeaturesProvider>();
            // register the providers for the OpenAuthentication module
            builder.RegisterType<OpenAuthKeyFeaturesProvider>().As<IKeyFeaturesProvider>();

            // We mock the IFeatureManager: normally, when there is a request to enable a feature it goes through
            // a bunch of methods before it's actually enabled and the events raised. Since we want to test the behaviour
            // of the event handler and the related provider, we mock all that away.
            var featureManager = new Mock<IFeatureManager>();

            // 
            featureManager.Setup(fm => fm.GetDisabledFeatures())
                .Returns(() => {
                    var enabled = EnabledFeatures ?? new List<string>();
                    return AllKeyFeaturesForTheTest().Where(fd => !enabled.Contains(fd.Id));
                });

            featureManager.Setup(fm => fm.EnableFeatures(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(names => {
                    EnabledFeatures = EnabledFeatures ?? new List<string>();
                    EnabledFeatures.AddRange(names);
                    var handler = _container.Resolve<IFeatureEventHandler>();
                    foreach (var name in names) {
                        handler.Enabled(new Feature() { Descriptor = new FeatureDescriptor { Id = name } });
                    }
                    return names;
                });

            _featureManager = featureManager.Object;
            builder.RegisterInstance(_featureManager).As<IFeatureManager>();

            // Register the handler we are testing
            builder.RegisterType<GDPRFeaturesEventHandler>().As<IFeatureEventHandler>();

            EnabledFeatures = new List<string>();

            _container = builder.Build();


        }

        [TearDown]
        public virtual void Cleanup() {
            if (_container != null)
                _container.Dispose();
        }

        public IEnumerable<FeatureDescriptor> AllKeyFeaturesForTheTest() {
            return new FeatureDescriptor[] {
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.Workflows",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Orchard.Workflows" },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.ContentPickerFieldExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Orchard.ContentPicker" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.MediaExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Orchard.MediaLibrary" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.ContactExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Laser.Orchard.CommunicationGateway" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.MobileExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Laser.Orchard.Mobile" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.PushGatewayExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Laser.Orchard.PushGateway" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.SmsGatewayExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Laser.Orchard.SmsGateway" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.SmsExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Laser.Orchard.Sms" }
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.GDPR.OpenAuthExtension",
                    Dependencies = new string[] { "Laser.Orchard.GDPR", "Laser.Orchard.OpenAuthentication" }
                },
                new FeatureDescriptor() {
                    Id = "Orchard.Workflows",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Orchard.ContentPicker",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Orchard.MediaLibrary",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Orchard.Workflows",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.CommunicationGateway",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.Mobile",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.PushGateway",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.SmsGateway",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.Sms",
                    Dependencies = new string[] { },
                },
                new FeatureDescriptor() {
                    Id = "Laser.Orchard.OpenAuthentication",
                    Dependencies = new string[] { },
                },
            };
        }

        protected List<string> EnabledFeatures { get; set; }

        [Test]
        public void EnablingADependencyByItselfDoesNothing() {
            // sanity check
            Assert.That(EnabledFeatures, Is.Empty);
            // enable "Laser.Orchard.GDPR"
            _featureManager.EnableFeatures(new string[] { "Laser.Orchard.GDPR" });

            // Only "Laser.Orchard.GDPR" is enabled
            Assert.That(EnabledFeatures.Count(), Is.EqualTo(1));
            Assert.That(EnabledFeatures.First(), Is.EqualTo("Laser.Orchard.GDPR"));
        }

        [Test]
        public void EnablingAllDependenciesEnablesTheKeyFeature() {
            // sanity check
            Assert.That(EnabledFeatures, Is.Empty);
            // get a KeyFeature
            var keyFeature = AllKeyFeaturesForTheTest().First();

            for (int i = 0; i < keyFeature.Dependencies.Count(); i++) {
                var dep = keyFeature.Dependencies.ElementAt(i);
                _featureManager.EnableFeatures(new string[] { dep });
                Assert.That(EnabledFeatures.Contains(dep));
                if (i == keyFeature.Dependencies.Count() - 1) {
                    // last dependency
                    Assert.That(EnabledFeatures.Count(), Is.EqualTo(keyFeature.Dependencies.Count() + 1));
                    Assert.That(EnabledFeatures.Contains(keyFeature.Id));
                } else {
                    Assert.That(EnabledFeatures.Count(), Is.EqualTo(i + 1));
                }
            }
        }
    }
}
