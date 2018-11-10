using NUnit.Framework;
using Orchard.Tests.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Laser.Orchard.GDPR.Services;
using Orchard.ContentManagement.Records;
using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Handlers;
using Moq;
using Orchard.ContentManagement;
using Laser.Orchard.GDPR.Tests.Helpers;
using Orchard.Data;
using Orchard.Caching;
using Orchard.Tests.Stubs;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Core.Settings.Handlers;
using Laser.Orchard.GDPR.Tests.Drivers;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.GDPR.Drivers;
using Orchard.Security;

namespace Laser.Orchard.GDPR.Tests.Services {
    [TestFixture]
    public class DefaultContentGDPRManagerTests : DatabaseEnabledTestsBase {

        private IContentGDPRManager _contentGDPRManager;
        private Mock<IContentGDPRHandler> _mockHandler;
        private IContentManager _contentManager;
        private ITransactionManager _transactionManager;

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<GDPRPartProtectedProvider>().As<IGDPRProcessAllowedProvider>();
            builder.RegisterType<DefaultContentGDPRManager>().As<IContentGDPRManager>();

            _mockHandler = new Mock<IContentGDPRHandler>();
            builder.RegisterInstance(_mockHandler.Object);

            var mockDefinitionManager = MockContentDefinitionManagerHelpers.MockManager();
            builder.RegisterInstance(mockDefinitionManager.Object);

            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>().InstancePerDependency();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();

            builder.RegisterInstance(new Mock<IAuthorizer>().Object);

            // handlers
            // coordinator should let me weld dynamic parts and fields
            builder.RegisterType<ContentPartDriverCoordinator>().As<IContentHandler>();
            builder.RegisterType<ContentFieldDriverCoordinator>().As<IContentHandler>();
            builder.RegisterType<FieldStorageProviderSelector>().As<IFieldStorageProviderSelector>();
            builder.RegisterType<InfosetStorageProvider>().As<IFieldStorageProvider>();
            builder.RegisterType<GDPRPartHandler>().As<IContentHandler>();
            builder.RegisterType<SiteSettingsPartHandler>().As<IContentHandler>();

            //drivers
            builder.RegisterType<AlphaPartDriver>().As<IContentPartDriver>();
            builder.RegisterType<GDPRPartDriver>().As<IContentPartDriver>();
            builder.RegisterType<AlphaFieldDriver>().As<IContentFieldDriver>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(ContentTypeRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentItemVersionRecord),
                    typeof(GDPRPartRecord)
                };
            }
        }

        public override void Init() {
            base.Init();

            _contentGDPRManager = _container.Resolve<IContentGDPRManager>();
            _contentManager = _container.Resolve<IContentManager>();
            _transactionManager = _container.Resolve<ITransactionManager>();
        }

        [Test]
        public void AnonymizeCallsHandlers() {
            var ci = ContentFromType("GDPRType");

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());

            _contentGDPRManager.Anonymize(ci);

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()));
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()));
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());
        }

        [Test]
        public void AnonymizeDoesNotCallHandlersIfThereIsNoGDPRPart() {
            var ci = ContentFromType("NoGDPRType");

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());

            _contentGDPRManager.Anonymize(ci);

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());
        }

        [Test]
        public void AnonymizeDoesNotCallHandlersIfItemIsProtected() {
            var ci = ContentFromType("GDPRType");
            ci.As<GDPRPart>().IsProtected = true;

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());

            _contentGDPRManager.Anonymize(ci);

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());
        }

        [Test]
        public void EraseCallsHandlers() {
            var ci = ContentFromType("GDPRType");

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());

            _contentGDPRManager.Erase(ci);

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()));
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()));
        }

        [Test]
        public void EraseDoesNotCallHandlersIfThereIsNoGDPRPart() {
            var ci = ContentFromType("NoGDPRType");

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());

            _contentGDPRManager.Erase(ci);

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());
        }

        [Test]
        public void EraseDoesNotCallHandlersIfItemIsProtected() {
            var ci = ContentFromType("GDPRType");
            ci.As<GDPRPart>().IsProtected = true;

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());

            _contentGDPRManager.Erase(ci);

            _mockHandler.Verify(m => m.Anonymizing(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Anonymized(It.IsAny<AnonymizeContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erasing(It.IsAny<EraseContentContext>()), Times.Never());
            _mockHandler.Verify(m => m.Erased(It.IsAny<EraseContentContext>()), Times.Never());
        }

        #region private helper methods
        private ContentItem ContentFromType(string typeName) {
            var ci = _contentManager.New(typeName);
            _contentManager.Create(ci);
            _transactionManager.RequireNew();
            return ci;
        }
        #endregion
    }
}
