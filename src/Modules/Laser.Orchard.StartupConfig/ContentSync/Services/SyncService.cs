
using Laser.Orchard.StartupConfig.ContentSync.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Taxonomies.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.ContentSync.Services {
    public class SyncService : ISyncService {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IUtilsServices _utilsServices;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IRepository<SyncPartRecord> _syncRepository;

        public SyncService(
                        Lazy<IEnumerable<IContentHandler>> handlers,
                        IOrchardServices services,
                        IRepository<SyncPartRecord> syncRepository,
                        ITaxonomyService taxonomyService,
                        IUtilsServices utilsServices) {
            _handlers = handlers;
            Services = services;
            _syncRepository = syncRepository;
            _taxonomyService = taxonomyService;
            _utilsServices = utilsServices;
            Logger = NullLogger.Instance;
        }

        public IEnumerable<IContentHandler> Handlers {
            get { return _handlers.Value; }
        }
        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; }

        public void Synchronize(SyncContext context) {
            context.Result = GetTargetContent(context);
            
            if (context.Result == null) return;
            UpdateTarget(context);

            if (context.Target.EnsurePublishing) {
                Services.ContentManager.Publish(context.Result);
            }
        }

        public ContentItem GetSynchronizedContent(string contentType, int sourceId) {
            return Services.ContentManager
                .Query<SyncPart, SyncPartRecord>()
                .ForType(contentType)
                .ForVersion(VersionOptions.Latest)
                .Where<SyncPartRecord>(x => x.SyncronizedRef == sourceId)
                .List<ContentItem>()
                .SingleOrDefault();
        }
        
        /// <summary>
        /// Updates the target ContentItem firing the right Handlers
        /// </summary>
        /// <param name="context">The SyncContext object</param>
        /// <param name="dest">The destination Content Item</param>
        private void UpdateTarget(SyncContext context) {
            var src = context.Source;
            var dest = context.Result;
            var targetContext = new UpdateContentContext(dest);
            var sourceContext = new CloneContentContext(src, dest);
            Handlers.Invoke(handler => handler.Updating(targetContext), Logger);
            Handlers.Invoke(handler => handler.Cloning(sourceContext), Logger);
            Handlers.Invoke(handler => handler.Cloned(sourceContext), Logger);
            if (context.Source.ContentType == "User") {
                var cp = dest.As<CommonPart>();
                var up = context.Source.As<UserPart>();
                if (cp != null && up != null && (cp.Owner == null || context.Target.ForceOwnerUpdate)) {
                    cp.Owner = up;
                }
            } else if (context.Source.As<CommonPart>() == null) {
                var cp = dest.As<CommonPart>();
                var up = Services.WorkContext.CurrentUser;
                if (cp != null && up != null && (cp.Owner == null || context.Target.ForceOwnerUpdate)) {
                    cp.Owner = up;
                }
            }

            dest.As<SyncPart>().SyncronizedRef = context.Source.Id;

            Handlers.Invoke(handler => handler.Updated(targetContext), Logger);
        }

        private ContentItem GetTargetContent(SyncContext context) {

            var contentExists = false;
            ContentItem target;
            var targetId = Services.ContentManager
                .Query<SyncPart, SyncPartRecord>()
                .ForType(context.Target.Type)
                .ForVersion(VersionOptions.Latest)
                .Where<SyncPartRecord>(x => x.SyncronizedRef == context.Source.Id)
                .List<ContentItem>()
                .Select(x => x.Id)
                .SingleOrDefault();
            contentExists = (targetId > 0);
            if (!contentExists) {
                if (!context.Target.EnsureCreating) {
                    return null; //exit if the content should not be created
                }
                target = Services.ContentManager.New(context.Target.Type);
                Services.ContentManager.Create(target, VersionOptions.Draft);
            }
            else {
                var option = VersionOptions.Published;
                if (context.Target.EnsureVersioning) {
                    option = VersionOptions.DraftRequired;
                }
                target = Services.ContentManager.Get(targetId, option);
            }
            return target;
        }
    }
}
