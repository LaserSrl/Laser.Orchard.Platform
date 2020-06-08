using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using OP = Orchard.Projections.ViewModels;

namespace Laser.Orchard.ContentExtension.ViewModels {
    public class DynamicProjectionVM {
        public DynamicProjectionPartEditViewModel Projection { get; set; }
        public DynamicProjectionPartVM Part { get; set; }
        public string Tenant { get; set; }
        public SelectList ListFormFile {
            get {
                try {
                    string[] filelist = Directory.GetFiles(HostingEnvironment.MapPath(@"~/App_Data/Sites/" + Tenant + @"/Code"));
                    return new SelectList(filelist.Select(x => new SelectListItem { Text = Path.GetFileName(x), Value = Path.GetFileName(x) }), "Value", "Text", Part.Shape);
                }
                catch { return null; }
            }
        }

    }
    public class DynamicProjectionPartVM {
        public string MenuLabel { get; set; }
        public bool OnAdminMenu { get; set; }
        public string AdminMenuText { get; set; }
        public string AdminMenuPosition { get; set; }
        public string Icon { get; set; }
        public string Shape { get; set; }
        public bool ReturnsHqlResults { get; set; }
        public string TypeForFilterForm { get; set; }
        public string ShapeForResults { get; set; }

    }

    public class DynamicProjectionPartEditViewModel {

        [Required, Range(0, int.MaxValue)]
        public int Items { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int ItemsPerPage { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Skip { get; set; }

        public string PagerSuffix { get; set; }

        public bool DisplayPager { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int MaxItems { get; set; }

        public string QueryLayoutRecordId { get; set; }

        public IEnumerable<QueryRecordEntry> QueryRecordEntries { get; set; }
    }

    public class QueryRecordEntry {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<LayoutRecordEntry> LayoutRecordEntries { get; set; }
    }

    public class LayoutRecordEntry {
        public int Id { get; set; }
        public string Description { get; set; }
    }


}