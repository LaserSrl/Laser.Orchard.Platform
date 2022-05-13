
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.ContentSync.Models;
using Laser.Orchard.StartupConfig.Fields;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentPicker.Fields;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;

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
            var contentExists = false;
            // var targetRecord = _syncRepository.Fetch(x => x.SyncronizedRef == context.Source.Id).Sin;
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
                    return; //exit if the content should not be created
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
            CopyParts(context.Source, target);
            target.As<SyncPart>().SyncronizedRef = context.Source.Id;
            if (context.Target.EnsurePublishing) {
                Services.ContentManager.Publish(target);
            }
            context.Result = target;
        }


        private void CopyParts(ContentItem src, ContentItem dest) {
            var context = new CloneContentContext(src, dest);
            Handlers.Invoke(handler => handler.Cloning(context), Logger);
            Handlers.Invoke(handler => handler.Cloned(context), Logger);
        }
    }
}
