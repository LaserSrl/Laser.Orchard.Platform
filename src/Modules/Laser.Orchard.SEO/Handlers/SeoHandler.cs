using Laser.Orchard.SEO.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.SEO.Handlers {

    public class SeoHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;

        public SeoHandler(
            IOrchardServices orchardServices, 
            IRepository<SeoVersionRecord> repository, 
            ITokenizer tokenizer) {

            _orchardServices = orchardServices;
            _tokenizer = tokenizer;
            
            Filters.Add(StorageFilter.For(repository));

            //we initialize a date that is valid for the database.
            OnInitializing<SeoPart>((context, part) => {
                int currYear = DateTime.Now.Year;
                int currMonth = DateTime.Now.Month;
                int currDay = DateTime.Now.Day;
                part.RobotsUnavailableAfterDate = new DateTime(currYear, currMonth, currDay);
            });


            //load the presets from the settings
            OnInitialized<SeoPart>((context, part) => {
                var settings = part.Settings.GetModel<SeoPartSettings>();
                //copy presets
                part.CanonicalUrl = settings.CanonicalUrl;
                part.RobotsNoIndex = settings.RobotsNoIndex;
                part.RobotsNoFollow = settings.RobotsNoFollow;
                part.RobotsNoSnippet = settings.RobotsNoSnippet;
                part.RobotsNoOdp = settings.RobotsNoOdp;
                part.RobotsNoArchive = settings.RobotsNoArchive;
                part.RobotsUnavailableAfter = settings.RobotsUnavailableAfter;
                part.RobotsNoImageIndex = settings.RobotsNoImageIndex;
                part.GoogleNoSiteLinkSearchBox = settings.GoogleNoSiteLinkSearchBox;
                part.GoogleNoTranslate = settings.GoogleNoTranslate;
            });



            OnGetDisplayShape<SeoPart>((context, part) => {
             
                if (context.DisplayType == "Detail") {
                    var settings = part.Settings.GetModel<SeoPartSettings>();
                    var layout = (dynamic)context.Layout;

                    if (!string.IsNullOrEmpty(part.TitleOverride)) {
                        layout.Head.Add(context.New.TitleScript(Title: part.TitleOverride));
                    }
                    else {
                        if (((dynamic)part.ContentItem).TitlePart?.Title != null) {
                            layout.Head.Add(context.New.TitleScript(Title: ((dynamic)part.ContentItem).TitlePart?.Title));
                        }
                    }

                    // Canonical URL
                    var canonicalUrl = part.CanonicalUrl;
                    if (string.IsNullOrWhiteSpace(canonicalUrl)) {
                        // fallback: content type settings
                        canonicalUrl = part.Settings.GetModel<SeoPartSettings>().CanonicalUrl;
                    }
                    if (string.IsNullOrWhiteSpace(canonicalUrl)) {
                        // fallback: site settings
                        canonicalUrl = _orchardServices.WorkContext.CurrentSite
                            ?.As<SeoSiteSettingsPart>()?.CanonicalUrl;
                    }
                    if (!string.IsNullOrWhiteSpace(canonicalUrl)) {
                        var tokensDictionary = new Dictionary<string, object> { { "Content", (dynamic)part.ContentItem } };
                        layout.Head.Add(context.New.CanonicalUrlScript(CanonicalUrl: _tokenizer.Replace(canonicalUrl, tokensDictionary)));
                    }
                                     
                    //eval text box area
                    if (!string.IsNullOrEmpty(settings.JsonLd) && !part.HideDetailMicrodata) {
                        string script = scriptEval(settings, part);
                        layout.Head.Add(context.New.SeoMicrodataScript(ScriptMicrodata: script));
                    }

                    //carousel microdata - attualmente funzionante per projection e taxonomy term
                    if (layout.SummaryMicrodata != null && settings.ShowAggregatedMicrodata && !part.HideAggregatedMicrodata) {
                        string script = buildCarouselMicrodata(layout.SummaryMicrodata);
                        layout.Head.Add(context.New.SeoMicrodataScript(ScriptMicrodata: script));
                    }
                } else if (context.DisplayType == "Summary") {
                    if (!part.HideDetailMicrodata) {
                        var layout = (dynamic)context.Layout;

                        if (layout.SummaryMicrodata == null)
                            layout.SummaryMicrodata = new List<string>();

                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

                        layout.SummaryMicrodata.Add(urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(part)));
                    }
                }
            });

        }

        private string scriptEval(SeoPartSettings settings, SeoPart part) {
            var tokensDictionary = new Dictionary<string, object> { { "Content", part.ContentItem } };
            return _tokenizer.Replace(settings.JsonLd, tokensDictionary); ;
        }

        private string buildCarouselMicrodata(List<string> urlList) {
            List<string> templateList = new List<string>();
            string elementTemplate = "{{\"@type\":\"ListItem\",\"position\":{0},\"url\":\"{1}\"}}";

            int counter = 1;
            foreach (string url in urlList) {
                templateList.Add(string.Format(elementTemplate, counter, url));
                counter++;
            }

            return string.Format("<script type=\"application/ld+json\">{{\"@context\":\"http://schema.org\",\"@type\":\"ItemList\",\"itemListElement\":[{0}]}}</script>", string.Join(",", templateList));
        }

    }

}