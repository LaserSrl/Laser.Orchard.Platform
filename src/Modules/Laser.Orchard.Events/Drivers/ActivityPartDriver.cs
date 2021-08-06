using AutoMapper;
using Laser.Orchard.Events.Models;
using Laser.Orchard.Events.ViewModels;
using Laser.Orchard.StartupConfig.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Globalization;
using Orchard.ContentManagement.Handlers;
using Orchard;

namespace Laser.Orchard.Events.Drivers {
    public class ActivityPartDriver : ContentPartCloningDriver<ActivityPart> {
        public Localizer T { get; set; }

        private readonly IDateLocalization _dataLocalization;
        private readonly IOrchardServices _orchardServices;

        public ActivityPartDriver(IDateLocalization dataLocalization, IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            _dataLocalization = dataLocalization;
            _orchardServices = orchardServices;
        }

        protected override string Prefix
        {
            get { return "Activity"; }
        }

        /// <summary>
        /// Defines the shapes required for the part's main view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="displayType">The display type.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Display(ActivityPart part, string displayType, dynamic shapeHelper) {

            var partSettings = part.Settings.GetModel<ActivityPartSettings>();

            DateTime? localDateTimeStart = _dataLocalization.ReadDateLocalized(part.DateTimeStart);
            DateTime? localDateTimeEnd = _dataLocalization.ReadDateLocalized(part.DateTimeEnd);
            DateTime? localDateRepeatEnd = _dataLocalization.ReadDateLocalized(part.RepeatEndDate);
            ActivityViewModel activityVM = new ActivityViewModel();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<ActivityPart, ActivityViewModel>()
                    .ForMember(dest => dest.DateStart, opt => opt.Ignore())
                    .ForMember(dest => dest.DateEnd, opt => opt.Ignore())
                    .ForMember(dest => dest.TimeStart, opt => opt.Ignore())
                    .ForMember(dest => dest.TimeEnd, opt => opt.Ignore())
                    .ForMember(dest => dest.RepeatEndDate, opt => opt.Ignore())
                    .ForMember(dest => dest.Monday, opt => opt.Ignore())
                    .ForMember(dest => dest.Tuesday, opt => opt.Ignore())
                    .ForMember(dest => dest.Wednesday, opt => opt.Ignore())
                    .ForMember(dest => dest.Thursday, opt => opt.Ignore())
                    .ForMember(dest => dest.Friday, opt => opt.Ignore())
                    .ForMember(dest => dest.Saturday, opt => opt.Ignore())
                    .ForMember(dest => dest.Sunday, opt => opt.Ignore())
                    .ForMember(dest => dest.RepeatByDayNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.Settings, opt => opt.Ignore());
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map(part, activityVM);

            activityVM.DateStart = _dataLocalization.WriteDateLocalized(localDateTimeStart);
            activityVM.DateEnd = _dataLocalization.WriteDateLocalized(localDateTimeEnd);
            activityVM.TimeStart = _dataLocalization.WriteTimeLocalized(localDateTimeStart);
            activityVM.TimeEnd = _dataLocalization.WriteTimeLocalized(localDateTimeEnd);
            activityVM.RepeatEndDate = _dataLocalization.WriteDateLocalized(part.RepeatEndDate);
            activityVM.Settings = partSettings;
            if (part.RepeatType == "W") {
                activityVM.Monday = part.RepeatDetails.Contains(DayOfWeek.Monday.ToString());
                activityVM.Tuesday = part.RepeatDetails.Contains(DayOfWeek.Tuesday.ToString());
                activityVM.Wednesday = part.RepeatDetails.Contains(DayOfWeek.Wednesday.ToString());
                activityVM.Thursday = part.RepeatDetails.Contains(DayOfWeek.Thursday.ToString());
                activityVM.Friday = part.RepeatDetails.Contains(DayOfWeek.Friday.ToString());
                activityVM.Saturday = part.RepeatDetails.Contains(DayOfWeek.Saturday.ToString());
                activityVM.Sunday = part.RepeatDetails.Contains(DayOfWeek.Sunday.ToString());
            } else {
                activityVM.Monday = false;
                activityVM.Tuesday = false;
                activityVM.Wednesday = false;
                activityVM.Thursday = false;
                activityVM.Friday = false;
                activityVM.Saturday = false;
                activityVM.Sunday = false;
            }

