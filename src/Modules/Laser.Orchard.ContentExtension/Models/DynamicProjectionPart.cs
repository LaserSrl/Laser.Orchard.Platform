using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using Orchard.Projections.Models;

namespace Laser.Orchard.ContentExtension.Models {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionPart : ContentPart<DynamicProjectionPartRecord> {
        public string MenuLabel {
            get {return this.Retrieve(x => x.MenuLabel);}
            set { this.Store(x => x.MenuLabel, value); }
        }
        public string Icon {
            get { return this.Retrieve(x => x.Icon); }
            set { this.Store(x => x.Icon, value); }
        }
        public bool OnAdminMenu {
            get { return this.Retrieve<bool>(x=>x.OnAdminMenu); }
            set { this.Store(x => x.OnAdminMenu, value); }
        }

        [StringLength(DynamicProjectionPartRecord.DefaultMenuTextLength)]
        public string AdminMenuText {
            get { return this.Retrieve<string>(x => x.AdminMenuText); }
            set { this.Store(x => x.AdminMenuText, value); }
        }

        public string AdminMenuPosition {
            get { return this.Retrieve<string>(x => x.AdminMenuPosition); }
            set { this.Store(x => x.AdminMenuPosition, value); }
        }

        public string Shape {
            get { return this.Retrieve<string>(x => x.Shape); }
            set { this.Store(x => x.Shape, value); }
        }

        public bool ReturnsHqlResults {
            get { return this.Retrieve<bool>(x => x.ReturnsHqlResults); }
            set { this.Store(x => x.ReturnsHqlResults, value); }
        }

        public string TypeForFilterForm {
            get { return this.Retrieve<string>(x => x.TypeForFilterForm); }
            set { this.Store(x => x.TypeForFilterForm, value); }
        }

        public string ShapeForResults {
            get { return this.Retrieve<string>(x => x.ShapeForResults); }
            set { this.Store(x => x.ShapeForResults, value); }
        }

    }
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionPartRecord:ContentPartRecord {
        public const ushort DefaultMenuTextLength = 255;

        public DynamicProjectionPartRecord() {
            MaxItems = 0;
        }

        [StringLength(DefaultMenuTextLength)]
        public virtual string AdminMenuText { get; set; }
        public virtual string AdminMenuPosition { get; set; }
        public virtual bool OnAdminMenu { get; set; }
        public virtual string Icon { get; set; }
        public virtual string Shape { get; set; } /*Shape for custom filter forms */
        public virtual string TypeForFilterForm { get; set; }
        public virtual bool ReturnsHqlResults { get; set; }
        public virtual string ShapeForResults { get; set; }



        public virtual int Items { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public virtual int ItemsPerPage { get; set; }

        /// <summary>
        /// Number of items to skip
        /// </summary>
        public virtual int Skip { get; set; }

        /// <summary>
        /// Suffix to use when multiple pagers are available on the same page
        /// </summary>
        [StringLength(255)]
        public virtual string PagerSuffix { get; set; }

        /// <summary>
        /// The maximum number of items which can be requested at once. 
        /// </summary>
        public virtual int MaxItems { get; set; }

        /// <summary>
        /// True to render a pager
        /// </summary>
        public virtual bool DisplayPager { get; set; }

        /// <summary>
        /// The query to execute
        /// </summary>
        [Aggregate]
        public virtual QueryPartRecord QueryPartRecord { get; set; }

        /// <summary>
        /// The layout to render
        /// </summary>
        [Aggregate]
        public virtual LayoutRecord LayoutRecord { get; set; }
    }
}