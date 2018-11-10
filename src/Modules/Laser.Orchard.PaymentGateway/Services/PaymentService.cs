using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.PaymentGateway.Services {
    public class PaymentService : IPaymentService {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<PaymentRecord> _repository;
        private readonly ICommonsServices _commonsServices;

        public PaymentService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, ICommonsServices commonsServices) {
            _orchardServices = orchardServices;
            _commonsServices = commonsServices;
            _repository = repository;
        }
        public List<PaymentRecord> GetPayments(int userId, bool lastToFirst = true) {
            List<PaymentRecord> result = null;
            IEnumerable<PaymentRecord> records = null;
            if (lastToFirst) {
                records = _repository.Fetch(x => x.UserId == userId, y => y.Desc(z => z.UpdateDate));
            }
            else {
                records = _repository.Fetch(x => x.UserId == userId, y => y.Asc(z => z.UpdateDate));
            }
            result = records.ToList();
            return result;
        }
        public List<PaymentRecord> GetAllPayments(bool lastToFirst = true) {
            List<PaymentRecord> result = null;
            IEnumerable<PaymentRecord> records = null;
            if (lastToFirst) {
                records = _repository.Fetch(x => true, y => y.Desc(z => z.UpdateDate));
            }
            else {
                records = _repository.Fetch(x => true, y => y.Asc(z => z.UpdateDate));
            }
            result = records.ToList();
            return result;
        }
        public PaymentRecord GetPayment(int paymentId) {
            PaymentRecord record = null;
            var list = _repository.Fetch(x => x.Id == paymentId);
            if(list != null) {
                record = list.FirstOrDefault();
            }
            return record;
        }
        public PaymentRecord GetPaymentByTransactionId(string transactionId) {
            PaymentRecord record = null;
            var list = _repository.Fetch(x => x.TransactionId == transactionId);
            if (list != null) {
                record = list.FirstOrDefault();
            }
            return record;
        }
        public PaymentRecord GetPaymentByGuid(string paymentGuid) {
            PaymentRecord record = null;
            var list = _repository.Fetch(x => x.Guid == paymentGuid);
            if (list != null) {
                record = list.FirstOrDefault();
            }
            return record;
        }
        public string CreatePaymentNonce(PaymentRecord paymentData) {
            var data = string.Format("reason={0}&amount={1}&currency={2}&itemId={3}",
                Uri.EscapeDataString(paymentData.Reason),
                Uri.EscapeDataString(Convert.ToString(paymentData.Amount, CultureInfo.InvariantCulture)),
                Uri.EscapeDataString(paymentData.Currency),
                Uri.EscapeDataString(Convert.ToString(paymentData.ContentItemId, CultureInfo.InvariantCulture)));
            var nonceDuration = _orchardServices.WorkContext.CurrentSite.As<PaymentGatewaySiteSettingsPart>().NonceMinutesDuration;
            return Uri.EscapeDataString(_commonsServices.CreateNonce(data , TimeSpan.FromMinutes(nonceDuration)));
        }
        public PaymentRecord DecryptPaymentNonce(string nonce) {
            PaymentRecord record = null;
            string parametri = null;
            if(_commonsServices.DecryptNonce(Uri.UnescapeDataString(nonce), out parametri)) {
                record = new PaymentRecord();
                var aux = parametri.Split('&');
                foreach(var par in aux) {
                    var kv = par.Split('=');
                    if(kv.Length == 2) {
                        switch (kv[0]) {
                            case "reason":
                                record.Reason = Uri.UnescapeDataString(kv[1]);
                                break;
                            case "amount":
                                record.Amount = Convert.ToDecimal(Uri.UnescapeDataString(kv[1]), CultureInfo.InvariantCulture);
                                break;
                            case "currency":
                                record.Currency = Uri.UnescapeDataString(kv[1]);
                                break;
                            case "itemId":
                                record.ContentItemId = Convert.ToInt32(Uri.UnescapeDataString(kv[1]), CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                }
            }
            return record;
        }
    }
}