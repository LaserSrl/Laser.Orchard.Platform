using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Laser.Orchard.TaskScheduler.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization.Services;

namespace Laser.Orchard.TaskScheduler.ViewModels {

  
    public class ScheduledTaskViewModel {

        public ContentItem LinkedContent { get; set; }
        //properties corresponding to the part
        public int Id;
        public string SignalName { get; set; }
        private DateTime? _scheduledStartUTC;
        public DateTimeEditor ScheduledStartUTCEditor
        {
            get
            {
                return new DateTimeEditor {
                    Date = _scheduledStartUTC == null
                      ? DateTime.MinValue.ToString("d", culture) //CultureInfo.InvariantCulture)
                      : _scheduledStartUTC.Value.ToString("d", culture), //CultureInfo.InvariantCulture),
                    Time = _scheduledStartUTC == null
                      ? DateTime.MinValue.TimeOfDay.ToString(@"hh\:mm")
                      : _scheduledStartUTC.Value.TimeOfDay.ToString(@"hh\:mm"),
                    ShowDate = true,
                    ShowTime = true
                };
            }
            set
            {
                DateTime? dateFromString = _dateServices.ConvertFromLocalizedDateString(value.Date);
                if (dateFromString != null) {
                    //we want to keep the Time part of _scheduledStartUTC, and only update the Date part of it
                    if (_scheduledStartUTC != null) {
                        _scheduledStartUTC = new DateTime(
                            year: dateFromString.Value.Year,
                            month: dateFromString.Value.Month,
                            day: dateFromString.Value.Day,
                            hour: _scheduledStartUTC.Value.Hour,
                            minute: _scheduledStartUTC.Value.Minute,
                            second: _scheduledStartUTC.Value.Second,
                            millisecond: ScheduledStartUTC.Value.Millisecond);
                    }
                    else {
                        _scheduledStartUTC = new DateTime(
                            year: dateFromString.Value.Year,
                            month: dateFromString.Value.Month,
                            day: dateFromString.Value.Day);
                    }
                }
                TimeSpan timeFromString;
                if (TimeSpan.TryParse(value.Time, out timeFromString)) {
                    //we want to keep the Date part of _scheduledStartUTC, and only update the Time part of it
                    if (_scheduledStartUTC != null) {
                        _scheduledStartUTC = new DateTime(
                            year: _scheduledStartUTC.Value.Year,
                            month: _scheduledStartUTC.Value.Month,
                            day: _scheduledStartUTC.Value.Day,
                            hour: timeFromString.Hours,
                            minute: timeFromString.Minutes,
                            second: timeFromString.Seconds);
                    }
                    else {
                        _scheduledStartUTC = DateTime.Today.Add(timeFromString);
                    }
                }
            }
        }
        public DateTime? ScheduledStartUTC
        {
            get { return _scheduledStartUTC; }
            set { _scheduledStartUTC = value; }
        }
        public int PeriodicityTime { get; set; }
        public TimeUnits PeriodicityUnit { get; set; }
        public int ContentItemId { get; set; }
        public int Running { get; set; }
        //boolean to mark task for deletion
        public bool Delete { get; set; }
        //boolean to mark task for scheduling/unscheduling
        public bool Scheduling;
        public bool Autodestroy{get;set;}

        public ExecutionTypes ExecutionType { get; set; }
        public bool LongTask { get; set; }
        //these properties are public so that reflection can get at them when generating nested forms
        public readonly IDateLocalizationServices _dateServices;
        public CultureInfo culture;

        public ScheduledTaskViewModel() {
            //this empty constructor is used for reflection when creating nested forms
            //there we have to pass the cultureInffo and the date service as well
            _scheduledStartUTC = DateTime.UtcNow.ToLocalTime();
            PeriodicityUnit = TimeUnits.Minutes;
            ExecutionType = ExecutionTypes.WorkFlow;
            Running = 0;
            Delete = false;
            LinkedContent = null;
        }
        public ScheduledTaskViewModel(ScheduledTaskPart part) {
            Id = part.Id;
            SignalName = part.SignalName;
            ScheduledStartUTC = part.ScheduledStartUTC == null ? (DateTime?)null :
                part.ScheduledStartUTC.Value.ToLocalTime();
            PeriodicityTime = part.PeriodicityTime;
            PeriodicityUnit = part.PeriodicityUnit;
            ContentItemId = part.ContentItemId;
            Running = part.RunningTaskId;
            Delete = false;
            Autodestroy = part.Autodestroy;
            ExecutionType = part.ExecutionType;
            LongTask = part.LongTask;
        }

