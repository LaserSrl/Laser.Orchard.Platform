using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;


namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityUserPart : ContentPart<FidelityUserPartRecord>
    {
        public string FidelityUsername
        {
            get { return this.Retrieve(x => x.FidelityUsername); }
            set { this.Store(x => x.FidelityUsername, value); }
        }

        public string FidelityPassword
        {
            get { return this.Retrieve(x => x.FidelityPassword); }
            set { this.Store(x => x.FidelityPassword, value); }
        }

        public string CustomerId
        {
            get { return this.Retrieve(x => x.CustomerId); }
            set { this.Store(x => x.CustomerId, value); }
        }
    }

    public class FidelityUserPartRecord : ContentPartRecord
    {
        public virtual string FidelityUsername { set; get; }

        public virtual string FidelityPassword { set; get; }

        public virtual string CustomerId { set; get; }
    }
}