            if (part.RepeatType == "M")
                activityVM.RepeatByDayNumber = part.RepeatDetails.Contains("DayNum");
            else
                activityVM.RepeatByDayNumber = true;

            return ContentShape("Parts_ActivityDisplay",
                                    () => shapeHelper.EditorTemplate(
                                          TemplateName: "Parts/ActivityDisplay",
                                          Model: activityVM,
                                          Prefix: Prefix));
        }

        /// <summary>
        /// Defines the shapes required for the editor view. Runs upon the GET of the editor view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Editor(ActivityPart part, dynamic shapeHelper) {
            var partSettings = part.Settings.GetModel<ActivityPartSettings>();

            DateTime? localDateTimeStart = _dataLocalization.ReadDateLocalized(part.DateTimeStart);
            DateTime? localDateTimeEnd = _dataLocalization.ReadDateLocalized(part.DateTimeEnd);
            DateTime? localDateRepeatEnd = _dataLocalization.ReadDateLocalized(part.RepeatEndDate);
            ActivityViewModel activityVM = new ActivityViewModel();

            CultureInfo culture = CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            activityVM.DateFormat = culture.DateTimeFormat.ShortDatePattern;

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<ActivityPart, ActivityViewModel>()
                    .ForMember(dest => dest.DateStart, opt => opt.Ignore())
                    .ForMember(dest => dest.DateEnd, opt => opt.Ignore())
                    .ForMember(dest => dest.TimeStart, opt => opt.Ignore())
                    .ForMember(dest => dest.TimeEnd, opt => opt.Ignore())
                    .ForMember(dest => dest.RepeatEndDate, opt => opt.Ignore())
                    .ForMember(dest => dest.Monday, opt => opt.Ignore())
                    .ForMember(dest => dest.Tuesday, opt => opt.Ignore())
                    .ForMember(dest => dest.Wednesday, opt => opt.Ignore())
                    .ForMember(dest => dest.Thursday, opt => opt.Ignore())
                    .ForMember(dest => dest.Friday, opt => opt.Ignore())
                    .ForMember(dest => dest.Saturday, opt => opt.Ignore())
                    .ForMember(dest => dest.Sunday, opt => opt.Ignore())
                    .ForMember(dest => dest.RepeatByDayNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.Settings, opt => opt.Ignore());
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map(part, activityVM);

            activityVM.DateStart = _dataLocalization.WriteDateLocalized(localDateTimeStart);
            activityVM.DateEnd = _dataLocalization.WriteDateLocalized(localDateTimeEnd);
            activityVM.TimeStart = _dataLocalization.WriteTimeLocalized(localDateTimeStart);
            activityVM.TimeEnd = _dataLocalization.WriteTimeLocalized(localDateTimeEnd);
            activityVM.RepeatEndDate = _dataLocalization.WriteDateLocalized(part.RepeatEndDate);
            activityVM.Settings = partSettings;
            if (part.RepeatType == "W") {
                activityVM.Monday = part.RepeatDetails.Contains(DayOfWeek.Monday.ToString());
                activityVM.Tuesday = part.RepeatDetails.Contains(DayOfWeek.Tuesday.ToString());
                activityVM.Wednesday = part.RepeatDetails.Contains(DayOfWeek.Wednesday.ToString());
                activityVM.Thursday = part.RepeatDetails.Contains(DayOfWeek.Thursday.ToString());
                activityVM.Friday = part.RepeatDetails.Contains(DayOfWeek.Friday.ToString());
                activityVM.Saturday = part.RepeatDetails.Contains(DayOfWeek.Saturday.ToString());
                activityVM.Sunday = part.RepeatDetails.Contains(DayOfWeek.Sunday.ToString());
            }
            else {
                activityVM.Monday = false;
                activityVM.Tuesday = false;
                activityVM.Wednesday = false;
                activityVM.Thursday = false;
                activityVM.Friday = false;
                activityVM.Saturday = false;
                activityVM.Sunday = false;
            }

            if (part.RepeatType == "M")
                activityVM.RepeatByDayNumber = part.RepeatDetails.Contains("DayNum");
            else
                activityVM.RepeatByDayNumber = true;