        public ScheduledTaskViewModel(IOrchardServices orchardServices, IDateLocalizationServices dateServices) {
            _dateServices = dateServices;
            culture = new CultureInfo(orchardServices.WorkContext.CurrentCulture);

            _scheduledStartUTC = DateTime.UtcNow;
            PeriodicityUnit = TimeUnits.Minutes;
            ExecutionType = ExecutionTypes.WorkFlow;
            Running = 0;
            Delete = false;
            LongTask = false;
        }

        public ScheduledTaskViewModel(ScheduledTaskPart part, IOrchardServices orchardServices, IDateLocalizationServices dateServices) : this(orchardServices, dateServices) {
            Id = part.Id;
            SignalName = part.SignalName;
            _scheduledStartUTC = part.ScheduledStartUTC == null ? (DateTime?)null :
                part.ScheduledStartUTC.Value.ToLocalTime();
            PeriodicityTime = part.PeriodicityTime;
            PeriodicityUnit = part.PeriodicityUnit;
            ContentItemId = part.ContentItemId;
            Running = part.RunningTaskId;
            Delete = false;
            Autodestroy = part.Autodestroy;
            ExecutionType = part.ExecutionType;
            LongTask = part.LongTask;
        }

        //public ScheduledTaskPart CreatePartFromVM() {
        //    return new ScheduledTaskPart {
        //        SignalName = this.SignalName,
        //        ScheduledStartUTC = this.ScheduledStartUTC,
        //        PeriodicityTime = this.PeriodicityTime,
        //        PeriodicityUnit = this.PeriodicityUnit,
        //        ContentItemId = this.ContentItemId,
        //        RunningTaskId = this.Running
        //    };
        //}

        //public LaserTaskSchedulerRecord CreateRecordFromVM() {
        //    LaserTaskSchedulerRecord ltsr = new LaserTaskSchedulerRecord();
        //    ltsr.SignalName = this.SignalName;
        //    ltsr.ScheduledStartUTC = this.ScheduledStartUTC;
        //    ltsr.PeriodicityTime = this.PeriodicityTime;
        //    ltsr.PeriodicityUnit = this.PeriodicityUnit.ToString();
        //    ltsr.ContentItemId = this.ContentItemId;
        //    ltsr.RunningTaskId = this.Running;
        //    return ltsr;
        //    //return new LaserTaskSchedulerRecord {
        //    //    SignalName = this.SignalName,
        //    //    ScheduledStartUTC = this.ScheduledStartUTC,
        //    //    PeriodicityTime = this.PeriodicityTime,
        //    //    PeriodicityUnit = this.PeriodicityUnit.ToString(),
        //    //    ContentItemId = this.ContentItemId,
        //    //    RunningTaskId = this.Running
        //    //};
        //}

        public void UpdatePart(ScheduledTaskPart part) {
            part.SignalName = this.SignalName;
            try {
                part.ScheduledStartUTC = _scheduledStartUTC == null ? (DateTime?)null :
                    _scheduledStartUTC.Value.ToUniversalTime();
            }
            catch (Exception) {
                //the date in the input was not valid
                part.ScheduledStartUTC = null;
            }
            part.PeriodicityTime = this.PeriodicityTime;
            part.PeriodicityUnit = this.PeriodicityUnit;
            part.ContentItemId = this.ContentItemId;
            part.RunningTaskId = this.Running;
            part.Autodestroy = this.Autodestroy;
            part.ExecutionType = this.ExecutionType;
            part.LongTask = this.LongTask;
        }
    }
}