using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Laser.Orchard.Events.Models;
using System;

namespace Laser.Orchard.Events.Handlers
{
    public class ActivityPartHandler : ContentHandler
    {
        public ActivityPartHandler(IRepository<ActivityPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));

            OnInitializing<ActivityPart>((context, part) =>
            {
                int currYear = DateTime.Now.Year;
                int currMonth = DateTime.Now.Month;
                int currDay = DateTime.Now.Day;

                part.DateTimeStart = new DateTime(currYear, currMonth, currDay);
                part.DateTimeEnd = new DateTime(currYear, currMonth, currDay,23,59,59);

                part.RepeatValue = 1;
                part.RepeatEnd = false;
                part.RepeatEndDate = new DateTime(currYear + 1, currMonth, currDay);
            });
        }
    }
}