            return ContentShape("Parts_Activity_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/Activity",
                                    Model: activityVM,
                                    Prefix: Prefix));
        }

        /// <summary>
        /// Runs upon the POST of the editor view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="updater">The updater.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Editor(ActivityPart part, IUpdateModel updater, dynamic shapeHelper) {
            var partSettings = part.Settings.GetModel<ActivityPartSettings>();
            ActivityViewModel activityVM = new ActivityViewModel();

            if (updater.TryUpdateModel(activityVM, Prefix, null, null)) {
               

                if (partSettings.DateTimeType == DateTimeTypes.FromToOnSameDate) {
                        activityVM.DateEnd = activityVM.DateStart;
                }

                var mapperConfiguration = new MapperConfiguration(cfg => {
                    cfg.CreateMap<ActivityViewModel, ActivityPart>()
                    .ForMember(dest => dest.DateTimeStart, opt => opt.Ignore())
                    .ForMember(dest => dest.DateTimeEnd, opt => opt.Ignore())
                    .ForMember(dest => dest.RepeatEndDate, opt => opt.Ignore())
                    .ForMember(dest => dest.RepeatDetails, opt => opt.Ignore())
                    //ContentPart has a Settings property, that would clash with the Settings property from ActivityViewModel
                    .ForMember(dest => dest.Settings, opt => opt.Ignore());
                });
                IMapper _mapper = mapperConfiguration.CreateMapper();

                _mapper.Map(activityVM, part);

                if (!partSettings.UseRecurrences) part.Repeat = false;

                if (!String.IsNullOrWhiteSpace(activityVM.DateStart) && 

                    (!String.IsNullOrWhiteSpace(activityVM.DateEnd) || partSettings.SingleDate) &&
                    (activityVM.AllDay || (!activityVM.AllDay && !String.IsNullOrWhiteSpace(activityVM.TimeStart) && !String.IsNullOrWhiteSpace(activityVM.TimeEnd)))) {
                    try {
                        DateTime? startDate;
                        DateTime? endDate;

                        if (activityVM.AllDay) {
                            startDate = _dataLocalization.StringToDatetime(activityVM.DateStart, "");
                            endDate = _dataLocalization.StringToDatetime(activityVM.DateEnd, "");
                        } else {
                            startDate = _dataLocalization.StringToDatetime(activityVM.DateStart, activityVM.TimeStart).Value;
                            endDate = _dataLocalization.StringToDatetime(activityVM.DateEnd, activityVM.TimeEnd).Value;
                            if (partSettings.DateTimeType == DateTimeTypes.FromToOnSameDate && startDate.Value.TimeOfDay > endDate.Value.TimeOfDay)
                                endDate = endDate.Value.AddDays(1);
                        }

                        if(!partSettings.SingleDate && startDate.HasValue && endDate.HasValue && DateTime.Compare(startDate.Value, endDate.Value) > 0) {
                            updater.AddModelError(Prefix + "DateFormatError", T("The ending date is greater than the starting date."));
                        }

                        part.DateTimeStart = startDate;
                        part.DateTimeEnd = endDate;
  

                        if (partSettings.SingleDate) {
                            part.DateTimeEnd = part.DateTimeStart;
                        }

                    }catch(OrchardException) {
                        updater.AddModelError(Prefix + "DateFormatError", T("The starting date or ending date are not valid."));
                    }                  
                }
                else
                    updater.AddModelError(Prefix + "DateRequiredError", T("The starting date and ending date of the event are required."));

                if (part.Repeat) {
                    try{
                        part.RepeatEndDate = _dataLocalization.StringToDatetime(activityVM.RepeatEndDate, "");
                    } catch (OrchardException) {
                        updater.AddModelError(Prefix + "DateRepeateFormatError", T("The repeat date is not valid."));
                    }

                    if (part.RepeatEnd && part.RepeatEndDate == null){
                        updater.AddModelError(Prefix + "DateRepeateRequiredError", T("The repeat end date is required."));
                    }
                    string repeatDetails = "";
                    if (activityVM.RepeatType == "W") {
                        if (activityVM.Monday)
                            repeatDetails += DayOfWeek.Monday + ",";
                        if (activityVM.Tuesday)
                            repeatDetails += DayOfWeek.Tuesday + ",";
                        if (activityVM.Wednesday)
                            repeatDetails += DayOfWeek.Wednesday + ",";
                        if (activityVM.Thursday)
                            repeatDetails += DayOfWeek.Thursday + ",";
                        if (activityVM.Friday)
                            repeatDetails += DayOfWeek.Friday + ",";
                        if (activityVM.Saturday)
                            repeatDetails += DayOfWeek.Saturday + ",";
                        if (activityVM.Sunday)
                            repeatDetails += DayOfWeek.Sunday + ",";
                    }
                    else if (activityVM.RepeatType == "M") {
                        if (activityVM.RepeatByDayNumber)
                            repeatDetails = "DayNum";
                        else
                            repeatDetails = "DayWeek";
                    }
                    if (repeatDetails.EndsWith(",")) {
                        repeatDetails = repeatDetails.Substring(0, repeatDetails.Length - 1);
                    }
                    part.RepeatDetails = repeatDetails;
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(ActivityPart part, global::Orchard.ContentManagement.Handlers.ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var AllDay = root.Attribute("AllDay");
            if (AllDay != null) {
                part.AllDay = Convert.ToBoolean(AllDay.Value);
            }
            var DateTimeEnd = root.Attribute("DateTimeEnd");
            if (DateTimeEnd != null) {
                part.DateTimeEnd = Convert.ToDateTime(DateTimeEnd.Value, CultureInfo.InvariantCulture);
            }
            var DateTimeStart = root.Attribute("DateTimeStart");
            if (DateTimeStart != null) {
                part.DateTimeStart = Convert.ToDateTime(DateTimeStart.Value, CultureInfo.InvariantCulture);
            }
            var Repeat = root.Attribute("Repeat");
            if (Repeat != null) {
                part.Repeat = Convert.ToBoolean(Repeat.Value);
            }
            var RepeatDetails = root.Attribute("RepeatDetails");
            if (RepeatDetails != null) {
                part.RepeatDetails = RepeatDetails.Value;
            }
            var RepeatEnd = root.Attribute("RepeatEnd");
            if (RepeatEnd != null) {
                part.RepeatEnd = Convert.ToBoolean(RepeatEnd.Value);
            }
            var RepeatEndDate = root.Attribute("RepeatEndDate");
            if (RepeatEndDate != null) {
                part.RepeatEndDate = Convert.ToDateTime(RepeatEndDate.Value, CultureInfo.InvariantCulture);
            }
            var RepeatType = root.Attribute("RepeatType");
            if (RepeatType != null) {
                part.RepeatType = RepeatType.Value;
            }
            var RepeatValue = root.Attribute("RepeatValue");
            if (RepeatValue != null) {
                part.RepeatValue = Convert.ToInt32(RepeatValue.Value);
            }
        }

        protected override void Exporting(ActivityPart part, global::Orchard.ContentManagement.Handlers.ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("AllDay", part.AllDay);
            root.SetAttributeValue("DateTimeEnd", part.DateTimeEnd.HasValue ? part.DateTimeEnd.Value.ToString(CultureInfo.InvariantCulture) : null);
            root.SetAttributeValue("DateTimeStart", part.DateTimeStart.HasValue ? part.DateTimeStart.Value.ToString(CultureInfo.InvariantCulture) : null);
            root.SetAttributeValue("Repeat", part.Repeat);
            root.SetAttributeValue("RepeatDetails", part.RepeatDetails);
            root.SetAttributeValue("RepeatEnd", part.RepeatEnd);
            root.SetAttributeValue("RepeatEndDate", part.RepeatEndDate.HasValue ? part.RepeatEndDate.Value.ToString(CultureInfo.InvariantCulture) : null);
            root.SetAttributeValue("RepeatType", part.RepeatType);
            root.SetAttributeValue("RepeatValue", part.RepeatValue);
        }

        protected override void Cloning(ActivityPart originalPart, ActivityPart clonePart, CloneContentContext context) {
            clonePart.DateTimeStart = originalPart.DateTimeStart;
            clonePart.DateTimeEnd = originalPart.DateTimeEnd;
            clonePart.AllDay = originalPart.AllDay;
            clonePart.Repeat = originalPart.Repeat;
            clonePart.RepeatType = originalPart.RepeatType;
            clonePart.RepeatValue = originalPart.RepeatValue;
            clonePart.RepeatDetails = originalPart.RepeatDetails;
            clonePart.RepeatEnd = originalPart.RepeatEnd;
            clonePart.RepeatEndDate = originalPart.RepeatEndDate;
        }
    }
}