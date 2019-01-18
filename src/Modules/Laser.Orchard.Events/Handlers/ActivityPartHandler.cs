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

            //Necessario per inizializzare l'oggetto nel caso in cui l'activity part venga aggiunta in un secondo momento
            OnLoaded<ActivityPart>((context, part) => {
                // RepeatValue non può mai essere < 1 a causa del vincolo sul ViewModel, quindi se ha questo valore significa che Orchard lo sta inizializzando con i valori di default
                if (part.RepeatValue < 1) {
                    int currYear = DateTime.Now.Year;
                    int currMonth = DateTime.Now.Month;
                    int currDay = DateTime.Now.Day;

                    part.DateTimeStart = new DateTime(currYear, currMonth, currDay);
                    part.DateTimeEnd = new DateTime(currYear, currMonth, currDay, 23, 59, 59);

                    part.RepeatValue = 1;
                    part.RepeatEnd = false;
                    part.RepeatEndDate = new DateTime(currYear + 1, currMonth, currDay);
                }
            });
        }
    }
}