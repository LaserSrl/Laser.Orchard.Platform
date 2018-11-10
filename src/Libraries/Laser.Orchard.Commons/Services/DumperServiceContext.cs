using Laser.Orchard.Commons.Enums;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Laser.Orchard.Commons.Services {
    public class DumperServiceContext {
        /// <summary>
        /// The content we are dumping
        /// </summary>
        public IContent Content { get; set; }

        /// <summary>
        /// The list of all the lists taht we dumped
        /// </summary>
        public List<string> ContentLists { get; set; }

        public Func<ObjectDumper> GetDumper { get; set; }

        public Action<XElement, StringBuilder> ConvertToJson { get; set; }

        public ResultTarget ResultTarget { get; set; }

        /// <summary>
        /// Parameters for pagination of lists.
        /// </summary>
        public int Page { get; set; }
        public int PageSize { get; set; }

        public DumperServiceContext(
            IContent content,
            Func<ObjectDumper> dumperConstructor,
            Action<XElement, StringBuilder> convertToJson,
            ResultTarget resultTarget,
            int page = 1,
            int pageSize = 10) {

            Content = content;
            GetDumper = dumperConstructor;
            ConvertToJson = convertToJson;
            Page = page;
            PageSize = pageSize;

            ContentLists = new List<string>();
        }
        
    }
}
