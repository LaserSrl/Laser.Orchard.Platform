using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Localization;
using Laser.Orchard.DataProtection.Services;

namespace Laser.Orchard.DataProtection.Projections {
    public class ApplyDataProtectionQueryFilter : IFilterProvider {
        private readonly IDataProtectionCheckerService _dataProtectionCheckerService;
        public Localizer T { get; set; }
        public ApplyDataProtectionQueryFilter(IDataProtectionCheckerService dataProtectionCheckerService) {
            _dataProtectionCheckerService = dataProtectionCheckerService;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeFilterContext describe) {
            describe.For("Search", T("DataProtection"), T("Data Protection"))
                .Element("Data Protection Filter", T("Apply Data Protection"), T("Apply Data Protection to current query."),
                    ApplyFilter,
                    DisplayFilter);
        }
        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Apply user data restrictions to current query.");
        }
        public void ApplyFilter(FilterContext context) {
            _dataProtectionCheckerService.CheckDataRestricitons(context);
        }
    }
}