using Orchard.Data.Conventions;

namespace Laser.Orchard.Mobile.Models {
    public class ManifestAppFileRecord {
        public virtual int Id { get; set; }
        [StringLengthMax] 
        public virtual string FileContent { get; set; }
        public virtual bool Enable { get; set; }
        [StringLengthMax] 
        public virtual string GoogleFileContent { get; set; }
        public virtual bool GoogleEnable { get; set; }
        [StringLengthMax]
        public virtual string DeveloperDomainText { get; set; }
        public virtual bool EnableDeveloperDomain { get; set; }
    }
}