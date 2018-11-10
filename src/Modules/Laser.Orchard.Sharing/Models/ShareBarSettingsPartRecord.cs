using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Sharing.Models {
    public class ShareBarSettingsPartRecord : ContentPartRecord {
        public virtual string AddThisAccount { get; set; }
    }
}