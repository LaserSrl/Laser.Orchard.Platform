using LaserProjections = Laser.Orchard.StartupConfig.Projections;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class DateCouponApplicabilityCriteria
        : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly Lazy<CultureInfo> _cultureInfo;

        public DateCouponApplicabilityCriteria(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) : base(workContextAccessor, cacheManager, signals) {
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
        }

        public override string ProviderName => "DateCouponApplicabilityCriteria";

        public override LocalizedString ProviderDisplayName => T("Criterion on the date validity of the coupon.");

        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
                .For("Date validity",
                    T("Range of dates the coupon can be used in"),
                    T("Range of dates the coupon can be used in"))
                .Element("Date validity of the coupon",
                    T("Range of dates the coupon can be used in"),
                    T("Range of dates the coupon can be used in"),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => ApplyCriterion(ctx),
                    (ctx) => Display(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    LaserProjections.DateTimeFilterForm.FormName);
        }

        private void ApplyCriterion(CouponApplicabilityCriterionContext context) {
            if (context.IsApplicable) {
                var formState = context.State;

                var op = (DateTimeOperator)Enum.Parse(typeof(DateTimeOperator), Convert.ToString(formState.Operator));

                string dateFrom = Convert.ToString(formState.DateFrom);
                string timeFrom = Convert.ToString(formState.TimeFrom);
                string dateTo = Convert.ToString(formState.DateTo);
                string timeTo = Convert.ToString(formState.TimeTo);

                switch (op) {
                    case DateTimeOperator.LessThan:
                        if (DateTime.UtcNow > Convert.ToDateTime(dateTo + " " + timeTo, CultureInfo.InvariantCulture)) {
                            context.IsApplicable = false;
                            context.ApplicabilityContext.IsApplicable = false;
                        }
                        break;

                    case DateTimeOperator.GreaterThan:
                        if (DateTime.UtcNow < Convert.ToDateTime(dateFrom + " " + timeFrom, CultureInfo.InvariantCulture)) {
                            context.IsApplicable = false;
                            context.ApplicabilityContext.IsApplicable = false;
                        }
                        break;

                    case DateTimeOperator.Between:
                        if (DateTime.UtcNow < Convert.ToDateTime(dateFrom + " " + timeFrom, CultureInfo.InvariantCulture) || 
                                DateTime.UtcNow > Convert.ToDateTime(dateTo + " " + timeTo, CultureInfo.InvariantCulture)) {
                            context.IsApplicable = false;
                            context.ApplicabilityContext.IsApplicable = false;
                        }

                        break;

                    default:
                        // Invalid operator.
                        context.IsApplicable = false;
                        context.ApplicabilityContext.IsApplicable = false;
                        break;

                }


            }
        }

        private LocalizedString Display(CouponContext context) {
            var op = (DateTimeOperator)Enum.Parse(typeof(DateTimeOperator), Convert.ToString(context.State.Operator));

            string dateFrom = Convert.ToString(context.State.DateFrom);
            string timeFrom = Convert.ToString(context.State.TimeFrom);
            string dateTo = Convert.ToString(context.State.DateTo);
            string timeTo = Convert.ToString(context.State.TimeTo);

            switch (op) {
                case DateTimeOperator.LessThan:
                    return T("{0} is before {1} {2}", T("Current date").Text, dateTo, timeTo);
                    
                case DateTimeOperator.GreaterThan:
                    return T("{0} is after {1} {2}", T("Current date").Text, dateFrom, timeFrom);

                case DateTimeOperator.Between:
                    return T("{0} is between {1} {2} and {3} {4}", T("Current date").Text, dateFrom, timeFrom, dateTo, timeTo);

                default:
                    return T("Invalid operator");

            }
        }
    }
}
