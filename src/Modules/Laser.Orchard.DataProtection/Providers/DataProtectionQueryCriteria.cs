using Orchard.ContentManagement;
using NHibernate;
using Orchard.UI.Admin;
using System.Web;
using Laser.Orchard.DataProtection.Services;
using Orchard;

namespace Laser.Orchard.DataProtection.Providers {
    public class DataProtectionQueryCriteria : IGlobalCriteriaProvider {
        private readonly IDataProtectionCheckerService _dataProtectionCheckerService;
        public DataProtectionQueryCriteria(IDataProtectionCheckerService dataProtectionCheckerService) {
            _dataProtectionCheckerService = dataProtectionCheckerService;
        }
        public void AddCriteria(ICriteria criteria) {
            _dataProtectionCheckerService.CheckDataRestricitons(criteria);
        }
    }
}