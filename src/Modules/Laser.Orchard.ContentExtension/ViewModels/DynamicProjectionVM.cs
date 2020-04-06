using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Orchard.Projections.ViewModels;

namespace Laser.Orchard.ContentExtension.ViewModels {
    public class DynamicProjectionVM {
        public ProjectionPartEditViewModel Projection { get; set; }
        public DynamicProjectionPartVM Part { get; set; }
        public string Tenant { get; set; }
        public SelectList ListFormFile {
            get
            {
                try {
                    string[] filelist = Directory.GetFiles(HostingEnvironment.MapPath(@"~/App_Data/Sites/" + Tenant + @"/Code"));
                    return new SelectList(filelist.Select(x => new SelectListItem { Text = Path.GetFileName(x), Value = Path.GetFileName(x) }), "Value", "Text", Part.Shape);
                }catch { return null; }
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
}