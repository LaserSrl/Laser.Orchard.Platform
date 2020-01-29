using System;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class PaymentInfoViewModel {
        public string PosName { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public string TransationId { get; set; }
    }
}