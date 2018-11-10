using System;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;
using Orchard.Projections.Providers.SortCriteria;
using Laser.Orchard.Events.Models;

namespace Laser.Orchard.Events.Projections
{
    public class ActivityStartDateSortCriterionProvider : ISortCriterionProvider
    {
        public Localizer T { get; set; }

        public ActivityStartDateSortCriterionProvider()
        {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeSortCriterionContext describe)
        {
            describe.For("ActivityPart", T("Activity Part Record"), T("Position in the list of activities"))
                    .Element("DateTimeStart", T("Activity Start Date"), T("Starting date of the event"),
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
                ? context.Query.OrderBy(alias => alias.ContentPartRecord(typeof(ActivityPartRecord)), x => x.Asc("DateTimeStart"))
                : context.Query.OrderBy(alias => alias.ContentPartRecord(typeof(ActivityPartRecord)), x => x.Desc("DateTimeStart"));
        }

        public LocalizedString DisplaySortCriterion(SortCriterionContext context)
        {
            bool ascending = Convert.ToBoolean(context.State.Sort);
            string orderedField = T("Start Date").Text;

            if (ascending)
                return T("Ordered by {0}, ascending", orderedField);
            else
                return T("Ordered by {0}, descending", orderedField);
        }
    }
}