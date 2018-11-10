using NUnit.Framework;
using Orchard.Tests.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Moq;
using Orchard.ContentManagement.MetaData.Models;
using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Handlers;
using Orchard.Data;
using Laser.Orchard.GDPR.Tests.Models;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Caching;
using Orchard.Tests.Stubs;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.GDPR.Tests.Handlers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Laser.Orchard.GDPR.Tests.Drivers;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using System.Globalization;
using Laser.Orchard.GDPR.Drivers;
using Orchard.Security;
using Laser.Orchard.GDPR.Tests.Helpers;

namespace Laser.Orchard.GDPR.Tests {
    [TestFixture]
    public class AnonymizeContentContextTests : DatabaseEnabledTestsBase {

        private IContentManager _contentManager;
        private ITransactionManager _transactionManager;

        public override void Init() {
            base.Init();

            _contentManager = _container.Resolve<IContentManager>();
            _transactionManager = _container.Resolve<ITransactionManager>();
        }

        public override void Register(ContainerBuilder builder) {
            // I need to mock the definition manager so I can "create" the types I need for the tests
            var mockDefinitionManager = MockContentDefinitionManagerHelpers.MockManager();
            builder.RegisterInstance(mockDefinitionManager.Object);
            
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);

            //For DefaultContentManager
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>().InstancePerDependency();

            builder.RegisterType<DefaultContentManager>().As<IContentManager>().SingleInstance();

            //handlers
            // coordinator should let me weld dynamic parts and fields
            builder.RegisterType<ContentPartDriverCoordinator>().As<IContentHandler>();
            builder.RegisterType<ContentFieldDriverCoordinator>().As<IContentHandler>();
            builder.RegisterType<FieldStorageProviderSelector>().As<IFieldStorageProviderSelector>();
            builder.RegisterType<InfosetStorageProvider>().As<IFieldStorageProvider>();
            builder.RegisterType<GDPRPartHandler>().As<IContentHandler>();

            //drivers
            builder.RegisterType<AlphaPartDriver>().As<IContentPartDriver>();
            builder.RegisterType<GDPRPartDriver>().As<IContentPartDriver>();
            builder.RegisterType<AlphaFieldDriver>().As<IContentFieldDriver>();
        }


        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(ContentItemVersionRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentTypeRecord),
                    typeof(GDPRPartRecord)
                };
            }
        }

        
        private AnonymizeContentContext MakeAnonymizationContextFromType(string typeName) {
            var ci = _contentManager.New(typeName);
            var ctx = new AnonymizeContentContext(ci);
            _transactionManager.RequireNew();
            return ctx;
        }

        private EraseContentContext MakeErasureContextFromType(string typeName) {
            var ci = _contentManager.New(typeName);
            var ctx = new EraseContentContext(ci);
            _transactionManager.RequireNew();
            return ctx;
        }

        /// <summary>
        /// Test all variants of the method. This is also checking that the absence
        /// of GDPRPart is the first check in all the ShouldProcess variants
        /// </summary>
        [Test]
        public void ShouldProcessReturnsFalseIfNoGDPRPart() {
            var context = MakeAnonymizationContextFromType("NoGDPRType");

            Assert.That(context.ShouldProcess<AlphaPart>(), Is.False);
            Assert.That(context.ShouldProcess<AlphaField>("AlphaField"), Is.False);
            // test the field in the AlphaPart
            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.As<AlphaPart>().Get(typeof(AlphaField), "AlphaField")),
                Is.False);
            // test the field in the ContentItem
            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "NoGDPRType")
                        .Fields.FirstOrDefault(fi => fi.Name == "AlphaField")),
                Is.False);
        }

        /// <summary>
        /// Test all variants of the method.
        /// </summary>
        [Test]
        public void ShouldProcessReturnsFalseIfProtectedItem() {
            var context = MakeAnonymizationContextFromType("GDPRType");
            context.ContentItem.As<GDPRPart>().IsProtected = true;

            Assert.That(context.ShouldProcess<GDPRPart>(), Is.False);
            Assert.That(context.ShouldProcess<AlphaField>("AlphaAnonymize"), Is.False);
            Assert.That(context.ShouldProcess<AlphaField>("AlphaErase"), Is.False);
            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "GDPRType")
                        .Fields.FirstOrDefault(fi => fi.Name == "AlphaAnonymize")),
                Is.False);
            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "GDPRType")
                        .Fields.FirstOrDefault(fi => fi.Name == "AlphaErase")),
                Is.False);
        }

        /// <summary>
        /// This tests, for all variants of the method, that during an anonymization
        /// of a part/field that should be anonymized the method returns true.
        /// </summary>
        [Test]
        public void ShouldProcessReturnsTrueForAnonymization() {
            var context = MakeAnonymizationContextFromType("GDPRType");

            Assert.That(context.ShouldProcess<AlphaField>("AlphaAnonymize"), Is.True);

            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "AnonymizeMePart")),
                Is.True);
        }

        /// <summary>
        /// This tests, for all variants of the method, that during an anonymization
        /// of a part/field that should not be anonymized the method returns false.
        /// </summary>
        [Test]
        public void ShouldProcessReturnsFalseForAnonymization() {
            var context = MakeAnonymizationContextFromType("GDPRType");

            Assert.That(context.ShouldProcess<AlphaField>("AlphaErase"), Is.False);

            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "EraseeMePart")),
                Is.False);
        }

        /// <summary>
        /// This tests, for all variants of the method, that during an erasure
        /// of a part/field that should be erased the method returns true.
        /// </summary>
        [Test]
        public void ShouldProcessReturnsTrueForErasure() {
            var context = MakeErasureContextFromType("GDPRType");

            Assert.That(context.ShouldProcess<AlphaField>("AlphaErase"), Is.True);

            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "EraseMePart")),
                Is.True);
        }

        /// <summary>
        /// This tests, for all variants of the method, that during an erasure
        /// of a part/field that should not be erased the method returns false.
        /// </summary>
        [Test]
        public void ShouldProcessReturnsFalseForErasure() {
            var context = MakeErasureContextFromType("GDPRType");

            Assert.That(context.ShouldProcess<AlphaField>("AlphaAnonymize"), Is.False);

            Assert.That(
                context.ShouldProcess(
                    context.ContentItem.Parts.FirstOrDefault(pa => pa.PartDefinition.Name == "AnonymizeMePart")),
                Is.False);
        }
    }
}
