using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Projections.Models;

namespace Laser.Orchard.Queries.ViewModels {
    public class QueryPickerVM {
        public int[] SelectedIds { get; set; }
        public IEnumerable<KeyValuePair<int, string>> AvailableQueries { get; set; }
        public int? TotalItemsCount { get; set; }
        public IEnumerable<KeyValuePair<int, string>> OneShotQueries { get; set; }
    }
}