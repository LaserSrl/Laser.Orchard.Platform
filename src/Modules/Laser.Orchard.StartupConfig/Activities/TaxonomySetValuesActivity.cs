using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Workflows.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Users.Models;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Services;
using Orchard.Localization.Services;

namespace Laser.Orchard.StartupConfig.Activities {
    public class TaxonomySetValuesActivity : Task {
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ILocalizationService _localizationService;
        public TaxonomySetValuesActivity(IContentManager contentManager, ITaxonomyService taxonomyService, ILocalizationService localizationService)
        {
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
            _localizationService = localizationService;
        }



        public override string Name {
            get {
                return "TaxonomySetValues";
                //    throw new NotImplementedException(); 
            }
        }

        public override string Form {
            get {
                return "TheFormTaxonomySetValues";
            }
        }

        public override LocalizedString Category {
            get {
                return T("Taxonomy");// throw new NotImplementedException();
            }
        }

        public override LocalizedString Description {
            get {
                return T("Assign Values to taxonomy");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Success"), T("Error") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            LocalizedString messageout = T("Success");
            try
            {
                var newterm = ((string)activityContext.GetState<string>("allterms")).Split(',').Select(Int32.Parse).ToList();
                if (newterm.Count() == 0)
                {
                    messageout = T("Error");
                }
                var content = workflowContext.Content;

                // List<TermPart> termParts = _contentManager
                //.Query<TermPart, TermPartRecord>()
                //.Where(x =>newterm.Contains( x.Id)).List();
                // List<Int32> termsId=termParts.Select(x=>x.Id).ToList();

                var taxonomieList = content.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(TaxonomyField).Name)).Cast<TaxonomyField>();
                foreach (var singletaxo in taxonomieList)
                {
                    var thetaxonomy_field = _taxonomyService.GetTaxonomyByName(singletaxo.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"]);
                    List<TaxonomyPart> taxonomies_localized = new List<TaxonomyPart>();
                    taxonomies_localized.Add(thetaxonomy_field);
                    foreach (var tax_local in _localizationService.GetLocalizations(thetaxonomy_field))                   
                        taxonomies_localized.Add(tax_local.ContentItem.As<TaxonomyPart>());
                    List<TermPart> nuovitermini = new List<TermPart>();
                    foreach (var thetaxonomy in taxonomies_localized)
                    {
                        IEnumerable<TermPart> termini = _taxonomyService.GetTerms(thetaxonomy.Id);                       
                        foreach (var singleTerm in termini)
                        {
                            if (newterm.Contains(singleTerm.Id) || newterm.Contains(thetaxonomy.Id))
                            {
                                nuovitermini.Add(singleTerm);
                            }
                        }
                    }
                    _taxonomyService.UpdateTerms(content.ContentItem, nuovitermini, singletaxo.Name.ToString());
                }


                //                .Parts.FirstOrDefault(x => x.Fields.Any(y => y.Name == "Website"));
                //var websiteLinkField = websitePart .Fields.FirstOrDefault(x => x.Name == "Website") as LinkField;
                //websiteLinkField.Value = "http://www.google.com";
                //websiteLinkField.Text = "Link to google";
                //_orchardServices.ContentManager.Publish(myItem);

                //    ((dynamic)content.ContentItem).CommonPart.Owner = userpart;
            }
            catch { messageout = T("Error"); }
            yield return messageout;
        }
    }
}


