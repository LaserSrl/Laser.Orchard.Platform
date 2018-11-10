namespace Proligence.QrCodes.Models {
    using Orchard.ContentManagement.Records;
    using Orchard.Environment.Extensions;

    public class QrCodePartRecord : ContentPartRecord {
        public virtual string Value { get; set; }
        public virtual int Size { get; set; }
    }
}