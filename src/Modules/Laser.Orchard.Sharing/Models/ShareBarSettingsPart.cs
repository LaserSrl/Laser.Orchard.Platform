using Orchard.ContentManagement;

namespace Laser.Orchard.Sharing.Models
{
    public class ShareBarSettingsPart : ContentPart<ShareBarSettingsPartRecord>
    {
        public string AddThisAccount
        {
            get { return Record.AddThisAccount; }
            set { Record.AddThisAccount = value; }
        }
    }
}