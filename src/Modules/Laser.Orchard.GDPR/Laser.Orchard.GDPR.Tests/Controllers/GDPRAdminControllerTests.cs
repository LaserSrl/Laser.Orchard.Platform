using Laser.Orchard.GDPR.Controllers;
using Orchard.ContentManagement.Records;
using Orchard.Tests.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Orchard.Security;
using Moq;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI.Navigation;
using System.Web.Mvc;
using Orchard.Core.Settings.Services;
using Orchard.Settings;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.Tests.Helpers;
using Laser.Orchard.GDPR.Models;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Laser.Orchard.GDPR.Handlers;
using Orchard.Caching;
using Orchard.Tests.Stubs;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.GDPR.Tests.Drivers;
using Laser.Orchard.GDPR.Drivers;
using Orchard.DisplayManagement.Descriptors;
using Laser.Orchard.GDPR.Permissions;
using System.Web;
using System.Web.Routing;
using System.Collections.Specialized;
using Orchard.Core.Settings.Handlers;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Data;

namespace Laser.Orchard.GDPR.Tests.Controllers {
    [TestFixture]
    public class GDPRAdminControllerTests : DatabaseEnabledTestsBase {

        private GDPRAdminController _controller;
        private Mock<IAuthorizer> _authorizer;
        private IContentManager _contentManager;
        private ITransactionManager _transactionManager;
        private Mock<INotifier> _notifier;

