using System;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;
using Orchard.Projections.Providers.SortCriteria;
using Laser.Orchard.Events.Models;

namespace Laser.Orchard.Events.Projections
{
    public class ActivityEndDateSortCriterionProvider : ISortCriterionProvider
    {
        public Localizer T { get; set; }

        public ActivityEndDateSortCriterionProvider()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeSortCriterionContext describe)
        {
            describe.For("ActivityPart", T("Activity Part Record"), T("Position in the list of activities"))
                    .Element("DateTimeEnd", T("Event End Date"), T("Ending date of the activity"),
                             context => ApplyFilter(context),
                             context => DisplaySortCriterion(context),
                             SortCriterionFormProvider.FormName
                            );
        }

        public void ApplyFilter(SortCriterionContext context)
        {
            //Non eseguo niente perchè l'ordinamento va eseguito su un valore calcolato ottenuto a posteriori dell'esecuzione di tale query.
            //Mi interessa quindi in questa fase solo la definizione del sort criterion, non la sua esecuzione.
            //Ripristino Ordinamento per gestire situazione semplice via projection, vedi expoincitta

            bool ascending = Convert.ToBoolean(context.State.Sort);
            context.Query = ascending
                ? context.Query.OrderBy(alias => alias.ContentPartRecord(typeof(ActivityPartRecord)), x => x.Asc("DateTimeEnd"))
                : context.Query.OrderBy(alias => alias.ContentPartRecord(typeof(ActivityPartRecord)), x => x.Desc("DateTimeEnd"));
        }

        public LocalizedString DisplaySortCriterion(SortCriterionContext context)
        {
            bool ascending = Convert.ToBoolean(context.State.Sort);
            string orderedField = T("End Date").Text;

            if (ascending)
                return T("Ordered by {0}, ascending", orderedField);
            else
                return T("Ordered by {0}, descending", orderedField);
        }
    }
}