namespace Proligence.QrCodes.Models {
    using Orchard.ContentManagement;

    public class QrCodePart : ContentPart<QrCodePartRecord> {
        public string Value { get; set; }
        public int Size { get; set; }
        public string ActualValue { get; set; }
    }
}