        public override void Register(ContainerBuilder builder) {


            // register the dependencies for the controller
            _authorizer = new Mock<IAuthorizer>();
            builder.RegisterInstance(_authorizer.Object);

            builder.RegisterType<SiteService>().As<ISiteService>();
            _notifier = new Mock<INotifier>();
            builder.RegisterInstance(_notifier.Object);
            builder.RegisterInstance(new Mock<IContentGDPRManager>().Object);
            // Mock IContentDefinitionManager to have types with GDPRPart
            var mockDefinitionManager = MockContentDefinitionManagerHelpers.MockManager();
            builder.RegisterInstance(mockDefinitionManager.Object);

            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>().InstancePerDependency();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();

            builder.RegisterInstance(new Work<IEnumerable<IShapeTableEventHandler>>(resolve => _container.Resolve<IEnumerable<IShapeTableEventHandler>>())).AsSelf();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();

            // register an instance of the controller we want to test
            builder.RegisterType<GDPRAdminController>().SingleInstance();

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

        public override void Init() {
            base.Init();

            _controller = _container.Resolve<GDPRAdminController>();
            _contentManager = _container.Resolve<IContentManager>();
            _transactionManager = _container.Resolve<ITransactionManager>();
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

        #region tests for Index action
        [Test]
        public void UserWithNoPermissionCannotAccessIndex() {
            // setup the authorizer to return false for every permission
            UserHasNoPermission();

            var pager = new PagerParameters { Page = 1, PageSize = 10 };

            var result = _controller.Index(null, pager);
            Assert.That(result, Is.TypeOf<HttpUnauthorizedResult>());
        }

        [Test]
        public void AnonymizationManagerCanAccessIndex() {
            UserIsAnonymizationManager();

            var pager = new PagerParameters { Page = 1, PageSize = 10 };

            var result = _controller.Index(null, pager);
            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public void ErasureManagerCanAccessIndex() {
            UserIsErasureManager();

            var pager = new PagerParameters { Page = 1, PageSize = 10 };

            var result = _controller.Index(null, pager);
            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        #endregion

        #region tests for Anonymize action
        [Test]
        public void UserWithNoPermissionCannotAccessAnonymize() {
            // setup the authorizer to return false for every permission
            UserHasNoPermission();

            var item = ContentFromType("ProfileItemType");

            var result = _controller.Anonymize(item.Id, null);
            Assert.That(result, Is.TypeOf<HttpUnauthorizedResult>());
        }

        [Test]
        public void AnonymizationManagerCanCallAnonymize() {
            UserIsAnonymizationManager();

            // try on an item that does not exist
            var result = _controller.Anonymize(42, null);
            Assert.That(result, Is.TypeOf<HttpNotFoundResult>());


            // In case of errors in the configuration of teh ContentItem we notify the 
            // back office. We need to check what the notifier does.
            var profileItem = ContentFromType("ProfileItemType");
            var noGDPRItem = ContentFromType("NoGDPRType");
            var protectedItem = ContentFromType("ProfileItemType");
            protectedItem.As<GDPRPart>().IsProtected = true;
            var notProfile = ContentFromType("GDPRType");
            
            //we set the Mock<INotifier> to set a bool in case of error
            var error = false;
            //_notifier.Error(message) is an extension method that calls notifier.Add(NotifyType.Error, message);
            _notifier.Setup(n => n
                .Add(It.Is<NotifyType>(nt => nt == NotifyType.Error), It.IsAny<LocalizedString>()))
                .Callback(() => error = true);

            result = _controller.Anonymize(noGDPRItem.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(error);

            error = false; // reset
            result = _controller.Anonymize(protectedItem.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(error);

            error = false; // reset
            result = _controller.Anonymize(notProfile.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(error);

            error = false; // reset
            // then try on item with no errors
            result = _controller.Anonymize(profileItem.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(!error);
        }

        [Test]
        public void ErasureManagerCannotCallAnonymize() {
            UserIsErasureManager();

            var item = ContentFromType("ProfileItemType");

            var result = _controller.Anonymize(item.Id, null);
            Assert.That(result, Is.TypeOf<HttpUnauthorizedResult>());
        }

        #endregion

        #region tests for Erase action
        [Test]
        public void UserWithNoPermissionCannotAccessErase() {
            // setup the authorizer to return false for every permission
            UserHasNoPermission();

            var item = ContentFromType("ProfileItemType");

            var result = _controller.Erase(item.Id, null);
            Assert.That(result, Is.TypeOf<HttpUnauthorizedResult>());
        }

        [Test]
        public void ErasureManagerCanCallErase() {
            UserIsErasureManager();
            
            // try on an item that does not exist
            var result = _controller.Erase(42, null);
            Assert.That(result, Is.TypeOf<HttpNotFoundResult>());

            // In case of errors in the configuration of teh ContentItem we notify the 
            // back office. We need to check what the notifier does.
            var profileItem = ContentFromType("ProfileItemType");
            var noGDPRItem = ContentFromType("NoGDPRType");
            var protectedItem = ContentFromType("ProfileItemType");
            protectedItem.As<GDPRPart>().IsProtected = true;
            var notProfile = ContentFromType("GDPRType");

            //we set the Mock<INotifier> to set a bool in case of error
            var error = false;
            //_notifier.Error(message) is an extension method that calls notifier.Add(NotifyType.Error, message);
            _notifier.Setup(n => n
                .Add(It.Is<NotifyType>(nt => nt == NotifyType.Error),It.IsAny<LocalizedString>()))
                .Callback(() => error = true);
            
            result = _controller.Erase(noGDPRItem.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(error);

            error = false; // reset
            result = _controller.Erase(protectedItem.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(error);

            error = false; // reset
            result = _controller.Erase(notProfile.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(error);

            error = false; // reset
            // then try on item with no errors
            result = _controller.Erase(profileItem.Id, null);
            Assert.That(result, Is.TypeOf<ViewResult>().Or.TypeOf<RedirectToRouteResult>());
            Assert.That(!error);
        }

        [Test]
        public void AnonymizationManagerCannotCallErase() {
            UserIsAnonymizationManager();

            var item = ContentFromType("ProfileItemType");

            var result = _controller.Erase(item.Id, null);
            Assert.That(result, Is.TypeOf<HttpUnauthorizedResult>());
        }

        #endregion

        #region private helper methods
        /// <summary>
        /// Configure the IAuthorizer to simulate a user with no permission
        /// </summary>
        private void UserHasNoPermission() {
            _authorizer.Setup(a => a
                .Authorize(It.IsAny<Permission>()))
                .Returns(false);
            _authorizer.Setup(a => a
                .Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>()))
                .Returns(false);
            _authorizer.Setup(a => a
                .Authorize(It.IsAny<Permission>(), It.IsAny<IContent>()))
                .Returns(false);
            _authorizer.Setup(a => a
                .Authorize(It.IsAny<Permission>(), It.IsAny<IContent>(), It.IsAny<LocalizedString>()))
                .Returns(false);
        }

        /// <summary>
        /// Configure the IAuthorizer to simulate a user with only the ManageAnonymization permission
        /// </summary>
        private void UserIsAnonymizationManager() {
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageAnonymization)))
                .Returns(true);
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageAnonymization), It.IsAny<LocalizedString>()))
                .Returns(true);
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageAnonymization), It.IsAny<IContent>()))
                .Returns(true);
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageAnonymization), It.IsAny<IContent>(), It.IsAny<LocalizedString>()))
                .Returns(true);
        }

        /// <summary>
        /// Configure the IAuthorizer to simulate a user with only the ManageErasure permission
        /// </summary>
        private void UserIsErasureManager() {
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageErasure)))
                .Returns(true);
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageErasure), It.IsAny<LocalizedString>()))
                .Returns(true);
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageErasure), It.IsAny<IContent>()))
                .Returns(true);
            _authorizer.Setup(a => a
                .Authorize(It.Is<Permission>(p => p == GDPRPermissions.ManageErasure), It.IsAny<IContent>(), It.IsAny<LocalizedString>()))
                .Returns(true);
        }
                
        private ContentItem ContentFromType(string typeName) {
            var ci = _contentManager.New(typeName);
            _contentManager.Create(ci);
            _transactionManager.RequireNew();
            return ci;
        }
        #endregion


    }
}
