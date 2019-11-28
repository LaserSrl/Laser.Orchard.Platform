using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.OutputCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Orchard.Tokens;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePartHandler : ContentHandler,  ICachingEventHandler {

        private readonly ICurrentContentAccessor _currentContentAccessor;
        private readonly ITokenizer _tokenizer;

        public PerItemCachePartHandler(IRepository<PerItemCachePartRecord> repository, ICurrentContentAccessor currentContentAccessor, ITokenizer tokenizer) {

            _currentContentAccessor = currentContentAccessor;
            _tokenizer = tokenizer;

            Filters.Add(StorageFilter.For(repository));
        }


        public void KeyGenerated(StringBuilder key) {
            var content = _currentContentAccessor.CurrentContentItem;

            var part = ((dynamic)content).PerItemCachePart;

            if (part == null)
                return;
      
            string settingKey;
            settingKey = part.PerItemKeyParam;

            if(String.IsNullOrEmpty(settingKey))
                settingKey = part.Settings["PerItemCachePartSettings.PerItemKeyParam"];
            
            if (String.IsNullOrEmpty(settingKey))
                return;
    
            var _additionalCacheKey = _tokenizer.Replace(settingKey, new Dictionary<string, object> { { "Content", content } });
            key.Append(_additionalCacheKey);
        }
    }
}